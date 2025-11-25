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
    private int choosedTowerIndex;

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

        for (int i = 0; i < upgradeUis.Length; i++)
        {
            int index = i;
            var button = upgradeUis[index].GetComponent<Button>();
            button.onClick.AddListener(() => OnNewAbilityCardClicked(index));
        }
    }

    private async UniTaskVoid OnEnable()
    {
        await UniTask.WaitUntil(() => AbilityManager.IsInitialized);

        itemChoosePanel.SetActive(false);
        upgradeChooseUis.SetActive(false);
        chooseTowerPanel.SetActive(false);
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
            towerAttack.AddAbility(abilities[index]);
        }
        else if (amplifierTower != null)
        {
            ability?.ApplyAbility(amplifierTower.gameObject);
            amplifierTower.AddAbility(abilities[index]);
        }

        upgradeChooseUis.SetActive(false);
        itemChoosePanel.SetActive(false);
        abilities.Clear();

        var towers = installControl.Towers;
        for (int i = 0; i < towers.Count; i++)
        {
            int slotIndex = i;
            var button = towers[slotIndex].GetComponentInChildren<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => towerInfoUI.SetInfo(slotIndex));
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
    }

    private void OnItemUseClicked()
    {
        if (!itemChoosePanel.activeSelf)
        {
            itemChoosePanel.SetActive(true);
            towerCountUpgradeButton.gameObject.SetActive(true);
            newAbilityUpgradeButton.gameObject.SetActive(true);
            abilities = new List<int>();
            choosedTowerIndex = -1;
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
        Time.timeScale = 1f;
        numlist = null;
    }

    private void SetUpCard(int i)
    {
        var ability = AbilityManager.GetAbility(abilities[i]);
        uiTexts[i].text = $"new\n{ability}";
        return;
    }
}