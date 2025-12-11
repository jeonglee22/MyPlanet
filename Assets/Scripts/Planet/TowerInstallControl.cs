using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks.Triggers;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TowerInstallType
{
    Attack,
    Amplifier,
    //Gold,
}

public class TowerInstallChoice
{
    public TowerInstallType InstallType;
    public TowerDataSO AttackTowerData;
    public AmplifierTowerDataSO AmplifierTowerData;
    public int ability;
    public int[] BuffSlotIndex; //basic buff
    public int[] RandomAbilitySlotIndex; //ability buff
}

public class TowerInstallControl : MonoBehaviour
{
    [SerializeField] private int towerCount;
    public int TowerCount { get => towerCount; }

    private int currentTowerCount;
    public int CurrentTowerCount
    {
        get { return currentTowerCount; }
        set
        {
            currentTowerCount = value;
            OnTowerInstalled?.Invoke();
        }
    }

    private int maxTowerCount = 6;
    public int MaxTowerCount { get => maxTowerCount; }

    public event Action OnTowerInstalled;

    [SerializeField] private RectTransform PlanetTransform;
    [SerializeField] private GameObject planetObj;

    [SerializeField] private float rotateSpeed = 300f;

    [SerializeField] private GameObject towerInfoObj;
    [SerializeField] private GameObject towerUIBasePrefab; //UI
    [SerializeField] private GameObject emptySlotPrefab; //UI

    private Planet planet;
    private List<GameObject> towers;
    public List<GameObject> Towers => towers;

    private float towerRadius = 100f;
    public float TowerRadius { get => towerRadius; }

    [SerializeField] private PlanetTowerUI planetTowerUI;
    private float currentAngle;
    public float CurrentAngle { get => currentAngle; }

    //test
    private bool[] emptyTower;
    [SerializeField] private List<TowerDataSO> availableTowerDatas; //Attack Tower List
    private TowerDataSO[] assignedTowerDatas;

    public bool IsReadyInstall { get; set; }
    public TowerInstallChoice ChoosedData { get; set; }

    public bool isInstall = true;
    private float dragRotateSpeed = 300f;

    //reinforceLevel
    private int maxReinforceLevel = 4;
    public int MaxReinforceLevel => maxReinforceLevel;

    [Header("Drag Settings")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private RectTransform viewportRect;
    private Vector2 beforeSize;
    [SerializeField] private GameObject dragImagePrefab; 
    [SerializeField] private TowerUpgradeSlotUI towerUpgradeSlotUI;

    [SerializeField] private RectTransform leftRotateRect;
    public RectTransform LeftRotateRect => leftRotateRect;
    [SerializeField] private RectTransform rightRotateRect;
    public RectTransform RightRotateRect => rightRotateRect;

    private bool isDraggingTower = false;
    private int dragSourceIndex = -1;
    private GameObject currentDragGhost;
    public GameObject CurrentDragGhost => currentDragGhost;
    private RectTransform currentDragGhostRect;

    private Image dragSourceImage;
    private Color dragSourceOriginalColor;

    [Header("Delete Settings")]
    [SerializeField] private GameObject trashIconObj;      
    [SerializeField] private RectTransform trashIconRect;  
    [SerializeField] private GameObject deleteConfirmPanel;
    [SerializeField] private GameObject yesOrNoPanel;
    [SerializeField] private Button deleteYesButton;
    [SerializeField] private Button deleteNoButton;

    private int pendingDeleteIndex = -1;

    private bool isTutorial = false;

    private void Awake()
    {
        planetTowerUI.TowerCount = towerCount;

        emptyTower = new bool[towerCount];

        var index = UnityEngine.Random.Range(0, towerCount);
        for (int i = 0; i < towerCount; i++)
        {
            emptyTower[i] = true;
        }
        assignedTowerDatas = new TowerDataSO[towerCount];
    }

    void Start()
    {
        planet = planetObj.GetComponent<Planet>();

        SetPlanetSize();

        ResetTowerSlot(towerCount);

        if (towerInfoObj != null)
            towerInfoObj.SetActive(false);
        CurrentTowerCount = 0;

        //rm tower
        if (trashIconObj != null)
            trashIconObj.SetActive(false);

        if (deleteConfirmPanel != null)
            deleteConfirmPanel.SetActive(false);

        if (deleteYesButton != null)
            deleteYesButton.onClick.AddListener(OnDeleteYes);

        if (deleteNoButton != null)
            deleteNoButton.onClick.AddListener(OnDeleteNo);
        
        SetIsTutorial(TutorialManager.Instance.IsTutorialMode);
    }

    private void Update()
    {
        if((currentDragGhost != null || (towerUpgradeSlotUI != null && towerUpgradeSlotUI.DragImage != null)) && TouchManager.Instance.IsTouching)
        {
            if(RectTransformUtility.RectangleContainsScreenPoint(leftRotateRect, TouchManager.Instance.TouchPos))
            {
                currentAngle += dragRotateSpeed * 0.3f * Time.unscaledDeltaTime * -1f;
                SettingTowerTransform(currentAngle);
            }
            else if (RectTransformUtility.RectangleContainsScreenPoint(rightRotateRect, TouchManager.Instance.TouchPos))
            {
                currentAngle += dragRotateSpeed * 0.3f * Time.unscaledDeltaTime * 1f;
                SettingTowerTransform(currentAngle);
            }
            planetTowerUI.Angle = currentAngle;
            return;
        }
        if (planetTowerUI != null && currentAngle != planetTowerUI.Angle)
        {
            var beforeDiff = currentAngle - planetTowerUI.Angle;

            if (planetTowerUI.IsStartDrag)
                currentAngle += dragRotateSpeed * Time.unscaledDeltaTime * (planetTowerUI.TowerRotateClock ? -1f : 1f);
            else
                currentAngle += rotateSpeed * Time.unscaledDeltaTime * (planetTowerUI.TowerRotateClock ? -1f : 1f);

            var newDiff = currentAngle - planetTowerUI.Angle;
            // Debug.Log(currentAngle.ToString() + " / " + planetTowerUI.Angle.ToString() + " / " + beforeDiff.ToString() + " / " + newDiff.ToString());

            SettingTowerTransform(currentAngle);

            if (beforeDiff * newDiff <= 0f)
            {
                currentAngle = planetTowerUI.Angle;
                SettingTowerTransform(currentAngle);
            }
        }
    }

    void LateUpdate()
    {
        SetPlanetSize();
        SettingTowerTransform(currentAngle);
    }

    private void SetPlanetSize()
    {
        Vector2 size;
        if (viewportRect != null)
        {
            size = viewportRect.rect.size;
        }
        else
            size = uiCanvas.GetComponent<RectTransform>().sizeDelta;

        if (size == beforeSize)
            return;
        
        var xSize = size.x;
        xSize += 20f;
        towerRadius = xSize * 0.5f;
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(xSize, xSize);
        beforeSize = size;
    }

    private void ResetTowerSlot(int slotCount)
    {
        towers = new List<GameObject>();

        for (int i = 0; i < slotCount; i++)
        {
            GameObject tower;
            int index = i;

            if (emptyTower[index])
            {
                tower = Instantiate(emptySlotPrefab, PlanetTransform);

                var buttonEmpty = tower.GetComponent<Button>();
                buttonEmpty.onClick.AddListener(() => IntallNewTower(index));

                towers.Add(tower);

                // Test (Slot Index Text)
                var numtext = tower.GetComponentInChildren<TextMeshProUGUI>();
                numtext.text = index.ToString();

                assignedTowerDatas[index] = null;

                var highlight = tower.GetComponent<TowerSlotHighlightUI>();
                if (highlight != null)
                {
                    highlight.RefreshDefaultColorFromImage();
                }
                continue;
            }

            //Install Attack Tower (Default: attack tower)
            tower = Instantiate(towerUIBasePrefab, PlanetTransform);
            var chosenData = PickRandomTowerData();
            assignedTowerDatas[index] = chosenData;

            TryAssignDataToTower(tower, chosenData);
            // attack.SetRandomAbility();

            //Install Attack System -> Planet.cs
            //Tower Targeting System Index Debug
            var targeting = tower.GetComponent<TowerTargetingSystem>();
            if (targeting != null)
            {
                targeting.SetSlotIndex(index);
                targeting.SetTowerData(chosenData);
            }

            //20251202 16:06 click -> seperate drag move tower 
            //var button = tower.GetComponent<Button>();
            //button.onClick.AddListener(() => OpenInfoUI(index));
            var inputHandler = tower.GetComponent<TowerSlotInputHandler>();
            if (inputHandler == null)
                inputHandler = tower.AddComponent<TowerSlotInputHandler>();
            inputHandler.Initialize(this, index);

            // Test (Color)
            var image = tower.GetComponentInChildren<Image>();
            image.color = Color.Lerp(Color.red, Color.blue, (float)i / (slotCount - 1));

            var text = tower.GetComponentInChildren<TextMeshProUGUI>();
            text.text = index.ToString();

            towers.Add(tower);
            planet?.SetAttackTower(assignedTowerDatas[index], index);

            var highlight2 = tower.GetComponent<TowerSlotHighlightUI>();
            if (highlight2 != null) highlight2.RefreshDefaultColorFromImage();
        }
        SettingTowerTransform(0f);
        currentAngle = 0f;
        RefreshSlotLabels();
        RefreshSlotInputs();
    }

    public bool IsUsedSlot(int index)
    {
        if (emptyTower == null) return false;

        return !emptyTower[index];
    }

    public void UpgradeTower(int index)
    {
        if (!IsReadyInstall) return;
        if (ChoosedData == null) return;
        if (IsSlotMaxLevel(index)) return;

        int abilityId = ChoosedData.ability;

        planet?.UpgradeTower(index);

        IsReadyInstall = false;
        isInstall = true;
    }


    private void TryAssignDataToTower(GameObject towerObj, TowerDataSO data)
    {
        if (data == null || towerObj == null) return;

        //UI (Test: Print Tower Name)
        var nameText = towerObj.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (nameText != null) nameText.text = data.towerId;
    }

    private TowerDataSO PickRandomTowerData()
    {
        int idx = UnityEngine.Random.Range(0, availableTowerDatas.Count);

        //tutorial
        if(isTutorial && Variables.Stage == 1)
        {
            idx = 1; // Basic Tower
        }      
        // idx = 3; // Missile
        // idx = 2; // Lazer
        // idx = 4; // ShootGun
        return availableTowerDatas[idx];
    }
    public TowerDataSO GetRandomAttackTowerDataForCard(ICollection<TowerDataSO> extraExcludes = null)
    {
        if (availableTowerDatas == null || availableTowerDatas.Count == 0)
            return null;

        if(isTutorial && Variables.Stage == 1)
        {
            var tutorialIdx = 1; // Basic Tower
            return availableTowerDatas[tutorialIdx];
        }       

        HashSet<TowerDataSO> excludeSet = new HashSet<TowerDataSO>();

        for (int i = 0; i < towerCount; i++)
        {
            var data = GetTowerData(i);
            if (data != null)
                excludeSet.Add(data);
        }

        if (extraExcludes != null)
        {
            foreach (var d in extraExcludes)
            {
                if (d != null)
                    excludeSet.Add(d);
            }
        }

        List<TowerDataSO> candidates = new List<TowerDataSO>();
        foreach (var d in availableTowerDatas)
        {
            if (d == null) continue;
            if (!excludeSet.Contains(d))
                candidates.Add(d);
        }

        if (candidates.Count == 0)
            return null;

        // Weight Pick
        if(CollectionManager.Instance == null || !CollectionManager.Instance.IsInitialized)
        {
            int colIdx = UnityEngine.Random.Range(0, candidates.Count);
            return candidates[colIdx];
        }

        List<float> weights = new List<float>();
        float totalWeight = 0f;

        foreach(var towerData in candidates)
        {
            int towerId = towerData.towerIdInt;
            float weight = CollectionManager.Instance.GetWeight(towerId);
            weights.Add(weight);
            totalWeight += weight;
        }

        float rand = UnityEngine.Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        for(int i = 0; i < candidates.Count; i++)
        {
            cumulativeWeight += weights[i];
            if(rand <= cumulativeWeight)
            {
                return candidates[i];
            }
        }

        return candidates[candidates.Count - 1];
    }

    public void IntallNewTower(int index)
    {
        if (!IsReadyInstall) return;

        if (currentTowerCount >= maxTowerCount)
        {
            Debug.Log("Tower limit");
            return;
        }

        // test Install
        // var newTower = Instantiate(towerBasePrefab, PlanetTransform);
        // var chosenData = PickRandomTowerData();
        if (ChoosedData == null) return;

        Destroy(towers[index]);

        GameObject newTower = null;

        // Attack||Amplifier Tower Branch ------------
        if (ChoosedData.InstallType == TowerInstallType.Attack)
        {
            //Install Attack Tower
            newTower = Instantiate(towerUIBasePrefab, PlanetTransform);
            towers[index] = newTower;

            TowerDataSO chosenData = ChoosedData.AttackTowerData;
            assignedTowerDatas[index] = chosenData;
            //UI
            TryAssignDataToTower(newTower, chosenData);

            //Attack Tower Targeting System
            var targeting = newTower.GetComponent<TowerTargetingSystem>();
            if (targeting != null)
            {
                targeting.SetSlotIndex(index); //Debug
                targeting.SetTowerData(chosenData);
            }

            //Set Attack Tower In Planet
            planet?.SetAttackTower(
                assignedTowerDatas[index],
                index,
                ChoosedData.ability);
        }
        else if (ChoosedData.InstallType == TowerInstallType.Amplifier)
        {
            //Install Amplifier Tower
            newTower = Instantiate(towerUIBasePrefab, PlanetTransform);
            towers[index] = newTower;

            assignedTowerDatas[index] = null; //(assignedTowerDatas: only attack tower)

            //Set Amplifier Tower In Planet
            if (ChoosedData.AmplifierTowerData != null)
            {
                planet?.SetAmplifierTower(
                    ChoosedData.AmplifierTowerData,
                    index,
                    ChoosedData.ability,
                    ChoosedData.BuffSlotIndex,
                    ChoosedData.RandomAbilitySlotIndex
                    );
            }
        }
        //---------------------------------------------------------
        //UI-------------------------------------------------------
        if (newTower != null)
        {

            //20251202 16:11 click -> seperate drag move tower 
            //var button = newTower.GetComponent<Button>();
            //button.onClick.AddListener(() => OpenInfoUI(index));
            var inputHandler = newTower.GetComponent<TowerSlotInputHandler>();
            if (inputHandler == null)
            {
                inputHandler = newTower.AddComponent<TowerSlotInputHandler>();
            }
            inputHandler.Initialize(this, index);

            var image = newTower.GetComponentInChildren<Image>();
            var text = newTower.GetComponentInChildren<TextMeshProUGUI>();

            if (ChoosedData.InstallType == TowerInstallType.Attack)
            {
                image.color = Color.Lerp(Color.red, Color.blue, (float)index / (towerCount - 1));
                text.text = index.ToString();
            }
            else
            {
                image.color = Color.yellow;
                text.text = $"{index}+";
            }

            var highlight = newTower.GetComponent<TowerSlotHighlightUI>();
            if (highlight != null)
            {
                highlight.RefreshDefaultColorFromImage();
            }
        }
        //----------------------------------------------------------

        SettingTowerTransform(currentAngle);

        emptyTower[index] = false;
        planetTowerUI.gameObject.SetActive(false);

        CurrentTowerCount += 1;
        IsReadyInstall = false;
        ChoosedData = null;
        isInstall = true;

        RefreshSlotLabels();
        RefreshSlotInputs();
    }

    public void OpenInfoUI(int index)
    {
        if (IsReadyInstall || towerInfoObj == null) return;

        towerInfoObj.SetActive(true);
        var info = towerInfoObj.GetComponent<TowerInfoUI>();
        info.SetInfo(index);

        var attack = GetAttackTower(index);
        var amp = GetAmplifierTower(index);

        if (amp != null && amp.AmplifierTowerData != null)
        {
            HighlightForAmplifierSlot(index);
            SizeUpSlot(index);
        }
        else if (attack != null && attack.AttackTowerData != null)
        {
            HighlightForAttackSlot(index);
            SizeUpSlot(index);
        }
        else 
        {
            ClearAllSlotHighlights();
        }
    }

    private void SizeUpSlot(int index)
    {
        var tower = towers[index];
        if (tower == null) return;

        tower.GetComponent<RectTransform>().localScale = Vector3.one * 1.5f;
    }

    public TowerDataSO GetTowerData(int index)
    {
        if (index < 0 || index >= assignedTowerDatas.Length) return null;
        return assignedTowerDatas[index];
    }

    private void SettingTowerTransform(float baseAngle)
    {
        // baseAngle = baseAngle % 360f;
        // if (baseAngle < 0f)
        //     baseAngle += 360f;
        // Debug.Log("SettingTowerTransform Angle : " + baseAngle.ToString());

        var sb = new StringBuilder();
        foreach (var tower in towers)
        {
            var pos = new Vector2(
                Mathf.Cos((baseAngle + 90f) * Mathf.Deg2Rad),
                Mathf.Sin((baseAngle + 90f) * Mathf.Deg2Rad)
                ) * (towerRadius + 17f);
            
            var rot = new Vector3(0, 0, baseAngle);

            var towerRect = tower.GetComponent<RectTransform>();
            towerRect.localPosition = pos;
            towerRect.rotation = Quaternion.Euler(rot);

            baseAngle += 360f / towerCount;
            // baseAngle += 270f / towerCount;
            // if (baseAngle >= 135f && baseAngle < 225f)
            //     baseAngle += 90f;

            // if (baseAngle >= 360f)
            //     baseAngle -= 360f;

            // sb.Append(baseAngle);
            // sb.Append(" / ");
        }
    }

    public TowerAttack GetAttackTower(int index)
    {
        if (planet == null) return null;
        return planet.GetAttackTowerToAmpTower(index);
    }

    public TowerAmplifier GetAmplifierTower(int index)
    {
        if (planet == null) return null;
        return planet.GetAmplifierTower(index);
    }

    public void UpgradeMaxTowerCount()
    {
        maxTowerCount += 1;
    }
    public TowerDataSO GetRandomAttackTowerDataForCard() //For Pick Card
    {
        return GetRandomAttackTowerDataForCard(null);
    }

    //Slot Highliht Helper (TowerSlotHighlightUI) ------------------
    private TowerSlotHighlightUI GetSlotHighlight(int index)
    {
        if (towers == null) return null;
        if (index < 0 || index >= towers.Count) return null;

        return towers[index].GetComponent<TowerSlotHighlightUI>();
    }

    private void ClearAllSlotHighlights()
    {
        if (towers == null) return;

        foreach (var t in towers)
        {
            if (t == null) continue;

            var highlight = t.GetComponent<TowerSlotHighlightUI>();
            if (highlight != null)
            {
                highlight.SetHighlight(TowerHighlightType.None);
            }

            t.GetComponent<RectTransform>().localScale = Vector3.one;
        }
    }

    private void HighlightForAttackSlot(int attackIndex)  // buff tower to attack tower
    {
        ClearAllSlotHighlights();

        int count = towerCount;
        if (planet == null || count <= 0) return;

        for (int i = 0; i < count; i++)
        {
            var amp = GetAmplifierTower(i);
            if (amp == null || amp.AmplifierTowerData == null) continue;

            var buffSlots = amp.BuffedSlotIndex;
            var randomSlots = amp.RandomAbilitySlotIndex;

            bool isBuffTarget = buffSlots != null && buffSlots.Contains(attackIndex);
            bool isRandomTarget = randomSlots != null && randomSlots.Contains(attackIndex);

            if (!isBuffTarget && !isRandomTarget) continue;

            var highlight = GetSlotHighlight(i);
            if (highlight == null) continue;

            highlight.SetHighlight(TowerHighlightType.FromAttackSource);
        }
    }

    private void HighlightForAmplifierSlot(int ampIndex)
    {
        ClearAllSlotHighlights();

        var amp = GetAmplifierTower(ampIndex);
        if (amp == null || amp.AmplifierTowerData == null) return;

        var buffSlots = amp.BuffedSlotIndex;       
        var randomSlots = amp.RandomAbilitySlotIndex; 

        if (buffSlots != null) // buff target
        {
            foreach (var slot in buffSlots)
            {
                var highlight = GetSlotHighlight(slot);
                if (highlight == null) continue;

                highlight.SetHighlight(TowerHighlightType.BuffTarget);
            }
        }

        if (randomSlots != null) // ability target
        {
            foreach (var slot in randomSlots)
            {
                if (buffSlots != null && buffSlots.Contains(slot))
                    continue;

                var highlight = GetSlotHighlight(slot);
                if (highlight == null) continue;

                highlight.SetHighlight(TowerHighlightType.RandomOnlyTarget);
            }
        }
    }
    //--------------------------------------------------------------
    //Move Tower----------------------------------------------------
    public void OnSlotClick(int index)
    {
        OpenInfoUI(index);
    }

    public void OnSlotLongPressStart(int index, Vector2 screenPos)
    {
        if (IsReadyInstall) return; //Consider preventing Install mod 

        if (towerInfoObj != null && towerInfoObj.activeSelf) 
        {
            towerInfoObj.SetActive(false);
        }

        ClearAllSlotHighlights();

        if (isDraggingTower) return;

        //tower null check
        if (towers == null || index < 0 || index >= towers.Count) return;
        if (emptyTower != null && emptyTower[index]) return; 
        isDraggingTower = true;
        dragSourceIndex = index;


        //rm tower
        bool canShowTrash = false;
        var attack = GetAttackTower(index);
        var amp = GetAmplifierTower(index);

        if (attack != null)
        {
            int attackCount = planet != null ? planet.GetAttackTowerCount() : 0;
            canShowTrash = attackCount > 1;
        }
        else if (amp != null)
        {
            canShowTrash = true;
        }

        if (trashIconObj != null)
            trashIconObj.SetActive(canShowTrash);
        //drag prefab
        if (dragImagePrefab != null && uiCanvas != null)
        {
            currentDragGhost = Instantiate(dragImagePrefab, uiCanvas.transform);
            currentDragGhostRect = currentDragGhost.GetComponent<RectTransform>();

            var sourceObj = towers[index];
            var sourceImage = sourceObj.GetComponentInChildren<Image>();
            dragSourceImage = sourceImage; 

            if (sourceImage != null)
            {
                dragSourceOriginalColor = sourceImage.color;

                var ghostUI = currentDragGhost.GetComponent<TowerDragImageUI>();
                if (ghostUI != null && ghostUI.IconImage != null)
                {
                    var ghostImage = ghostUI.IconImage;
                    ghostImage.sprite = sourceImage.sprite;
                    ghostImage.color = sourceImage.color;
                    ghostImage.preserveAspect = true;
                    ghostImage.rectTransform.sizeDelta = new Vector2(25f, 50f);
                }
                var c = sourceImage.color;
                c.a = 0f;
                sourceImage.color = c;
            }
            UpdateDragGhostPosition(screenPos);
        }
    }

    public void OnSlotLongPressDrag(int index, Vector2 screenPos)
    {
        if (!isDraggingTower) return;
        if (index != dragSourceIndex) return; 

        UpdateDragGhostPosition(screenPos);
    }
    private void UpdateDragGhostPosition(Vector2 screenPos)
    {
        if (currentDragGhostRect == null || uiCanvas == null) return;

        RectTransform canvasRect = uiCanvas.transform as RectTransform;

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : uiCanvas.worldCamera,
            out localPos);

        currentDragGhostRect.anchoredPosition = localPos;
    }

    public void OnSlotLongPressEnd(int index, Vector2 screenPos)
    {
        if (!isDraggingTower) return;

        int sourceIndex = dragSourceIndex; // Cleanup 전에 따로 보관

        // ---- 휴지통 영역에 드랍됐는지 먼저 체크 ----
        bool droppedOnTrash = false;
        if (trashIconObj != null && trashIconObj.activeSelf && trashIconRect != null && uiCanvas != null)
        {
            Camera cam = uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : uiCanvas.worldCamera;

            if (RectTransformUtility.RectangleContainsScreenPoint(trashIconRect, screenPos, cam))
            {
                droppedOnTrash = true;
            }
        }

        if (droppedOnTrash)
        {
            // 1) 드래그 고스트 정리 & 원래 슬롯 시각 복구
            CleanupDragVisual();  // 여기서 dragSourceIndex는 -1로 초기화되지만 sourceIndex에 이미 저장해둠

            // 2) 삭제 확인 팝업 세팅
            pendingDeleteIndex = sourceIndex;
            ShowDeleteConfirm();

            Debug.Log($"[TowerInstallControl] Drop on trash from {sourceIndex}");
        }
        else
        {
            // 기존 슬롯 드랍 로직 (스왑/이동)
            int targetIndex = FindSlotIndexFromScreenPos(screenPos);
            if (targetIndex >= 0 && targetIndex != sourceIndex)
            {
                MoveOrSwapTower(sourceIndex, targetIndex);
                Debug.Log($"[TowerInstallControl] Long press drag end from {sourceIndex} to {targetIndex}");
            }
            else
            {
                Debug.Log($"[TowerInstallControl] Long press drag end, no valid target. index={sourceIndex}");
            }

            // 고스트/투명 처리 복구
            CleanupDragVisual();
        }

        // 휴지통 아이콘은 항상 끄기
        if (trashIconObj != null)
            trashIconObj.SetActive(false);
    }


    private void CleanupDragVisual()
    {
        if (currentDragGhost != null)
        {
            Destroy(currentDragGhost);
            currentDragGhost = null;
            currentDragGhostRect = null;
        }

        if (dragSourceImage != null)
        {
            dragSourceImage.color = dragSourceOriginalColor;
            dragSourceImage = null;
        }

        isDraggingTower = false;
        dragSourceIndex = -1;
    }
    // ---------------------- [UI & 데이터: 공격 타워 -> 빈 슬롯 이동] ----------------------
    private void MoveAttackSlotUIAndData(int fromIndex, int toIndex)
    {
        if (towers == null) return;
        if (fromIndex < 0 || fromIndex >= towers.Count) return;
        if (toIndex < 0 || toIndex >= towers.Count) return;
        if (fromIndex == toIndex) return;

        // from에는 공격 타워 UI, to에는 빈 슬롯 UI라고 가정
        GameObject fromGO = towers[fromIndex];
        GameObject toGO = towers[toIndex];

        if (fromGO == null || toGO == null) return;

        // 데이터 쪽: assignedTowerDatas / emptyTower
        TowerDataSO moveData = assignedTowerDatas[fromIndex];
        assignedTowerDatas[fromIndex] = null;
        assignedTowerDatas[toIndex] = moveData;

        emptyTower[fromIndex] = true;
        emptyTower[toIndex] = false;

        // UI 쪽: from은 EmptySlotUI, to는 towerUIBasePrefab 로 교체
        // 1) fromIndex -> Empty Slot UI 로 교체
        Destroy(fromGO);
        GameObject newEmpty = Instantiate(emptySlotPrefab, PlanetTransform);
        towers[fromIndex] = newEmpty;

        var buttonEmpty = newEmpty.GetComponent<Button>();
        if (buttonEmpty != null)
        {
            int capturedIndex = fromIndex;
            buttonEmpty.onClick.AddListener(() => IntallNewTower(capturedIndex));
        }

        var numTextEmpty = newEmpty.GetComponentInChildren<TextMeshProUGUI>();
        if (numTextEmpty != null)
        {
            numTextEmpty.text = fromIndex.ToString();
        }

        var highlightEmpty = newEmpty.GetComponent<TowerSlotHighlightUI>();
        if (highlightEmpty != null)
        {
            highlightEmpty.RefreshDefaultColorFromImage();
        }

        // 2) toIndex -> Attack Slot UI 로 교체
        Destroy(toGO);
        GameObject newAttack = Instantiate(towerUIBasePrefab, PlanetTransform);
        towers[toIndex] = newAttack;

        // Input Handler
        var inputHandler = newAttack.GetComponent<TowerSlotInputHandler>();
        if (inputHandler == null)
            inputHandler = newAttack.AddComponent<TowerSlotInputHandler>();
        inputHandler.Initialize(this, toIndex);

        // 색 / 텍스트 (인덱스 표시)
        var img = newAttack.GetComponentInChildren<Image>();
        var txt = newAttack.GetComponentInChildren<TextMeshProUGUI>();
        if (img != null)
            img.color = Color.Lerp(Color.red, Color.blue, (float)toIndex / (towerCount - 1));
        if (txt != null)
            txt.text = toIndex.ToString();

        var highlightAttack = newAttack.GetComponent<TowerSlotHighlightUI>();
        if (highlightAttack != null)
            highlightAttack.RefreshDefaultColorFromImage();

        // 원형 배치 다시 세팅
        SettingTowerTransform(currentAngle);
    }
    // ---------------------- [UI & 데이터: 공격 타워 <-> 공격 타워 스왑] ----------------------
    private void SwapAttackSlotUIAndData(int indexA, int indexB)
    {
        if (towers == null) return;
        if (indexA < 0 || indexA >= towers.Count) return;
        if (indexB < 0 || indexB >= towers.Count) return;
        if (indexA == indexB) return;

        // 둘 다 비어있지 않고, 공격 타워가 설치되어 있다고 가정 (emptyTower == false)
        if (emptyTower[indexA] || emptyTower[indexB]) return;

        // 데이터 쪽: assignedTowerDatas 스왑
        var tmpData = assignedTowerDatas[indexA];
        assignedTowerDatas[indexA] = assignedTowerDatas[indexB];
        assignedTowerDatas[indexB] = tmpData;

        // UI 쪽은 실제로 프리팹 타입(Attack/Empty)이 동일하므로
        // 여기서는 굳이 GameObject를 건들지 않아도 된다.
        // (각 슬롯은 여전히 Attack 슬롯이므로 색/텍스트는 인덱스 기준 유지)

        // 필요하다면 나중에 "타워 종류에 따라 아이콘" 같은 걸 쓸 때 여기서 Sprite 교체 가능.

        // 원형 배치 다시 세팅 (실제로는 인덱스 고정이므로 크게 변하는 건 없음)
        SettingTowerTransform(currentAngle);
    }

    private int FindSlotIndexFromScreenPos(Vector2 screenPos)
    {
        if (towers == null || uiCanvas == null) return -1;

        Camera cam = uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : uiCanvas.worldCamera;

        for (int i = 0; i < towers.Count; i++)
        {
            var go = towers[i];
            if (go == null) continue;

            var rect = go.GetComponent<RectTransform>();
            if (rect == null) continue;

            if (RectTransformUtility.RectangleContainsScreenPoint(rect, screenPos, cam))
            {
                return i;
            }
        }

        return -1;
    }

    private void MoveOrSwapTower(int sourceIndex, int targetIndex)
    {
        if (sourceIndex == targetIndex) return;
        if (towers == null) return;
        if (sourceIndex < 0 || sourceIndex >= towers.Count) return;
        if (targetIndex < 0 || targetIndex >= towers.Count) return;

        ClearAllSlotHighlights();

        // 1) Planet 쪽 실제 타워 이동/스왑 (공격 ↔ 증폭 모두 허용)
        planet?.MoveTower(sourceIndex, targetIndex);

        // 2) UI 오브젝트 스왑
        var tmpGo = towers[sourceIndex];
        towers[sourceIndex] = towers[targetIndex];
        towers[targetIndex] = tmpGo;

        // 3) 공격타워 데이터(assignedTowerDatas) 스왑
        if (assignedTowerDatas != null)
        {
            var tmpData = assignedTowerDatas[sourceIndex];
            assignedTowerDatas[sourceIndex] = assignedTowerDatas[targetIndex];
            assignedTowerDatas[targetIndex] = tmpData;
        }

        // 4) 빈 슬롯 여부 플래그 스왑
        if (emptyTower != null)
        {
            bool tmpEmpty = emptyTower[sourceIndex];
            emptyTower[sourceIndex] = emptyTower[targetIndex];
            emptyTower[targetIndex] = tmpEmpty;
        }

        // 5) 위치/회전 다시 잡기
        SettingTowerTransform(currentAngle);

        // 6) 슬롯 인풋/버튼 다시 바인딩
        RefreshSlotInputs();
        RefreshSlotLabels();

        if (towerInfoObj != null && towerInfoObj.activeSelf)
        {
            var info = towerInfoObj.GetComponent<TowerInfoUI>();
            if (info != null)
            {
                int focusIndex = info.CurrentSlotIndex;

                // 유효한 인덱스면, "지금 보고 있던 슬롯"을 다시 열어서
                // 새 상태 기준으로 하이라이트를 다시 세팅
                if (focusIndex >= 0 && focusIndex < towerCount)
                {
                    OpenInfoUI(focusIndex);
                }
                else
                {
                    ClearAllSlotHighlights();
                }
            }
            else
            {
                ClearAllSlotHighlights();
            }
        }
        else
        {
            ClearAllSlotHighlights();
        }
    }
    private void RefreshSlotInputs()
    {
        if (towers == null) return;

        for (int i = 0; i < towerCount; i++)
        {
            var go = towers[i];
            if (go == null) continue;

            var input = go.GetComponent<TowerSlotInputHandler>();
            var button = go.GetComponent<Button>();

            // 빈 슬롯
            if (emptyTower != null && emptyTower[i])
            {
                // 드래그 입력 제거
                if (input != null)
                {
                    Destroy(input);
                }

                // 설치 버튼 다시 연결
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    int captured = i;
                    button.onClick.AddListener(() => IntallNewTower(captured));
                }
            }
            // 타워가 있는 슬롯
            else
            {
                // 드래그/탭 인풋 붙이기
                if (input == null)
                {
                    input = go.AddComponent<TowerSlotInputHandler>();
                }
                input.Initialize(this, i);

                // 설치 버튼은 사용 안 하므로 리스너 제거
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                }
            }
        }
    }
    private void RefreshSlotLabels()
    {
        if (towers == null) return;

        for (int i = 0; i < towerCount; i++)
        {
            var go = towers[i];
            if (go == null) continue;

            var text = go.GetComponentInChildren<TextMeshProUGUI>();
            if (text == null) continue;

            // 빈 슬롯이면
            if (emptyTower != null && emptyTower[i])
            {
                // 디버그용으로 인덱스를 계속 보고 싶으면:
                text.text = i.ToString();
                // 완전히 숨기고 싶으면 아래처럼:
                // text.text = "";
                continue;
            }

            // 여기부터는 "타워가 있는 슬롯"

            bool isAttack = assignedTowerDatas != null && assignedTowerDatas[i] != null;
            bool isAmplifier = !isAttack; // 공격타워가 아니면서 emptyTower=false면 증폭타워

            if (isAttack)
            {
                // 공격 타워 슬롯: 인덱스만 출력
                text.text = i.ToString();
            }
            else if (isAmplifier)
            {
                // 증폭 타워 슬롯: 기존처럼 "i+"
                text.text = $"{i}+";
            }
        }
    }
    //--------------------------------------------------------------
    //rm tower -----------------------------------------------------
    private void ShowDeleteConfirm()
    {
        if (deleteConfirmPanel != null)
        {
            deleteConfirmPanel.SetActive(true);
            yesOrNoPanel.gameObject.SetActive(true);
            deleteYesButton.gameObject.SetActive(true);
            deleteNoButton.gameObject.SetActive(true);
        }
    }

    private void OnDeleteYes()
    {
        if (pendingDeleteIndex >= 0)
        {
            PerformDelete(pendingDeleteIndex);
        }

        pendingDeleteIndex = -1;

        if (deleteConfirmPanel != null)
        {
            deleteConfirmPanel.SetActive(false);
            yesOrNoPanel.gameObject.SetActive(false);
            deleteYesButton.gameObject.SetActive(false);
            deleteNoButton.gameObject.SetActive(false);
        }
    }

    private void OnDeleteNo()
    {
        pendingDeleteIndex = -1;

        if (deleteConfirmPanel != null)
            deleteConfirmPanel.SetActive(false);
    }
    private void PerformDelete(int index)
    {
        if (towers == null) return;
        if (index < 0 || index >= towers.Count) return;
        if (emptyTower != null && emptyTower[index]) return;

        planet?.RemoveTowerAt(index);

        var oldUI = towers[index];
        if (oldUI != null) Destroy(oldUI);

        GameObject newEmpty = Instantiate(emptySlotPrefab, PlanetTransform);
        towers[index] = newEmpty;

        var button = newEmpty.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            int captured = index;
            button.onClick.AddListener(() => IntallNewTower(captured));
        }

        var numText = newEmpty.GetComponentInChildren<TextMeshProUGUI>();
        if (numText != null)
        {
            numText.text = index.ToString();
        }

        var highlight = newEmpty.GetComponent<TowerSlotHighlightUI>();
        if (highlight != null)
        {
            highlight.RefreshDefaultColorFromImage();
        }

        if (emptyTower != null)
            emptyTower[index] = true;

        if (assignedTowerDatas != null)
            assignedTowerDatas[index] = null;

        CurrentTowerCount = Mathf.Max(0, CurrentTowerCount - 1);

        RefreshSlotInputs();
        ClearAllSlotHighlights();
        SettingTowerTransform(currentAngle);
    }
    //--------------------------------------------------------------

    private void SetIsTutorial(bool isTutorial)
    {
        this.isTutorial = isTutorial;
    }
    public IEnumerable<TowerAmplifier> GetAllAmplifiers()
    {
        if (planet == null) yield break;

        int count = towerCount;
        for (int i = 0; i < count; i++)
        {
            var amp = GetAmplifierTower(i);
            if (amp != null && amp.AmplifierTowerData != null)
            {
                yield return amp;
            }
        }
    }

    public int GetSlotReinforceLevel(int index)
    {
        var attack = GetAttackTower(index);
        if (attack != null)
        {
            return Mathf.Max(0, attack.ReinforceLevel);
        }

        var amp = GetAmplifierTower(index);
        if (amp != null)
        {
            return Mathf.Max(0, amp.ReinforceLevel);
        }
        return 0;
    }

    public bool IsSlotMaxLevel(int index)
    {
        return GetSlotReinforceLevel(index) >= maxReinforceLevel;
    }
    private void RotatePlanetWithDragImage()
    {
        var dragGo = currentDragGhost;
        var dragRect = currentDragGhostRect;

        if (dragGo == null || dragRect == null || uiCanvas == null) return;
    }
    public int GetUpgradeableTowerCount() //reinforceable tower count
    {
        int upgradableCount = 0;
        for(int i=0; i<towerCount; i++)
        {
            if (!IsUsedSlot(i)) continue;
            if (IsSlotMaxLevel(i)) continue;
            upgradableCount++;
        }
        return upgradableCount;
    }
}