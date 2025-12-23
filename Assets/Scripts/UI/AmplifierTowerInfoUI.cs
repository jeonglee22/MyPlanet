using TMPro;
using UnityEngine;

public class AmplifierTowerInfoUI : MonoBehaviour
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
}
