using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomAbilityInfoUI : MonoBehaviour
{
    [SerializeField] private Button closeBtn;
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI abilityDescribeText;
    [SerializeField] private TextMeshProUGUI abilityEffectOneText;
    [SerializeField] private TextMeshProUGUI abilityEffectTwoText;

    public void Initialize(RandomAbilityData data)
    {
        var textData = DataTableManager.RandomAbilityTextTable.Get(data.RandomAbilityText_ID);

        abilityNameText.text = textData.RandomAbilityName;
        abilityDescribeText.text = textData.RandomAbilityDescribe;

        abilityEffectOneText.text = $"{data.RandomAbility2Name}";
        abilityEffectTwoText.text = $"{data.RandomAbility3Name}";

        closeBtn.onClick.AddListener(OnExitBtnClicked);
    }

    private void OnDestroy()
    {
        closeBtn.onClick.RemoveListener(OnExitBtnClicked);
    }

    private void OnExitBtnClicked()
    {
        gameObject.SetActive(false);
    }
}
