using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RandomAbilityInfoUI : MonoBehaviour
{
    [SerializeField] private Button closeBtn;
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI abilityDescribeText;
    [SerializeField] private GameObject abilityEffectOneText;
    [SerializeField] private GameObject abilityEffectTwoText;
    [SerializeField] private GameObject abilityEffectThreeText;

    private TextMeshProUGUI abilityEffectOneTMP;
    private TextMeshProUGUI abilityEffectTwoTMP;
    private TextMeshProUGUI abilityEffectThreeTMP;

    private void Awake()
    {
        closeBtn.onClick.AddListener(OnExitBtnClicked);

        abilityEffectOneTMP = abilityEffectOneText.GetComponentInChildren<TextMeshProUGUI>();
        abilityEffectTwoTMP = abilityEffectTwoText.GetComponentInChildren<TextMeshProUGUI>();
        abilityEffectThreeTMP = abilityEffectThreeText.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Initialize(RandomAbilityData data)
    {
        var textData = DataTableManager.RandomAbilityTextTable.Get(data.RandomAbilityText_ID);
        var specialEffect = DataTableManager.SpecialEffectTable.Get(data.SpecialEffect_ID);

        abilityNameText.text = textData.RandomAbilityName;
        abilityDescribeText.text = textData.RandomAbilityDescribe;

        if(specialEffect != null)
        {
            abilityEffectOneTMP.text = $"{specialEffect.SpecialEffectName} {data.SpecialEffectValue} 중가";
        }
        else
        {
            abilityEffectOneText.SetActive(false);
        }

        if(data.RandomAbility2Name == "없음")
        {
            abilityEffectTwoText.SetActive(false);
        }
        if(data.RandomAbility3Name == "없음")
        {
            abilityEffectThreeText.SetActive(false);
        }

        abilityEffectTwoTMP.text = $"{data.RandomAbility2Name}";
        abilityEffectThreeTMP.text = $"{data.RandomAbility3Name}";
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
