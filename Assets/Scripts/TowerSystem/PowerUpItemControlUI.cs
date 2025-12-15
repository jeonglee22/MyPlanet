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

    private void Start()
    {
        towerCountUpgradeButton.onClick.AddListener(OnMaxTowerCountUpgradeClicked);
        newAbilityUpgradeButton.onClick.AddListener(OnNewAbilityUpgradeClicked);
        itemUseButton.onClick.AddListener(OnItemUseClicked);

        for (int i = 0; i < upgradeUis.Length; i++)
        {
            int index = i;
            var button = upgradeUis[index].GetComponent<Button>();
            button.onClick.AddListener(() => OnNewAbilityCardClicked(index));
        }

        SetIsTutorial(TutorialManager.Instance.IsTutorialMode);

        OnQuaserChanged();
        Variables.OnQuasarChanged += OnQuaserChanged;
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
        if (!SettingAbilities(index))
            return;

        choosedTowerIndex = index;
        chooseTowerPanel.SetActive(false);
        upgradeChooseUis.SetActive(true);

        for (int i = 0; i < uiTexts.Length; i++)
        {
            SetUpCard(i);
        }
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
        SetActiveItemUseButton(true);
    }

    public void SetDeactiveQuasarUiGameObjects()
    {
        itemChoosePanel.SetActive(false);
        towerCountUpgradeButton.gameObject.SetActive(false);
        newAbilityUpgradeButton.gameObject.SetActive(false);
        chooseTowerPanel.SetActive(false);
        upgradeChooseUis.SetActive(false);
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
            towerCountUpgradeButton.gameObject.SetActive(true);
            newAbilityUpgradeButton.gameObject.SetActive(true);
            abilities = new List<int>();
            choosedTowerIndex = -1;
        }
        else
        {
            SetDeactiveQuasarUiGameObjects();
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
    }

    private void SetUpCard(int i)
    {
        var ability = AbilityManager.GetAbility(abilities[i]);
        uiTexts[i].text = $"new\n{ability}";
        return;
    }

    private void SetIsTutorial(bool isTutorial)
    {
        this.isTutorial = isTutorial;
    }

    private void OnQuaserChanged()
    {
        quasarText.text = $"퀘이사 X{Variables.Quasar}";
    }
}