using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpItemControlUI : MonoBehaviour
{
    [SerializeField] private TowerInstallControl installControl;
    [SerializeField] private GameObject[] upgradeUis;
    [SerializeField] private TowerInfoUI towerInfoUI;
    [SerializeField] private TextMeshProUGUI towerInstallText;
    [SerializeField] private GameObject itemChoosePanel;
    [SerializeField] private TowerUpgradeSlotUI towerUpgradeSlotUI;

    [SerializeField] private Button towerCountUpgradeButton;
    [SerializeField] private Button newAbilityUpgradeButton;
    [SerializeField] private Button itemUseButton;
    [SerializeField] private TextMeshProUGUI quasarText;

    [SerializeField] private GameObject chooseTowerPanel;
    [SerializeField] private GameObject upgradeChooseUis;

    [SerializeField] private Button selectTowerButton;

    private List<int> numlist;
    [SerializeField] private TextMeshProUGUI[] uiTexts;
    private List<int> abilities;

    private bool isNotUpgradeOpen = false;
    private int choosedTowerIndex;

    public bool IsNotUpgradeOpen
    {
        get { return isNotUpgradeOpen; }
        set { isNotUpgradeOpen = value; }
    }

    private bool isUsedItem = false;
    private bool isInfiniteItem = false;
    public bool IsInfiniteItem
    {
        get { return isInfiniteItem; }
        set { isInfiniteItem = value; }
    }

    private bool isTutorial = false;

    private bool isTowerChoosingState = false;
    public bool IsTowerChoosingState
    {
        get { return isTowerChoosingState; }
        set { isTowerChoosingState = value; }
    }

    private void Start()
    {
        towerCountUpgradeButton.onClick.AddListener(OnMaxTowerCountUpgradeClicked);
        newAbilityUpgradeButton.onClick.AddListener(OnNewAbilityUpgradeClicked);
        itemUseButton.onClick.AddListener(OnItemUseClicked);
        // selectTowerButton.onClick.AddListener(OnUpgradeTowerClicked);

        for (int i = 0; i < upgradeUis.Length; i++)
        {
            int index = i;
            var button = upgradeUis[index].GetComponent<Button>();
            button.onClick.AddListener(() => OnNewAbilityCardClicked(index));
        }

        SetIsTutorial(TutorialManager.Instance.IsTutorialMode);

        OnQuaserChanged();
        Variables.OnQuasarChanged += OnQuaserChanged;
        Variables.OnQuasarChanged += SetDeactiveQuasarUiGameObjects;

        AddButtonSound();
    }

    private void AddButtonSound()
    {
        towerCountUpgradeButton.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        newAbilityUpgradeButton.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        itemUseButton.onClick.AddListener(() => SoundManager.Instance.PlayQuasarSelect());

        for (int i = 0; i < upgradeUis.Length; i++)
        {
            int index = i;
            var button = upgradeUis[index].GetComponent<Button>();
            button.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        }
    }

    private async UniTaskVoid OnEnable()
    {
        await UniTask.WaitUntil(() => AbilityManager.IsInitialized);

        itemChoosePanel.SetActive(false);
        upgradeChooseUis.SetActive(false);
        chooseTowerPanel.SetActive(false);

        Variables.OnQuasarChanged += CheckQuasarForReactivation;
    }

    private void OnDestroy()
    {
        Variables.OnQuasarChanged -= OnQuaserChanged;
        Variables.OnQuasarChanged -= SetDeactiveQuasarUiGameObjects;
    }

    public void OnUpgradeTowerClicked()
    {
        if (!SettingAbilities(choosedTowerIndex))
            return;

        towerInfoUI.gameObject.SetActive(false);
        chooseTowerPanel.SetActive(false);
        upgradeChooseUis.SetActive(true);
        installControl.ClearAllSlotHighlights();

        for (int i = 0; i < uiTexts.Length; i++)
        {
            SetUpCard(i);
        }
    }

    private void OnNewAbilityCardClicked(int index)
    {
        if(choosedTowerIndex == -1)
            return;
        
        var ability = AbilityManager.GetAbility(abilities[index]);

        var towerAttack = installControl.GetAttackTower(choosedTowerIndex);
        var amplifierTower = installControl.GetAmplifierTower(choosedTowerIndex);
        
        if (towerAttack == null && amplifierTower == null) return;

        if (towerAttack != null)
        {
            ability?.ApplyAbility(towerAttack.gameObject);
            ability?.Setting(towerAttack.gameObject);
            towerAttack.AddBaseAbility(abilities[index]);
        }
        else if (amplifierTower != null)
        {
            ability?.ApplyAbility(amplifierTower.gameObject);
            ability?.Setting(amplifierTower.gameObject);
            amplifierTower.AddAbility(abilities[index]);
        }

        upgradeChooseUis.SetActive(false);
        itemChoosePanel.SetActive(false);
        abilities.Clear();

        SetTowerOpenInfoTouch();

        Variables.Quasar--;
        SetActiveItemUseButton(false);

        ResumeGame();
    }

    private void ResumeGame()
    {
        if (towerInfoUI != null && towerUpgradeSlotUI != null)
        {
            towerInfoUI.gameObject.SetActive(false);
            towerUpgradeSlotUI.gameObject.SetActive(false);
        }
        GamePauseManager.Instance.Resume();
        isTowerChoosingState = false;
    }

    private void SetTowerOpenInfoTouch()
    {
        var towers = installControl.Towers;
        for (int i = 0; i < installControl.TowerCount; i++)
        {
            int slotIndex = i;

            if(!installControl.IsUsedSlot(slotIndex))
                continue;

            var button = towers[slotIndex].GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => installControl.OpenInfoUI(slotIndex));
        }
    }

    private void OnTowerSelectClicked(int index)
    {
        choosedTowerIndex = index;
    }

    private bool SettingAbilities(int towerIndex)
    {
        var towerAttack = installControl.GetAttackTower(towerIndex);
        var amplifierTower = installControl.GetAmplifierTower(towerIndex);

        if (towerAttack == null && amplifierTower == null) return false;

        var towerAbilities = new List<int>();
        var abilityGroupId = -1;
        if (towerAttack != null)
        {
            towerAbilities = towerAttack.Abilities;
            abilityGroupId = towerAttack.AttackTowerData.randomAbilityGroupId;
        }
        else if (amplifierTower != null)
        {
            towerAbilities = amplifierTower.Abilities;
            abilityGroupId = amplifierTower.AmplifierTowerData.RandomAbilityGroupId;
        }

        while (abilities.Count < 3)
            {
                int ability = DataTableManager.RandomAbilityGroupTable.GetRandomAbilityInGroup(abilityGroupId);
                if (!(towerAbilities.Contains(ability) && DataTableManager.RandomAbilityTable.Get(ability).DuplicateType == 1))
                {
                    if (abilities.Contains(ability))
                        continue;
                    
                    abilities.Add(ability);
                }
            }

        Debug.Log("SettingAbilities Success");
        return true;
    }

    private void OnMaxTowerCountUpgradeClicked()
    {
        installControl.UpgradeMaxTowerCount();
        SetTowerInstallText();
        itemChoosePanel.SetActive(false);

        Variables.Quasar--;
        SetActiveItemUseButton(false);
        isTowerChoosingState = false;

        ResumeGame();
    }

    public void SetActiveItemUseButton(bool isActive)
    {
        if(Variables.Quasar > 0 || isInfiniteItem)
        {
            itemUseButton.interactable = true;
            isUsedItem = false;
        }
        else
        {
            itemUseButton.interactable = isActive;
            isUsedItem = !isActive;
        }
    }

    public void CheckQuasarForReactivation()
    {
        if (Variables.Quasar == 0)
            SetActiveItemUseButton(false);
        else
            SetActiveItemUseButton(true);
    }

    private void SetDeactiveQuasarUiGameObjects()
    {
        itemChoosePanel.SetActive(false);
        towerCountUpgradeButton.gameObject.SetActive(false);
        newAbilityUpgradeButton.gameObject.SetActive(false);
        chooseTowerPanel.SetActive(false);
        upgradeChooseUis.SetActive(false);
        towerInfoUI.gameObject.SetActive(false);
        isTowerChoosingState = false;
    }

    private void OnItemUseClicked()
    {
        if (isUsedItem)
            return;

        if (!itemChoosePanel.activeSelf)
        {
            if(isTutorial && Variables.Stage == 2)
            {
                TutorialManager.Instance.ShowTutorialStep(7);
            }

            itemChoosePanel.SetActive(true);
            towerUpgradeSlotUI.IsNotUpgradeOpen = true;
            towerUpgradeSlotUI.gameObject.SetActive(true);
            towerUpgradeSlotUI.IsQuasarItemUsed = true;
            towerCountUpgradeButton.gameObject.SetActive(true);
            newAbilityUpgradeButton.gameObject.SetActive(true);
            abilities = new List<int>();
            choosedTowerIndex = -1;
        }
        else
        {
            // SetDeactiveQuasarUiGameObjects();
            SetTowerOpenInfoTouch();
        }
    }

    private void OnNewAbilityUpgradeClicked()
    {
        if(isTutorial && Variables.Stage == 2)
        {
            TutorialManager.Instance.ShowTutorialStep(8);
        }

        chooseTowerPanel.SetActive(true);
        towerCountUpgradeButton.gameObject.SetActive(false);
        newAbilityUpgradeButton.gameObject.SetActive(false);

        var towers = installControl.Towers;

        for (int i = 0; i < towers.Count; i++)
        {
            int index = i;

            if(!installControl.IsUsedSlot(index))
                continue;

            var button = towers[index].GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnTowerSelectClicked(index));
        }

        towerInfoUI.SetTitleText(GameStrings.TowerUpgradePopupTitle);
        towerInfoUI.SetCheckText(GameStrings.Choose);
        towerInfoUI.SetActiveCancelButton(true);
        towerInfoUI.SetConfirmButtonFunction(OnUpgradeTowerClicked);

        isTowerChoosingState = true;
    }

    private void SetTowerInstallText()
    {
        Debug.Log("SetTowerInstallText");
        towerInstallText.text = $"({installControl.CurrentTowerCount}/{installControl.MaxTowerCount})";
    }

    private void OnDisable()
    {
        GamePauseManager.Instance.Resume();
        numlist = null;

        Variables.OnQuasarChanged -= CheckQuasarForReactivation;

        SoundManager.Instance.PlayDeployClose();
    }

    private void SetUpCard(int i)
    {
        // var ability = AbilityManager.GetAbility(abilities[i]);
        // var abilityTextId = DataTableManager.RandomAbilityTable.Get(abilities[i]).RandomAbilityText_ID;
        // var abilityExplainTextData = DataTableManager.RandomAbilityTextTable.Get(abilityTextId);
        // var abilityExplain = abilityExplainTextData?.RandomAbilityDescribe ?? string.Empty;
        // uiTexts[i].text = abilityExplain;

        var abilityId = abilities[i];
        var abilityData = DataTableManager.RandomAbilityTable.Get(abilityId);
        var abilityAmount = abilityData.SpecialEffectValue;
        var abilityName = abilityData.RandomAbilityName;
        if (abilityId == (int)AbilityId.Hitscan)
            uiTexts[i].text = $"{abilityName}\n활성화";
        else
            uiTexts[i].text = $"{abilityName}\n+{abilityAmount}";

        return;
    }

    private void SetIsTutorial(bool isTutorial)
    {
        this.isTutorial = isTutorial;
    }

    private void OnQuaserChanged()
    {
        quasarText.text = $"퀘이사\nX{Variables.Quasar}";
    }
}