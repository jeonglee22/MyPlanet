using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
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

        if (Variables.Stage == 1)
        {
            hasLastChosenOption = false;
            lastChosenOption = default;
            tutorialPistolInstalled = false;
            tutorialAmp1Installed = false;
            tutorialAmp2Installed = false;
        }

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
        if (planetTowerUI.IsBackBtnClicked)
        {
            return;
        }

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

        if (Variables.Stage == 1)
        {
            SettingStage1Cards(totalTowerCount);
            return;
        }

        if (totalTowerCount == 0)
        {
            List<int> emptySlot = new List<int>();
            for (int i = 0; i < installControl.TowerCount; i++)
            {
                if (!installControl.IsUsedSlot(i) &&
                    installControl.CurrentTowerCount < installControl.MaxTowerCount)
                {
                    emptySlot.Add(i);
                }
            }

            usedAttackTowerTypesThisRoll.Clear();
            usedAmplifierTowerTypesThisRoll.Clear();
            initialOptionKeys = new TowerOptionKey[upgradeUIs.Length];

            for (int cardIndex = 0; cardIndex < upgradeUIs.Length; cardIndex++)
            {
                if (emptySlot.Count == 0)
                {
                    numlist.Add(-1);
                    // uiTexts[cardIndex].text = "No Slot";
                    continue;
                }

                int slotIdx = UnityEngine.Random.Range(0, emptySlot.Count);
                int slotNumber = emptySlot[slotIdx];
                emptySlot.RemoveAt(slotIdx);

                numlist.Add(slotNumber);
                SetUpNewAttackCard(cardIndex, slotNumber, isInitial: true);
            }
            return;
        }

        usedAttackTowerTypesThisRoll.Clear();
        initialOptionKeys = new TowerOptionKey[upgradeUIs.Length];

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
            for (int cardIndex = 0; cardIndex < upgradeUIs.Length; cardIndex++)
            {
                int slotNumber = -1;

                if(emptySlots.Count > 0)
                {
                    int slotIdx = Random.Range(0, emptySlots.Count);
                    slotNumber = emptySlots[slotIdx];
                    emptySlots.RemoveAt(slotIdx);
                }

                numlist.Add(slotNumber);
                SetUpNewInstallCard(cardIndex, slotNumber, isInitial: true);
            }

            numlist.Add(-1);
            // uiTexts[uiTexts.Length - 1].text = "100\nGOLD";

            var goldBtn = upgradeUIs[upgradeUIs.Length - 1].GetComponentInChildren<Button>();
            if(goldBtn != null)
            {
                goldBtn.interactable = true;
            }

            return;
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
                if (ShouldShowGoldCard() && cardIndex == upgradeUIs.Length - 1)
                {
                    numlist.Add(-1);
                    // uiTexts[cardIndex].text = "100\nGOLD";
                }
                else
                {
                    int slotNum = -1;

                    if(emptySlots.Count > 0)
                    {
                        int slotIdx = Random.Range(0, emptySlots.Count);
                        slotNum = emptySlots[slotIdx];
                        emptySlots.RemoveAt(slotIdx);
                    }

                    numlist.Add(slotNum);
                    SetUpNewInstallCard(cardIndex, slotNum, isInitial: true);
                }

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
                // Debug.Log(
                //     $"[CardRoll] cardIndex={cardIndex}, r={r:F2}, " +
                //     $"newProb={newProb:F2}, chooseNew={chooseNew}, " +
                //     $"canNew={canNew}, canUpgrade={canUpgrade}"
                // );

            }

            if (mustGuaranteeAmplifierThisRoll &&
                !amplifierCardAlreadyMade &&
                cardIndex == upgradeUIs.Length - 1 &&
                hasAmplifierCandidateForNew)
            {
                chooseNew = true;
            }

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
            else
            {
                if (upgradeSlots.Count == 0)
                {
                    int slotNum = -1;

                    if(emptySlots.Count > 0)
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
                if (!installControl.IsSlotMaxLevel(i))
                    upgradeSlots.Add(i);
            }
        }

        // ★ Stage1도 동일하게: 전체 상태가 풀강일 때만 GOLD
        if (ShouldShowGoldCard())
        {
            for (int cardIndex = 0; cardIndex < upgradeUIs.Length; cardIndex++)
            {
                int slotNum = -1;

                if(emptySlots.Count > 0)
                {
                    int slotIdx = Random.Range(0, emptySlots.Count);
                    slotNum = emptySlots[slotIdx];
                    emptySlots.RemoveAt(slotIdx);
                }

                numlist.Add(slotNum);
                SetUpNewInstallCard(cardIndex, slotNum, isInitial: true);
            }

            numlist.Add(-1);
            // uiTexts[uiTexts.Length - 1].text = "100\nGOLD";

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
            // 1) 아직 요격타워를 설치 안했고, 전체 타워 개수가 0인 최초 상황 → 권총만
            if (!hasPistol && totalTowerCount == 0)
            {
                if (emptySlots.Count > 0)
                {
                    int slotIdx = UnityEngine.Random.Range(0, emptySlots.Count);
                    int slotNumber = emptySlots[slotIdx];
                    emptySlots.RemoveAt(slotIdx);

                    numlist.Add(slotNumber);
                    SetUpTutorialAttackCard(cardIndex, slotNumber, isInitial: true);
                }
                else
                {
                    int slotNumber = -1;
                    numlist.Add(slotNumber);
                    SetUpTutorialAttackCard(cardIndex, slotNumber, isInitial: true);
                }
                continue;
            }

            // 2) 남은 튜토리얼 증폭타워가 있고, 설치 가능한 빈 슬롯도 있을 때 → 남은 증폭타워 우선
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

            // 3) 그 외에는 업그레이드 카드
            if (upgradeSlots.Count > 0)
            {
                int listIdx = UnityEngine.Random.Range(0, upgradeSlots.Count);
                int slotNumber = upgradeSlots[listIdx];
                upgradeSlots.RemoveAt(listIdx);

                numlist.Add(slotNumber);
                SetUpgradeCardForUsedSlot(cardIndex, slotNumber, isInitial: true);
                continue;
            }

            // 4) 여기까지 왔는데 더 이상 설치/업그레이드 둘 다 불가하면 빈칸이나 GOLD
            if(ShouldShowGoldCard() && cardIndex == upgradeUIs.Length - 1)
            {
                numlist.Add(-1);
                // uiTexts[cardIndex].text = "100\nGOLD";
            }
            else
            {
                int slotNum = -1;

                for(int i = 0; i < installControl.TowerCount; i++)
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
    }
    /// <summary>
    /// 새로 설치 가능한 슬롯이 하나도 없고,
    /// 설치된 모든 타워가 MaxLevel일 때만 true 반환.
    /// GOLD 카드를 보여줄지 여부 전역 판단.
    /// </summary>
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

                // 하나라도 MaxLevel이 아닌 타워가 있으면 GOLD 불가
                if (!installControl.IsSlotMaxLevel(i))
                    hasNonMax = true;
            }
            else
            {
                // 아직 설치 가능한 빈 슬롯이 있으면 GOLD 불가
                if (installControl.CurrentTowerCount < installControl.MaxTowerCount)
                    hasEmptyInstallableSlot = true;
            }
        }

        // 설치 가능한 빈 슬롯이 있으면 GOLD X
        if (hasEmptyInstallableSlot)
            return false;

        // 설치된 타워가 최소 한 개 이상이고, 모두 MaxLevel이어야 GOLD
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
            if (ReferenceEquals(data, target))
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
                // uiTexts[cardIndex].text = "No Slot";
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

        // uiTexts[i].text =
        //     $"{ampName}\n" +
        //     buffBlock +
        //     $"---\n" +
        //     $"{ampAbilityName}\n" +
        //     randomBlock;

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
            Debug.LogWarning("[Tutorial] tutorialPistolTower 가 비어있어서 기본 공격타워 로직 사용");
            SetUpNewAttackCard(i, slotNumber, isInitial);
            return;
        }

        if (isInitial)
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

            // uiTexts[i].text = $"{towerName}\n\n{abilityName}";
            InstallNewAttackTower(i, towerData, abilityId);
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
            // uiTexts[i].text =
            //     $"{ampName}\n" +
            //     buffBlock +
            //     $"---" +
            //     $"\n{ampAbilityName}\n" +
            //     randomBlock;

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
            // var towerUI = installedTower.GetComponent<NewAttackTowerCardUiSetting>();
            // towerUI.SettingNewTowerCard(towerData.towerIdInt, abilityId);
        }
    }

    private void SetUpNewAttackCard(int i, int slotNumber, bool isInitial)
    {
        TowerDataSO towerData = isInitial
            ? installControl.GetRandomAttackTowerDataForCard(usedAttackTowerTypesThisRoll)
            : installControl.GetRandomAttackTowerDataForCard();

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
                int randomSlot = upgradableSlots[UnityEngine.Random.Range(0, upgradableSlots.Count)];

                if (numlist != null && i >= 0 && i < numlist.Count)
                    numlist[i] = randomSlot;

                SetUpgradeCardForUsedSlot(i, randomSlot, isInitial);
            }
            else                                                    //max: gold card
            {
                if(ShouldShowGoldCard() && i == upgradeUIs.Length - 1)
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

                    // var btn = upgradeUIs[i].GetComponent<Button>();
                    // if (btn != null)
                    //     btn.interactable = false;
                    
                    if (i == upgradeUIs.Length - 1)
                        Instantiate(goldCardPrefab, upgradeUIs[i].transform);
                        // uiTexts[i].text = "100\nGOLD";
                    else
                    {
                        SetUpNewAmplifierCard(i, slotNumber, isInitial);
                    }
                }
            }
            return;
        }
        if (isInitial) usedAttackTowerTypesThisRoll.Add(towerData);

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

        // uiTexts[i].text = $"{towerName}\n\n{abilityName}";
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

        if (isInitial)
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

    private void ResetUpgradeCard(int index)
    {
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
        // upgradeUIs[index].GetComponentInChildren<Image>().color = Color.white;
        outlineObjects[index].SetActive(false);

        List<int> candidates = new List<int>();

        if (ShouldShowGoldCard())
        {
            if(index == upgradeUIs.Length - 1)
            {
                numlist[index] = -1;
                // uiTexts[index].text = "100\nGOLD";

                var btn = upgradeUIs[index].GetComponent<Button>();
                if (btn != null)
                {
                    btn.interactable = true;
                }

                return;
            }
            else
            {
                List<int> emptySlots = new List<int>();
                for(int i = 0; i < installControl.TowerCount; i++)
                {
                    if(numlist != null && i != index && numlist.Contains(i))
                    {
                        continue;
                    }

                    if (!installControl.IsUsedSlot(i) && installControl.CurrentTowerCount < installControl.MaxTowerCount)
                    {
                        emptySlots.Add(i);
                    }
                }

                int slotNumber = -1;
                if(emptySlots.Count > 0)
                {
                    int slotIdx = Random.Range(0, emptySlots.Count);
                    slotNumber = emptySlots[slotIdx];
                }

                numlist[index] = slotNumber;
                SetUpNewInstallCard(index, slotNumber, isInitial: false);

                return ;
            }
        }

        for(int i = 0; i < installControl.TowerCount; i++)
        {
            if(numlist != null && i != index && numlist.Contains(i))
            {
                continue;
            }

            bool used = installControl.IsUsedSlot(i);

            if (!used)
            {
                if(installControl.CurrentTowerCount < installControl.MaxTowerCount)
                {
                    candidates.Add(i);
                }
            }
            else
            {
                if(!installControl.IsSlotMaxLevel(i))
                {
                    candidates.Add(i);
                }
            }
        }

        if (candidates.Count == 0)
        {
            int slotNumber = -1;

            for(int i = 0; i < installControl.TowerCount; i++)
            {
                if(numlist != null && i != index && numlist.Contains(i))
                {
                    continue;
                }

                if(!installControl.IsUsedSlot(i) && installControl.CurrentTowerCount < installControl.MaxTowerCount)
                {
                    slotNumber = i;
                    break;
                }
            }

            numlist[index] = slotNumber;
            SetUpNewInstallCard(index, slotNumber, isInitial: false);

            return;
        }

        //int number = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        int number = (int)PickUpgradeSlotByWeight(candidates);
        numlist[index] = number;

        if (!installControl.IsUsedSlot(number))
        {
            SetUpNewInstallCard(index, number, isInitial: false);
        }
        else
        {
            SetUpgradeCardForUsedSlot(index, number, isInitial: false);
        }
    }


    private void SetUpgradeCardForUsedSlot(int index, int number, bool isInitial)
    {
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
            {
                usedAttackTowerTypesThisRoll.Add(towerData);
            }

            UpgradeTowerCard(index);
            // uiTexts[index].text =
            //     $"Upgrade\n{number}\n\n{towerData.towerId}";
            // DeleteAlreadyInstalledCard(index);
            // var upgradedTower = Instantiate(upgradeTowerCardPrefab, upgradeUIs[index].transform);
            // upgradedTower.transform.SetAsFirstSibling();
            // var installedTowerButton = upgradedTower.GetComponentInChildren<Button>();
            // installedTowerButton.onClick.AddListener(() => OnClickUpgradeUIClicked(index));
            // var towerUI = installedTower.GetComponent<NewAttackTowerCardUiSetting>();
            // towerUI.SettingNewTowerCard(towerData.towerIdInt, abilityId);

        }
        else if (ampTower != null && ampTower.AmplifierTowerData != null)
        {
            choices[index].InstallType = TowerInstallType.Amplifier;
            choices[index].AttackTowerData = null;
            choices[index].AmplifierTowerData = ampTower.AmplifierTowerData;
            choices[index].BuffSlotIndex = null;
            choices[index].RandomAbilitySlotIndex = null;

            var ampData = ampTower.AmplifierTowerData;
            string ampName = !string.IsNullOrEmpty(ampData.BuffTowerName)
                ? ampData.BuffTowerName
                : ampData.AmplifierType.ToString();

            UpgradeTowerCard(index);
            // DeleteAlreadyInstalledCard(index);
            // var upgradedTower = Instantiate(upgradeTowerCardPrefab, upgradeUIs[index].transform);
            // upgradedTower.transform.SetAsFirstSibling();
            // var installedTowerButton = upgradedTower.GetComponentInChildren<Button>();
            // installedTowerButton.onClick.AddListener(() => OnClickUpgradeUIClicked(index));
            // uiTexts[index].text =
            //     $"Upgrade\n{number}\n\n{ampName}";
            
            if(isTutorial && Variables.Stage == 1)
            {
                TutorialManager.Instance.ShowTutorialStep(4);
            }
        }
        else
        {
            choices[index].InstallType = TowerInstallType.Attack;
            choices[index].AttackTowerData = null;
            choices[index].AmplifierTowerData = null;
            choices[index].BuffSlotIndex = null;
            choices[index].RandomAbilitySlotIndex = null;

            UpgradeTowerCard(index);
            // DeleteAlreadyInstalledCard(index);
            // var upgradedTower = Instantiate(upgradeTowerCardPrefab, upgradeUIs[index].transform);
            // var installedTowerButton = upgradedTower.GetComponentInChildren<Button>();
            // installedTowerButton.onClick.AddListener(() => OnClickUpgradeUIClicked(index));
            // uiTexts[index].text = "Upgrade\n-\n-";

            abilities[index] = -1;
            choices[index].ability = -1;
            return;
        }

        abilities[index] = -1;
        choices[index].ability = -1;
    }


    public void OnClickRefreshButton(int index)
    {
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
        if (choices == null || cardIndex < 0 || cardIndex >= choices.Length)
            return;

        // var img = upgradeUIs[cardIndex].GetComponentInChildren<Image>();
        // if (img != null)
        //     img.color = Color.white;

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
            // string towerName = towerData.towerId;
            // string abilityName = GetAbilityName(abilityId);
            // uiTexts[cardIndex].text = $"{towerName}\n\n{abilityName}";
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
        if (upgradeUIs[index].transform.childCount > 1)
            {
                for (int c = 0; c < upgradeUIs[index].transform.childCount-1; c++)
                {
                    Destroy(upgradeUIs[index].transform.GetChild(c).gameObject);
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
            // upgradeUIs[index].GetComponentInChildren<Image>().color = Color.white;
            installControl.IsReadyInstall = false;
            return;
        }
        // {
        //     installControl.IsReadyInstall = false;
        //     // upgradeUIs[index].GetComponentInChildren<Image>().color = Color.white;
        //     return;
        // }

        // choosedColor = towerColor;
        outlineObjects[index].SetActive(true);
        outlineObjects[(index + 1) % 3].SetActive(false);
        outlineObjects[(index + 2) % 3].SetActive(false);
        // upgradeUIs[index].GetComponentInChildren<Image>().color = choosedColor;
        // upgradeUIs[(index + 1) % 3].GetComponentInChildren<Image>().color = Color.white;
        // upgradeUIs[(index + 2) % 3].GetComponentInChildren<Image>().color = Color.white;
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
            Debug.Log(
    $"[Card][UpgradeClick] cardIndex={index}, targetSlot={numlist[index]}, " +
    $"choiceType={choices[index].InstallType}, ability={choices[index].ability}"
);

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
            // upgradeUIs[i].GetComponentInChildren<Image>().color = Color.white;
            outlineObjects[i].SetActive(false);
            //abilities[i] = AbilityManager.GetRandomAbility();
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
            towerUI.SettingUpgradeCard(attackTower.AttackTowerData.towerIdInt, attackTower.ReinforceLevel + 1);
        }
        else if (ampTower != null && ampTower.AmplifierTowerData != null)
        {
            towerUI.SettingUpgradeCard(ampTower.AmplifierTowerData.BuffTowerId, ampTower.ReinforceLevel + 1);
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

        // Debug.Log("choosedIndex: " + choosedIndex);
        if (choosedIndex == -1)
            return;

        int targetSlot = (numlist != null && choosedIndex < numlist.Count) ? numlist[choosedIndex] : -1;

        if(targetSlot < 0)
        {
            bool hasTowerData = (choices != null && choosedIndex < choices.Length) &&
            (choices[choosedIndex].AttackTowerData != null || choices[choosedIndex].AmplifierTowerData != null);

            if (hasTowerData)
            {
                for(int i = 0; i < installControl.TowerCount; i++)
                {
                    if(!installControl.IsUsedSlot(i) && installControl.CurrentTowerCount < installControl.MaxTowerCount)
                    {
                        targetSlot = i;
                        if(numlist != null && choosedIndex < numlist.Count)
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

        if (currentPhase == InputActionPhase.Canceled)
        {
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
            towerKey = ampData != null ? ampData.GetInstanceID() : -1,
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
        if (abilityId <= 0) return false;

        int towerKey = -1;

        if (type == TowerInstallType.Attack)
        {
            if (towerData == null) return false;
            towerKey = towerData.towerIdInt;
        }
        else if (type == TowerInstallType.Amplifier)
        {
            if (ampData == null) return false;
            towerKey = ampData.GetInstanceID();
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
                if (c.ability <= 0) continue;

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
                    c.AmplifierTowerData.GetInstanceID() == towerKey &&
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

        if(isTutorial && Variables.Stage == 1)
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
        if(CollectionManager.Instance == null || !CollectionManager.Instance.IsInitialized)
        {
            int colIdx = UnityEngine.Random.Range(0, candidates.Count);
            return candidates[colIdx];
        }

        List<float> weights = new List<float>();
        float totalWeight = 0f;

        foreach(var ampData in candidates)
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
            if (numlist != null && i != index && numlist.Contains(i))
                continue;

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
            if(index < upgradeUIs.Length - 1)
            {
                int slotNumber = -1;
                if(emptySlots.Count > 0)
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
                // uiTexts[index].text = "100\nGOLD";

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
        if(!hasAmp1 && damageMatrixCoreSO != null)
        {
            remainingAmps.Add(damageMatrixCoreSO);
        }
        if(!hasAmp2 && proejctileCoreSO != null)
        {
            remainingAmps.Add(proejctileCoreSO);
        }

        if(!hasPistol && totalTowerCount == 0)
        {
            if(emptySlots.Count > 0)
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

        if(remainingAmps.Count > 0 && emptySlots.Count > 0)
        {
            int slotIdx = Random.Range(0, emptySlots.Count);
            int slotNumber = emptySlots[slotIdx];
            
            var ampData = remainingAmps[Random.Range(0, remainingAmps.Count)];

            numlist[index] = slotNumber;
            SetUpTutorialAmplifierCard(index, slotNumber, ampData, isInitial: false);
            return;
        }

        if(upgradeSlots.Count > 0)
        {
            int listIdx = Random.Range(0, upgradeSlots.Count);
            int slotNumber = upgradeSlots[listIdx];

            numlist[index] = slotNumber;
            SetUpgradeCardForUsedSlot(index, slotNumber, isInitial: false);
            return;
        }

        if(ShouldShowGoldCard() && index == upgradeUIs.Length - 1)
        {
            numlist[index] = -1;
            // uiTexts[index].text = "100\nGOLD";
        }
        else
        {
            int slotNumber = -1;
            for(int i = 0; i < installControl.TowerCount; i++)
            {
                if(numlist != null && numlist.Contains(i))
                {
                    continue;
                }

                if(!installControl.IsUsedSlot(i) && installControl.CurrentTowerCount < installControl.MaxTowerCount)
                {
                    slotNumber = i;
                    break;
                }
            }

            numlist[index] = slotNumber;
            SetUpTutorialAttackCard(index, slotNumber, isInitial: false);
        }
    }

}