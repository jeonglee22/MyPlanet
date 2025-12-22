using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeToweCardUiSetting : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private Image towerImage;

    [SerializeField] private Image[] upgradeStarImages;

    private float blinkingTime = 0.5f;
    private float currentTime = 0f;
    private bool isStarVisible = true;

    private int nextLevel = 0;

    public void SettingUpgradeCard(int towerId, int towerLevel)
    {
        var towerData = DataTableManager.AttackTowerTable.GetById(towerId);
        var towerName = towerData.AttackTowerName;

        SetTowerName(towerName);

        var attackTowerAssetName = towerData.AttackTowerAssetCut;
        var attackTowerAsset = LoadManager.GetLoadedGameTexture(attackTowerAssetName);
        if(attackTowerAsset!=null)
        {
            towerImage.sprite = LoadManager.GetLoadedGameTexture(attackTowerAssetName);
        }
        Debug.Log(towerName + "Upgrade Tower Level: " + towerLevel);
        for(int i=0; i<upgradeStarImages.Length; i++)
        {
            if(i<towerLevel)
            {
                upgradeStarImages[i].gameObject.SetActive(true);
            }
            else
            {
                upgradeStarImages[i].gameObject.SetActive(false);
            }
        }

        nextLevel = towerLevel;
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.unscaledDeltaTime;
        if(currentTime>=blinkingTime)
        {
            isStarVisible = !isStarVisible;
            currentTime = 0f;
            upgradeStarImages[nextLevel - 1].gameObject.SetActive(isStarVisible);
        }
    }

    private void SetTowerName(string towerName)
    {
        towerNameText.text = towerName;
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
