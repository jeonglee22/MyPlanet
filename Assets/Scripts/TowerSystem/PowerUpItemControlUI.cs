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

    [SerializeField] private Button towerCountUpgradeButton;
    [SerializeField] private Button newAbilityUpgradeButton;
    [SerializeField] private Button itemUseButton;

    [SerializeField] private GameObject chooseTowerPanel;
    [SerializeField] private GameObject upgradeChooseUis;

    private List<int> numlist;
    [SerializeField] private TextMeshProUGUI[] uiTexts;
    private List<int> abilities;

    private bool isNotUpgradeOpen = false;
    public bool IsNotUpgradeOpen
    {
        get { return isNotUpgradeOpen; }
        set { isNotUpgradeOpen = value; }
    }

    private void Start()
    {
        towerCountUpgradeButton.onClick.AddListener(OnMaxTowerCountUpgradeClicked);
        newAbilityUpgradeButton.onClick.AddListener(OnNewAbilityUpgradeClicked);
        itemUseButton.onClick.AddListener(OnItemUseClicked);
    }

    private async UniTaskVoid OnEnable()
    {
        await UniTask.WaitUntil(() => AbilityManager.IsInitialized);

        towerCountUpgradeButton.gameObject.SetActive(false);
        newAbilityUpgradeButton.gameObject.SetActive(false);   
        upgradeChooseUis.SetActive(false);
        chooseTowerPanel.SetActive(false);    
    }

    private void OnTowerSelectClicked(int index)
    {
        if (!SettingAbilities(index))
            return;

        chooseTowerPanel.SetActive(false);
        upgradeChooseUis.SetActive(true);
        foreach (var ui in upgradeUis)
        {
            ui.SetActive(true);
        }

        for (int i = 0; i < upgradeUis.Length; i++)
        {
            SetUpCard(i);
        }
    }

    private bool SettingAbilities(int towerIndex)
    {
        var planet = GameObject.FindWithTag(TagName.Planet).GetComponent<Planet>();
        var towerAttack = planet.GetAttackTowerToAmpTower(towerIndex);
        var amplifierTower = planet.GetAmplifierTower(towerIndex);

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
                    abilities.Add(ability);
                }
            }

        return true;
    }

    private void OnMaxTowerCountUpgradeClicked()
    {
        installControl.UpgradeMaxTowerCount();
        SetTowerInstallText();
        itemChoosePanel.SetActive(false);
    }

    private void OnItemUseClicked()
    {
        if (itemChoosePanel.activeSelf)
        {
            itemChoosePanel.SetActive(true);
            towerCountUpgradeButton.gameObject.SetActive(true);
            newAbilityUpgradeButton.gameObject.SetActive(true);
            abilities = new List<int>();
        }
        else
        {
            itemChoosePanel.SetActive(false);
            towerCountUpgradeButton.gameObject.SetActive(false);
            newAbilityUpgradeButton.gameObject.SetActive(false);
            chooseTowerPanel.SetActive(false);
            upgradeChooseUis.SetActive(false);
        }
    }

    private void OnNewAbilityUpgradeClicked()
    {
        chooseTowerPanel.SetActive(true);
        towerCountUpgradeButton.gameObject.SetActive(false);
        newAbilityUpgradeButton.gameObject.SetActive(false);

        var towers = installControl.Towers;

        for (int i = 0; i < towers.Count; i++)
        {
            int index = i;
            var button = towers[index].GetComponentInChildren<Button>();
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
        foreach (var ui in upgradeUis)
            ui.SetActive(false);

        Time.timeScale = 1f;
        numlist = null;
    }

    private void SetUpCard(int i)
    {
        var ability = AbilityManager.GetAbility(abilities[i]);
        uiTexts[i].text = $"new\n{ability}";
        return;
    }

    public void OnClickUpgradeUIClicked(int index)
    {    
        installControl.IsReadyInstall = true;

        if (installControl.IsUsedSlot(numlist[index]))
        {
            installControl.UpgradeTower(numlist[index]);
            towerInfoUI.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}