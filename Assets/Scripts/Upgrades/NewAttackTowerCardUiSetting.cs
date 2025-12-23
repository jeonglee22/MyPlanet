using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewAttackTowerCardUiSetting : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private TextMeshProUGUI towerExplainText;
    [SerializeField] private Image towerImage;
    [SerializeField] private TextMeshProUGUI specialAbilityText;
    [SerializeField] private GameObject specialAbilityObjectBackground;
    [SerializeField] private GameObject newTowerTextObject;

    [SerializeField] private GameObject randomAbilityObject;

    [SerializeField] private RectTransform contentRoot;

    void OnEnable()
    {
        specialAbilityObjectBackground.SetActive(true);
        specialAbilityText.gameObject.SetActive(true);
    }

    public void SettingNewTowerCard(int towerId, int ability)
    {
        var towerData = DataTableManager.AttackTowerTable.GetById(towerId);
        var towerExplainId = towerData.TowerText_ID;
        var towerExplainData = DataTableManager.TowerExplainTable.Get(towerExplainId);
        var towerExplainText = towerExplainData.TowerDescribe;
        var towerName = towerExplainData.TowerName;

        SetTowerName(towerName);
        SetTowerExplain(towerExplainText);

        var attackTowerAssetName = towerData.AttackTowerAssetCut;
        var attackTowerAsset = LoadManager.GetLoadedGameTexture(attackTowerAssetName);
        if(attackTowerAsset!=null)
        {
            towerImage.sprite = LoadManager.GetLoadedGameTexture(attackTowerAssetName);
        }

        var projectileId = towerData.Projectile_ID;
        var projectileData = DataTableManager.ProjectileTable.Get(projectileId);
        var projectileSpecialEffectId = projectileData.ProjectileProperties1_ID;

        if (projectileSpecialEffectId != 0)
        {
            var projectileAbilityData = GetRandomAbilityDataFromTowerId(towerId);
            var abilityName = projectileAbilityData.RandomAbilityName;
            var abilityValue = projectileData.ProjectileProperties1Value.ToString();
            SetSpecialAbility(abilityName, abilityValue);
        }
        else
        {
            specialAbilityObjectBackground.SetActive(false);
            specialAbilityText.gameObject.SetActive(false);
        }

        // newTowerTextObject.GetComponent<NewBadgeAnimator>().SetVisible(true);
        
        var abilityData = DataTableManager.RandomAbilityTable.Get(ability);

        var specialEffectName = abilityData.RandomAbilityName;
        var specialEffectValue = abilityData.SpecialEffectValue;
        var randomAbilityObj1 = Instantiate(randomAbilityObject, contentRoot);
        var abilityTexts = randomAbilityObj1.GetComponentsInChildren<TextMeshProUGUI>();
        
        if (abilityTexts.Length != 2)
            return;

        abilityTexts[0].text = specialEffectName;
        abilityTexts[1].text = specialEffectValue.ToString();

        if (abilityData.SpecialEffect2_ID == 0)
            return;
        
        var specialEffectName2 = abilityData.RandomAbility2Name;
        var specialEffectValue2 = abilityData.SpecialEffect2Value;
        var randomAbilityObj2 = Instantiate(randomAbilityObject, contentRoot);
        var abilityTexts2 = randomAbilityObj2.GetComponentsInChildren<TextMeshProUGUI>();
        if (abilityTexts2.Length != 2)
            return;

        abilityTexts2[0].text = specialEffectName2;
        abilityTexts2[1].text = specialEffectValue2.ToString();

        if (abilityData.SpecialEffect3_ID == 0)
            return;
        
        var specialEffectName3 = abilityData.RandomAbility3Name;
        var specialEffectValue3 = abilityData.SpecialEffect3Value;
        var randomAbilityObj3 = Instantiate(randomAbilityObject, contentRoot);
        var abilityTexts3 = randomAbilityObj3.GetComponentsInChildren<TextMeshProUGUI>();
        if (abilityTexts3.Length != 2)
            return;

        abilityTexts3[0].text = specialEffectName3;
        abilityTexts3[1].text = specialEffectValue3.ToString();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetTowerName(string towerName)
    {
        towerNameText.text = towerName;
    }

    private void SetTowerExplain(string explain)
    {
        towerExplainText.text = explain;
    }

    private void SetSpecialAbility(string abilityName, string abilityValue)
    {
        specialAbilityText.text = $"{abilityName} +{abilityValue}";
    }

    private RandomAbilityData GetRandomAbilityDataFromTowerId(int towerId)
    {
        var towerData = DataTableManager.AttackTowerTable.GetById(towerId);
        var projectileId = towerData.Projectile_ID;
        var projectileData = DataTableManager.ProjectileTable.Get(projectileId);
        var projectileSpecialEffectId = projectileData.ProjectileProperties1_ID;
        var proejctileSpecialEffectData = DataTableManager.SpecialEffectTable.Get(projectileSpecialEffectId);
        var abilityId = DataTableManager.RandomAbilityTable.GetAbilityIdFromEffectId(projectileSpecialEffectId);
        var abilityData = DataTableManager.RandomAbilityTable.Get(abilityId);

        return abilityData;
    }
}
