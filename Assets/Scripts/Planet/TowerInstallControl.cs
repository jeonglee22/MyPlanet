using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TowerInstallType
{
    Attack,
    Amplifier,
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

        ResetTowerSlot(towerCount);

        if (towerInfoObj != null)
            towerInfoObj.SetActive(false);
        CurrentTowerCount = 0;
    }

    private void Update()
    {
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
        // idx = 3; // Missile
        // idx = 2; // Lazer
        // idx = 4; // ShootGun
        return availableTowerDatas[idx];
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
        }
        else if (attack != null && attack.AttackTowerData != null)
        {
            HighlightForAttackSlot(index);
        }
        else ClearAllSlotHighlights();
    }

    public TowerDataSO GetTowerData(int index)
    {
        if (index < 0 || index >= assignedTowerDatas.Length) return null;
        return assignedTowerDatas[index];
    }

    private void SettingTowerTransform(float baseAngle)
    {
        foreach (var tower in towers)
        {
            var pos = new Vector2(
                Mathf.Cos((baseAngle + 90f) * Mathf.Deg2Rad),
                Mathf.Sin((baseAngle + 90f) * Mathf.Deg2Rad)
                ) * towerRadius;
            var rot = new Vector3(0, 0, baseAngle);
            var towerRect = tower.GetComponent<RectTransform>();
            towerRect.localPosition = pos;
            towerRect.rotation = Quaternion.Euler(rot);

            baseAngle += 360f / towerCount;
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
        return PickRandomTowerData();
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
        Debug.Log($"[TowerInstallControl] Long press start on slot {index}, pos={screenPos}");

        // ⚠ 여기서는 TowerInfoUI를 새로 열지 않는다.
        //    (기존에 떠 있던 정보창이 있으면 그대로 유지)

        // 다음 단계에서:
        // - 드래그 상태로 전환
        // - 드래그 고스트 생성
        // - 원래 슬롯 UI 투명하게 만들기
        // 등을 여기에 붙일 예정
    }
    //--------------------------------------------------------------
}