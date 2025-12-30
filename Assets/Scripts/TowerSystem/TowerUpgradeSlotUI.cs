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
    private HashSet<int> shownUpgradeSlotsThisRoll = new HashSet<int>();

    private List<bool> usedRefreshButton;
    private const int MaxReinforceLevel = 4;

    [SerializeField] private PlanetTowerUI planetTowerUI;
    private bool hasInitializedForStage1 = false;
    private const int UpgradeOptionAbilityId = 0;
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
        installControl.OnTowerInstalled += SetTowerInstallText;
        SetIsTutorial(TutorialManager.Instance.IsTutorialMode);
        if (isTutorial && Variables.Stage == 1)
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

        if (isTutorial && Variables.Stage == 1)
        {
            TutorialManager.Instance.ShowTutorialStep(1);
        }
        if (isTutorial && Variables.Stage == 2)
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

    private bool IsPointerDown_V2()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) return true;
        return false;
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
        numlist = new List<int>(upgradeUIs.Length);
        for (int i = 0; i < upgradeUIs.Length; i++) numlist.Add(-1);

        usedAttackTowerTypesThisRoll.Clear();
        usedAmplifierTowerTypesThisRoll.Clear();
        shownUpgradeSlotsThisRoll.Clear();
        initialOptionKeys = new TowerOptionKey[upgradeUIs.Length];

        int totalTowerCount = GetTotalTowerCount();

        if (Variables.Stage == 1)
        {
            SettingStage1Cards(totalTowerCount);
            return;
        }

        bool showGold = ShouldShowGoldCard();
        int goldIndex = upgradeUIs.Length - 1;
        for (int i = 0; i < upgradeUIs.Length; i++)
        {
            if (showGold && i == goldIndex) continue;
            ResetUpgradeCard(i, checkGoldCard: false);
            CacheInitialOptionKey(i);
        }
        if (showGold)
        {
            UnregisterUsedTypeForCard(goldIndex);
            shownUpgradeSlotsThisRoll.Remove(numlist[goldIndex]);
            numlist[goldIndex] = -1;

            SetupGoldCardUI(goldIndex);
            CacheInitialOptionKey(goldIndex);
        }
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

    private void ApplyCardPlans_UI_V2(SlotCardPlan[] plans)
    {
        for (int i = 0; i < plans.Length; i++)
        {
            switch (plans[i].type)
            {
                case SlotCardType.Empty:

                    SetupEmptyCardUI(i);
                    break;

                case SlotCardType.Gold:
                    // ✅ 2번 방식
                    SetupGoldCardUI(i);
                    break;

                case SlotCardType.Upgrade:
                    // TODO: 기존 업그레이드 카드 UI 세팅(네 코드 호출)
                    // SetupUpgradeCardUI(i, plans[i].towerId, plans[i].upgradeId, plans[i].payload);
                    break;

                case SlotCardType.InstallTower:
                    // TODO: 기존 설치 카드 UI 세팅(네 코드 호출)
                    // SetupInstallTowerCardUI(i, plans[i].payload);
                    break;
            }
        }

        // 필요하면 outlineObjects 처리도 여기서 통일
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
                if (!installControl.IsSlotMaxLevel(i))
                    upgradeSlots.Add(i);
            }
        }

        if (ShouldShowGoldCard())
        {
            shownUpgradeSlotsThisRoll.Clear();

            for (int cardIndex = 0; cardIndex < upgradeUIs.Length; cardIndex++)
            {
                numlist[cardIndex] = -1;

                if (cardIndex == upgradeUIs.Length - 1) SetupGoldCardUI(cardIndex);
                else SetupEmptyCardUI(cardIndex);
            }
            return;
        }

        bool hasPistol = HasTowerTypeInstalled(tutorialPistolTower);
        bool hasAmp1 = HasAmplifierInstalled(damageMatrixCoreSO);
        bool hasAmp2 = HasAmplifierInstalled(proejctileCoreSO);

        List<AmplifierTowerDataSO> remainingAmps = new List<AmplifierTowerDataSO>();
        if (!hasAmp1 && damageMatrixCoreSO != null) remainingAmps.Add(damageMatrixCoreSO);
        if (!hasAmp2 && proejctileCoreSO != null) remainingAmps.Add(proejctileCoreSO);

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

                numlist[cardIndex] = slotNumber;
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

                numlist[cardIndex] = slotNumber;
                SetUpTutorialAmplifierCard(cardIndex, slotNumber, ampData, isInitial: true);
                continue;
            }

            if (upgradeSlots.Count > 0)
            {
                List<int> availableUpgradeSlots = new List<int>();

                foreach (int slot in upgradeSlots)
                {
                    var towerData = installControl.GetTowerData(slot);
                    var ampTower = installControl.GetAmplifierTower(slot);

                    bool isAlreadyUsed = false;

                    if (towerData != null && IsAttackTypeUsedThisRoll(towerData))
                        isAlreadyUsed = true;

                    if (ampTower != null && ampTower.AmplifierTowerData != null &&
                        IsAmplifierTypeUsedThisRoll(ampTower.AmplifierTowerData))
                        isAlreadyUsed = true;

                    bool isForbidden = false;
                    if (towerData != null)
                        isForbidden = IsForbiddenAttackCombo(towerData, UpgradeOptionAbilityId, isInitial: true);
                    else if (ampTower != null && ampTower.AmplifierTowerData != null)
                        isForbidden = IsForbiddenAmplifierCombo(ampTower.AmplifierTowerData, UpgradeOptionAbilityId, isInitial: true);

                    if (isForbidden)
                        continue;

                    if (!isAlreadyUsed)
                        availableUpgradeSlots.Add(slot);
                }

                int slotNumber = -1;

                if (availableUpgradeSlots.Count > 0)
                {
                    int listIdx = UnityEngine.Random.Range(0, availableUpgradeSlots.Count);
                    slotNumber = availableUpgradeSlots[listIdx];
                }
                else
                {
                    int listIdx = UnityEngine.Random.Range(0, upgradeSlots.Count);
                    slotNumber = upgradeSlots[listIdx];
                }

                if (slotNumber != -1)
                {
                    upgradeSlots.Remove(slotNumber);

                    var pickedTowerData = installControl.GetTowerData(slotNumber);
                    if (pickedTowerData != null) usedAttackTowerTypesThisRoll.Add(pickedTowerData);

                    var pickedAmpTower = installControl.GetAmplifierTower(slotNumber);
                    if (pickedAmpTower != null && pickedAmpTower.AmplifierTowerData != null)
                        AddUsedAmplifierType(pickedAmpTower.AmplifierTowerData); // ✅ 아래 수정 3의 헬퍼

                    numlist[cardIndex] = slotNumber;
                    SetUpgradeCardForUsedSlot(cardIndex, slotNumber, isInitial: true);
                    continue;
                }
            }
            int fallbackSlot = -1;
            for (int s = 0; s < installControl.TowerCount; s++)
            {
                if (!installControl.IsUsedSlot(s) && installControl.CurrentTowerCount < installControl.MaxTowerCount)
                {
                    fallbackSlot = s;
                    break;
                }
            }

            numlist[cardIndex] = fallbackSlot;
            SetUpNewInstallCard(cardIndex, fallbackSlot, isInitial: true);
        }
    }


    private bool ShouldShowGoldCard()
    {
        bool hasAnyTower = false;
        bool hasNonMax = false;
        bool hasEmptyInstallableSlot = false;

        for (int i = 0; i < installControl.TowerCount; i++)
        {
            bool used = installControl.IsUsedSlot(i);

            if (used)
            {
                hasAnyTower = true;
                if (!installControl.IsSlotMaxLevel(i))
                    hasNonMax = true;
            }
            else
            {
                if (installControl.CurrentTowerCount < installControl.MaxTowerCount)
                    hasEmptyInstallableSlot = true;
            }
        }

        if (hasEmptyInstallableSlot) return false;

        return hasAnyTower && !hasNonMax;
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
        if (Variables.Stage == 1)
        {
            if (target == damageMatrixCoreSO)
                return tutorialAmp1Installed;
            if (target == proejctileCoreSO)
                return tutorialAmp2Installed;
        }

        if (target == null) return false;

        for (int i = 0; i < installControl.TowerCount; i++)
        {
            var amp = installControl.GetAmplifierTower(i);
            if (amp == null || amp.AmplifierTowerData == null)
                continue;

            var data = amp.AmplifierTowerData;
            if (data != null && target != null && data.BuffTowerId == target.BuffTowerId)
                return true;
        }
        return false;
    }

    private void SetupTutorialStage1InitialCards(List<int> emptySlots)
    {
        for (int cardIndex = 0; cardIndex < upgradeUIs.Length; cardIndex++)
        {
            if (emptySlots.Count == 0)
            {
                numlist.Add(-1);
                continue;
            }

            int slotNumber = emptySlots[0];
            emptySlots.RemoveAt(0);
            numlist.Add(slotNumber);

            if (cardIndex == 0)
            {
                SetUpTutorialAttackCard(cardIndex, slotNumber, isInitial: true);
            }
            else if (cardIndex == 1)
            {
                SetUpTutorialAmplifierCard(cardIndex, slotNumber, damageMatrixCoreSO, isInitial: true);
            }
            else if (cardIndex == 2)
            {
                SetUpTutorialAmplifierCard(cardIndex, slotNumber, proejctileCoreSO, isInitial: true);
            }
            else
            {
                SetUpNewAttackCard(cardIndex, slotNumber, isInitial: true);
            }
        }
    }

    private void SetUpTutorialAmplifierCard(int i, int slotNumber, AmplifierTowerDataSO ampData, bool isInitial)
    {
        if (ampData == null)
        {
            SetUpNewAmplifierCard(i, slotNumber, isInitial);
            return;
        }
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

            // uiTexts[i].text = $"{towerName}\n\n{abilityName}";
            InstallNewAttackTower(i, towerData, abilityId);
            return;
        }

        if (towerType == 1)
        {
            if (isTutorial && Variables.Stage == 1)
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
        TowerDataSO towerData = installControl.GetRandomAttackTowerDataForCard(usedAttackTowerTypesThisRoll);
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
                    if (IsSlotReservedByOtherCard(s, i))
                        continue;

                    var td = installControl.GetTowerData(s);
                    var amp = installControl.GetAmplifierTower(s);

                    if (td != null && IsAttackTypeUsedThisRoll(td))
                        continue;
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
                    picked = (int)PickUpgradeSlotByWeight(upgradableSlots); // 최후 fallback

                if (picked != -1)
                {
                    if (numlist != null && i >= 0 && i < numlist.Count)
                        numlist[i] = picked;

                    var pickedTd = installControl.GetTowerData(picked);
                    if (pickedTd != null) usedAttackTowerTypesThisRoll.Add(pickedTd);

                    var pickedAmp = installControl.GetAmplifierTower(picked);
                    if (pickedAmp != null && pickedAmp.AmplifierTowerData != null)
                        usedAmplifierTowerTypesThisRoll.Add(pickedAmp.AmplifierTowerData);

                    SetUpgradeCardForUsedSlot(i, picked, isInitial);
                }
            }
            else                                                    //max: gold card
            {
                if (ShouldShowGoldCard() && i == upgradeUIs.Length - 1)
                {
                    abilities[i] = -1;

                    choices[i].InstallType = TowerInstallType.Attack; //default
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
                    {
                        SetUpNewAmplifierCard(i, slotNumber, isInitial);
                    }
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
        var row = DataTableManager.RandomAbilityTable.Get(abilityId);
        return row.RandomAbilityName;
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

        int goldIndex = upgradeUIs.Length - 1;
        if (checkGoldCard && ShouldShowGoldCard() && index == goldIndex)
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
                if (!installControl.IsSlotMaxLevel(i))
                {
                    candidates.Add(i);
                }
            }
        }

        if (candidates.Count == 0)
        {
            int slotNumber = -1;

            for (int i = 0; i < installControl.TowerCount; i++)
            {
                if (IsSlotReservedByOtherCard(i, index)) continue;

                if (!installControl.IsUsedSlot(i) && installControl.CurrentTowerCount < installControl.MaxTowerCount)
                {
                    slotNumber = i;
                    break;
                }
            }
            numlist[index] = slotNumber;
            if (slotNumber != -1) shownUpgradeSlotsThisRoll.Add(slotNumber);
            SetUpNewInstallCard(index, slotNumber, isInitial: false);
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

                if (!isForbidden)
                {
                    nonForbiddenUpgradeSlots.Add(slot);
                    if (!isAlreadyUsed)
                        preferredUpgradeSlots.Add(slot);
                }

                if (preferredUpgradeSlots.Count > 0)
                {
                    number = (int)PickUpgradeSlotByWeight(preferredUpgradeSlots);
                }
                else if (nonForbiddenUpgradeSlots.Count > 0)
                {
                    number = (int)PickUpgradeSlotByWeight(nonForbiddenUpgradeSlots);
                }

                if (isForbidden) continue;
                if (!isAlreadyUsed) availableUpgradeSlots.Add(slot);
            }

            if (availableUpgradeSlots.Count > 0)
            {
                number = (int)PickUpgradeSlotByWeight(availableUpgradeSlots);
            }
            else if (upgradeOnlySlots.Count > 0)
            {
                number = (int)PickUpgradeSlotByWeight(upgradeOnlySlots);
            }
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
        if (number == -1)
        {
            SetupEmptyCardUI(index);
            return;
        }
        numlist[index] = number;
        if (number != -1) shownUpgradeSlotsThisRoll.Add(number);

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

            if (isTutorial && Variables.Stage == 1)
                TutorialManager.Instance.ShowTutorialStep(4);

            return;
        }

        choices[index].InstallType = TowerInstallType.Attack;
        choices[index].AttackTowerData = null;
        choices[index].AmplifierTowerData = null;
        choices[index].BuffSlotIndex = null;
        choices[index].RandomAbilitySlotIndex = null;
        choices[index].ability = -1;
        abilities[index] = -1;

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

        if (targetSlot < 0 || installControl.IsUsedSlot(targetSlot))
            return;

        dragImage = Instantiate(dragImagePrefab, upgradeUIs[choosedIndex].transform);
        var dragImageComp = dragImage.transform.GetChild(0).gameObject.GetComponent<Image>();
        var choosedAttackTowerData = choices[choosedIndex].AttackTowerData;
        var choosedAmplifierTowerData = choices[choosedIndex].AmplifierTowerData;
        if (choosedAttackTowerData != null)
        {
            var towerData = DataTableManager.AttackTowerTable.GetById(choosedAttackTowerData.towerIdInt);
            var towerAssetName = towerData.AttackTowerAssetCut;
            Debug.Log("towerAssetName: " + towerAssetName);
            dragImageComp.sprite = LoadManager.GetLoadedGameTexture(towerAssetName);
        }
        else if (choosedAmplifierTowerData != null)
        {
            var ampData = DataTableManager.BuffTowerTable.Get(choosedAmplifierTowerData.BuffTowerId);
            var ampAssetName = ampData.BuffTowerAssetCut;
            Debug.Log("ampAssetName: " + ampAssetName);
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
        for (int i = 0; i < installControl.TowerCount; i++)
        {
            if (!installControl.IsUsedSlot(i) && RectTransformUtility.RectangleContainsScreenPoint(towers[i].GetComponent<RectTransform>(), touchPos))
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

        if (isTutorial && Variables.Stage == 1)
        {
            int tutorialIdx = Random.Range(0, 2); // Basic Amplifier Towers Only
            return allAmplifierTowers[tutorialIdx];
        }

        HashSet<AmplifierTowerDataSO> excludeSet = new HashSet<AmplifierTowerDataSO>();

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
            SetUpNewAttackCard(i, slotNumber, isInitial);
        }
        else
        {
            SetUpNewAmplifierCard(i, slotNumber, isInitial);
        }
    }
    //----------------------------------------

    private bool HasAnyNewAttackTowerCandidate()
    {
        TowerDataSO candidate = installControl.GetRandomAttackTowerDataForCard();
        return candidate != null;
    }

    private bool HasAnyAmplifierCandidateForCard()
    {
        var ampData = GetRandomAmplifierForCard(usedAmplifierTowerTypesThisRoll);
        return ampData != null;
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
        int previousSlot = (numlist != null && index >= 0 && index < numlist.Count) ? numlist[index] : -1;
        if (previousSlot != -1) shownUpgradeSlotsThisRoll.Remove(previousSlot);
        if (numlist != null && index >= 0 && index < numlist.Count) numlist[index] = -1;
        if (choices != null && index >= 0 && index < choices.Length)
        {
            choices[index].InstallType = TowerInstallType.Attack; // default
            choices[index].AttackTowerData = null;
            choices[index].AmplifierTowerData = null;
            choices[index].BuffSlotIndex = null;
            choices[index].RandomAbilitySlotIndex = null;
            choices[index].ability = -1;
        }

        abilities[index] = -1;
        installControl.IsReadyInstall = false;
        upgradeUIs[index].GetComponentInChildren<Image>().color = Color.white;

        List<int> emptySlots = new List<int>();
        List<int> upgradeSlots = new List<int>();

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
                    emptySlots.Add(i);
                }
            }
            else
            {
                if (!installControl.IsSlotMaxLevel(i))
                {
                    upgradeSlots.Add(i);
                }
            }
        }

        if (ShouldShowGoldCard())
        {
            if (index < upgradeUIs.Length - 1)
            {
                int slotNumber = -1;
                if (emptySlots.Count > 0)
                {
                    int slotIdx = Random.Range(0, emptySlots.Count);
                    slotNumber = emptySlots[slotIdx];
                }

                numlist[index] = slotNumber;
                SetUpTutorialAttackCard(index, slotNumber, isInitial: false);
            }
            else
            {
                numlist[index] = -1;

                var btn = upgradeUIs[index].GetComponent<Button>();
                if (btn != null)
                {
                    btn.interactable = true;
                }
            }

            return;
        }

        int totalTowerCount = GetTotalTowerCount();
        bool hasPistol = HasTowerTypeInstalled(tutorialPistolTower);
        bool hasAmp1 = HasAmplifierInstalled(damageMatrixCoreSO);
        bool hasAmp2 = HasAmplifierInstalled(proejctileCoreSO);

        List<AmplifierTowerDataSO> remainingAmps = new List<AmplifierTowerDataSO>();
        if (!hasAmp1 && damageMatrixCoreSO != null)
        {
            remainingAmps.Add(damageMatrixCoreSO);
        }
        if (!hasAmp2 && proejctileCoreSO != null)
        {
            remainingAmps.Add(proejctileCoreSO);
        }

        if (!hasPistol && totalTowerCount == 0)
        {
            if (emptySlots.Count > 0)
            {
                int slotIdx = Random.Range(0, emptySlots.Count);
                int slotNumber = emptySlots[slotIdx];
                numlist[index] = slotNumber;
                SetUpTutorialAttackCard(index, slotNumber, isInitial: false);
            }
            else
            {
                int slotNumber = -1;
                numlist[index] = slotNumber;
                SetUpTutorialAttackCard(index, slotNumber, isInitial: false);
            }
            return;
        }

        if (remainingAmps.Count > 0 && emptySlots.Count > 0)
        {
            int slotIdx = Random.Range(0, emptySlots.Count);
            int slotNumber = emptySlots[slotIdx];

            var ampData = remainingAmps[Random.Range(0, remainingAmps.Count)];

            numlist[index] = slotNumber;
            SetUpTutorialAmplifierCard(index, slotNumber, ampData, isInitial: false);
            return;
        }

        if (upgradeSlots.Count > 0)
        {
            List<int> availableUpgradeSlots = new List<int>();
            foreach (int slot in upgradeSlots)
            {
                var towerData = installControl.GetTowerData(slot);
                var ampTower = installControl.GetAmplifierTower(slot);

                bool isAlreadyUsed = false;

                if (towerData != null && IsAttackTypeUsedThisRoll(towerData))
                    isAlreadyUsed = true;

                if (ampTower != null && ampTower.AmplifierTowerData != null &&
                    IsAmplifierTypeUsedThisRoll(ampTower.AmplifierTowerData))
                    isAlreadyUsed = true;

                if (!isAlreadyUsed)
                    availableUpgradeSlots.Add(slot);
            }

            int slotNumber = -1;

            if (availableUpgradeSlots.Count > 0)
            {
                int listIdx = Random.Range(0, availableUpgradeSlots.Count);
                slotNumber = availableUpgradeSlots[listIdx];
            }
            else if (upgradeSlots.Count > 0)
            {
                int listIdx = Random.Range(0, upgradeSlots.Count);
                slotNumber = upgradeSlots[listIdx];
            }

            if (slotNumber != -1)
            {
                var pickedTowerData = installControl.GetTowerData(slotNumber);
                if (pickedTowerData != null)
                {
                    usedAttackTowerTypesThisRoll.Add(pickedTowerData);
                }

                var pickedAmpTower = installControl.GetAmplifierTower(slotNumber);
                if (pickedAmpTower != null && pickedAmpTower.AmplifierTowerData != null)
                {
                    usedAmplifierTowerTypesThisRoll.Add(pickedAmpTower.AmplifierTowerData);
                }

                numlist[index] = slotNumber;
                SetUpgradeCardForUsedSlot(index, slotNumber, isInitial: false);
                return;
            }
        }

        if (ShouldShowGoldCard() && index == upgradeUIs.Length - 1)
        {
            numlist[index] = -1;
        }
        else
        {
            int slotNumber = -1;
            for (int i = 0; i < installControl.TowerCount; i++)
            {
                if (numlist != null && numlist.Contains(i))
                {
                    continue;
                }

                if (!installControl.IsUsedSlot(i) && installControl.CurrentTowerCount < installControl.MaxTowerCount)
                {
                    slotNumber = i;
                    break;
                }
            }

            numlist[index] = slotNumber;
            SetUpTutorialAttackCard(index, slotNumber, isInitial: false);
        }
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

        GameObject goldObj = null;

        if (goldCardPrefab != null)
        {
            goldObj = Instantiate(goldCardPrefab, upgradeUIs[index].transform);
            goldObj.transform.SetAsFirstSibling();
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
        Debug.Log($"[GoldCard] Bind start obj={(goldObj != null ? goldObj.name : "NULL")} index={index}");

        // 1) goldObj 루트 버튼
        var rootBtn = goldObj != null ? goldObj.GetComponent<Button>() : null;

        // 2) goldObj 자식 버튼들
        var childBtns = goldObj != null ? goldObj.GetComponentsInChildren<Button>(true) : null;

        // 3) 슬롯(업그레이드 UI) 쪽 버튼(프리팹에 버튼이 없을 때 대비)
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
            Debug.Log($"[GoldCard] Bound {tag} => {b.gameObject.name}");
        }

        // 루트 버튼 먼저
        Bind(rootBtn, "root");

        // 자식 버튼들 전부(루트랑 중복이어도 OK)
        if (childBtns != null)
        {
            foreach (var b in childBtns)
                Bind(b, "child");
        }

        // 슬롯 버튼도 백업으로 바인딩
        Bind(slotBtn, "slot");

        Debug.Log($"[GoldCard] Bind done. bindCount={bindCount}");
    }


    private void OnClickGoldCard(int index)
    {
        Debug.Log($"[GoldCard] CLICK! index={index} reward={goldCardRewardAmount}");

        // 1) 전투씬 UI 반영
        var battleUI = FindObjectOfType<BattleUI>();
        if (battleUI != null)
        {
            battleUI.AddCoinGainText(goldCardRewardAmount);
            Debug.Log($"[GoldCard] BattleUI coinGain updated.");
        }
        else
        {
            Debug.LogWarning("[GoldCard] BattleUI not found.");
        }

        // 2) 전투 종료 후 전체 재화 반영용 누적
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.AddAccumulateGold(goldCardRewardAmount);
            Debug.Log($"[GoldCard] WaveManager accumulateGold now={WaveManager.Instance.AccumulateGold}");
        }
        else
        {
            Debug.LogWarning("[GoldCard] WaveManager.Instance is null.");
        }

        if (towerInfoUI != null) towerInfoUI.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }



    private void SetupEmptyCardUI(int index)
    {
        DeleteAlreadyInstalledCard(index);

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

        var btn = upgradeUIs[index].GetComponentInChildren<Button>();
        if (btn != null) btn.interactable = false;
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
}