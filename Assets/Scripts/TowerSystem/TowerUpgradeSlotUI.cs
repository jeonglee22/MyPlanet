using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TowerUpgradeSlotUI : MonoBehaviour
{
    [SerializeField] private GameObject[] upgradeUIs;
    [SerializeField] private TowerInstallControl installControl;
    [SerializeField] private TowerInfoUI towerInfoUI;
    [SerializeField] private GameObject dragImagePrefab;
    [SerializeField] private TextMeshProUGUI towerInstallText;
    private bool towerImageIsDraging = false;
    private bool isNewTouch;
    private bool isStartTouch = false;
    private Vector2 initTouchPos;

    //Add Amplifier Choice
    [Header("Legacy (optional)")]
    [SerializeField] private AmplifierTowerDataSO damageMatrixCoreSO;
    [SerializeField] private AmplifierTowerDataSO proejctileCoreSO;

    [SerializeField] private AmplifierTowerDataSO[] allAmplifierTowers;
    private TowerInstallChoice[] choices; //Tuple(Ability,Index) -> Struct(Add Tower Type)

    //test
    private Color towerColor;
    private List<int> numlist;
    [SerializeField] private TextMeshProUGUI[] uiTexts;
    private int[] abilities;
    public Color choosedColor { get; private set; }

    private bool isNotUpgradeOpen = false;
    public bool IsNotUpgradeOpen
    {
        get { return isNotUpgradeOpen; }
        set { isNotUpgradeOpen = value; }
    }

    private GameObject dragImage = null;
    private int choosedIndex = -1;
    private int firstTouchIndex = -1;
    private bool isFirstInstall = true;
    public bool IsFirstInstall => isFirstInstall;
    [SerializeField] private Button[] refreshButtons;

    private bool isTutorial = false;

    private void Start()
    {
        // foreach (var ui in upgradeUIs)
        //     ui.SetActive(false);
        towerColor = Color.yellow;

        // SetActiveRefreshButtons(false);
        installControl.OnTowerInstalled += SetTowerInstallText;

        SetIsTutorial(TutorialManager.Instance.IsTutorialMode);

        if(isTutorial && Variables.Stage == 1)
        {
            TutorialManager.Instance.ShowTutorialStep(2);
        }
    }

    void OnDestroy()
    {
        installControl.OnTowerInstalled -= SetTowerInstallText;
    }

    private async UniTaskVoid OnEnable()
    {
        await UniTask.WaitUntil(() => AbilityManager.IsInitialized);

        if (isNotUpgradeOpen)
        {
            isNotUpgradeOpen = false;
            SetActiveRefreshButtons(false);
            return;
        }

        if (upgradeUIs == null || upgradeUIs.Length == 0)
            return;
        if (refreshButtons == null || refreshButtons.Length != upgradeUIs.Length)
            return;

        foreach (var ui in upgradeUIs)
            ui.SetActive(true);

        foreach (var refreshButton in refreshButtons)
        {
            refreshButton.gameObject.SetActive(true);
            refreshButton.interactable = true;
        }
        SettingUpgradeCards();
    }

    private void SetTowerInstallText()
    {
        Debug.Log("SetTowerInstallText");
        towerInstallText.text = $"({installControl.CurrentTowerCount}/{installControl.MaxTowerCount})";
    }

    private void OnDisable()
    {
        foreach (var ui in upgradeUIs)
            ui.SetActive(false);
        SetActiveRefreshButtons(false);

        GamePauseManager.Instance.Resume();
        numlist = null;
        choosedIndex = -1;
        isStartTouch = false;
        towerImageIsDraging = false;
        isFirstInstall = false;

        if(isTutorial && Variables.Stage == 1)
        {
            TutorialManager.Instance.ShowTutorialStep(1);
        }
    }

    private void Update()
    {
        if (towerInfoUI != null && towerInfoUI.gameObject.activeSelf) return;

        OnTouchStateCheck();
        OnTouchMakeDrageImage();
    }

    private void SetActiveRefreshButtons(bool active)
    {
        foreach (var refreshButton in refreshButtons)
            refreshButton.gameObject.SetActive(active);
    }

    private void SettingUpgradeCards()
    {
        ResetChoose();
        installControl.IsReadyInstall = false;

        numlist = new List<int>();
        for (int i = 0; i < uiTexts.Length; i++)
        {
            int number;
            
            while (true)
            {
                number = UnityEngine.Random.Range(0, installControl.TowerCount);

                if (installControl.MaxTowerCount == installControl.CurrentTowerCount && 
                    !installControl.IsUsedSlot(number))
                {
                    continue;
                }

                if(!numlist.Contains(number))
                    break;
            }

            numlist.Add(number);

            if (installControl == null) continue;
            //UsedSlot ? UpgradeCard : New Tower-------------------
            if (!installControl.IsUsedSlot(number))
            {
                SetUpCard(i, number);
            }
            else
            {
                SetUpgradeCardForUsedSlot(i, number);
            }
        }
    }

    private void SetUpCard(int i, int slotNumber)
    {
        //Random Tower Type (0: Attack, 1: Amplifier)
        int towerType = Random.Range(0, 2);

        if(isFirstInstall) towerType = 0;

        if (towerType == 0) //Attack
        {
            var towerData = installControl.GetRandomAttackTowerDataForCard();

            choices[i].InstallType = TowerInstallType.Attack;
            choices[i].AttackTowerData = towerData;
            choices[i].AmplifierTowerData = null;
            choices[i].BuffSlotIndex = null;
            choices[i].RandomAbilitySlotIndex = null;

            int abilityId = GetAbilityIdForAttackTower(towerData);
            abilities[i] = abilityId;
            choices[i].ability = abilityId;

            string towerName = towerData != null ? towerData.towerId : "AttackTower";
            string abilityName = GetAbilityName(abilityId);

            uiTexts[i].text = $"{towerName}\n\n{abilityName}";
            return;
        }

        if(towerType==1)
        {
            if(isTutorial && Variables.Stage == 1)
            {
                TutorialManager.Instance.ShowTutorialStep(3);
            }

            var ampData = GetRandomAmplifier();

            choices[i].InstallType = TowerInstallType.Amplifier;
            choices[i].AmplifierTowerData = ampData;
            choices[i].AttackTowerData = null;

            //AmpTower Random Ability
            int ampAbilityId = GetRandomAbilityForAmplifier(ampData);
            choices[i].ability = ampAbilityId;  // using in Planet.SetAmplifierTower �� TowerAmplifier
            abilities[i] = ampAbilityId;

            string ampAbilityName = GetAbilityName(ampAbilityId);

            // ---------- PlaceType / RandonSlotNum / AddSlotNum ----------
            int[] buffOffsets = null;
            int[] randomOffsets = null;

            int placeType = 0;
            int randomSlotNum = 0;
            int addSlotNum = 0;

            var raRow = DataTableManager.RandomAbilityTable.Get(ampAbilityId);
            if (raRow != null)
            {
                placeType = raRow.PlaceType;
                randomSlotNum = Mathf.Max(0, raRow.RandonSlotNum);
                addSlotNum = Mathf.Max(0, raRow.AddSlotNum);
            }

            int baseBuffCount = Mathf.Max(1, ampData.FixedBuffedSlotCount);
            int effectiveBuffCount = baseBuffCount;

            switch (placeType)
            {
                case 0:
                default:
                    {
                        effectiveBuffCount = baseBuffCount;
                        buffOffsets = GetRandomBuffSlot(effectiveBuffCount);

                        if (randomSlotNum > 0)
                            randomOffsets = GetRandomBuffSlot(randomSlotNum);
                        break;
                    }
                case 1:
                    {
                        effectiveBuffCount = baseBuffCount;
                        buffOffsets = GetRandomBuffSlot(effectiveBuffCount);

                        if (buffOffsets != null && buffOffsets.Length > 0)
                        {
                            int idx = Random.Range(0, buffOffsets.Length);
                            randomOffsets = new int[] { buffOffsets[idx] };
                        }
                        break;
                    }
                case 2:
                    {
                        effectiveBuffCount = baseBuffCount + addSlotNum;
                        buffOffsets = GetRandomBuffSlot(effectiveBuffCount);

                        if (randomSlotNum > 0)
                            randomOffsets = GetRandomBuffSlot(randomSlotNum);
                        break;
                    }
            }

            choices[i].BuffSlotIndex = buffOffsets;
            choices[i].RandomAbilitySlotIndex = randomOffsets;

            //Card Text Info ------------------------------------
            string ampName = string.IsNullOrEmpty(ampData.BuffTowerName)
            ? ampData.AmplifierType.ToString()
            : ampData.BuffTowerName;

            string buffBlock=FormatOffsetArray(buffOffsets);
            string randomBlock=FormatOffsetArray(randomOffsets);
            uiTexts[i].text =
                $"{ampName}\n" +
                buffBlock +
                $"---" +
                $"\n{ampAbilityName}\n" +
                randomBlock;
        }
    }
    private string FormatOffsetArray(int[] offsets)
    {
        if (offsets == null || offsets.Length == 0)
            return string.Empty;

        List<int> rightList = new List<int>();
        List<int> leftList = new List<int>();

        foreach (int offset in offsets)
        {
            if (offset > 0) 
                rightList.Add(offset);              
            else if (offset < 0)
                leftList.Add(System.Math.Abs(offset));       
        }

        if (rightList.Count == 0 && leftList.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();

        sb.AppendLine("요격타워 기준");

        if (rightList.Count > 0)
        {
            var rightPos = new List<string>();
            foreach (int v in rightList)
                rightPos.Add($"{v}번째");

            sb.AppendLine($"오른쪽 {string.Join(", ", rightPos)}");
        }

        if (leftList.Count > 0)
        {
            var leftPos = new List<string>();
            foreach (int v in leftList)
                leftPos.Add($"{v}번째");

            sb.AppendLine($"왼쪽 {string.Join(", ", leftPos)}");
        }
        return sb.ToString();
    }

    private int GetRandomAbilityForAmplifier(AmplifierTowerDataSO ampData)
    {
        if (ampData == null) return -1;

        int groupId = ampData.RandomAbilityGroupId;
        if (groupId <= 0) return -1;

        return AbilityManager.GetRandomAbilityFromGroup(
            groupId,
            requiredTowerType: 1,
            useWeight: true);
    }

    private string GetAbilityName(int abilityId)
    {
        var row = DataTableManager.RandomAbilityTable.Get(abilityId);
        return row.RandomAbilityName;
    }

    private void ResetUpgradeCard(int index)
    {
        //abilities[index] = AbilityManager.GetRandomAbility();
        abilities[index] = -1;
        installControl.IsReadyInstall = false;
        upgradeUIs[index].GetComponentInChildren<Image>().color = Color.white;

        int number;
        while (true)
        {
            number = Random.Range(0, installControl.TowerCount);

            if (installControl.MaxTowerCount == installControl.CurrentTowerCount && 
                !installControl.IsUsedSlot(number))
            {
                continue;
            }

            if(!numlist.Contains(number))
                break;
        }

        numlist[index] = number;

        if (installControl == null) return;

        if (!installControl.IsUsedSlot(number))
        {
            SetUpCard(index, number);
        }
        else
        {
            SetUpgradeCardForUsedSlot(index, number);
        }
    }

    private void SetUpgradeCardForUsedSlot(int index, int number)
    {
        var towerData = installControl.GetTowerData(number);
        var ampTower = installControl.GetAmplifierTower(number);

        int abilityId = 0;

        if (towerData != null)
        {
            choices[index].InstallType = TowerInstallType.Attack;
            choices[index].AttackTowerData = towerData;
            choices[index].AmplifierTowerData = null;
            choices[index].BuffSlotIndex = null;
            choices[index].RandomAbilitySlotIndex = null;

            abilityId = GetAbilityIdForAttackTower(towerData);
        }
        else if (ampTower != null && ampTower.AmplifierTowerData != null)
        {
            choices[index].InstallType = TowerInstallType.Amplifier;
            choices[index].AttackTowerData = null;
            choices[index].AmplifierTowerData = ampTower.AmplifierTowerData;
            choices[index].BuffSlotIndex = null;
            choices[index].RandomAbilitySlotIndex = null;

            abilityId = GetRandomAbilityForAmplifier(ampTower.AmplifierTowerData);
        }

        abilities[index] = abilityId;
        choices[index].ability = abilityId;

        string towerName = "-";

        if (towerData != null)
        {
            towerName = towerData.towerId;
        }
        else if (ampTower != null && ampTower.AmplifierTowerData != null)
        {
            var ampData = ampTower.AmplifierTowerData;
            towerName = !string.IsNullOrEmpty(ampData.BuffTowerName)
                ? ampData.BuffTowerName
                : ampData.AmplifierType.ToString();
        }
        uiTexts[index].text = $"Upgrade\n{number}\n{towerName}";
    }

    public void OnClickRefreshButton(int index)
    {
        ResetUpgradeCard(index);
        if(refreshButtons == null) return;

        refreshButtons[index].interactable = false;
    }

    public void OnClickUpgradeUIClicked(int index)
    {
        var currentColor = upgradeUIs[index].GetComponentInChildren<Image>().color;
        if (currentColor != Color.white)
        {
            installControl.IsReadyInstall = false;
            upgradeUIs[index].GetComponentInChildren<Image>().color = Color.white;
            return;
        }

        choosedColor = towerColor;
        upgradeUIs[index].GetComponentInChildren<Image>().color = choosedColor;
        upgradeUIs[(index + 1) % 3].GetComponentInChildren<Image>().color = Color.white;
        upgradeUIs[(index + 2) % 3].GetComponentInChildren<Image>().color = Color.white;
        installControl.IsReadyInstall = true;
        installControl.ChoosedData = choices[index];

        if (installControl.IsUsedSlot(numlist[index]))
        {
            installControl.UpgradeTower(numlist[index]);
            if (towerInfoUI != null)
                towerInfoUI.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    private void ResetChoose()
    {
        abilities = new int[upgradeUIs.Length];
        choices = new TowerInstallChoice[upgradeUIs.Length];

        for (int i = 0; i < upgradeUIs.Length; i++)
        {
            upgradeUIs[i].GetComponentInChildren<Image>().color = Color.white;
            //abilities[i] = AbilityManager.GetRandomAbility();
            abilities[i] = -1;
            choices[i] = new TowerInstallChoice();
            choices[i].BuffSlotIndex = null;
            choices[i].RandomAbilitySlotIndex = null;
            choices[i].AttackTowerData = null;
            choices[i].AmplifierTowerData = null;
        }
    }

    public void OnTouchMakeDrageImage()
    {
        var touchPos = TouchManager.Instance.TouchPos;

        if(!TouchManager.Instance.IsTouching || towerImageIsDraging)
            return;

        if(!isStartTouch)
        {
            isStartTouch = true;
            initTouchPos = touchPos;

            bool isTouchOnUpgradeCard = false;
            for (int i = 0; i < upgradeUIs.Length; i++)
            {
                if(RectTransformUtility.RectangleContainsScreenPoint(upgradeUIs[i].GetComponent<RectTransform>(), initTouchPos))
                {
                    isTouchOnUpgradeCard = true;
                    firstTouchIndex = i;
                    break;
                }
            }

            if(!isTouchOnUpgradeCard)
            {
                return;
            }
        }

        if(Vector2.Distance(initTouchPos, touchPos) < 5f || !isNewTouch)
            return;

        choosedIndex = -1;
        for (int i = 0; i < upgradeUIs.Length; i++)
        {
            if(RectTransformUtility.RectangleContainsScreenPoint(upgradeUIs[i].GetComponent<RectTransform>(), touchPos))
            {
                choosedIndex = i;
                Debug.Log(firstTouchIndex + " / " + choosedIndex);
                if(firstTouchIndex != choosedIndex)
                {
                    return;
                }
            }
        }

        if (choosedIndex == -1 || 
            (numlist != null && installControl.IsUsedSlot(numlist[choosedIndex])))
            return;

        dragImage = Instantiate(dragImagePrefab, upgradeUIs[choosedIndex].transform);
        towerImageIsDraging = true;
        dragImage.SetActive(true);
        dragImage.transform.position = touchPos;
    }

    public void OnTouchStateCheck()
    {
        var currentPhase = TouchManager.Instance.TouchPhase;
        
        if (currentPhase == InputActionPhase.Canceled)
        {
            isStartTouch = false;
            towerImageIsDraging = false;
            isNewTouch = true;

            var index = GetEndTouchOnInstallArea();
            if (index != -1 && dragImage != null && choosedIndex != -1)
            {
                installControl.IsReadyInstall = true;
                installControl.ChoosedData = choices[choosedIndex];
                installControl.IntallNewTower(index);
                gameObject.SetActive(false);
            }

            Destroy(dragImage);
            dragImage = null;

            choosedIndex = -1;
            firstTouchIndex = -1;
        }
    }

    private int GetEndTouchOnInstallArea()
    {
        var touchPos = TouchManager.Instance.TouchPos; 
        var towers = installControl.Towers;
        for (int i = 0; i < installControl.TowerCount; i++)
        {
            if(!installControl.IsUsedSlot(i) && RectTransformUtility.RectangleContainsScreenPoint(towers[i].GetComponent<RectTransform>(), touchPos))
            {
                return i;
            }
        }
        return -1;
    }

    private int[] GetRandomBuffSlot(int count) //Seperate Pick Random Slots Logic Method
    {
        int towerCount = installControl.TowerCount;
        if (towerCount <= 1 || count <= 0) return new int[0];

        count = Mathf.Max(1, count);

        List<int> candidates = new List<int>();
        int leftMax = towerCount / 2;
        int rightMax = towerCount - leftMax;

        for (int offset = -leftMax + 1; offset <= -1; offset++)
        {
            candidates.Add(offset);
        }

        for (int offset = 1; offset <= rightMax; offset++)
        {
            candidates.Add(offset);
        }

        if (candidates.Count == 0)
            return new int[0];

        count = Mathf.Clamp(count, 1, candidates.Count);

        int[] results = new int[count];

        for (int n = 0; n < count; n++)
        {
            int randIndex = UnityEngine.Random.Range(0, candidates.Count);
            results[n] = candidates[randIndex];
            candidates.RemoveAt(randIndex);
        }

        return results;
    }
    private AmplifierTowerDataSO GetRandomAmplifier()
    {
        if (allAmplifierTowers == null || allAmplifierTowers.Length == 0)
            return null;

        int idx = Random.Range(0, allAmplifierTowers.Length);
        if(isTutorial && Variables.Stage == 1)
        {
            idx = Random.Range(0, 2); // Basic Amplifier Towers Only
        }

        return allAmplifierTowers[idx];
    }

    private int GetAbilityIdForAttackTower(TowerDataSO towerData)
    {
        int abilityId = -1;

        if (towerData != null && towerData.randomAbilityGroupId > 0)
        {
            abilityId = AbilityManager.GetRandomAbilityFromGroup(
                towerData.randomAbilityGroupId,
                requiredTowerType: 0,    // 0: Attack Tower
                useWeight: true);
        }
        return abilityId;
    }

    private void SetIsTutorial(bool isTutorial)
    {
        this.isTutorial = isTutorial;
    }
}