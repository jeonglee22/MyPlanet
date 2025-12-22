using TMPro;
using UnityEngine;

public class TowerAbilityInfoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI abilityName;
    [SerializeField] private TextMeshProUGUI abilityValue;
    
    public void SetAbilityInfo(int abilityId)
    {
        var abilityData = DataTableManager.RandomAbilityTable.Get(abilityId);
        if (abilityData == null)
        {
            abilityName.text = "Unknown Ability";
            abilityValue.text = "-";
            return;
        }

        abilityName.text = abilityData.RandomAbilityName;
        abilityValue.text = abilityData.SpecialEffectValue.ToString();
    }

    public void SetAbilityInfo(string name, string value)
    {
        abilityName.text = name;
        abilityValue.text = value;
    }

    public void SetAbilityInfo(string name)
    {
        abilityName.text = name;
        abilityValue.text = "-";
    }
}
