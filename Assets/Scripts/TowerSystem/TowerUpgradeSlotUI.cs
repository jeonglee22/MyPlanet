using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TowerUpgradeSlotUI : MonoBehaviour
{
    [SerializeField] private GameObject[] upgradeUIs;
    [SerializeField] private GameObject[] outlineObjects;
    [SerializeField] private Button gameResumeButton;
    [SerializeField] private TowerInstallControl installControl;
    [SerializeField] private TowerInfoUI towerInfoUI;
    [SerializeField] private GameObject dragImagePrefab;
    [SerializeField] private TextMeshProUGUI towerInstallText;

    [Header("Tutorial")]
    [SerializeField] private AmplifierTowerDataSO damageMatrixCoreSO;
    [SerializeField] private AmplifierTowerDataSO proejctileCoreSO;
    [SerializeField] private TowerDataSO tutorialPistolTower;

    [Header("Tower Install UI Cards Prefab")]
    [SerializeField] private GameObject newAttackTowerCardPrefab;
    [SerializeField] private GameObject newAmplifierTowerCardPrefab;
    [SerializeField] private GameObject upgradeTowerCardPrefab;
    [SerializeField] private GameObject goldCardPrefab;

    private bool tutorialPistolInstalled = false;
    private bool tutorialAmp1Installed = false;
    private bool tutorialAmp2Installed = false;

    private bool towerImageIsDraging = false;
    private bool isNewTouch;
    private bool isStartTouch = false;
    private Vector2 initTouchPos;

    [SerializeField] private AmplifierTowerDataSO[] allAmplifierTowers;
    private TowerInstallChoice[] choices; //Tuple(Ability,Index) -> Struct(Add Tower Type)

    private const int UpgradeAbilityKey = 0;
    private const int MaxReinforceLevelInternal = 3;

    //test
    private Color towerColor;
    private List<int> numlist;
    // [SerializeField] private TextMeshProUGUI[] uiTexts;
    private int[] abilities;
    public Color choosedColor { get; private set; }

    private bool isNotUpgradeOpen = false;
    public bool IsNotUpgradeOpen
    {
        get { return isNotUpgradeOpen; }
        set { isNotUpgradeOpen = value; }
    }
    private bool isOpenDeploy = false;

    private GameObject dragImage = null;
    public GameObject DragImage => dragImage;
    private int choosedIndex = -1;
    private int firstTouchIndex = -1;
    private bool isFirstInstall = true;
    public bool IsFirstInstall => isFirstInstall;
    public bool IsQuasarItemUsed { get; set; }

    [SerializeField] private Button[] refreshButtons;

    //debug
    [SerializeField] private bool debugForceAmplifier = false;
    [SerializeField] private int debugAmplifierIndex = 0; 

    private bool isTutorial = false;

    //Upgrade System -------------------------
    private struct TowerOptionKey
    {
        public TowerInstallType InstallType;
        public int towerKey;
        public int abilityId;
    }
    private static bool hasLastChosenOption = false;
    private static TowerOptionKey lastChosenOption;
    private TowerOptionKey[] initialOptionKeys;
    private HashSet<TowerDataSO> usedAttackTowerTypesThisRoll
        = new HashSet<TowerDataSO>();
    private HashSet<AmplifierTowerDataSO> usedAmplifierTowerTypesThisRoll
    = new HashSet<AmplifierTowerDataSO>();
    private List<bool> usedRefreshButton;
    private const int MaxReinforceLevel = 4;

    [SerializeField] private PlanetTowerUI planetTowerUI;

    private bool hasInitializedForStage1 = false;
    //----------------------------------------

    private void Awake()
    {
        if (Variables.Stage == 1)
        {
            ExclusiveAbilityRunTracker.Clear();
            hasLastChosenOption = false;
            lastChosenOption = default;
            tutorialPistolInstalled = false;
            tutorialAmp1Installed = false;
            tutorialAmp2Installed = false;
        }
    }

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
            installControl.MaxTowerCount = 3;
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
            isOpenDeploy = true;

            if(IsQuasarItemUsed)
            {
                IsQuasarItemUsed = false;
                planetTowerUI.SetTopBannerText(GameStrings.QuasarItemUsed);
                planetTowerUI.IsTowerSetting = false;
                planetTowerUI.IsQuasarItemUsed = true;
                SetActiveRefreshButtons(false);
                return;
            }
            
            planetTowerUI.SetTopBannerText(GameStrings.TowerSetting);
            planetTowerUI.IsTowerSetting = true;
            planetTowerUI.IsQuasarItemUsed = false;
            SetActiveRefreshButtons(false);
            return;
        }

        // gameResumeButton.interactable = false;
        planetTowerUI.SetTopBannerText(GameStrings.TowerUpgrade);
        planetTowerUI.IsQuasarItemUsed = false;
        planetTowerUI.IsTowerSetting = false;

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

        if (Variables.Stage == 1 && !hasInitializedForStage1)
        {
            hasLastChosenOption = false;
            lastChosenOption = default;
            tutorialPistolInstalled = false;
            tutorialAmp1Installed = false;
            tutorialAmp2Installed = false;

            hasInitializedForStage1 = true;
        }

        SettingUpgradeCards();

        if (usedRefreshButton != null)
            usedRefreshButton.Clear();
        
        usedRefreshButton = new List<bool>();
        foreach (var btn in refreshButtons)
        {
            usedRefreshButton.Add(false);
        }
    }

    private void SetTowerInstallText()
    {
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

/*        if (Variables.Stage == 1)
        {
            hasLastChosenOption = false;
            lastChosenOption = default;
            tutorialPistolInstalled = false;
            tutorialAmp1Installed = false;
            tutorialAmp2Installed = false;
        }*/

        if(isTutorial && Variables.Stage == 1)
        {
            TutorialManager.Instance.ShowTutorialStep(1);
        }
        if(isTutorial && Variables.Stage == 2)
        {
            TutorialManager.Instance.ShowTutorialStep(6);
        }

        gameResumeButton.interactable = true;

        if(isOpenDeploy)
        {
            isOpenDeploy = false;
            SoundManager.Instance.PlayDeployClose();
            return;
        }
    }

    private void Update()
    {
        bool hasInput = Input.touchCount > 0 || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame);
        if (planetTowerUI.IsBackBtnClicked) return;
        if (towerInfoUI != null && towerInfoUI.gameObject.activeSelf) return;
        if (UIBlockPanelControl.IsBlockedPanel) return;
        if (planetTowerUI.ISConfirmPanelActive) return;

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

        usedAttackTowerTypesThisRoll.Clear();
        usedAmplifierTowerTypesThisRoll.Clear();
        initialOptionKeys = new TowerOptionKey[upgradeUIs.Length];

        int totalTowerCount = GetTotalTowerCount();
        bool forcedNewAttackMade = false;

        if (Variables.Stage == 1)
        {
            SettingStage1Cards(totalTowerCount);
            return;
        }

        List<int> emptySlots = new List<int>();
        List<int> upgradeSlots = new List<int>();

        if (ShouldShowGoldCard())
        {
            for (int cardIndex = 0; cardIndex < upgradeUIs.Length - 1; cardIndex++)
            {
                numlist.Add(-1);
                SetUpNewInstallCard(cardIndex, -1, isInitial: true);
            }

            numlist.Add(-1);

            abilities[upgradeUIs.Length - 1] = -1;
            choices[upgradeUIs.Length - 1] = new TowerInstallChoice();
            choices[upgradeUIs.Length - 1].InstallType = TowerInstallType.Attack;
            choices[upgradeUIs.Length - 1].AttackTowerData = null;
            choices[upgradeUIs.Length - 1].AmplifierTowerData = null;
            choices[upgradeUIs.Length - 1].BuffSlotIndex = null;
            choices[upgradeUIs.Length - 1].RandomAbilitySlotIndex = null;
            choices[upgradeUIs.Length - 1].ability = -1;

            DeleteAlreadyInstalledCard(upgradeUIs.Length - 1);

            if (goldCardPrefab != null && upgradeUIs[upgradeUIs.Length - 1] != null)
            {
                var parent = upgradeUIs[upgradeUIs.Length - 1].transform;
                for (int i = parent.childCount - 1; i >= 0; i--)
                {
                    var child = parent.GetChild(i);
                    Destroy(child.gameObject);
                }
                var goldCard = Instantiate(goldCardPrefab, parent);
                BindGoldCardClick(goldCard);
            }
            return;
        }

        if (totalTowerCount == 0)
        {
            emptySlots.Clear();
            for (int i = 0; i < installControl.TowerCount; i++)
            {
                if (!installControl.IsUsedSlot(i) &&
                    installControl.CurrentTowerCount < installControl.MaxTowerCount)
                {
                    emptySlots.Add(i);
                }
            }

            usedAttackTowerTypesThisRoll.Clear();
            usedAmplifierTowerTypesThisRoll.Clear();
            initialOptionKeys = new TowerOptionKey[upgradeUIs.Length];

            for (int cardIndex = 0; cardIndex < upgradeUIs.Length; cardIndex++)
            {
                if (emptySlots.Count == 0)
                {
                    numlist.Add(-1);
                    continue;
                }

                int slotIdx = UnityEngine.Random.Range(0, emptySlots.Count);
                int slotNumber = emptySlots[slotIdx];
                emptySlots.RemoveAt(slotIdx);

                numlist.Add(slotNumber);
                SetUpNewAttackCard(cardIndex, slotNumber, isInitial: true);
            }
            return;
        }

        usedAttackTowerTypesThisRoll.Clear();
        initialOptionKeys = new TowerOptionKey[upgradeUIs.Length];

        emptySlots.Clear();
        upgradeSlots.Clear();

        for (int i = 0; i < installControl.TowerCount; i++)
        {
            bool used = installControl.IsUsedSlot(i);

            if (!used)
            {
                if (installControl.CurrentTowerCount < installControl.MaxTowerCount)
                    emptySlots.Add(i);
            }
            else
            {
                int reinforceLevel = installControl.GetSlotReinforceLevel(i);
                if (!installControl.IsSlotMaxLevel(i) && reinforceLevel < MaxReinforceLevel)
                    upgradeSlots.Add(i);
            }
        }

        float newProb, upgradeProb;
        GetNewUpgradeProbabilities(out newProb, out upgradeProb);

        bool allSlotsUsed =
            (emptySlots.Count == 0 &&
             installControl.CurrentTowerCount >= installControl.MaxTowerCount);

        bool globalHasAmpCandidate = HasAnyAmplifierCandidateForCard();
        bool mustGuaranteeAmplifierThisRoll = allSlotsUsed && globalHasAmpCandidate;
        bool amplifierCardAlreadyMade = false;

        for (int cardIndex = 0; cardIndex < upgradeUIs.Length; cardIndex++)
        {
            bool hasAttackCandidateForNew = HasAnyNewAttackTowerCandidate();
            bool hasAmplifierCandidateForNew = HasAnyAmplifierCandidateForCard();
            bool canNew = (hasAttackCandidateForNew || hasAmplifierCandidateForNew);
            bool canUpgrade = upgradeSlots.Count > 0;

            if (!canNew && !canUpgrade)
            {
                int slotNum = -1;

                if (emptySlots.Count > 0)
                {
                    int slotIdx = Random.Range(0, emptySlots.Count);
                    slotNum = emptySlots[slotIdx];
                    emptySlots.RemoveAt(slotIdx);
                }
                numlist.Add(slotNum);
                SetUpNewInstallCard(cardIndex, slotNum, isInitial: true);
                continue;
            }

            bool chooseNew;
            if (!canNew && canUpgrade)
            {
                chooseNew = false;
            }
            else if (canNew && !canUpgrade)
            {
                chooseNew = true;
            }
            else
            {
                float r = UnityEngine.Random.Range(0f, 1f);
                chooseNew = (r < newProb);
            }

            if (mustGuaranteeAmplifierThisRoll &&
                !amplifierCardAlreadyMade &&
                cardIndex == upgradeUIs.Length - 1 &&
                hasAmplifierCandidateForNew)
            {
                chooseNew = true;
            }

            bool canInstallNew = installControl.CurrentTowerCount < installControl.MaxTowerCount;
            int remainingNewAttackCount = GetRemainingUninstalledAttackTowerTypeCount();

            if (chooseNew)
            {
                int slotNumber = -1;

                if (emptySlots.Count > 0)
                {
                    int slotIdx = UnityEngine.Random.Range(0, emptySlots.Count);
                    slotNumber = emptySlots[slotIdx];
                    emptySlots.RemoveAt(slotIdx);
                }

                numlist.Add(slotNumber);

                if (canInstallNew && remainingNewAttackCount > 0 && !forcedNewAttackMade)
                {
                    SetUpNewAttackCard(cardIndex, slotNumber, isInitial: true);

                    bool madeAttackNew =
                        choices[cardIndex].InstallType == TowerInstallType.Attack &&
                        choices[cardIndex].AttackTowerData != null &&
                        choices[cardIndex].ability != UpgradeAbilityKey;

                    if (madeAttackNew)
                    {
                        forcedNewAttackMade = true;
                        continue;
                    }
                    SetUpNewInstallCard(cardIndex, slotNumber, isInitial: true);
                }
                else
                {
                    if (mustGuaranteeAmplifierThisRoll &&
                        !amplifierCardAlreadyMade &&
                        hasAmplifierCandidateForNew)
                    {
                        SetUpNewAmplifierCard(cardIndex, slotNumber, isInitial: true);
                        amplifierCardAlreadyMade = true;
                    }
                    else
                    {
                        SetUpNewInstallCard(cardIndex, slotNumber, isInitial: true);

                        if (!amplifierCardAlreadyMade &&
                            choices[cardIndex].InstallType == TowerInstallType.Amplifier)
                        {
                            amplifierCardAlreadyMade = true;
                        }
                    }
                }
            }
            else
            {
                if (upgradeSlots.Count == 0)
                {
                    int slotNum = -1;

                    if (emptySlots.Count > 0)
                    {
                        int slotIdx = Random.Range(0, emptySlots.Count);
                        slotNum = emptySlots[slotIdx];
                        emptySlots.RemoveAt(slotIdx);
                    }

                    numlist.Add(slotNum);
                    SetUpNewInstallCard(cardIndex, slotNum, isInitial: true);

                    continue;
                }

                int pickedIndexInList = -1;
                TowerDataSO pickedData = null;

                int tryCountMax = upgradeSlots.Count * 2;
                for (int t = 0; t < tryCountMax; t++)
                {
                    int slotNumber = (int)PickUpgradeSlotByWeight(upgradeSlots);
                    var data = installControl.GetTowerData(slotNumber);

                    if (data == null || !usedAttackTowerTypesThisRoll.Contains(data))
                    {
                        pickedIndexInList = slotNumber;
                        pickedData = data;
                        break;
                    }
                }

                if (pickedIndexInList == -1)
                {
                    pickedIndexInList = upgradeSlots[UnityEngine.Random.Range(0, upgradeSlots.Count)];
                }

                upgradeSlots.Remove(pickedIndexInList);

                numlist.Add(pickedIndexInList);
                SetUpgradeCardForUsedSlot(cardIndex, pickedIndexInList, isInitial: true);

                var pickedTowerData = installControl.GetTowerData(pickedIndexInList);
                if (pickedTowerData != null)
                {
                    usedAttackTowerTypesThisRoll.Add(pickedTowerData);
                }
            }
        }
    }

    private void SettingStage1Cards(int totalTowerCount)
    {
        List<int> emptySlots = new List<int>();
        List<int> upgradeSlots = new List<int>();

        for (int i = 0; i < installControl.TowerCount; i++)
        {
            bool used = installControl.IsUsedSlot(i);

            if (!used)
            {
                if (installControl.CurrentTowerCount < installControl.MaxTowerCount)
                    emptySlots.Add(i);
            }
            else
            {
                int reinforceLevel = installControl.GetSlotReinforceLevel(i);
                if (!installControl.IsSlotMaxLevel(i) && reinforceLevel < MaxReinforceLevel)
                    upgradeSlots.Add(i);
            }
        }

        if (ShouldShowGoldCard())
        {
            SetupStage1AllGoldCards();
            return;
        }

        bool hasPistol = HasTowerTypeInstalled(tutorialPistolTower);
        bool hasAmp1 = HasAmplifierInstalled(damageMatrixCoreSO);
        bool hasAmp2 = HasAmplifierInstalled(proejctileCoreSO);

        List<AmplifierTowerDataSO> remainingAmps = new List<AmplifierTowerDataSO>();
        if (!hasAmp1 && damageMatrixCoreSO != null)
            remainingAmps.Add(damageMatrixCoreSO);
        if (!hasAmp2 && proejctileCoreSO != null)
            remainingAmps.Add(proejctileCoreSO);

        for (int cardIndex = 0; cardIndex < upgradeUIs.Length; cardIndex++)
        {
            if (!hasPistol && totalTowerCount == 0)
            {
                int slotNumber = -1;
                if (emptySlots.Count > 0)
                {
                    int slotIdx = UnityEngine.Random.Range(0, emptySlots.Count);
                    slotNumber = emptySlots[slotIdx];
                    emptySlots.RemoveAt(slotIdx);
                }

                numlist.Add(slotNumber);
                SetUpTutorialAttackCard(cardIndex, slotNumber, isInitial: true);
                continue;
            }

            if (remainingAmps.Count > 0 && emptySlots.Count > 0)
            {
                int slotIdx = UnityEngine.Random.Range(0, emptySlots.Count);
                int slotNumber = emptySlots[slotIdx];
                emptySlots.RemoveAt(slotIdx);

                var ampData = remainingAmps[0];
                remainingAmps.RemoveAt(0);

                numlist.Add(slotNumber);
                SetUpTutorialAmplifierCard(cardIndex, slotNumber, ampData, isInitial: true);
                continue;
            }

            if (upgradeSlots.Count > 0)
            {
                int safe = 0;
                int pickedIdx = -1;

                while (safe++ < 50 && upgradeSlots.Count > 0)
                {
                    int listIdx = UnityEngine.Random.Range(0, upgradeSlots.Count);
                    int candidate = upgradeSlots[listIdx];

                    if (numlist != null && numlist.Contains(candidate))
                    {
                        upgradeSlots.RemoveAt(listIdx);
                        continue;
                    }
                    var candidateData = installControl.GetTowerData(candidate);
                    var candidateAmp = installControl.GetAmplifierTower(candidate);

                    bool isDuplicate = false;
                    if (candidateData != null && usedAttackTowerTypesThisRoll.Contains(candidateData))
                    {
                        isDuplicate = true;
                    }
                    else if (candidateAmp != null && candidateAmp.AmplifierTowerData != null &&
                             usedAmplifierTowerTypesThisRoll.Contains(candidateAmp.AmplifierTowerData))
                    {
                        isDuplicate = true;
                    }

                    if (isDuplicate)
                    {
                        upgradeSlots.RemoveAt(listIdx);
                        continue;
                    }

                    pickedIdx = listIdx;
                    break;
                }

                if (pickedIdx == -1)
                {
                }
                else
                {
                    int slotNumber = upgradeSlots[pickedIdx];
                    upgradeSlots.RemoveAt(pickedIdx);

                    numlist.Add(slotNumber);
                    SetUpgradeCardForUsedSlot(cardIndex, slotNumber, isInitial: true);
                    continue;
                }
            }

            int slotNum = -1;
            for (int i = 0; i < installControl.TowerCount; i++)
            {
                if (!installControl.IsUsedSlot(i) && installControl.CurrentTowerCount < installControl.MaxTowerCount)
                {
                    slotNum = i;
                    break;
                }
            }

            numlist.Add(slotNum);
            SetUpNewInstallCard(cardIndex, slotNum, isInitial: true);
        }
    }

    private bool ShouldShowGoldCard()
    {
        bool isFieldFull = (installControl.CurrentTowerCount >= installControl.MaxTowerCount);
        if (!isFieldFull) return false;
        bool hasAnyTower = false;

        for (int i = 0; i < installControl.TowerCount; i++)
        {
            if (installControl.IsUsedSlot(i))
            {
                hasAnyTower = true;

                int level = installControl.GetSlotReinforceLevel(i);
                bool isMax = installControl.IsSlotMaxLevel(i);

                if (!isMax) return false;
            }
        }
        bool result = hasAnyTower;
        return result;
    }



    private bool HasTowerTypeInstalled(TowerDataSO towerData)
    {
        if (Variables.Stage == 1 && towerData == tutorialPistolTower)
            return tutorialPistolInstalled;

        if (towerData == null) return false;

        for (int i = 0; i < installControl.TowerCount; i++)
        {
            if (!installControl.IsUsedSlot(i)) continue;

            var data = installControl.GetTowerData(i);
            if (data != null && data.towerIdInt == towerData.towerIdInt)
                return true;
        }
        return false;
    }


    private bool HasAmplifierInstalled(AmplifierTowerDataSO target)
    {
        if (target == null) return false;
        if (Variables.Stage == 1)
        {
            if (target == damageMatrixCoreSO && tutorialAmp1Installed) return true;
            if (target == proejctileCoreSO && tutorialAmp2Installed) return true;
        }

        int targetId = target.BuffTowerId;
        for (int i = 0; i < installControl.TowerCount; i++)
        {
            var amp = installControl.GetAmplifierTower(i);
            if (amp == null || amp.AmplifierTowerData == null) continue;
            if (amp.AmplifierTowerData.BuffTowerId == targetId) return true;
        }
        return false;
    }


    private void SetUpTutorialAmplifierCard(int i, int slotNumber, AmplifierTowerDataSO ampData, bool isInitial)
    {
        if (ampData == null)
        {
            SetUpNewAmplifierCard(i, slotNumber, isInitial);
            return;
        }

        if (isInitial)
            usedAmplifierTowerTypesThisRoll.Add(ampData);

        int ampAbilityId = GetRandomAbilityForAmplifier(ampData);
        FillAmplifierCardCommon(i, ampData, ampAbilityId, isInitial);
    }

    private void FillAmplifierCardCommon(int i, AmplifierTowerDataSO ampData, int ampAbilityId, bool isInitial)
    {
        if (ampData == null) return;

        choices[i].InstallType = TowerInstallType.Amplifier;
        choices[i].AmplifierTowerData = ampData;
        choices[i].AttackTowerData = null;
        choices[i].ability = ampAbilityId;
        abilities[i] = ampAbilityId;

        // BuffSlotIndex와 RandomAbilitySlotIndex 새로 생성
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
                effectiveBuffCount = baseBuffCount;
                buffOffsets = GetRandomBuffSlot(effectiveBuffCount);
                if (randomSlotNum > 0)
                    randomOffsets = GetRandomBuffSlot(randomSlotNum);
                break;

            case 1:
                effectiveBuffCount = baseBuffCount;
                buffOffsets = GetRandomBuffSlot(effectiveBuffCount);
                if (buffOffsets != null && buffOffsets.Length > 0)
                {
                    int idx = Random.Range(0, buffOffsets.Length);
                    randomOffsets = new int[] { buffOffsets[idx] };
                }
                break;

            case 2:
                effectiveBuffCount = baseBuffCount + addSlotNum;
                buffOffsets = GetRandomBuffSlot(effectiveBuffCount);
                if (randomSlotNum > 0)
                    randomOffsets = GetRandomBuffSlot(randomSlotNum);
                break;
        }

        choices[i].BuffSlotIndex = buffOffsets;
        choices[i].RandomAbilitySlotIndex = randomOffsets;

        if (isInitial && initialOptionKeys != null && i < initialOptionKeys.Length)
        {
            initialOptionKeys[i] = MakeKey(ampData, ampAbilityId);
        }

        string ampName = string.IsNullOrEmpty(ampData.BuffTowerName)
            ? ampData.AmplifierType.ToString()
            : ampData.BuffTowerName;

        string buffBlock = FormatOffsetArray(buffOffsets);
        string randomBlock = FormatOffsetArray(randomOffsets);
        string ampAbilityName = GetAbilityName(ampAbilityId);

        List<int> leftSlotIndexes = new List<int>();
        List<int> rightSlotIndexes = new List<int>();

        foreach (int offset in buffOffsets)
        {
            if (offset < 0)
                leftSlotIndexes.Add(System.Math.Abs(offset));
            else
                rightSlotIndexes.Add(offset);
        }

        InstallNewAmplifierTower(i, ampData, ampAbilityId, leftSlotIndexes, rightSlotIndexes);
    }



    private void SetUpTutorialAttackCard(int i, int slotNumber, bool isInitial, TowerOptionKey? previousKey = null)
    {
        TowerDataSO towerData = tutorialPistolTower;
        if (towerData == null)
        {
            SetUpNewAttackCard(i, slotNumber, isInitial, previousKey);
            return;
        }

        usedAttackTowerTypesThisRoll.Add(towerData);

        int abilityId = -1;
        int safe = 0;
        do
        {
            abilityId = GetAbilityIdForAttackTower(towerData);
            safe++;
            if (safe > 20) break;
        }
        while (abilityId > 0 && IsForbiddenAttackCombo(towerData, abilityId, isInitial, previousKey));

        abilities[i] = abilityId;

        choices[i].InstallType = TowerInstallType.Attack;
        choices[i].AttackTowerData = towerData;
        choices[i].AmplifierTowerData = null;
        choices[i].BuffSlotIndex = null;
        choices[i].RandomAbilitySlotIndex = null;
        choices[i].ability = abilityId;

        if (isInitial && initialOptionKeys != null && i < initialOptionKeys.Length)
        {
            initialOptionKeys[i] = MakeKey(towerData, abilityId);
        }

        string towerName = towerData.towerId;
        string abilityName = GetAbilityName(abilityId);

        InstallNewAttackTower(i, towerData, abilityId);
    }

    private void SetUpTutorialAmplifierCard(int i, int slotNumber, AmplifierTowerDataSO ampData, bool isInitial, TowerOptionKey? previousKey = null)
    {
        if (ampData == null)
        {
            SetUpNewAmplifierCard(i, slotNumber, isInitial, previousKey);
            return;
        }

        usedAmplifierTowerTypesThisRoll.Add(ampData);

        int ampAbilityId = -1;
        int safe = 0;
        do
        {
            ampAbilityId = GetRandomAbilityForAmplifier(ampData);
            safe++;
            if (safe > 20) break;
        }
        while (ampAbilityId > 0 && IsForbiddenAmplifierCombo(ampData, ampAbilityId, isInitial, previousKey));
        FillAmplifierCardCommon(i, ampData, ampAbilityId, isInitial);
    }

    private void SetUpNewAttackCard(int i, int slotNumber, bool isInitial, TowerOptionKey? previousKey = null)
    {
        TowerDataSO towerData = null;

        int remainingNewCount = GetRemainingUninstalledAttackTowerTypeCount();
        if (remainingNewCount == 1)
        {
            towerData = GetOnlyRemainingUninstalledAttackTowerType();
            if (towerData != null)
                usedAttackTowerTypesThisRoll.Add(towerData);
        }
        else
        {
            towerData = GetRandomNewAttackTowerCandidate(usedAttackTowerTypesThisRoll);
            if (towerData != null)
            {
                usedAttackTowerTypesThisRoll.Add(towerData);
            }
        }

        if (towerData == null)
        {
            List<int> upgradableSlots = new List<int>();

            for (int s = 0; s < installControl.TowerCount; s++)
            {
                if (installControl.IsUsedSlot(s) && installControl.GetTowerData(s) != null)
                {
                    if (IsAttackSlotUpgradable(s))
                    {
                        upgradableSlots.Add(s);
                    }
                }
            }

            if (upgradableSlots.Count > 0)
            {
                int pickedSlot = -1;
                int safe = 0;
                while (safe++ < 50 && upgradableSlots.Count > 0)
                {
                    int idx = UnityEngine.Random.Range(0, upgradableSlots.Count);
                    int candidateSlot = upgradableSlots[idx];

                    var candidateData = installControl.GetTowerData(candidateSlot);
                    if (candidateData != null && usedAttackTowerTypesThisRoll.Contains(candidateData))
                    {
                        upgradableSlots.RemoveAt(idx);
                        continue;
                    }

                    pickedSlot = candidateSlot;
                    break;
                }
                if (pickedSlot == -1)
                {
                    pickedSlot = upgradableSlots[UnityEngine.Random.Range(0, upgradableSlots.Count)];
                }

                if (numlist != null && i >= 0 && i < numlist.Count)
                    numlist[i] = pickedSlot;

                SetUpgradeCardForUsedSlot(i, pickedSlot, isInitial);
            }
            else
            {
                if (ShouldShowGoldCard() && i == upgradeUIs.Length - 1)
                {
                    abilities[i] = -1;

                    choices[i].InstallType = TowerInstallType.Attack;
                    choices[i].AttackTowerData = null;
                    choices[i].AmplifierTowerData = null;
                    choices[i].BuffSlotIndex = null;
                    choices[i].RandomAbilitySlotIndex = null;
                    choices[i].ability = -1;

                    if (numlist != null && i >= 0 && i < numlist.Count)
                        numlist[i] = -1;

                    if (i == upgradeUIs.Length - 1)
                        Instantiate(goldCardPrefab, upgradeUIs[i].transform);
                    else
                        SetUpNewAmplifierCard(i, slotNumber, isInitial, previousKey);
                }
            }
            return;
        }

        int abilityId = -1;
        int safe2 = 0;

        do
        {
            abilityId = GetAbilityIdForAttackTower(towerData);
            safe2++;
            if (safe2 > 20) break;
        }
        while (abilityId > 0 && IsForbiddenAttackCombo(towerData, abilityId, isInitial, previousKey));

        abilities[i] = abilityId;

        choices[i].InstallType = TowerInstallType.Attack;
        choices[i].AttackTowerData = towerData;
        choices[i].AmplifierTowerData = null;
        choices[i].BuffSlotIndex = null;
        choices[i].RandomAbilitySlotIndex = null;
        choices[i].ability = abilityId;

        if (isInitial && initialOptionKeys != null && i < initialOptionKeys.Length)
        {
            initialOptionKeys[i] = MakeKey(towerData, abilityId);
        }
        InstallNewAttackTower(i, towerData, abilityId);
    }



    private void SetUpNewAmplifierCard(int i, int slotNumber, bool isInitial, TowerOptionKey? previousKey = null)
    {
        var ampData = GetRandomAmplifierForCard(usedAmplifierTowerTypesThisRoll);
        if (ampData == null)
        {
            SetUpNewAttackCard(i, slotNumber, isInitial, previousKey);
            return;
        }

        usedAmplifierTowerTypesThisRoll.Add(ampData);

        int ampAbilityId = -1;
        int safe = 0;

        do
        {
            ampAbilityId = GetRandomAbilityForAmplifier(ampData);
            safe++;
            if (safe > 20) break;
        }
        while (ampAbilityId > 0 && IsForbiddenAmplifierCombo(ampData, ampAbilityId, isInitial, previousKey));

        FillAmplifierCardCommon(i, ampData, ampAbilityId, isInitial);
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

        rightList.Sort();
        leftList.Sort();

        sb.AppendLine("증폭타워 기준");

        if (rightList.Count > 0)
        {
            var rightPos = new List<string>();
            foreach (int v in rightList)
                rightPos.Add($"{v}번째");

            sb.AppendLine($"왼쪽 {string.Join(", ", rightPos)}");
        }

        if (leftList.Count > 0)
        {
            var leftPos = new List<string>();
            foreach (int v in leftList)
                leftPos.Add($"{v}번째");

            sb.AppendLine($"오른쪽 {string.Join(", ", leftPos)}");
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
        if (choices == null || index < 0 || index >= choices.Length)
            return;

        if (upgradeUIs == null || index >= upgradeUIs.Length || upgradeUIs[index] == null)
            return;

        TowerOptionKey? previousKey = null;
        var prevChoice = choices[index];

        if (prevChoice != null)
        {
            if (prevChoice.InstallType == TowerInstallType.Attack && prevChoice.AttackTowerData != null)
            {
                previousKey = MakeKey(prevChoice.AttackTowerData, prevChoice.ability);

                if (usedAttackTowerTypesThisRoll != null && usedAttackTowerTypesThisRoll.Contains(prevChoice.AttackTowerData))
                {
                    usedAttackTowerTypesThisRoll.Remove(prevChoice.AttackTowerData);
                }
            }
            else if (prevChoice.InstallType == TowerInstallType.Amplifier && prevChoice.AmplifierTowerData != null)
            {
                previousKey = MakeKey(prevChoice.AmplifierTowerData, prevChoice.ability);

                if (usedAmplifierTowerTypesThisRoll != null && usedAmplifierTowerTypesThisRoll.Contains(prevChoice.AmplifierTowerData))
                {
                    usedAmplifierTowerTypesThisRoll.Remove(prevChoice.AmplifierTowerData);
                }
            }
        }

        choices[index].InstallType = TowerInstallType.Attack;
        choices[index].AttackTowerData = null;
        choices[index].AmplifierTowerData = null;
        choices[index].BuffSlotIndex = null;
        choices[index].RandomAbilitySlotIndex = null;
        choices[index].ability = -1;

        abilities[index] = -1;
        installControl.IsReadyInstall = false;

        if (outlineObjects != null && index < outlineObjects.Length && outlineObjects[index] != null)
        {
            outlineObjects[index].SetActive(false);
        }

        if (ShouldShowGoldCard() && index == upgradeUIs.Length - 1)
        {
            numlist[index] = -1;

            abilities[index] = -1;
            choices[index].InstallType = TowerInstallType.Attack;
            choices[index].AttackTowerData = null;
            choices[index].AmplifierTowerData = null;
            choices[index].BuffSlotIndex = null;
            choices[index].RandomAbilitySlotIndex = null;
            choices[index].ability = -1;

            DeleteAlreadyInstalledCard(index);
            if (goldCardPrefab != null && upgradeUIs[index] != null)
            {
                var goldCard = Instantiate(goldCardPrefab, upgradeUIs[index].transform);
                BindGoldCardClick(goldCard);
            }
            return;
        }

        List<int> emptySlotsList = new List<int>();
        List<int> upgradeSlotsList = new List<int>();

        int currentSlot = (numlist != null && index < numlist.Count) ? numlist[index] : -9999;

        for (int i = 0; i < installControl.TowerCount; i++)
        {
            if (numlist != null && numlist.Contains(i) && i != currentSlot)
                continue;

            bool used = installControl.IsUsedSlot(i);

            if (!used)
            {
                if (installControl.CurrentTowerCount < installControl.MaxTowerCount)
                    emptySlotsList.Add(i);
            }
            else
            {
                if (!installControl.IsSlotMaxLevel(i))
                    upgradeSlotsList.Add(i);
            }
        }


        bool canNew = (emptySlotsList.Count > 0) || HasAnyNewAttackTowerCandidate() || HasAnyAmplifierCandidateForCard();
        bool canUpgrade = upgradeSlotsList.Count > 0;

        if (!canNew && !canUpgrade)
        {
            int slotNum = -1;
            if (emptySlotsList.Count > 0)
            {
                int slotIdx = Random.Range(0, emptySlotsList.Count);
                slotNum = emptySlotsList[slotIdx];
            }

            numlist[index] = slotNum;
            SetUpNewInstallCard(index, slotNum, isInitial: false, previousKey: previousKey);
            UpdateInitialOptionKey(index);
            return;
        }

        float newProb, upgradeProb;
        GetNewUpgradeProbabilities(out newProb, out upgradeProb);

        bool chooseNew;
        if (!canNew && canUpgrade)
        {
            chooseNew = false;
        }
        else if (canNew && !canUpgrade)
        {
            chooseNew = true;
        }
        else
        {
            float r = Random.Range(0f, 1f);
            chooseNew = (r < newProb);
        }

        if (chooseNew)
        {
            int slotNumber = -1;
            if (emptySlotsList.Count > 0)
            {
                int slotIdx = Random.Range(0, emptySlotsList.Count);
                slotNumber = emptySlotsList[slotIdx];
            }

            numlist[index] = slotNumber;
            SetUpNewInstallCard(index, slotNumber, isInitial: false, previousKey: previousKey);
        }
        else
        {
            if (upgradeSlotsList.Count == 0)
            {
                int slotNum = -1;
                if (emptySlotsList.Count > 0)
                {
                    int slotIdx = Random.Range(0, emptySlotsList.Count);
                    slotNum = emptySlotsList[slotIdx];
                }

                numlist[index] = slotNum;
                SetUpNewInstallCard(index, slotNum, isInitial: false, previousKey: previousKey);
                UpdateInitialOptionKey(index);
                return;
            }

            int pickedSlot = (int)PickUpgradeSlotByWeight(upgradeSlotsList);
            numlist[index] = pickedSlot;
            SetUpgradeCardForUsedSlot(index, pickedSlot, isInitial: false);
        }
        UpdateInitialOptionKey(index);
    }

    private void SetUpgradeCardForUsedSlot(int index, int number, bool isInitial)
    {
        if (installControl.IsSlotMaxLevel(number))
        {
            SetUpNewInstallCard(index, -1, isInitial);
            return;
        }

        var towerData = installControl.GetTowerData(number);
        var ampTower = installControl.GetAmplifierTower(number);

        if (towerData != null)
        {
            choices[index].InstallType = TowerInstallType.Attack;
            choices[index].AttackTowerData = towerData;
            choices[index].AmplifierTowerData = null;
            choices[index].BuffSlotIndex = null;
            choices[index].RandomAbilitySlotIndex = null;

            if (!usedAttackTowerTypesThisRoll.Contains(towerData))
                usedAttackTowerTypesThisRoll.Add(towerData);

            abilities[index] = UpgradeAbilityKey;
            choices[index].ability = UpgradeAbilityKey;

            if (isInitial && initialOptionKeys != null && index < initialOptionKeys.Length)
                initialOptionKeys[index] = MakeKey(towerData, UpgradeAbilityKey);

            UpgradeTowerCard(index);
            return;
        }
        else if (ampTower != null && ampTower.AmplifierTowerData != null)
        {
            choices[index].InstallType = TowerInstallType.Amplifier;
            choices[index].AttackTowerData = null;
            choices[index].AmplifierTowerData = ampTower.AmplifierTowerData;
            choices[index].BuffSlotIndex = null;
            choices[index].RandomAbilitySlotIndex = null;

            abilities[index] = UpgradeAbilityKey;
            choices[index].ability = UpgradeAbilityKey;

            if (isInitial && initialOptionKeys != null && index < initialOptionKeys.Length)
                initialOptionKeys[index] = MakeKey(ampTower.AmplifierTowerData, UpgradeAbilityKey);

            UpgradeTowerCard(index);

            if (isTutorial && Variables.Stage == 1)
                TutorialManager.Instance.ShowTutorialStep(4);
            return;
        }

        choices[index].InstallType = TowerInstallType.Attack;
        choices[index].AttackTowerData = null;
        choices[index].AmplifierTowerData = null;
        choices[index].BuffSlotIndex = null;
        choices[index].RandomAbilitySlotIndex = null;
        abilities[index] = -1;
        choices[index].ability = -1;

        UpgradeTowerCard(index);
    }



    public void OnClickRefreshButton(int index)
    {
        SoundManager.Instance.PlayRefreshSound();

        if (isTutorial && Variables.Stage == 1)
        {
            RefreshStage1SingleCard(index);
        }
        else
        {
            ResetUpgradeCard(index);
        }

        if (refreshButtons == null) return;
        refreshButtons[index].interactable = false;
        usedRefreshButton[index] = true;
    }

    private void DeleteAlreadyInstalledCard(int index)
    {
        if (upgradeUIs == null || index < 0 || index >= upgradeUIs.Length)
            return;

        if (upgradeUIs[index] == null)
            return;

        Transform parent = upgradeUIs[index].transform;

        if (parent.childCount > 1)
        {
            // 역순으로 삭제 (인덱스 꼬임 방지)
            for (int c = parent.childCount - 2; c >= 0; c--)
            {
                var child = parent.GetChild(c);
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    public void OnClickUpgradeUIClicked(int index)
    {
        if (choices == null || index < 0 || index >= choices.Length)
            return;

        bool hasTowerData =
        choices[index].AttackTowerData != null ||
        choices[index].AmplifierTowerData != null;

        int targetSlot = (numlist != null && index < numlist.Count) ? numlist[index] : -1;

        if(hasTowerData && targetSlot < 0)
        {
            for(int i = 0; i < installControl.TowerCount; i++)
            {
                if(!installControl.IsUsedSlot(i) && installControl.CurrentTowerCount < installControl.MaxTowerCount)
                {
                    targetSlot = i;
                    if(numlist != null && index < numlist.Count)
                    {
                        numlist[index] = i;
                    }
                    break;
                }
            }
        }

        if (!hasTowerData || targetSlot < 0)
        {
            if (!hasTowerData)
            {
                if (towerInfoUI != null)
                    towerInfoUI.gameObject.SetActive(false);
                gameObject.SetActive(false);
            }
            
            return;
        }

        var outlineImage = outlineObjects[index];
        if (outlineImage.activeSelf == true)
        {
            outlineImage.SetActive(false);
            installControl.IsReadyInstall = false;
            return;
        }
        outlineObjects[index].SetActive(true);
        outlineObjects[(index + 1) % 3].SetActive(false);
        outlineObjects[(index + 2) % 3].SetActive(false);

        installControl.IsReadyInstall = true;
        installControl.ChoosedData = choices[index];

        if (choices[index].InstallType == TowerInstallType.Attack &&
        choices[index].AttackTowerData != null)
        {
            RegisterLastChosenOption(
                TowerInstallType.Attack,
                choices[index].AttackTowerData,
                null,
                choices[index].ability);
        }
        else if (choices[index].InstallType == TowerInstallType.Amplifier &&
                 choices[index].AmplifierTowerData != null)
        {
            RegisterLastChosenOption(
                TowerInstallType.Amplifier,
                null,
                choices[index].AmplifierTowerData,
                choices[index].ability);
        }

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
            if (upgradeUIs[i] == null) continue;

            if (outlineObjects != null && i < outlineObjects.Length && outlineObjects[i] != null)
            {
                outlineObjects[i].SetActive(false);
            }

            abilities[i] = -1;
            choices[i] = new TowerInstallChoice();
            choices[i].BuffSlotIndex = null;
            choices[i].RandomAbilitySlotIndex = null;
            choices[i].AttackTowerData = null;
            choices[i].AmplifierTowerData = null;
        }
    }

    private void InstallNewAttackTower(int index, TowerDataSO towerData, int abilityId)
    {
        DeleteAlreadyInstalledCard(index);

        var installedTower = Instantiate(newAttackTowerCardPrefab, upgradeUIs[index].transform);
        installedTower.transform.SetAsFirstSibling();
        var installedTowerButton = installedTower.GetComponentInChildren<Button>();
        installedTowerButton.onClick.AddListener(() => OnClickUpgradeUIClicked(index));
        var towerUI = installedTower.GetComponent<NewAttackTowerCardUiSetting>();
        towerUI.SettingNewTowerCard(towerData.towerIdInt, abilityId);
    }

    private void InstallNewAmplifierTower(int index, AmplifierTowerDataSO ampData, int abilityId, List<int> leftIndices, List<int> rightIndices)
    {
        DeleteAlreadyInstalledCard(index);

        var installedTower = Instantiate(newAmplifierTowerCardPrefab, upgradeUIs[index].transform);
        installedTower.transform.SetAsFirstSibling();
        var installedTowerButton = installedTower.GetComponentInChildren<Button>();
        installedTowerButton.onClick.AddListener(() => OnClickUpgradeUIClicked(index));
        var towerUI = installedTower.GetComponent<NewAmplifierTowerCardUiSetting>();
        towerUI.SettingNewTowerCard(ampData.BuffTowerId, abilityId, rightIndices, leftIndices);
    }

    private void UpgradeTowerCard(int index)
    {
        DeleteAlreadyInstalledCard(index);
        var upgradedTower = Instantiate(upgradeTowerCardPrefab, upgradeUIs[index].transform);
        upgradedTower.transform.SetAsFirstSibling();
        var installedTowerButton = upgradedTower.GetComponentInChildren<Button>();
        installedTowerButton.onClick.AddListener(() => OnClickUpgradeUIClicked(index));
        var towerUI = upgradedTower.GetComponent<UpgradeToweCardUiSetting>();

        var attackTower = installControl.GetAttackTower(numlist[index]);
        var ampTower = installControl.GetAmplifierTower(numlist[index]);

        if (attackTower != null)
        {
            towerUI.SettingAttackTowerUpgradeCard(attackTower.AttackTowerData.towerIdInt, attackTower.ReinforceLevel + 1);
        }
        else if (ampTower != null && ampTower.AmplifierTowerData != null)
        {
            towerUI.SettingAmplifierTowerUpgradeCard(ampTower.AmplifierTowerData.BuffTowerId, ampTower.ReinforceLevel + 1);
        }
    }

    public void OnTouchMakeDrageImage()
    {
        var touchPos = TouchManager.Instance.TouchPos;

        if (!TouchManager.Instance.IsTouching || towerImageIsDraging) return;
        if (!isStartTouch)
        {
            isStartTouch = true;
            initTouchPos = touchPos;

            bool isTouchOnUpgradeCard = false;
            for (int i = 0; i < upgradeUIs.Length; i++)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(upgradeUIs[i].GetComponent<RectTransform>(), initTouchPos))
                {
                    isTouchOnUpgradeCard = true;
                    firstTouchIndex = i;

                    if (IsGoldCardSlot(i)) break;
                    break;
                }
            }

            if (!isTouchOnUpgradeCard) return;
        }

        bool isCurrentGoldCard = IsGoldCardSlot(firstTouchIndex);
        if (isCurrentGoldCard) return;

        if (Vector2.Distance(initTouchPos, touchPos) < 5f || !isNewTouch) return;

        choosedIndex = -1;
        for (int i = 0; i < upgradeUIs.Length; i++)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(upgradeUIs[i].GetComponent<RectTransform>(), touchPos))
            {
                choosedIndex = i;
                if (firstTouchIndex != choosedIndex) return;
            }
        }

        if (choosedIndex == -1) return;
        if (IsGoldCardSlot(choosedIndex)) return;

        int targetSlot = (numlist != null && choosedIndex < numlist.Count) ? numlist[choosedIndex] : -1;

        if (targetSlot < 0)
        {
            bool hasTowerData = (choices != null && choosedIndex < choices.Length) &&
                (choices[choosedIndex].AttackTowerData != null || choices[choosedIndex].AmplifierTowerData != null);

            if (hasTowerData)
            {
                for (int i = 0; i < installControl.TowerCount; i++)
                {
                    if (!installControl.IsUsedSlot(i) && installControl.CurrentTowerCount < installControl.MaxTowerCount)
                    {
                        targetSlot = i;
                        if (numlist != null && choosedIndex < numlist.Count) numlist[choosedIndex] = i;
                        break;
                    }
                }
            }
        }

        if (targetSlot < 0) return;
        if (installControl.IsUsedSlot(targetSlot)) return;

        dragImage = Instantiate(dragImagePrefab, upgradeUIs[choosedIndex].transform);
        var dragImageComp = dragImage.transform.GetChild(0).gameObject.GetComponent<Image>();
        var choosedAttackTowerData = choices[choosedIndex].AttackTowerData;
        var choosedAmplifierTowerData = choices[choosedIndex].AmplifierTowerData;

        if (choosedAttackTowerData != null)
        {
            var towerData = DataTableManager.AttackTowerTable.GetById(choosedAttackTowerData.towerIdInt);
            var towerAssetName = towerData.AttackTowerAssetCut;
            dragImageComp.sprite = LoadManager.GetLoadedGameTexture(towerAssetName);
        }
        else if (choosedAmplifierTowerData != null)
        {
            var ampData = DataTableManager.BuffTowerTable.Get(choosedAmplifierTowerData.BuffTowerId);
            var ampAssetName = ampData.BuffTowerAssetCut;
            dragImageComp.sprite = LoadManager.GetLoadedGameTexture(ampAssetName);
        }

        towerImageIsDraging = true;
        dragImage.SetActive(true);
        dragImage.transform.position = touchPos;

        BlockUpgradeSlotTouch(true);

        installControl.LeftRotateRect.gameObject.SetActive(true);
        installControl.RightRotateRect.gameObject.SetActive(true);
    }

    private void BlockUpgradeSlotTouch(bool v)
    {
        for (int i = 0; i < refreshButtons.Length; i++)
        {
            if (usedRefreshButton[i])
                continue;

            refreshButtons[i].interactable = !v;
        }

        for (int i = 0; i < upgradeUIs.Length; i++)
        {
            var button = upgradeUIs[i].GetComponentInChildren<Button>();
            if (button != null)
            {
                button.interactable = !v;
            }
        }
    }

    public void OnTouchStateCheck()
    {
        var currentPhase = TouchManager.Instance.TouchPhase;

        if (currentPhase == InputActionPhase.Canceled)
        {
            if (isStartTouch && firstTouchIndex >= 0)
            {
                if (IsGoldCardSlot(firstTouchIndex))
                {
                    var touchPos = TouchManager.Instance.TouchPos;
                    float distance = Vector2.Distance(initTouchPos, touchPos);
                    if (distance < 20f)
                    {
                        OnClickGoldCard();

                        isStartTouch = false;
                        firstTouchIndex = -1;
                        return;
                    }
                }
            }

            isStartTouch = false;
            towerImageIsDraging = false;
            isNewTouch = true;

            var index = GetEndTouchOnInstallArea();
            if (index != -1 && dragImage != null && choosedIndex != -1)
            {
                var choice = choices[choosedIndex];

                if (isTutorial && Variables.Stage == 1)
                {
                    if (choice.InstallType == TowerInstallType.Attack &&
                        choice.AttackTowerData == tutorialPistolTower)
                    {
                        tutorialPistolInstalled = true;
                    }
                    else if (choice.InstallType == TowerInstallType.Amplifier &&
                             choice.AmplifierTowerData != null)
                    {
                        if (choice.AmplifierTowerData == damageMatrixCoreSO)
                            tutorialAmp1Installed = true;
                        else if (choice.AmplifierTowerData == proejctileCoreSO)
                            tutorialAmp2Installed = true;
                    }
                }

                installControl.IsReadyInstall = true;
                installControl.ChoosedData = choice;

                if (choice.InstallType == TowerInstallType.Attack &&
                    choice.AttackTowerData != null &&
                    choice.ability > 0)
                {
                    int towerId = choice.AttackTowerData.towerIdInt;
                    if (IsExclusiveUnlockAbility(towerId, choice.ability))
                    {
                        ExclusiveAbilityRunTracker.MarkTaken(choice.ability);
                    }
                }

                installControl.IntallNewTower(index);
                gameObject.SetActive(false);
            }

            if (dragImage != null)
                BlockUpgradeSlotTouch(false);

            Destroy(dragImage);
            dragImage = null;

            installControl.LeftRotateRect.gameObject.SetActive(false);
            installControl.RightRotateRect.gameObject.SetActive(false);

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

    private int[] GetRandomBuffSlot(int count) 
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
  
    private int GetAbilityIdForAttackTower(TowerDataSO towerData)
    {
        int abilityId = -1;

        if (towerData == null || towerData.randomAbilityGroupId <= 0)return -1;

        int attackTowerId = towerData.towerIdInt;
        int exclusiveId = GetExclusiveUnlockAbilityId(attackTowerId);

        int safe = 0;
        while (true)
        {
            safe++;
            if (safe > 200) break; 

            abilityId = AbilityManager.GetRandomAbilityFromGroup(
                towerData.randomAbilityGroupId,
                requiredTowerType: 0,   
                useWeight: true);

            if (abilityId <= 0) continue;
            if (exclusiveId > 0 && abilityId == exclusiveId)
            {
                if (!IsTowerUpgradeLevelAtLeast4(attackTowerId))
                    continue;

                if (ExclusiveAbilityRunTracker.IsTaken(abilityId))
                    continue;
            }
            return abilityId;
        }

        return -1;
    }


    private void SetIsTutorial(bool isTutorial)
    {
        this.isTutorial = isTutorial;
    }

    //Upgrade System -------------------------
    private TowerOptionKey MakeKey(TowerDataSO towerData, int abilityId)
    {
        return new TowerOptionKey
        {
            InstallType = TowerInstallType.Attack,
            towerKey = towerData != null ? towerData.towerIdInt : -1,
            abilityId = abilityId
        };
    }

    private TowerOptionKey MakeKey(AmplifierTowerDataSO ampData, int abilityId)
    {
        return new TowerOptionKey
        {
            InstallType = TowerInstallType.Amplifier,
            towerKey = ampData != null ? ampData.BuffTowerId : -1, 
            abilityId = abilityId
        };
    }


    private bool IsForbiddenCombo(
    TowerInstallType type,
    TowerDataSO towerData,
    AmplifierTowerDataSO ampData,
    int abilityId,
    bool isInitial,
    TowerOptionKey? previousKey = null)
    {
        if (abilityId < 0) return false;

        int towerKey = -1;

        if (type == TowerInstallType.Attack)
        {
            if (towerData == null) return false;
            towerKey = towerData.towerIdInt;
        }
        else if (type == TowerInstallType.Amplifier)
        {
            if (ampData == null) return false;
            towerKey = ampData.BuffTowerId;
        }
        else
        {
            return false;
        }

        if (previousKey.HasValue)
        {
            var prev = previousKey.Value;
            if (prev.InstallType == type &&
                prev.towerKey == towerKey &&
                prev.abilityId == abilityId)
            {
                return true;
            }
        }

        if (hasLastChosenOption &&
            lastChosenOption.InstallType == type &&
            lastChosenOption.towerKey == towerKey &&
            lastChosenOption.abilityId == abilityId)
        {
            return true;
        }

        if (!isInitial && initialOptionKeys != null)
        {
            for (int i = 0; i < initialOptionKeys.Length; i++)
            {
                var key = initialOptionKeys[i];
                if (key.InstallType == type &&
                    key.towerKey == towerKey &&
                    key.abilityId == abilityId)
                {
                    return true;
                }
            }
        }

        if (choices != null)
        {
            for (int i = 0; i < choices.Length; i++)
            {
                var c = choices[i];
                if (c == null) continue;
                if (c.ability < 0) continue;

                if (type == TowerInstallType.Attack &&
                    c.InstallType == TowerInstallType.Attack &&
                    c.AttackTowerData != null &&
                    c.AttackTowerData.towerIdInt == towerKey &&
                    c.ability == abilityId)
                {
                    return true;
                }

                if (type == TowerInstallType.Amplifier &&
                    c.InstallType == TowerInstallType.Amplifier &&
                    c.AmplifierTowerData != null &&
                    c.AmplifierTowerData.BuffTowerId == towerKey &&
                    c.ability == abilityId)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsForbiddenAttackCombo(TowerDataSO towerData, int abilityId, bool isInitial, TowerOptionKey? previousKey = null)
    {
        return IsForbiddenCombo(TowerInstallType.Attack, towerData, null, abilityId, isInitial, previousKey);
    }

    private bool IsForbiddenAmplifierCombo(AmplifierTowerDataSO ampData, int abilityId, bool isInitial, TowerOptionKey? previousKey = null)
    {
        return IsForbiddenCombo(TowerInstallType.Amplifier, null, ampData, abilityId, isInitial, previousKey);
    }

    private bool IsForbiddenAttackCombo(TowerDataSO towerData, int abilityId, bool isInitial)
    {
        return IsForbiddenCombo(TowerInstallType.Attack, towerData, null, abilityId, isInitial);
    }

    private bool IsForbiddenAmplifierCombo(AmplifierTowerDataSO ampData, int abilityId, bool isInitial)
    {
        return IsForbiddenCombo(TowerInstallType.Amplifier, null, ampData, abilityId, isInitial);
    }

    private void RegisterLastChosenOption(
    TowerInstallType type,
    TowerDataSO towerData,
    AmplifierTowerDataSO ampData,
    int abilityId)
    {
        if (type == TowerInstallType.Attack && towerData == null) return;
        if (type == TowerInstallType.Amplifier && ampData == null) return;

        lastChosenOption = (type == TowerInstallType.Attack)
            ? MakeKey(towerData, abilityId)
            : MakeKey(ampData, abilityId);

        hasLastChosenOption = true;
    }

    private void GetNewUpgradeProbabilities(out float newProb, out float upgradeProb)
    {
        int upgradableCount = installControl.GetUpgradeableTowerCount();
        bool isFieldFull = installControl.CurrentTowerCount >= installControl.MaxTowerCount;
        int c = Mathf.Max(1, upgradableCount);
       
        if(!isFieldFull)
        {
            if (c == 1)
            {
                newProb = 0.9f;
                upgradeProb = 0.1f;
            }
            else if (c == 2)
            {
                newProb = 0.8f;
                upgradeProb = 0.2f;
            }
            else if (c == 3)
            {
                newProb = 0.7f;
                upgradeProb = 0.3f;
            }
            else if (c == 4)
            {
                newProb = 0.5f;
                upgradeProb = 0.5f;
            }
            else if (c == 5)
            {
                newProb = 0.3f;
                upgradeProb = 0.7f;
            }
            else
            {
                newProb = 0.1f;
                upgradeProb = 0.9f;
            }
        }
        else
        {
            if (c == 1)
            {
                newProb = 0.3f;
                upgradeProb = 0.7f;
            }
            else if (c == 2)
            {
                newProb = 0.2f;
                upgradeProb = 0.8f;
            }
            else
            {
                newProb = 0.1f;
                upgradeProb = 0.9f;
            }
        }
    }
    private int GetTotalTowerCount() //used in tutorial, gold
    {
        return installControl.CurrentTowerCount;
    }

    private AmplifierTowerDataSO GetRandomAmplifierForCard(ICollection<AmplifierTowerDataSO> extraExcludes = null)
    {
        // debug
        if (debugForceAmplifier)
        {
            if (allAmplifierTowers == null || allAmplifierTowers.Length == 0)
                return null;

            int id = Mathf.Clamp(debugAmplifierIndex, 0, allAmplifierTowers.Length - 1);
            return allAmplifierTowers[id];
        }

        if (allAmplifierTowers == null || allAmplifierTowers.Length == 0)
            return null;

        HashSet<int> excludeIds = new HashSet<int>();

        for (int i = 0; i < installControl.TowerCount; i++)
        {
            var amp = installControl.GetAmplifierTower(i);
            if (amp != null && amp.AmplifierTowerData != null)
                excludeIds.Add(amp.AmplifierTowerData.BuffTowerId);
        }

        if (extraExcludes != null)
        {
            foreach (var d in extraExcludes)
            {
                if (d != null) excludeIds.Add(d.BuffTowerId);
            }
        }

        List<AmplifierTowerDataSO> candidates = new List<AmplifierTowerDataSO>();

        if (isTutorial && Variables.Stage == 1)
        {
            int limit = Mathf.Min(2, allAmplifierTowers.Length);
            for (int i = 0; i < limit; i++)
            {
                var d = allAmplifierTowers[i];
                if (d == null) continue;
                if (!excludeIds.Contains(d.BuffTowerId))
                    candidates.Add(d);
            }
        }
        else
        {
            foreach (var d in allAmplifierTowers)
            {
                if (d == null) continue;
                if (!excludeIds.Contains(d.BuffTowerId))
                    candidates.Add(d);
            }
        }

        if (candidates.Count == 0) return null;

        // weight pick
        if (CollectionManager.Instance == null || !CollectionManager.Instance.IsInitialized)
        {
            int colIdx = UnityEngine.Random.Range(0, candidates.Count);
            return candidates[colIdx];
        }

        List<float> weights = new List<float>();
        float totalWeight = 0f;

        foreach (var ampData in candidates)
        {
            float w = CollectionManager.Instance.GetWeight(ampData.BuffTowerId);
            weights.Add(w);
            totalWeight += w;
        }

        float randValue = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            cumulative += weights[i];
            if (randValue <= cumulative)
                return candidates[i];
        }
        return candidates[candidates.Count - 1];
    }
    private bool IsAttackSlotUpgradable(int slotIndex)
    {
        var attack = installControl.GetAttackTower(slotIndex);
        if (attack == null) return false;
        return attack.ReinforceLevel < MaxReinforceLevelInternal;
    }

    private void SetUpNewInstallCard(int i, int slotNumber, bool isInitial, TowerOptionKey? previousKey = null)
    {
        if (GetTotalTowerCount() == 0)
        {
            SetUpNewAttackCard(i, slotNumber, isInitial, previousKey);
            return;
        }

        bool canSpawnAmplifier = (allAmplifierTowers != null && allAmplifierTowers.Length > 0);
        bool hasAttackCandidate = HasAnyNewAttackTowerCandidate();

        int towerType = 0;

        if (canSpawnAmplifier)
        {
            if (!hasAttackCandidate)
            {
                towerType = 1;
            }
            else
            {
                towerType = Random.Range(0, 2);
            }
        }
        else
        {
            towerType = 0;
        }

        if (towerType == 0)
        {
            SetUpNewAttackCard(i, slotNumber, isInitial, previousKey);
        }
        else
        {
            SetUpNewAmplifierCard(i, slotNumber, isInitial, previousKey);
        }
    }
    //----------------------------------------

    private bool HasAnyNewAttackTowerCandidate()
    {
        return GetRemainingUninstalledAttackTowerTypeCount() > 0;
    }

    private bool HasAnyAmplifierCandidateForCard()
    {
        var ampData = GetRandomAmplifierForCard(usedAmplifierTowerTypesThisRoll);
        return ampData != null;
    }

    //weight pick
    private float PickUpgradeSlotByWeight(List<int> upgradeSlots)
    {
        if(upgradeSlots == null || upgradeSlots.Count == 0)
        {
            return -1f;
        }

        if(CollectionManager.Instance == null || !CollectionManager.Instance.IsInitialized)
        {
            int randIdx = UnityEngine.Random.Range(0, upgradeSlots.Count);
            return upgradeSlots[randIdx];
        }

        List<float> weights = new List<float>();
        float totalWeight = 0f;

        foreach(int slotIdx in upgradeSlots)
        {
            float weight = 0f;

            var attackTowerData = installControl.GetTowerData(slotIdx);
            if(attackTowerData != null)
            {
                weight = CollectionManager.Instance.GetWeight(attackTowerData.towerIdInt);
            }
            else
            {
                var ampTower = installControl.GetAmplifierTower(slotIdx);
                if(ampTower != null && ampTower.AmplifierTowerData != null)
                {
                    weight = CollectionManager.Instance.GetWeight(ampTower.AmplifierTowerData.BuffTowerId);
                }
            }
            weights.Add(weight);
            totalWeight += weight;
        }

        if(totalWeight <= 0)
        {
            return upgradeSlots[UnityEngine.Random.Range(0, upgradeSlots.Count)];
        }

        float randValue = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < upgradeSlots.Count; i++)
        {
            cumulative += weights[i];
            if (randValue <= cumulative)
            {
                return upgradeSlots[i];
            }
        }
        return upgradeSlots[upgradeSlots.Count - 1];
    }
    private void RefreshStage1SingleCard(int index)
    {
        if (ShouldShowGoldCard())
        {
            SetupStage1AllGoldCards();
            return;
        }

        if (choices == null || index < 0 || index >= choices.Length) return;

        TowerOptionKey? previousKey = null;
        var prevChoice = choices[index];

        if (prevChoice != null)
        {
            if (prevChoice.InstallType == TowerInstallType.Attack && prevChoice.AttackTowerData != null)
            {
                previousKey = MakeKey(prevChoice.AttackTowerData, prevChoice.ability);

                if (usedAttackTowerTypesThisRoll.Contains(prevChoice.AttackTowerData))
                    usedAttackTowerTypesThisRoll.Remove(prevChoice.AttackTowerData);
            }
            else if (prevChoice.InstallType == TowerInstallType.Amplifier && prevChoice.AmplifierTowerData != null)
            {
                previousKey = MakeKey(prevChoice.AmplifierTowerData, prevChoice.ability);

                if (usedAmplifierTowerTypesThisRoll.Contains(prevChoice.AmplifierTowerData))
                    usedAmplifierTowerTypesThisRoll.Remove(prevChoice.AmplifierTowerData);
            }
        }

        choices[index].InstallType = TowerInstallType.Attack;
        choices[index].AttackTowerData = null;
        choices[index].AmplifierTowerData = null;
        choices[index].BuffSlotIndex = null;
        choices[index].RandomAbilitySlotIndex = null;
        choices[index].ability = -1;

        abilities[index] = -1;
        installControl.IsReadyInstall = false;
        outlineObjects[index].SetActive(false);

        List<int> emptySlots = new List<int>();
        List<int> upgradeSlots = new List<int>();

        int currentSlot = (numlist != null && index < numlist.Count) ? numlist[index] : -9999;

        for (int i = 0; i < installControl.TowerCount; i++)
        {
            if (numlist != null && numlist.Contains(i) && i != currentSlot) continue;

            bool used = installControl.IsUsedSlot(i);

            if (!used)
            {
                if (installControl.CurrentTowerCount < installControl.MaxTowerCount)
                    emptySlots.Add(i);
            }
            else
            {
                int reinforceLevel = installControl.GetSlotReinforceLevel(i);
                if (!installControl.IsSlotMaxLevel(i) && reinforceLevel < MaxReinforceLevel)
                    upgradeSlots.Add(i);
            }
        }

        int totalTowerCount = GetTotalTowerCount();
        bool hasPistol = HasTowerTypeInstalled(tutorialPistolTower);
        bool hasAmp1 = HasAmplifierInstalled(damageMatrixCoreSO);
        bool hasAmp2 = HasAmplifierInstalled(proejctileCoreSO);

        List<AmplifierTowerDataSO> remainingAmps = new List<AmplifierTowerDataSO>();
        if (!hasAmp1 && damageMatrixCoreSO != null) remainingAmps.Add(damageMatrixCoreSO);
        if (!hasAmp2 && proejctileCoreSO != null) remainingAmps.Add(proejctileCoreSO);

        if (!hasPistol && totalTowerCount == 0)
        {
            int slotNumber = -1;
            if (emptySlots.Count > 0)
            {
                int slotIdx = Random.Range(0, emptySlots.Count);
                slotNumber = emptySlots[slotIdx];
            }

            numlist[index] = slotNumber;
            SetUpTutorialAttackCard(index, slotNumber, isInitial: false, previousKey: previousKey);
            UpdateInitialOptionKey(index);
            return;
        }

        if (remainingAmps.Count > 0 && emptySlots.Count > 0)
        {
            int slotIdx = Random.Range(0, emptySlots.Count);
            int slotNumber = emptySlots[slotIdx];

            var ampData = remainingAmps[Random.Range(0, remainingAmps.Count)];

            numlist[index] = slotNumber;
            SetUpTutorialAmplifierCard(index, slotNumber, ampData, isInitial: false, previousKey: previousKey);
            UpdateInitialOptionKey(index);
            return;
        }

        if (upgradeSlots.Count > 0)
        {
            int safe = 0;
            int pickedIdx = -1;

            while (safe++ < 50 && upgradeSlots.Count > 0)
            {
                int listIdx = Random.Range(0, upgradeSlots.Count);
                int candidate = upgradeSlots[listIdx];

                bool alreadyUsedByOtherCard =
                    (numlist != null && numlist.Contains(candidate) && candidate != currentSlot);

                if (alreadyUsedByOtherCard)
                {
                    upgradeSlots.RemoveAt(listIdx);
                    continue;
                }

                var candidateData = installControl.GetTowerData(candidate);
                var candidateAmp = installControl.GetAmplifierTower(candidate);

                bool isDuplicate = false;
                if (candidateData != null && usedAttackTowerTypesThisRoll.Contains(candidateData))
                {
                    isDuplicate = true;
                }
                else if (candidateAmp != null && candidateAmp.AmplifierTowerData != null &&
                         usedAmplifierTowerTypesThisRoll.Contains(candidateAmp.AmplifierTowerData))
                {
                    isDuplicate = true;
                }

                if (isDuplicate)
                {
                    upgradeSlots.RemoveAt(listIdx);
                    continue;
                }
                pickedIdx = listIdx;
                break;
            }

            if (pickedIdx != -1)
            {
                int slotNumber = upgradeSlots[pickedIdx];

                numlist[index] = slotNumber;
                SetUpgradeCardForUsedSlot(index, slotNumber, isInitial: false);
                UpdateInitialOptionKey(index);
                return;
            }
        }

        int slotNum = -1;
        for (int i = 0; i < installControl.TowerCount; i++)
        {
            if (numlist != null && numlist.Contains(i)) continue;
            if (!installControl.IsUsedSlot(i) && installControl.CurrentTowerCount < installControl.MaxTowerCount)
            {
                slotNum = i;
                break;
            }
        }
        numlist[index] = slotNum;
        SetUpNewInstallCard(index, slotNum, isInitial: false, previousKey: previousKey);
        UpdateInitialOptionKey(index);
    }
    //unlock
    private int GetTowerUpgradeLevel(int attackTowerId)
    {
        var mgr = UserTowerUpgradeManager.Instance;
        if (mgr == null || mgr.CurrentTowerUpgradeData == null) return 0;

        var data = mgr.CurrentTowerUpgradeData;
        if (data.towerIds == null || data.upgradeLevels == null) return 0;

        int idx = data.towerIds.IndexOf(attackTowerId);
        if (idx < 0 || idx >= data.upgradeLevels.Count) return 0;

        return data.upgradeLevels[idx];
    }

    private bool IsTowerUpgradeLevelAtLeast4(int attackTowerId)
    {
        return GetTowerUpgradeLevel(attackTowerId) >= 4;
    }

    private int GetExclusiveUnlockAbilityId(int attackTowerId)
    {
        if (!DataTableManager.IsInitialized) return -1;

        var table = DataTableManager.TowerUpgradeAbilityUnlockTable;
        if (table == null) return -1;

        int rowId = table.GetDataId(attackTowerId);
        if (rowId <= 0) return -1;

        var row = table.Get(rowId);
        if (row == null) return -1;

        return row.RandomAbility_ID;
    }

    private bool IsExclusiveUnlockAbility(int attackTowerId, int abilityId)
    {
        int exclusiveId = GetExclusiveUnlockAbilityId(attackTowerId);
        return exclusiveId > 0 && abilityId == exclusiveId;
    }

    private void UpdateInitialOptionKey(int index)
    {
        if (initialOptionKeys == null || index < 0 || index >= initialOptionKeys.Length)
            return;

        if (choices == null || index >= choices.Length)
            return;

        var choice = choices[index];

        if (choice.InstallType == TowerInstallType.Attack && choice.AttackTowerData != null)
        {
            initialOptionKeys[index] = MakeKey(choice.AttackTowerData, choice.ability);
        }
        else if (choice.InstallType == TowerInstallType.Amplifier && choice.AmplifierTowerData != null)
        {
            initialOptionKeys[index] = MakeKey(choice.AmplifierTowerData, choice.ability);
        }
    }

    public void OnClickGoldCard()
    {
        var battleUI = FindObjectOfType<BattleUI>();
        if (battleUI != null)
        {
            battleUI.AddCoinGainText(100);
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.AddAccumulateGold(100);
        }

        if (towerInfoUI != null)
            towerInfoUI.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private void BindGoldCardClick(GameObject goldCard)
    {
        if (goldCard == null) return;

        var buttons = goldCard.GetComponentsInChildren<Button>(true);
        Button pick = null;

        var rootBtn = goldCard.GetComponent<Button>();
        if (rootBtn != null)
        {
            pick = rootBtn;
        }
        else if (buttons != null && buttons.Length > 0)
        {
            foreach (var b in buttons)
            {
                if (b != null && b.gameObject.activeInHierarchy)
                {
                    pick = b;
                    break;
                }
            }
        }

        if (pick == null) return;

        if (pick.targetGraphic == null)
        {
            var img = pick.GetComponent<Image>();
            if (img == null) img = pick.GetComponentInChildren<Image>(true);
            if (img != null)
            {
                pick.targetGraphic = img;
                img.raycastTarget = true;
            }
        }
        else
        {
            pick.targetGraphic.raycastTarget = true;
        }

        pick.onClick.RemoveAllListeners();
        pick.onClick.AddListener(() => {OnClickGoldCard();});
        pick.interactable = true;

        var eventTrigger = goldCard.GetComponent<EventTrigger>();
        if (eventTrigger == null)
            eventTrigger = goldCard.AddComponent<EventTrigger>();

        var clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((data) => {OnClickGoldCard();});
        eventTrigger.triggers.Add(clickEntry);

        var canvas = goldCard.GetComponent<Canvas>();
        if (canvas == null)
            canvas = goldCard.AddComponent<Canvas>();

        canvas.overrideSorting = true;
        canvas.sortingOrder = 1000;

        var raycaster = goldCard.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
            goldCard.AddComponent<GraphicRaycaster>();
    }
    private TowerDataSO GetRandomNewAttackTowerCandidate(ICollection<TowerDataSO> rollExcludes)
    {
        HashSet<int> installedIds = new HashSet<int>();
        for (int i = 0; i < installControl.TowerCount; i++)
        {
            if (!installControl.IsUsedSlot(i)) continue;
            var d = installControl.GetTowerData(i);
            if (d != null) installedIds.Add(d.towerIdInt);
        }
        HashSet<TowerDataSO> localExclude = rollExcludes != null
            ? new HashSet<TowerDataSO>(rollExcludes)
            : new HashSet<TowerDataSO>();

        for (int t = 0; t < 60; t++)
        {
            var candidate = installControl.GetRandomAttackTowerDataForCard(localExclude);
            if (candidate == null) return null;
            if (!HasTowerTypeInstalled(candidate) && !installedIds.Contains(candidate.towerIdInt))
                return candidate;

            localExclude.Add(candidate);
        }
        return null;
    }

    private bool IsGoldCardSlot(int cardIndex)
    {
        if (cardIndex < 0) return false;

        bool slotIsMinusOne =
            (numlist != null && cardIndex < numlist.Count && numlist[cardIndex] == -1);

        bool choiceIsEmpty =
            (choices != null && cardIndex < choices.Length &&
             choices[cardIndex].AttackTowerData == null &&
             choices[cardIndex].AmplifierTowerData == null);

        return slotIsMinusOne && choiceIsEmpty;
    }

    private void SetupStage1AllGoldCards()
    {
        if (numlist == null) numlist = new List<int>();
        numlist.Clear();

        if (abilities == null || abilities.Length != upgradeUIs.Length)
            abilities = new int[upgradeUIs.Length];

        if (choices == null || choices.Length != upgradeUIs.Length)
            choices = new TowerInstallChoice[upgradeUIs.Length];

        for (int i = 0; i < upgradeUIs.Length; i++)
        {
            numlist.Add(-1);

            abilities[i] = -1;
            choices[i] = new TowerInstallChoice
            {
                InstallType = TowerInstallType.Attack,
                AttackTowerData = null,
                AmplifierTowerData = null,
                BuffSlotIndex = null,
                RandomAbilitySlotIndex = null,
                ability = -1
            };

            if (outlineObjects != null && i < outlineObjects.Length && outlineObjects[i] != null)
                outlineObjects[i].SetActive(false);

            DeleteAlreadyInstalledCard(i);

            if (goldCardPrefab != null && upgradeUIs[i] != null)
            {
                var gold = Instantiate(goldCardPrefab, upgradeUIs[i].transform);
                gold.transform.SetAsFirstSibling();
                BindGoldCardClick(gold);
            }
        }

        if (refreshButtons != null)
        {
            for (int i = 0; i < refreshButtons.Length; i++)
            {
                if (refreshButtons[i] == null) continue;
                refreshButtons[i].interactable = false;
            }
        }
        if (usedRefreshButton != null)
        {
            for (int i = 0; i < usedRefreshButton.Count; i++)
                usedRefreshButton[i] = true;
        }
    }

    // TowerUpgradeSlotUI 클래스 안 아무데나 (아래쪽 추천)
    private int GetRemainingUninstalledAttackTowerTypeCount()
    {
        if (installControl == null) return 0;

        var list = installControl.AvailableAttackTowers;
        if (list == null) return 0;

        HashSet<int> installedIds = new HashSet<int>();
        for (int i = 0; i < installControl.TowerCount; i++)
        {
            if (!installControl.IsUsedSlot(i)) continue;

            var d = installControl.GetTowerData(i);
            if (d != null) installedIds.Add(d.towerIdInt);
        }

        int count = 0;
        foreach (var d in list)
        {
            if (d == null) continue;
            if (!installedIds.Contains(d.towerIdInt))
                count++;
        }
        return count;
    }

    private TowerDataSO GetOnlyRemainingUninstalledAttackTowerType()
    {
        if (installControl == null) return null;
        var list = installControl.AvailableAttackTowers;
        if (list == null) return null;

        HashSet<int> installedIds = new HashSet<int>();
        for (int i = 0; i < installControl.TowerCount; i++)
        {
            if (!installControl.IsUsedSlot(i)) continue;
            var d = installControl.GetTowerData(i);
            if (d != null) installedIds.Add(d.towerIdInt);
        }

        TowerDataSO only = null;
        foreach (var d in list)
        {
            if (d == null) continue;
            if (installedIds.Contains(d.towerIdInt)) continue;
            if (only != null) return null; 
            only = d;
        }
        return only;
    }
}