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
    private enum SlotCardType { Empty, Gold, Upgrade, InstallTower }
    private struct SlotCardPlan
    {
        public SlotCardType type;
        public int towerId;
        public int upgradeId;
        public object payload;
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
    [SerializeField] private int goldCardRewardAmount = 100;

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
    = new HashSet<TowerDataSO>(new TowerDataSoIdComparer());
    private HashSet<AmplifierTowerDataSO> usedAmplifierTowerTypesThisRoll
    = new HashSet<AmplifierTowerDataSO>(new AmpDataSoIdComparer());
    private readonly HashSet<TowerDataSO> installedAttackTowerTypesCache
    = new HashSet<TowerDataSO>(new TowerDataSoIdComparer());
    private HashSet<int> shownUpgradeSlotsThisRoll = new HashSet<int>();
    private HashSet<int> stage1ShownUpgradeAttackIdsThisRoll = new HashSet<int>();
    private HashSet<int> stage1ShownUpgradeAmpIdsThisRoll = new HashSet<int>();

    private List<bool> usedRefreshButton;
    private const int MaxReinforceLevel = 4;

    [SerializeField] private PlanetTowerUI planetTowerUI;
    private bool hasInitializedForStage1 = false;
    private bool hasShownStep4ThisRoll = false;
    private const int UpgradeOptionAbilityId = 0;

    private void Start()
    {
        installControl.OnTowerInstalled += SetTowerInstallText;
        SetIsTutorial(TutorialManager.Instance.IsTutorialMode);
        if (Variables.Stage == 1)
            installControl.MaxTowerCount = 3;
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

            if (IsQuasarItemUsed)
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
        Debug.Log("SetTowerInstallText");
        towerInstallText.text = $"({installControl.CurrentTowerCount}/{installControl.MaxTowerCount})";
    }

    private void OnDisable()
    {
        foreach (var ui in upgradeUIs)
            ui.SetActive(false);
        SetActiveRefreshButtons(false);

        if(isTutorial && Variables.Stage == 1)
        {
            TutorialManager.Instance.ShowTutorialStep(1);
        }
        else if(isTutorial && Variables.Stage == 2)
        {
            TutorialManager.Instance.ShowTutorialStep(6);
        }
        else
        {
            GamePauseManager.Instance.Resume();
        }

        numlist = null;
        choosedIndex = -1;
        isStartTouch = false;
        towerImageIsDraging = false;
        isFirstInstall = false;

        if (Variables.Stage == 1)
        {
            hasLastChosenOption = false;
            lastChosenOption = default;
            tutorialPistolInstalled = false;
            tutorialAmp1Installed = false;
            tutorialAmp2Installed = false;
        }

        numlist = null;
        choosedIndex = -1;
        isStartTouch = false;
        towerImageIsDraging = false;
        isFirstInstall = false;
        gameResumeButton.interactable = true;

        if (isOpenDeploy)
        {
            isOpenDeploy = false;
            SoundManager.Instance.PlayDeployClose();
            return;
        }
    }
    private void Update()
    {
        if (planetTowerUI.IsBackBtnClicked) return;
        if (towerInfoUI != null && towerInfoUI.gameObject.activeSelf) return;
        if (UIBlockPanelControl.IsBlockedPanel) return;
        if (planetTowerUI.ISConfirmPanelActive) return;
        OnTouchStateCheck();

        if (!TouchManager.Instance.IsTouching && !towerImageIsDraging) return;
        if (towerImageIsDraging && dragImage != null)
            dragImage.transform.position = TouchManager.Instance.TouchPos;
        OnTouchMakeDrageImage();
    }
    private void SetActiveRefreshButtons(bool active)
    {
        foreach (var refreshButton in refreshButtons)
            refreshButton.gameObject.SetActive(active);
    }
    private void SettingUpgradeCards()
    {
        RebuildInstalledAttackTowerTypeCache();
        ResetChoose();
        installControl.IsReadyInstall = false;
        numlist = new List<int>(upgradeUIs.Length);
        for (int i = 0; i < upgradeUIs.Length; i++) numlist.Add(-1);

        usedAttackTowerTypesThisRoll.Clear();
        usedAmplifierTowerTypesThisRoll.Clear();
        shownUpgradeSlotsThisRoll.Clear();
        stage1ShownUpgradeAttackIdsThisRoll.Clear();
        stage1ShownUpgradeAmpIdsThisRoll.Clear();

        initialOptionKeys = new TowerOptionKey[upgradeUIs.Length];
        for (int i = 0; i < initialOptionKeys.Length; i++)
        {
            initialOptionKeys[i] = new TowerOptionKey
            {
                InstallType = TowerInstallType.Attack,
                towerKey = -1,
                abilityId = -1
            };
        }

        int totalTowerCount = GetTotalTowerCount();

        if (Variables.Stage == 1 && isTutorial)
        {
            SettingStage1Cards(totalTowerCount);
            return;
        }

        bool showGold = ShouldShowGoldCardByStage();
        SettingNormalStageCards_Shuffled(showGold);

    }


    private void CacheInitialOptionKey(int index)
    {
        if (initialOptionKeys == null || choices == null) return;
        if (index < 0 || index >= initialOptionKeys.Length) return;
        if (index < 0 || index >= choices.Length) return;

        var c = choices[index];
        int towerKey = -1;

        if (c.InstallType == TowerInstallType.Attack && c.AttackTowerData != null)
            towerKey = c.AttackTowerData.towerIdInt;
        else if (c.InstallType == TowerInstallType.Amplifier && c.AmplifierTowerData != null)
            towerKey = c.AmplifierTowerData.BuffTowerId;

        initialOptionKeys[index] = new TowerOptionKey
        {
            InstallType = c.InstallType,
            towerKey = towerKey,
            abilityId = c.ability
        };
    }
    private void SettingStage1Cards(int totalTowerCount)
    {
        if (IsStage1AllGoldState())
        {
            usedAttackTowerTypesThisRoll.Clear();
            usedAmplifierTowerTypesThisRoll.Clear();
            shownUpgradeSlotsThisRoll.Clear();

            for (int i = 0; i < upgradeUIs.Length; i++)
                SetupGoldCardUI(i);
            return;
        }

        if (IsStage1FirstRollPistolOnly())
        {
            for (int i = 0; i < upgradeUIs.Length; i++)
            {
                DeleteAlreadyInstalledCard(i);
                outlineObjects[i].SetActive(false);
                if (numlist != null && i < numlist.Count) numlist[i] = -1;

                SetUpTutorialAttackCard(i, -1, isInitial: true);
            }
            return;
        }

        bool pistolMissing = !HasTowerTypeInstalled(tutorialPistolTower);
        bool amp1Missing = !HasAmplifierInstalled(damageMatrixCoreSO);
        bool amp2Missing = !HasAmplifierInstalled(proejctileCoreSO);
        bool canInstall = CanInstallAny_Strict();

        List<int> upgradeSlots = new List<int>();
        for (int s = 0; s < installControl.TowerCount; s++)
        {
            if (installControl.IsUsedSlot(s) && IsSlotUpgradeable_Strict(s))
                upgradeSlots.Add(s);
        }

        List<int> uniqueUpgradeSlots = BuildStage1UniqueUpgradeSlots(upgradeSlots);
        List<int> emptySlots = new List<int>();
        if (canInstall)
        {
            for (int s = 0; s < installControl.TowerCount; s++)
            {
                if (!installControl.IsUsedSlot(s))
                    emptySlots.Add(s);
            }
        }

        var missingTypes = GetStage1MissingTypes();
        bool allowDuplicateInstallForSingleMissing = (missingTypes.Count == 1);

        List<System.Action<int, int>> installPool = new List<System.Action<int, int>>();

        if (pistolMissing && tutorialPistolTower != null)
        {
            installPool.Add((cardIdx, slotNum) =>
            {
                usedAttackTowerTypesThisRoll.Add(tutorialPistolTower);
                numlist[cardIdx] = slotNum;
                if (slotNum != -1) shownUpgradeSlotsThisRoll.Add(slotNum);
                SetUpTutorialAttackCard(cardIdx, slotNum, isInitial: true);
            });
        }
        if (amp1Missing && damageMatrixCoreSO != null)
        {
            installPool.Add((cardIdx, slotNum) =>
            {
                AddUsedAmplifierType(damageMatrixCoreSO);
                numlist[cardIdx] = slotNum;
                if (slotNum != -1) shownUpgradeSlotsThisRoll.Add(slotNum);
                SetUpTutorialAmplifierCard(cardIdx, slotNum, damageMatrixCoreSO, isInitial: true);
            });
        }
        if (amp2Missing && proejctileCoreSO != null)
        {
            installPool.Add((cardIdx, slotNum) =>
            {
                AddUsedAmplifierType(proejctileCoreSO);
                numlist[cardIdx] = slotNum;
                if (slotNum != -1) shownUpgradeSlotsThisRoll.Add(slotNum);
                SetUpTutorialAmplifierCard(cardIdx, slotNum, proejctileCoreSO, isInitial: true);
            });
        }
        List<int> cardIndices = new List<int>();
        for (int i = 0; i < upgradeUIs.Length; i++) cardIndices.Add(i);
        for (int i = cardIndices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (cardIndices[i], cardIndices[j]) = (cardIndices[j], cardIndices[i]);
        }

        foreach (int cardIndex in cardIndices)
        {
            bool placed = false;
            if (uniqueUpgradeSlots.Count > 0)
            {
                int pickIdx = Random.Range(0, uniqueUpgradeSlots.Count);
                int pickedSlot = uniqueUpgradeSlots[pickIdx];
                uniqueUpgradeSlots.RemoveAt(pickIdx);

                if (!IsSlotReservedByOtherCard(pickedSlot, cardIndex) && !shownUpgradeSlotsThisRoll.Contains(pickedSlot))
                {
                    numlist[cardIndex] = pickedSlot;
                    shownUpgradeSlotsThisRoll.Add(pickedSlot);
                    MarkStage1UpgradeTypeShown(pickedSlot);

                    var pickedTd = installControl.GetTowerData(pickedSlot);
                    if (pickedTd != null) usedAttackTowerTypesThisRoll.Add(pickedTd);

                    var pickedAmp = installControl.GetAmplifierTower(pickedSlot);
                    if (pickedAmp != null && pickedAmp.AmplifierTowerData != null)
                        AddUsedAmplifierType(pickedAmp.AmplifierTowerData);

                    SetUpgradeCardForUsedSlot(cardIndex, pickedSlot, isInitial: true);
                    placed = true;
                }
            }

            if (!placed && canInstall && emptySlots.Count > 0 && installPool.Count > 0)
            {
                int slotIdx = Random.Range(0, emptySlots.Count);
                int slotNumber = emptySlots[slotIdx];
                emptySlots.RemoveAt(slotIdx);

                int actionIdx = Random.Range(0, installPool.Count);
                var action = installPool[actionIdx];
                if (!allowDuplicateInstallForSingleMissing)
                    installPool.RemoveAt(actionIdx);

                action.Invoke(cardIndex, slotNumber);
                placed = true;
            }

            if (!placed && canInstall && emptySlots.Count > 0)
            {
                int slotIdx = Random.Range(0, emptySlots.Count);
                int slotNumber = emptySlots[slotIdx];
                emptySlots.RemoveAt(slotIdx);

                numlist[cardIndex] = slotNumber;
                shownUpgradeSlotsThisRoll.Add(slotNumber);

                SetUpNewInstallCard(cardIndex, slotNumber, isInitial: true);
                placed = true;
            }
            if (!placed)
            {
                SetupGoldCardUI(cardIndex);
            }
        }

    }

    private bool ShouldShowGoldCard()
    {
        if (Variables.Stage == 1)
        {
            if (installControl.CurrentTowerCount <= 0) return false;

            bool canInstall = CanInstallAny_Strict();
            int upgradable = GetUpgradeableCount_Strict();

            if (canInstall) return false;
            if (upgradable > 0) return false;

            return true;
        }
        if (GetUpgradeableCount_Strict() > 0) return false;

        bool hasNewAttackCandidate =
             GetRandomAttackTowerDataForCard_ExcludeInstalled(usedAttackTowerTypesThisRoll) != null;

        bool hasNewAmplifierCandidate =
            GetRandomAmplifierForCard(usedAmplifierTowerTypesThisRoll) != null;

        if (hasNewAttackCandidate || hasNewAmplifierCandidate) return false;

        return true;
    }
    private bool ShouldShowGoldCard_Stage1()
    {
        return ShouldShowGoldCard();
    }

    private bool ShouldShowGoldCard_Normal()
    {
        if (installControl == null) return false;
        if (installControl.CurrentTowerCount <= 0) return false;

        bool hasAnyUpgradable = GetUpgradeableCount_Strict() > 0;
        bool isFieldFull = installControl.CurrentTowerCount >= installControl.MaxTowerCount;

        if (isFieldFull && !hasAnyUpgradable) return true;
        return false;
    }

    private bool ShouldShowGoldCardByStage()
    {
        if (Variables.Stage == 1) return ShouldShowGoldCard_Stage1();
        return ShouldShowGoldCard_Normal();
    }




    private bool HasTowerTypeInstalled(TowerDataSO towerData)
    {
        if (towerData == null) return false;

        for (int i = 0; i < installControl.TowerCount; i++)
        {
            if (!installControl.IsUsedSlot(i)) continue;

            var data = installControl.GetTowerData(i);
            if (data != null && data.towerIdInt == towerData.towerIdInt)
                return true;
        }

        if (isTutorial && Variables.Stage == 1 && towerData == tutorialPistolTower)
            return tutorialPistolInstalled;

        return false;
    }

    private bool HasAmplifierInstalled(AmplifierTowerDataSO target)
    {
        if (target == null) return false;

        for (int i = 0; i < installControl.TowerCount; i++)
        {
            var amp = installControl.GetAmplifierTower(i);
            if (amp == null || amp.AmplifierTowerData == null) continue;

            if (amp.AmplifierTowerData.BuffTowerId == target.BuffTowerId)
                return true;
        }

        if (isTutorial && Variables.Stage == 1)
        {
            if (target == damageMatrixCoreSO) return tutorialAmp1Installed;
            if (target == proejctileCoreSO) return tutorialAmp2Installed;
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

        usedAmplifierTowerTypesThisRoll.Add(ampData);

        int ampAbilityId = -1;
        int safe = 0;
        do
        {
            ampAbilityId = GetRandomAbilityForAmplifier(ampData);
            safe++;
            if (safe > 50) break;
        }
        while (ampAbilityId > 0 && IsForbiddenAmplifierCombo(ampData, ampAbilityId, isInitial));

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

    private void SetUpTutorialAttackCard(int i, int slotNumber, bool isInitial)
    {
        TowerDataSO towerData = tutorialPistolTower;
        if (towerData == null)
        {
            SetUpNewAttackCard(i, slotNumber, isInitial);
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
        while (abilityId > 0 && IsForbiddenAttackCombo(towerData, abilityId, isInitial));

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

    private void SetUpCard(int i, int slotNumber)
    {
        //Random Tower Type (0: Attack, 1: Amplifier)
        int towerType = Random.Range(0, 2);

        if (isFirstInstall) towerType = 0;

        if (towerType == 0) //Attack
        {
            var towerData = GetRandomAttackTowerDataForCard_ExcludeInstalled(usedAttackTowerTypesThisRoll);
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

            InstallNewAttackTower(i, towerData, abilityId);
            return;
        }

        if (towerType == 1)
        {
            if (isTutorial && Variables.Stage == 1)
            {
                TutorialManager.Instance.ShowTutorialStep(3);
            }

            var ampData = GetRandomAmplifierForCard(usedAmplifierTowerTypesThisRoll);

            choices[i].InstallType = TowerInstallType.Amplifier;
            choices[i].AmplifierTowerData = ampData;
            choices[i].AttackTowerData = null;

            //AmpTower Random Ability
            int ampAbilityId = GetRandomAbilityForAmplifier(ampData);
            choices[i].ability = ampAbilityId;
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

            string ampName = string.IsNullOrEmpty(ampData.BuffTowerName)
            ? ampData.AmplifierType.ToString()
            : ampData.BuffTowerName;

            string buffBlock = FormatOffsetArray(buffOffsets);
            string randomBlock = FormatOffsetArray(randomOffsets);

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
    }

    private void SetUpNewAttackCard(int i, int slotNumber, bool isInitial)
    {
        TowerDataSO towerData = GetRandomAttackTowerDataForCard_ExcludeInstalled(usedAttackTowerTypesThisRoll);
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
                List<int> filtered = new List<int>();

                foreach (int s in upgradableSlots)
                {
                    if (IsSlotReservedByOtherCard(s, i)) continue;
                    if (shownUpgradeSlotsThisRoll.Contains(s)) continue;

                    var td = installControl.GetTowerData(s);
                    var amp = installControl.GetAmplifierTower(s);

                    if (td != null && IsAttackTypeUsedThisRoll(td)) continue;
                    if (amp != null && amp.AmplifierTowerData != null && IsAmplifierTypeUsedThisRoll(amp.AmplifierTowerData))
                        continue;

                    bool forbidden = false;
                    if (td != null)
                        forbidden = IsForbiddenAttackCombo(td, UpgradeOptionAbilityId, isInitial);
                    else if (amp != null && amp.AmplifierTowerData != null)
                        forbidden = IsForbiddenAmplifierCombo(amp.AmplifierTowerData, UpgradeOptionAbilityId, isInitial);

                    if (forbidden)
                        continue;

                    filtered.Add(s);
                }

                int picked = -1;
                if (filtered.Count > 0)
                    picked = (int)PickUpgradeSlotByWeight(filtered);
                else
                    picked = (int)PickUpgradeSlotByWeight(upgradableSlots);

                if (picked != -1)
                {
                    if (numlist != null && i >= 0 && i < numlist.Count)
                        numlist[i] = picked;

                    shownUpgradeSlotsThisRoll.Add(picked);

                    var pickedTd = installControl.GetTowerData(picked);
                    if (pickedTd != null) usedAttackTowerTypesThisRoll.Add(pickedTd);

                    var pickedAmp = installControl.GetAmplifierTower(picked);
                    if (pickedAmp != null && pickedAmp.AmplifierTowerData != null)
                        usedAmplifierTowerTypesThisRoll.Add(pickedAmp.AmplifierTowerData);

                    SetUpgradeCardForUsedSlot(i, picked, isInitial);
                }

            }
            else
            {
                if (i == upgradeUIs.Length - 1 && ShouldShowGoldCardByStage())
                {
                    SetupGoldCardUI(i);
                }
                else
                {
                    SetUpNewAmplifierCard(i, slotNumber, isInitial);
                }
            }
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
        while (abilityId > 0 && IsForbiddenAttackCombo(towerData, abilityId, isInitial));

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

    private void SetUpNewAmplifierCard(int i, int slotNumber, bool isInitial)
    {
        var ampData = GetRandomAmplifierForCard(usedAmplifierTowerTypesThisRoll);
        if (ampData == null)
        {
            SetUpNewAttackCard(i, slotNumber, isInitial);
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
        while (ampAbilityId > 0 && IsForbiddenAmplifierCombo(ampData, ampAbilityId, isInitial));
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
        if (abilityId <= 0) return string.Empty;
        var row = DataTableManager.RandomAbilityTable.Get(abilityId);
        return row != null ? row.RandomAbilityName : string.Empty;
    }

    private void ResetUpgradeCard(int index, bool checkGoldCard = true)
    {
        UnregisterUsedTypeForCard(index);
        int previousSlot = (numlist != null && index >= 0 && index < numlist.Count) ? numlist[index] : -1;

        if (numlist != null && index >= 0 && index < numlist.Count) numlist[index] = -1;
        if (choices != null && index >= 0 && index < choices.Length)
        {
            choices[index].InstallType = TowerInstallType.Attack;
            choices[index].AttackTowerData = null;
            choices[index].AmplifierTowerData = null;
            choices[index].BuffSlotIndex = null;
            choices[index].RandomAbilitySlotIndex = null;
            choices[index].ability = -1;
        }

        abilities[index] = -1;
        installControl.IsReadyInstall = false;
        outlineObjects[index].SetActive(false);

        List<int> candidates = new List<int>();
        if (checkGoldCard && CanShowGoldAtIndex_NormalStage(index))
        {
            SetupGoldCardUI(index);
            return;
        }

        for (int i = 0; i < installControl.TowerCount; i++)
        {
            if (IsSlotReservedByOtherCard(i, index)) continue;
            if (i == previousSlot) continue;
            if (shownUpgradeSlotsThisRoll.Contains(i)) continue;

            bool used = installControl.IsUsedSlot(i);
            if (!used)
            {
                if (installControl.CurrentTowerCount < installControl.MaxTowerCount)
                {
                    candidates.Add(i);
                }
            }
            else
            {
                if (IsSlotUpgradeable_Strict(i))
                {
                    candidates.Add(i);
                }
            }
        }
        int usedCnt = 0, upCandCnt = 0, emptyCandCnt = 0;
        for (int t = 0; t < candidates.Count; t++)
        {
            if (installControl.IsUsedSlot(candidates[t])) { usedCnt++; upCandCnt++; }
            else emptyCandCnt++;
        }

        if(candidates.Count == 0)
        {
            if (numlist != null && index >= 0 && index < numlist.Count) numlist[index] = -1;
            SetUpNewInstallCard(index, -1, isInitial: false);
            return;
        }

        int number = -1;
        List<int> upgradeOnlySlots = new List<int>();
        List<int> nonForbiddenUpgradeSlots = new List<int>();
        List<int> preferredUpgradeSlots = new List<int>();

        foreach (int slot in candidates)
        {
            if (installControl.IsUsedSlot(slot))
                upgradeOnlySlots.Add(slot);
        }

        if (upgradeOnlySlots.Count > 0)
        {
            List<int> availableUpgradeSlots = new List<int>();
            nonForbiddenUpgradeSlots.Clear();
            preferredUpgradeSlots.Clear();

            foreach (int slot in upgradeOnlySlots)
            {
                var towerData = installControl.GetTowerData(slot);
                var ampTower = installControl.GetAmplifierTower(slot);

                bool isAlreadyUsed = false;
                bool isForbidden = false;

                if (towerData != null && IsAttackTypeUsedThisRoll(towerData))
                    isAlreadyUsed = true;

                if (ampTower != null && ampTower.AmplifierTowerData != null &&
                    IsAmplifierTypeUsedThisRoll(ampTower.AmplifierTowerData))
                    isAlreadyUsed = true;

                if (towerData != null)
                    isForbidden = IsForbiddenAttackCombo(towerData, UpgradeOptionAbilityId, isInitial: false);
                else if (ampTower != null && ampTower.AmplifierTowerData != null)
                    isForbidden = IsForbiddenAmplifierCombo(ampTower.AmplifierTowerData, UpgradeOptionAbilityId, isInitial: false);

                if (isForbidden) continue;
                if (isAlreadyUsed) continue;

                nonForbiddenUpgradeSlots.Add(slot);
                preferredUpgradeSlots.Add(slot);
                availableUpgradeSlots.Add(slot);

            }

            if (preferredUpgradeSlots.Count > 0)
                number = (int)PickUpgradeSlotByWeight(preferredUpgradeSlots);
            else if (nonForbiddenUpgradeSlots.Count > 0)
                number = (int)PickUpgradeSlotByWeight(nonForbiddenUpgradeSlots);
            else if (upgradeOnlySlots.Count > 0)
                number = (int)PickUpgradeSlotByWeight(upgradeOnlySlots);
        }


        if (number == -1)
        {
            List<int> emptySlots = new List<int>();
            foreach (int slot in candidates)
            {
                if (!installControl.IsUsedSlot(slot))
                    emptySlots.Add(slot);
            }

            if (emptySlots.Count > 0)
            {
                int slotIdx = Random.Range(0, emptySlots.Count);
                number = emptySlots[slotIdx];
            }
        }
        numlist[index] = number;

        if (number != -1 && installControl.IsUsedSlot(number))
            shownUpgradeSlotsThisRoll.Add(number);
        if (number == -1 || !installControl.IsUsedSlot(number))
        {
            if (numlist != null && index < numlist.Count) numlist[index] = -1;
            SetUpNewInstallCard(index, -1, isInitial: false);
        }
        else
        {
            var pickedTowerData = installControl.GetTowerData(number);
            if (pickedTowerData != null) usedAttackTowerTypesThisRoll.Add(pickedTowerData);

            var pickedAmpTower = installControl.GetAmplifierTower(number);
            if (pickedAmpTower != null && pickedAmpTower.AmplifierTowerData != null)
                usedAmplifierTowerTypesThisRoll.Add(pickedAmpTower.AmplifierTowerData);

            SetUpgradeCardForUsedSlot(index, number, isInitial: false);
        }

        if (!installControl.IsUsedSlot(number))
        {
            SetUpNewInstallCard(index, number, isInitial: false);
        }
        else
        {
            var pickedTowerData = installControl.GetTowerData(number);
            if (pickedTowerData != null)
            {
                usedAttackTowerTypesThisRoll.Add(pickedTowerData);
            }

            var pickedAmpTower = installControl.GetAmplifierTower(number);
            if (pickedAmpTower != null && pickedAmpTower.AmplifierTowerData != null)
            {
                usedAmplifierTowerTypesThisRoll.Add(pickedAmpTower.AmplifierTowerData);
            }
            SetUpgradeCardForUsedSlot(index, number, isInitial: false);
        }
    }
    private void SetUpgradeCardForUsedSlot(int index, int number, bool isInitial)
    {
        shownUpgradeSlotsThisRoll.Add(number);

        var towerData = installControl.GetTowerData(number);
        var ampTower = installControl.GetAmplifierTower(number);

        if (towerData != null)
        {
            choices[index].InstallType = TowerInstallType.Attack;
            choices[index].AttackTowerData = towerData;
            choices[index].AmplifierTowerData = null;
            choices[index].BuffSlotIndex = null;
            choices[index].RandomAbilitySlotIndex = null;
            choices[index].ability = UpgradeOptionAbilityId;
            abilities[index] = UpgradeOptionAbilityId;

            if (isInitial && initialOptionKeys != null && index < initialOptionKeys.Length)
                initialOptionKeys[index] = MakeKey(towerData, UpgradeOptionAbilityId);

            UpgradeTowerCard(index);
            return;
        }

        if (ampTower != null && ampTower.AmplifierTowerData != null)
        {
            choices[index].InstallType = TowerInstallType.Amplifier;
            choices[index].AttackTowerData = null;
            choices[index].AmplifierTowerData = ampTower.AmplifierTowerData;
            choices[index].BuffSlotIndex = null;
            choices[index].RandomAbilitySlotIndex = null;
            choices[index].ability = UpgradeOptionAbilityId;
            abilities[index] = UpgradeOptionAbilityId;

            if (isInitial && initialOptionKeys != null && index < initialOptionKeys.Length)
                initialOptionKeys[index] = MakeKey(ampTower.AmplifierTowerData, UpgradeOptionAbilityId);

            UpgradeTowerCard(index);

            if (isTutorial && Variables.Stage == 1 && !hasShownStep4ThisRoll)
            {
                TutorialManager.Instance.ShowTutorialStep(4);
                hasShownStep4ThisRoll = true;
            }
            return;
        }

        shownUpgradeSlotsThisRoll.Remove(number);

        if (CanInstallAny_Strict())
        {
            if (numlist != null && index < numlist.Count) numlist[index] = -1;
            SetUpNewInstallCard(index, -1, isInitial);
            return;
        }
        if (CanShowGoldAtIndex_NormalStage(index)) SetupGoldCardUI(index);
        else
        {
            DeleteAlreadyInstalledCard(index);
            if (numlist != null && index < numlist.Count) numlist[index] = -1;
        }
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

    private void RefreshTutorialStage1Card(int cardIndex)
    {
        if (choices == null || cardIndex < 0 || cardIndex >= choices.Length) return;

        outlineObjects[cardIndex].SetActive(false);

        installControl.IsReadyInstall = false;

        var choice = choices[cardIndex];

        if (choice.InstallType == TowerInstallType.Attack && choice.AttackTowerData != null)
        {
            var towerData = choice.AttackTowerData;

            int abilityId = -1;
            int safe = 0;
            do
            {
                abilityId = GetAbilityIdForAttackTower(towerData);
                safe++;
                if (safe > 20) break;
            }
            while (abilityId > 0 && IsForbiddenAttackCombo(towerData, abilityId, isInitial: false));

            abilities[cardIndex] = abilityId;
            choices[cardIndex].ability = abilityId;

            InstallNewAttackTower(cardIndex, towerData, abilityId);
        }
        else if (choice.InstallType == TowerInstallType.Amplifier && choice.AmplifierTowerData != null)
        {
            var ampData = choice.AmplifierTowerData;

            int ampAbilityId = -1;
            int safe = 0;
            do
            {
                ampAbilityId = GetRandomAbilityForAmplifier(ampData);
                safe++;
                if (safe > 20) break;
            }
            while (ampAbilityId > 0 && IsForbiddenAmplifierCombo(ampData, ampAbilityId, isInitial: false));
            FillAmplifierCardCommon(cardIndex, ampData, ampAbilityId, isInitial: false);
        }
    }

    private void DeleteAlreadyInstalledCard(int index)
    {
        if (upgradeUIs == null || index < 0 || index >= upgradeUIs.Length) return;

        var parent = upgradeUIs[index].transform;
        if (parent == null) return;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            if (child == null) continue;
            if (IsUpgradeCardInstance(child) || IsGoldCardInstance(child))
            {
                Destroy(child.gameObject);
            }

        }
    }
    private bool IsGoldCardInstance(Transform root)
    {
        if (root == null) return false;
        if (goldCardPrefab != null)
        {
            if (root.name.StartsWith(goldCardPrefab.name))
                return true;
        }
        if (root.name.Contains("GoldCard") || root.name.Contains("Gold"))
            return true;

        return false;
    }

    private bool IsUpgradeCardInstance(Transform root)
    {
        if (root == null) return false;
        if (root.GetComponentInChildren<NewAttackTowerCardUiSetting>(true) != null) return true;
        if (root.GetComponentInChildren<NewAmplifierTowerCardUiSetting>(true) != null) return true;
        if (root.GetComponentInChildren<UpgradeToweCardUiSetting>(true) != null) return true;
        return false;
    }


    public void OnClickUpgradeUIClicked(int index)
    {
        if (choices == null || index < 0 || index >= choices.Length)
            return;

        bool hasTowerData =
        choices[index].AttackTowerData != null ||
        choices[index].AmplifierTowerData != null;

        int targetSlot = (numlist != null && index < numlist.Count) ? numlist[index] : -1;

        if (hasTowerData && targetSlot < 0)
        {
            for (int i = 0; i < installControl.TowerCount; i++)
            {
                if (!installControl.IsUsedSlot(i) && installControl.CurrentTowerCount < installControl.MaxTowerCount)
                {
                    targetSlot = i;
                    if (numlist != null && index < numlist.Count)
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
            outlineObjects[i].SetActive(false);
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
        installedTower.transform.SetAsLastSibling();
        var installedTowerButton = installedTower.GetComponentInChildren<Button>();
        installedTowerButton.onClick.AddListener(() => OnClickUpgradeUIClicked(index));
        var towerUI = installedTower.GetComponent<NewAttackTowerCardUiSetting>();
        towerUI.SettingNewTowerCard(towerData.towerIdInt, abilityId);
    }

    private void InstallNewAmplifierTower(int index, AmplifierTowerDataSO ampData, int abilityId, List<int> leftIndices, List<int> rightIndices)
    {
        DeleteAlreadyInstalledCard(index);

        var installedTower = Instantiate(newAmplifierTowerCardPrefab, upgradeUIs[index].transform);
        installedTower.transform.SetAsLastSibling();
        var installedTowerButton = installedTower.GetComponentInChildren<Button>();
        installedTowerButton.onClick.AddListener(() => OnClickUpgradeUIClicked(index));
        var towerUI = installedTower.GetComponent<NewAmplifierTowerCardUiSetting>();
        towerUI.SettingNewTowerCard(ampData.BuffTowerId, abilityId, rightIndices, leftIndices);
    }

    private void UpgradeTowerCard(int index)
    {
        DeleteAlreadyInstalledCard(index);
        var upgradedTower = Instantiate(upgradeTowerCardPrefab, upgradeUIs[index].transform);
        upgradedTower.transform.SetAsLastSibling();
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

        if (!TouchManager.Instance.IsTouching || towerImageIsDraging)
            return;

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
                    break;
                }
            }

            if (!isTouchOnUpgradeCard)
            {
                isStartTouch = false;
                return;
            }
        }

        if (Vector2.Distance(initTouchPos, touchPos) < 5f) return;
        choosedIndex = -1;

        for (int i = 0; i < upgradeUIs.Length; i++)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(upgradeUIs[i].GetComponent<RectTransform>(), touchPos))
            {
                choosedIndex = i;
                Debug.Log(firstTouchIndex + " / " + choosedIndex);
                if (firstTouchIndex != choosedIndex)
                {
                    return;
                }
            }
        }

        if (choosedIndex == -1) return;

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
                        if (numlist != null && choosedIndex < numlist.Count)
                        {
                            numlist[choosedIndex] = i;
                        }
                        break;
                    }
                }
            }
        }

        if (targetSlot < 0 || installControl.IsUsedSlot(targetSlot)) return;
        if (choices != null && choosedIndex >= 0 && choosedIndex < choices.Length)
        {
            if (choices[choosedIndex].ability == UpgradeOptionAbilityId) return;
        }


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
                button.interactable = !v;
        }
    }
    public void OnTouchStateCheck()
    {
        var currentPhase = TouchManager.Instance.TouchPhase;

        if (currentPhase == InputActionPhase.Started)
            isNewTouch = true;

        if (currentPhase == InputActionPhase.Canceled)
        {
            isStartTouch = false;
            towerImageIsDraging = false;
            isNewTouch = false;

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

        bool isFull = installControl.CurrentTowerCount >= installControl.MaxTowerCount;

        for (int i = 0; i < installControl.TowerCount; i++)
        {
            if (!isFull && installControl.IsUsedSlot(i)) continue;
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    towers[i].GetComponent<RectTransform>(), touchPos))
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

        return allAmplifierTowers[idx];
    }

    private int GetAbilityIdForAttackTower(TowerDataSO towerData)
    {
        int abilityId = -1;

        if (towerData == null || towerData.randomAbilityGroupId <= 0) return -1;

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
        bool isInitial)
    {
        if (abilityId == -1) return false;

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
                if (c.ability == -1) continue;

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

        if (!isFieldFull)
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
        if (allAmplifierTowers == null || allAmplifierTowers.Length == 0) return null;

        if (isTutorial && Variables.Stage == 1)
        {
            List<AmplifierTowerDataSO> basic = new List<AmplifierTowerDataSO>();
            int max = Mathf.Min(2, allAmplifierTowers.Length);
            HashSet<int> installedIds = new HashSet<int>();
            for (int i = 0; i < installControl.TowerCount; i++)
            {
                var amp = installControl.GetAmplifierTower(i);
                if (amp != null && amp.AmplifierTowerData != null)
                    installedIds.Add(amp.AmplifierTowerData.BuffTowerId);
            }

            if (tutorialAmp1Installed && damageMatrixCoreSO != null)
                installedIds.Add(damageMatrixCoreSO.BuffTowerId);

            if (tutorialAmp2Installed && proejctileCoreSO != null)
                installedIds.Add(proejctileCoreSO.BuffTowerId);

            HashSet<int> extraIds = new HashSet<int>();
            if (extraExcludes != null)
            {
                foreach (var d in extraExcludes)
                    if (d != null) extraIds.Add(d.BuffTowerId);
            }

            for (int i = 0; i < max; i++)
            {
                var d = allAmplifierTowers[i];
                if (d == null) continue;
                if (installedIds.Contains(d.BuffTowerId)) continue;
                if (extraIds.Contains(d.BuffTowerId)) continue;
                basic.Add(d);
            }

            if (basic.Count == 0) return null;
            return basic[Random.Range(0, basic.Count)];
        }
        HashSet<AmplifierTowerDataSO> excludeSet = new HashSet<AmplifierTowerDataSO>(new AmpDataSoIdComparer());

        for (int i = 0; i < installControl.TowerCount; i++)
        {
            var amp = installControl.GetAmplifierTower(i);
            if (amp != null && amp.AmplifierTowerData != null)
            {
                excludeSet.Add(amp.AmplifierTowerData);
            }
        }

        if (extraExcludes != null)
        {
            foreach (var d in extraExcludes)
            {
                if (d != null) excludeSet.Add(d);
            }
        }

        List<AmplifierTowerDataSO> candidates = new List<AmplifierTowerDataSO>();
        foreach (var d in allAmplifierTowers)
        {
            if (d == null) continue;
            if (!excludeSet.Contains(d))
                candidates.Add(d);
        }
        if (candidates.Count == 0) return null;

        //weight pick
        if (CollectionManager.Instance == null || !CollectionManager.Instance.IsInitialized)
        {
            int colIdx = UnityEngine.Random.Range(0, candidates.Count);
            return candidates[colIdx];
        }

        List<float> weights = new List<float>();
        float totalWeight = 0f;

        foreach (var ampData in candidates)
        {
            int towerId = ampData.BuffTowerId;
            float weight = CollectionManager.Instance.GetWeight(towerId);
            weights.Add(weight);
            totalWeight += weight;
        }

        float randValue = UnityEngine.Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < candidates.Count; i++)
        {
            cumulative += weights[i];
            if (randValue <= cumulative)
            {
                return candidates[i];
            }
        }
        return candidates[candidates.Count - 1];
    }

    private bool IsAttackSlotUpgradable(int slotIndex)
    {
        var attack = installControl.GetAttackTower(slotIndex);
        if (attack == null) return false;

        return attack.ReinforceLevel < MaxReinforceLevel;
    }

    private void SetUpNewInstallCard(int i, int slotNumber, bool isInitial)
    {
        if (GetTotalTowerCount() == 0)
        {
            SetUpNewAttackCard(i, slotNumber, isInitial);
            return;
        }

        bool hasAttackCandidate = HasAnyNewAttackTowerCandidate();
        bool hasAmpCandidate = HasAnyAmplifierCandidateForCard();
        if (!hasAttackCandidate && !hasAmpCandidate)
        {
            SetupGoldCardUI(i);
            return;
        }

        int towerType = 0;

        if (hasAttackCandidate && hasAmpCandidate)
        {
            towerType = Random.Range(0, 2); // 0: Attack, 1: Amplifier
        }
        else if (hasAmpCandidate)
        {
            towerType = 1;
        }
        else
        {
            towerType = 0;
        }

        if (towerType == 0)
            SetUpNewAttackCard(i, slotNumber, isInitial);
        else
            SetUpNewAmplifierCard(i, slotNumber, isInitial);
    }

    private bool HasAnyNewAttackTowerCandidate()
    {
        if (Variables.Stage != 1)
            return GetRandomAttackTowerDataForCard_ExcludeInstalled(usedAttackTowerTypesThisRoll) != null;

        return GetRandomNewAttackTowerDataForStage1(usedAttackTowerTypesThisRoll) != null;
    }
    private bool HasAnyAmplifierCandidateForCard()
    {
        if (allAmplifierTowers == null || allAmplifierTowers.Length == 0) return false;

        return GetRandomAmplifierForCard(usedAmplifierTowerTypesThisRoll) != null;
    }

    //weight pick
    private float PickUpgradeSlotByWeight(List<int> upgradeSlots)
    {
        if (upgradeSlots == null || upgradeSlots.Count == 0)
        {
            return -1f;
        }

        if (CollectionManager.Instance == null || !CollectionManager.Instance.IsInitialized)
        {
            int randIdx = UnityEngine.Random.Range(0, upgradeSlots.Count);
            return upgradeSlots[randIdx];
        }

        List<float> weights = new List<float>();
        float totalWeight = 0f;

        foreach (int slotIdx in upgradeSlots)
        {
            float weight = 0f;

            var attackTowerData = installControl.GetTowerData(slotIdx);
            if (attackTowerData != null)
            {
                weight = CollectionManager.Instance.GetWeight(attackTowerData.towerIdInt);
            }
            else
            {
                var ampTower = installControl.GetAmplifierTower(slotIdx);
                if (ampTower != null && ampTower.AmplifierTowerData != null)
                {
                    weight = CollectionManager.Instance.GetWeight(ampTower.AmplifierTowerData.BuffTowerId);
                }
            }
            weights.Add(weight);
            totalWeight += weight;
        }

        if (totalWeight <= 0)
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
        if (IsStage1AllGoldState())
        {
            UnregisterUsedTypeForCard(index);
            int prev = (numlist != null && index >= 0 && index < numlist.Count) ? numlist[index] : -1;
            if (prev != -1) shownUpgradeSlotsThisRoll.Remove(prev);
            UnmarkStage1UpgradeTypeShown(prev);

            SetupGoldCardUI(index);
            return;
        }

        bool canInstall = installControl.CurrentTowerCount < installControl.MaxTowerCount;
        var missing = GetStage1MissingTypes();
        int previousSlot = (numlist != null && index >= 0 && index < numlist.Count) ? numlist[index] : -1;
        if (previousSlot != -1 && installControl.IsUsedSlot(previousSlot))
        {
            UnmarkStage1UpgradeTypeShown(previousSlot);
        }

        UnregisterUsedTypeForCard(index);

        if (previousSlot != -1) shownUpgradeSlotsThisRoll.Remove(previousSlot);
        if (numlist != null && index >= 0 && index < numlist.Count) numlist[index] = -1;

        if (choices != null && index >= 0 && index < choices.Length)
        {
            choices[index].InstallType = TowerInstallType.Attack;
            choices[index].AttackTowerData = null;
            choices[index].AmplifierTowerData = null;
            choices[index].BuffSlotIndex = null;
            choices[index].RandomAbilitySlotIndex = null;
            choices[index].ability = -1;
        }
        if (abilities != null && index >= 0 && index < abilities.Length) abilities[index] = -1;

        installControl.IsReadyInstall = false;
        outlineObjects[index].SetActive(false);

        if (IsStage1FirstRollPistolOnly())
        {
            if (numlist != null && index < numlist.Count) numlist[index] = -1;
            SetUpTutorialAttackCard(index, -1, isInitial: false);
            return;
        }

        List<int> upgradeSlots = new List<int>();
        for (int s = 0; s < installControl.TowerCount; s++)
        {
            if (IsSlotReservedByOtherCard(s, index)) continue;
            if (s == previousSlot) continue;
            if (shownUpgradeSlotsThisRoll.Contains(s)) continue;

            if (installControl.IsUsedSlot(s) && IsSlotUpgradeable_Strict(s))
            {
                if (TryGetStage1UpgradeTypeKey(s, out bool isAttack, out int typeId))
                {
                    if (isAttack)
                    {
                        if (stage1ShownUpgradeAttackIdsThisRoll.Contains(typeId)) continue;
                    }
                    else
                    {
                        if (stage1ShownUpgradeAmpIdsThisRoll.Contains(typeId)) continue;
                    }
                }
                upgradeSlots.Add(s);
            }
        }

        if (upgradeSlots.Count > 0)
        {
            List<int> preferred = new List<int>();
            List<int> nonForbidden = new List<int>();

            foreach (int slot in upgradeSlots)
            {
                var td = installControl.GetTowerData(slot);
                var amp = installControl.GetAmplifierTower(slot);

                bool isAlreadyUsedThisRoll = false;
                if (td != null && IsAttackTypeUsedThisRoll(td)) isAlreadyUsedThisRoll = true;
                if (amp != null && amp.AmplifierTowerData != null && IsAmplifierTypeUsedThisRoll(amp.AmplifierTowerData))
                    isAlreadyUsedThisRoll = true;

                bool forbidden = false;
                if (td != null)
                    forbidden = IsForbiddenAttackCombo(td, UpgradeOptionAbilityId, isInitial: false);
                else if (amp != null && amp.AmplifierTowerData != null)
                    forbidden = IsForbiddenAmplifierCombo(amp.AmplifierTowerData, UpgradeOptionAbilityId, isInitial: false);

                if (forbidden) continue;

                nonForbidden.Add(slot);
                if (!isAlreadyUsedThisRoll)
                    preferred.Add(slot);
            }

            int pickedSlot = -1;
            if (preferred.Count > 0) pickedSlot = (int)PickUpgradeSlotByWeight(preferred);
            else if (nonForbidden.Count > 0) pickedSlot = (int)PickUpgradeSlotByWeight(nonForbidden);
            else pickedSlot = (int)PickUpgradeSlotByWeight(upgradeSlots);

            if (pickedSlot != -1)
            {
                if (numlist != null && index < numlist.Count) numlist[index] = pickedSlot;
                shownUpgradeSlotsThisRoll.Add(pickedSlot);
                MarkStage1UpgradeTypeShown(pickedSlot);

                var pickedTd = installControl.GetTowerData(pickedSlot);
                if (pickedTd != null) usedAttackTowerTypesThisRoll.Add(pickedTd);

                var pickedAmp = installControl.GetAmplifierTower(pickedSlot);
                if (pickedAmp != null && pickedAmp.AmplifierTowerData != null)
                    usedAmplifierTowerTypesThisRoll.Add(pickedAmp.AmplifierTowerData);

                SetUpgradeCardForUsedSlot(index, pickedSlot, isInitial: false);
                return;
            }
        }

        if (canInstall && missing.Count > 0)
        {
            List<int> emptySlots = new List<int>();
            for (int s = 0; s < installControl.TowerCount; s++)
            {
                if (IsSlotReservedByOtherCard(s, index)) continue;
                if (!installControl.IsUsedSlot(s))
                    emptySlots.Add(s);
            }

            if (emptySlots.Count > 0)
            {
                if (IsChoiceStillMissingTutorialType(index, missing))
                {
                    RefreshTutorialStage1Card(index);
                    return;
                }

                var t = missing[Random.Range(0, missing.Count)];
                int slotNumber = emptySlots[Random.Range(0, emptySlots.Count)];

                if (numlist != null && index < numlist.Count) numlist[index] = slotNumber;
                shownUpgradeSlotsThisRoll.Add(slotNumber);

                SetupStage1InstallCard(index, t, isInitial: false);
                return;
            }
        }

        if (installControl.CurrentTowerCount < installControl.MaxTowerCount)
        {
            int slotNumber = -1;
            for (int s = 0; s < installControl.TowerCount; s++)
            {
                if (IsSlotReservedByOtherCard(s, index)) continue;
                if (!installControl.IsUsedSlot(s))
                {
                    slotNumber = s;
                    break;
                }
            }

            if (slotNumber != -1)
            {
                if (numlist != null && index < numlist.Count) numlist[index] = slotNumber;
                shownUpgradeSlotsThisRoll.Add(slotNumber);
                SetUpNewInstallCard(index, slotNumber, isInitial: false);
                return;
            }
        }
        SetupGoldCardUI(index);
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

    private bool IsSlotReservedByOtherCard(int slot, int myCardIndex)
    {
        if (slot < 0) return false;
        if (numlist == null) return false;

        for (int k = 0; k < numlist.Count; k++)
        {
            if (k == myCardIndex) continue;
            if (numlist[k] == slot) return true;
        }
        return false;
    }

    private bool IsAttackTypeUsedByOtherCards(int towerIdInt, int ignoreIndex)
    {
        if (choices == null) return false;

        for (int i = 0; i < choices.Length; i++)
        {
            if (i == ignoreIndex) continue;

            var c = choices[i];
            if (c == null) continue;

            if (c.InstallType == TowerInstallType.Attack &&
                c.AttackTowerData != null &&
                c.AttackTowerData.towerIdInt == towerIdInt)
                return true;
        }
        return false;
    }

    private bool IsAmplifierTypeUsedByOtherCards(int buffTowerId, int ignoreIndex)
    {
        if (choices == null) return false;

        for (int i = 0; i < choices.Length; i++)
        {
            if (i == ignoreIndex) continue;

            var c = choices[i];
            if (c == null) continue;

            if (c.InstallType == TowerInstallType.Amplifier &&
                c.AmplifierTowerData != null &&
                c.AmplifierTowerData.BuffTowerId == buffTowerId)
                return true;
        }
        return false;
    }

    private void RemoveAttackTypeFromUsedSetById(int towerIdInt)
    {
        if (usedAttackTowerTypesThisRoll == null) return;

        TowerDataSO found = null;
        foreach (var td in usedAttackTowerTypesThisRoll)
        {
            if (td != null && td.towerIdInt == towerIdInt)
            {
                found = td;
                break;
            }
        }
        if (found != null) usedAttackTowerTypesThisRoll.Remove(found);
    }

    private void RemoveAmplifierTypeFromUsedSetById(int buffTowerId)
    {
        if (usedAmplifierTowerTypesThisRoll == null) return;

        AmplifierTowerDataSO found = null;
        foreach (var ad in usedAmplifierTowerTypesThisRoll)
        {
            if (ad != null && ad.BuffTowerId == buffTowerId)
            {
                found = ad;
                break;
            }
        }
        if (found != null) usedAmplifierTowerTypesThisRoll.Remove(found);
    }

    private void UnregisterUsedTypeForCard(int cardIndex)
    {
        if (choices == null || cardIndex < 0 || cardIndex >= choices.Length) return;

        var c = choices[cardIndex];
        if (c == null) return;

        if (c.InstallType == TowerInstallType.Attack && c.AttackTowerData != null)
        {
            int id = c.AttackTowerData.towerIdInt;
            if (!IsAttackTypeUsedByOtherCards(id, cardIndex))
            {
                RemoveAttackTypeFromUsedSetById(id);
            }
        }
        else if (c.InstallType == TowerInstallType.Amplifier && c.AmplifierTowerData != null)
        {
            int id = c.AmplifierTowerData.BuffTowerId;
            if (!IsAmplifierTypeUsedByOtherCards(id, cardIndex))
            {
                RemoveAmplifierTypeFromUsedSetById(id);
            }
        }
    }

    private void SetupGoldCardUI(int index)
    {
        DeleteAlreadyInstalledCard(index);

        if (goldCardPrefab != null)
        {
            var goldObj = Instantiate(goldCardPrefab, upgradeUIs[index].transform);
            goldObj.transform.SetAsLastSibling();
            BindGoldCardClick(goldObj, index);
        }

        if (choices != null && index >= 0 && index < choices.Length)
        {
            choices[index].InstallType = TowerInstallType.Attack;
            choices[index].AttackTowerData = null;
            choices[index].AmplifierTowerData = null;
            choices[index].BuffSlotIndex = null;
            choices[index].RandomAbilitySlotIndex = null;
            choices[index].ability = -1;
        }
        if (abilities != null && index >= 0 && index < abilities.Length) abilities[index] = -1;
        if (numlist != null && index >= 0 && index < numlist.Count) numlist[index] = -1;

        var btn = upgradeUIs[index].GetComponentInChildren<Button>(true);
        if (btn != null) btn.interactable = true;
    }

    private void BindGoldCardClick(GameObject goldObj, int index)
    {
        var rootBtn = goldObj != null ? goldObj.GetComponent<Button>() : null;
        var childBtns = goldObj != null ? goldObj.GetComponentsInChildren<Button>(true) : null;
        var slotBtn = (upgradeUIs != null && index >= 0 && index < upgradeUIs.Length)
            ? upgradeUIs[index].GetComponent<Button>()
            : null;

        int bindCount = 0;

        void Bind(Button b, string tag)
        {
            if (b == null) return;
            b.onClick.RemoveAllListeners();
            b.onClick.AddListener(() => OnClickGoldCard(index));
            b.interactable = true;
            bindCount++;
        }
        Bind(rootBtn, "root");
        if (childBtns != null)
        {
            foreach (var b in childBtns)
                Bind(b, "child");
        }
        Bind(slotBtn, "slot");
    }
    private void OnClickGoldCard(int index)
    {
        var battleUI = FindObjectOfType<BattleUI>();
        if (battleUI != null)
            battleUI.AddCoinGainText(goldCardRewardAmount);


        if (WaveManager.Instance != null)
            WaveManager.Instance.AddAccumulateGold(goldCardRewardAmount);

        if (towerInfoUI != null) towerInfoUI.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
    private bool IsAttackTypeUsedThisRoll(TowerDataSO towerData)
    {
        if (towerData == null) return false;
        int id = towerData.towerIdInt;

        foreach (var td in usedAttackTowerTypesThisRoll)
        {
            if (td != null && td.towerIdInt == id)
                return true;
        }
        return false;
    }

    private bool IsAmplifierTypeUsedThisRoll(AmplifierTowerDataSO ampData)
    {
        if (ampData == null) return false;
        int id = ampData.BuffTowerId;

        foreach (var ad in usedAmplifierTowerTypesThisRoll)
        {
            if (ad != null && ad.BuffTowerId == id)
                return true;
        }
        return false;
    }

    private void AddUsedAmplifierType(AmplifierTowerDataSO ampData)
    {
        if (ampData == null) return;
        if (IsAmplifierTypeUsedThisRoll(ampData)) return;
        usedAmplifierTowerTypesThisRoll.Add(ampData);
    }

    private bool IsStage1AllGoldState()
    {
        if (Variables.Stage != 1) return false;

        bool canInstall = CanInstallAny_Strict();
        int upgradable = GetUpgradeableCount_Strict();
        return !canInstall && upgradable <= 0 && installControl.CurrentTowerCount > 0;
    }

    private enum Stage1TutorialType
    {
        Pistol,
        AmpDamageMatrix,
        AmpProjectileCore
    }

    private List<Stage1TutorialType> GetStage1MissingTypes()
    {
        var list = new List<Stage1TutorialType>(3);

        if (!HasTowerTypeInstalled(tutorialPistolTower))
            list.Add(Stage1TutorialType.Pistol);

        if (!HasAmplifierInstalled(damageMatrixCoreSO))
            list.Add(Stage1TutorialType.AmpDamageMatrix);

        if (!HasAmplifierInstalled(proejctileCoreSO))
            list.Add(Stage1TutorialType.AmpProjectileCore);

        return list;
    }

    private void SetupStage1InstallCard(int cardIndex, Stage1TutorialType type, bool isInitial)
    {
        if (numlist != null && cardIndex >= 0 && cardIndex < numlist.Count)
            numlist[cardIndex] = -1;

        switch (type)
        {
            case Stage1TutorialType.Pistol:
                SetUpTutorialAttackCard(cardIndex, -1, isInitial);
                break;

            case Stage1TutorialType.AmpDamageMatrix:
                SetUpTutorialAmplifierCard(cardIndex, -1, damageMatrixCoreSO, isInitial);
                break;

            case Stage1TutorialType.AmpProjectileCore:
                SetUpTutorialAmplifierCard(cardIndex, -1, proejctileCoreSO, isInitial);
                break;
        }
    }

    private bool IsChoiceStillMissingTutorialType(int cardIndex, List<Stage1TutorialType> missing)
    {
        if (missing != null && missing.Count >= 2) return false;
        if (choices == null || cardIndex < 0 || cardIndex >= choices.Length) return false;
        var c = choices[cardIndex];

        if (c == null) return false;

        if (c.InstallType == TowerInstallType.Attack &&
            c.AttackTowerData == tutorialPistolTower)
            return missing.Contains(Stage1TutorialType.Pistol);

        if (c.InstallType == TowerInstallType.Amplifier && c.AmplifierTowerData != null)
        {
            if (c.AmplifierTowerData == damageMatrixCoreSO)
                return missing.Contains(Stage1TutorialType.AmpDamageMatrix);

            if (c.AmplifierTowerData == proejctileCoreSO)
                return missing.Contains(Stage1TutorialType.AmpProjectileCore);
        }
        return false;
    }

    private bool IsStage1FirstRollPistolOnly()
    {
        if (Variables.Stage != 1) return false;
        return installControl.CurrentTowerCount == 0
               && !HasTowerTypeInstalled(tutorialPistolTower)
               && (installControl.CurrentTowerCount < installControl.MaxTowerCount);
    }

    private TowerDataSO GetRandomNewAttackTowerDataForStage1(HashSet<TowerDataSO> extraExcludes = null)
    {
        if (Variables.Stage != 1)
            return installControl.GetRandomAttackTowerDataForCard(extraExcludes);

        const int SAFE_LIMIT = 200;

        for (int safe = 0; safe < SAFE_LIMIT; safe++)
        {
            var candidate = installControl.GetRandomAttackTowerDataForCard(extraExcludes);
            if (candidate == null) return null;
            if (HasTowerTypeInstalled(candidate))
                continue;

            return candidate;
        }
        return null;
    }

    private bool IsSlotUpgradeable_Strict(int slot)
    {
        if (slot < 0 || slot >= installControl.TowerCount) return false;
        if (!installControl.IsUsedSlot(slot)) return false;

        // Attack
        var attack = installControl.GetAttackTower(slot);
        if (attack != null)
            return attack.ReinforceLevel < MaxReinforceLevel;

        // Amplifier
        var amp = installControl.GetAmplifierTower(slot);
        if (amp != null)
            return amp.ReinforceLevel < MaxReinforceLevel;

        return false;
    }

    private int GetUpgradeableCount_Strict()
    {
        int count = 0;
        for (int s = 0; s < installControl.TowerCount; s++)
        {
            if (IsSlotUpgradeable_Strict(s)) count++;
        }
        return count;
    }

    private bool CanInstallAny_Strict()
    {
        if (installControl.CurrentTowerCount >= installControl.MaxTowerCount) return false;

        for (int s = 0; s < installControl.TowerCount; s++)
        {
            if (!installControl.IsUsedSlot(s))
                return true;
        }
        return false;
    }

    private bool TryGetSlotTowerTypeKey(int slot, out TowerInstallType type, out int towerKey)
    {
        type = TowerInstallType.Attack;
        towerKey = -1;

        if (slot < 0 || slot >= installControl.TowerCount) return false;
        if (!installControl.IsUsedSlot(slot)) return false;

        var td = installControl.GetTowerData(slot);
        if (td != null)
        {
            type = TowerInstallType.Attack;
            towerKey = td.towerIdInt;
            return true;
        }

        var amp = installControl.GetAmplifierTower(slot);
        if (amp != null && amp.AmplifierTowerData != null)
        {
            type = TowerInstallType.Amplifier;
            towerKey = amp.AmplifierTowerData.BuffTowerId;
            return true;
        }

        return false;
    }

    private bool IsTowerTypeAlreadyUsedThisRoll_ForUpgradeSlot(int slot)
    {
        if (!TryGetSlotTowerTypeKey(slot, out var type, out var key)) return false;

        if (type == TowerInstallType.Attack)
        {
            foreach (var td in usedAttackTowerTypesThisRoll)
            {
                if (td != null && td.towerIdInt == key) return true;
            }
            return false;
        }
        else
        {
            foreach (var ad in usedAmplifierTowerTypesThisRoll)
            {
                if (ad != null && ad.BuffTowerId == key) return true;
            }
            return false;
        }
    }
    private List<int> BuildUpgradeableSlotCandidates_Strict(int myCardIndex, int previousSlot, bool stage1NoDuplicateType)
    {
        var list = new List<int>();

        for (int s = 0; s < installControl.TowerCount; s++)
        {
            if (IsSlotReservedByOtherCard(s, myCardIndex)) continue;
            if (s == previousSlot) continue;
            if (shownUpgradeSlotsThisRoll.Contains(s)) continue;

            if (!IsSlotUpgradeable_Strict(s)) continue;

            if (stage1NoDuplicateType && IsTowerTypeAlreadyUsedThisRoll_ForUpgradeSlot(s))
                continue;

            list.Add(s);
        }

        return list;
    }
    private bool TryGetStage1UpgradeTypeKey(int slot, out bool isAttack, out int typeId)
    {
        isAttack = true;
        typeId = -1;

        if (slot < 0 || slot >= installControl.TowerCount) return false;
        if (!installControl.IsUsedSlot(slot)) return false;

        var td = installControl.GetTowerData(slot);
        if (td != null)
        {
            isAttack = true;
            typeId = td.towerIdInt;
            return true;
        }

        var amp = installControl.GetAmplifierTower(slot);
        if (amp != null && amp.AmplifierTowerData != null)
        {
            isAttack = false;
            typeId = amp.AmplifierTowerData.BuffTowerId;
            return true;
        }

        return false;
    }

    private List<int> BuildStage1UniqueUpgradeSlots(List<int> upgradeSlots)
    {
        var groups = new Dictionary<string, List<int>>();

        foreach (var slot in upgradeSlots)
        {
            if (!TryGetStage1UpgradeTypeKey(slot, out bool isAttack, out int id)) continue;
            if (!IsSlotUpgradeable_Strict(slot)) continue;
            if (isAttack)
            {
                if (stage1ShownUpgradeAttackIdsThisRoll.Contains(id)) continue;
            }
            else
            {
                if (stage1ShownUpgradeAmpIdsThisRoll.Contains(id)) continue;
            }

            string key = (isAttack ? "A:" : "M:") + id;
            if (!groups.TryGetValue(key, out var list))
            {
                list = new List<int>();
                groups[key] = list;
            }
            list.Add(slot);
        }

        var result = new List<int>();
        foreach (var kv in groups)
        {
            var slots = kv.Value;
            if (slots == null || slots.Count == 0) continue;
            int picked = (int)PickUpgradeSlotByWeight(slots);
            if (picked >= 0) result.Add(picked);
        }
        return result;
    }

    private void MarkStage1UpgradeTypeShown(int slot)
    {
        if (!TryGetStage1UpgradeTypeKey(slot, out bool isAttack, out int id)) return;

        if (isAttack) stage1ShownUpgradeAttackIdsThisRoll.Add(id);
        else stage1ShownUpgradeAmpIdsThisRoll.Add(id);
    }

    private void UnmarkStage1UpgradeTypeShown(int slot)
    {
        if (!TryGetStage1UpgradeTypeKey(slot, out bool isAttack, out int id)) return;

        if (isAttack) stage1ShownUpgradeAttackIdsThisRoll.Remove(id);
        else stage1ShownUpgradeAmpIdsThisRoll.Remove(id);
    }
    private void SettingNormalStageCards_Shuffled(bool showGold)
    {
        int len = upgradeUIs.Length;
        int goldIndex = len - 1;

        for (int i = 0; i < len; i++)
        {
            outlineObjects[i].SetActive(false);
            DeleteAlreadyInstalledCard(i);
            numlist[i] = -1;

            if (choices != null && i < choices.Length)
            {
                choices[i].InstallType = TowerInstallType.Attack;
                choices[i].AttackTowerData = null;
                choices[i].AmplifierTowerData = null;
                choices[i].BuffSlotIndex = null;
                choices[i].RandomAbilitySlotIndex = null;
                choices[i].ability = -1;
            }
            if (abilities != null && i < abilities.Length)
                abilities[i] = -1;
        }

        List<int> cardIndices = new List<int>(len);
        for (int i = 0; i < len; i++)
        {
            if (showGold && i == goldIndex) continue;
            cardIndices.Add(i);
        }

        for (int i = cardIndices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (cardIndices[i], cardIndices[j]) = (cardIndices[j], cardIndices[i]);
        }

        List<int> upgradableSlotsAll = new List<int>();
        List<int> emptySlots = new List<int>();

        for (int s = 0; s < installControl.TowerCount; s++)
        {
            if (installControl.IsUsedSlot(s))
            {
                if (IsSlotUpgradeable_Strict(s))
                    upgradableSlotsAll.Add(s);
            }
            else
            {
                if (installControl.CurrentTowerCount < installControl.MaxTowerCount)
                    emptySlots.Add(s);
            }
        }

        List<int> upgradableUniqueByType = BuildUniqueUpgradeSlotsByType(upgradableSlotsAll);
        HashSet<string> pickedUpgradeTypeKeys = new HashSet<string>();

        GetNewUpgradeProbabilities(out float newProb, out float upgradeProb);
        foreach (int cardIdx in cardIndices)
        {
            bool canUpgrade = upgradableSlotsAll.Count > 0;
            bool canInstall = CanInstallAny_Strict();
            bool pickUpgrade;
            if (canUpgrade && canInstall) pickUpgrade = (Random.value < upgradeProb);
            else pickUpgrade = canUpgrade;
            if (!pickUpgrade)
            {
                if (numlist != null && cardIdx < numlist.Count) numlist[cardIdx] = -1;
                SetUpNewInstallCard(cardIdx, -1, isInitial: true);
                CacheInitialOptionKey(cardIdx);
                continue;
            }

            int pickedSlot;
            bool picked = TryPickUpgradeSlot_UniqueType(
                upgradableUniqueByType,
                upgradableSlotsAll,
                pickedUpgradeTypeKeys,
                out pickedSlot);

            if (!picked || pickedSlot < 0)
            {
                if (numlist != null && cardIdx < numlist.Count) numlist[cardIdx] = -1;
                SetUpNewInstallCard(cardIdx, -1, isInitial: true);
                CacheInitialOptionKey(cardIdx);
                continue;
            }

            if (TryGetUpgradeTypeKey(pickedSlot, out var typeKey))
                pickedUpgradeTypeKeys.Add(typeKey);

            if (numlist != null && cardIdx < numlist.Count) numlist[cardIdx] = pickedSlot;
            shownUpgradeSlotsThisRoll.Add(pickedSlot);

            var td = installControl.GetTowerData(pickedSlot);
            if (td != null) usedAttackTowerTypesThisRoll.Add(td);

            var amp = installControl.GetAmplifierTower(pickedSlot);
            if (amp != null && amp.AmplifierTowerData != null)
                usedAmplifierTowerTypesThisRoll.Add(amp.AmplifierTowerData);

            SetUpgradeCardForUsedSlot(cardIdx, pickedSlot, isInitial: true);
            CacheInitialOptionKey(cardIdx);
        }
        if (CanShowGoldAtIndex_NormalStage(goldIndex))
        {
            SetupGoldCardUI(goldIndex);
            CacheInitialOptionKey(goldIndex);
        }

    }

    private bool TryGetUpgradeTypeKey(int slot, out string key)
    {
        key = null;
        if (!TryGetSlotTowerTypeKey(slot, out var type, out var towerKey)) return false;
        key = (type == TowerInstallType.Attack ? "A:" : "M:") + towerKey;
        return true;
    }

    private List<int> BuildUniqueUpgradeSlotsByType(List<int> upgradeSlots)
    {
        var groups = new Dictionary<string, List<int>>();

        foreach (var slot in upgradeSlots)
        {
            if (!IsSlotUpgradeable_Strict(slot)) continue;
            if (!TryGetUpgradeTypeKey(slot, out var key)) continue;

            if (!groups.TryGetValue(key, out var list))
            {
                list = new List<int>();
                groups[key] = list;
            }
            list.Add(slot);
        }

        var result = new List<int>();
        foreach (var kv in groups)
        {
            var slots = kv.Value;
            if (slots == null || slots.Count == 0) continue;

            int picked = (int)PickUpgradeSlotByWeight(slots);
            if (picked >= 0) result.Add(picked);
        }
        return result;
    }
    private sealed class TowerDataSoIdComparer : IEqualityComparer<TowerDataSO>
    {
        public bool Equals(TowerDataSO a, TowerDataSO b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            return a.towerIdInt == b.towerIdInt;
        }
        public int GetHashCode(TowerDataSO obj) => obj == null ? 0 : obj.towerIdInt;
    }

    private sealed class AmpDataSoIdComparer : IEqualityComparer<AmplifierTowerDataSO>
    {
        public bool Equals(AmplifierTowerDataSO a, AmplifierTowerDataSO b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            return a.BuffTowerId == b.BuffTowerId;
        }
        public int GetHashCode(AmplifierTowerDataSO obj) => obj == null ? 0 : obj.BuffTowerId;
    }

    private void RebuildInstalledAttackTowerTypeCache()
    {
        installedAttackTowerTypesCache.Clear();
        for (int i = 0; i < installControl.TowerCount; i++)
        {
            if (!installControl.IsUsedSlot(i)) continue;
            var td = installControl.GetTowerData(i);
            if (td != null) installedAttackTowerTypesCache.Add(td);
        }
        if (isTutorial && Variables.Stage == 1 && tutorialPistolInstalled && tutorialPistolTower != null)
        {
            installedAttackTowerTypesCache.Add(tutorialPistolTower);
        }
    }

    private TowerDataSO GetRandomAttackTowerDataForCard_ExcludeInstalled(ICollection<TowerDataSO> extraExcludes = null)
    {
        var exclude = new HashSet<TowerDataSO>(installedAttackTowerTypesCache, new TowerDataSoIdComparer());

        if (extraExcludes != null)
        {
            foreach (var t in extraExcludes)
                if (t != null) exclude.Add(t);
        }
        return installControl.GetRandomAttackTowerDataForCard(exclude);
    }
    private bool TryPickUpgradeSlot_UniqueType(
    List<int> upgradableUniqueByType,
    List<int> upgradableAll,
    HashSet<string> pickedTypeKeys,
    out int pickedSlot)
    {
        pickedSlot = -1;

        List<int> filtered = new List<int>();
        foreach (var s in upgradableUniqueByType)
        {
            if (TryGetUpgradeTypeKey(s, out var k) && !pickedTypeKeys.Contains(k))
                filtered.Add(s);
        }

        if (filtered.Count > 0)
        {
            pickedSlot = (int)PickUpgradeSlotByWeight(filtered);
            upgradableUniqueByType.Remove(pickedSlot);
            return true;
        }
        filtered.Clear();
        foreach (var s in upgradableAll)
        {
            if (TryGetUpgradeTypeKey(s, out var k) && !pickedTypeKeys.Contains(k))
                filtered.Add(s);
        }

        if (filtered.Count > 0)
        {
            pickedSlot = (int)PickUpgradeSlotByWeight(filtered);
            return true;
        }
        return false;
    }
    private int GoldIndex => (upgradeUIs != null && upgradeUIs.Length > 0) ? (upgradeUIs.Length - 1) : 2;
    private bool ShouldShowGoldCard_NormalStrict()
    {
        if (installControl == null) return false;
        if (installControl.CurrentTowerCount <= 0) return false;

        bool isFieldFull = installControl.CurrentTowerCount >= installControl.MaxTowerCount;
        bool hasAnyUpgradable = GetUpgradeableCount_Strict() > 0;

        return isFieldFull && !hasAnyUpgradable;
    }

    private bool CanShowGoldAtIndex_NormalStage(int cardIndex)
    {
        if (Variables.Stage == 1) return ShouldShowGoldCard_Stage1(); 
        return (cardIndex == GoldIndex) && ShouldShowGoldCard_NormalStrict();
    }
}