using Cysharp.Threading.Tasks.Triggers;
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

    [SerializeField] private GameObject basePanel;

    private TextMeshProUGUI abilityEffectOneTMP;
    private TextMeshProUGUI abilityEffectTwoTMP;
    private TextMeshProUGUI abilityEffectThreeTMP;

    private void Awake()
    {
        closeBtn.onClick.AddListener(OnExitBtnClicked);
        closeBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());

        abilityEffectOneTMP = abilityEffectOneText.GetComponentInChildren<TextMeshProUGUI>();
        abilityEffectTwoTMP = abilityEffectTwoText.GetComponentInChildren<TextMeshProUGUI>();
        abilityEffectThreeTMP = abilityEffectThreeText.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Initialize(RandomAbilityData data)
    {
        var textData = DataTableManager.RandomAbilityTextTable.Get(data.RandomAbilityText_ID);
        var specialEffect = DataTableManager.SpecialEffectTable.Get(data.SpecialEffect_ID);

        iconImg.sprite = LoadManager.GetLoadedGameTexture(specialEffect.SpecialEffectIcon);

        abilityNameText.text = textData.RandomAbilityName;
        abilityDescribeText.text = textData.RandomAbilityDescribe;

        if(specialEffect != null)
        {
            var specialEffect1 = DataTableManager.SpecialEffectTable.Get(data.SpecialEffect_ID);
            var specialEffect1TextData = DataTableManager.SpecialEffectTextTable.Get(specialEffect1.SpecialEffectText_ID);

            var isRate = specialEffect1.SpecialEffectValueType == 1;
            var suffix = isRate ? "%" : "";

            abilityEffectOneTMP.text = $"{specialEffect1TextData.Name} {data.SpecialEffectValue}{suffix} 중가";
        }
        else
        {
            abilityEffectOneText.SetActive(false);
        }

        if(data.SpecialEffect2_ID == 0 || data.SpecialEffect2_ID == null)
        {
            abilityEffectTwoTMP.text = "없음";
        }
        else
        {
            var specialEffect2 = DataTableManager.SpecialEffectTable.Get(data.SpecialEffect2_ID.Value);
            var specialEffect2TextData = DataTableManager.SpecialEffectTextTable.Get(specialEffect2.SpecialEffectText_ID);

            var isRate = specialEffect2.SpecialEffectValueType == 1;
            var suffix = isRate ? "%" : "";

            abilityEffectTwoTMP.text = $"{specialEffect2TextData.Name} {data.SpecialEffect2Value.Value}{suffix} 증가";
        }

        if(data.SpecialEffect3_ID == 0 || data.SpecialEffect3_ID == null)
        {
            abilityEffectThreeTMP.text = "없음";
        }
        else
        {
            var specialEffect3 = DataTableManager.SpecialEffectTable.Get(data.SpecialEffect3_ID.Value);
            var specialEffect3TextData = DataTableManager.SpecialEffectTextTable.Get(specialEffect3.SpecialEffectText_ID);

            var isRate = specialEffect3.SpecialEffectValueType == 1;
            var suffix = isRate ? "%" : "";

            abilityEffectThreeTMP.text = $"{specialEffect3TextData.Name} {data.SpecialEffect3Value.Value}{suffix} 증가";
        }
    }

    private void OnDestroy()
    {
        closeBtn.onClick.RemoveAllListeners();
    }

    private void OnExitBtnClicked()
    {
        gameObject.SetActive(false);
        basePanel.SetActive(true);
    }
}
