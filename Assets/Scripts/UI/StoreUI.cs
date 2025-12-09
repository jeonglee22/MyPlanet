using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreUI : MonoBehaviour
{
    [SerializeField] private GameObject lobbyPanel;

    [SerializeField] private Button backBtn;
    
    [SerializeField] private Transform scrollViewContent;
    [SerializeField] private GameObject shopCategoryPrefab;

    [SerializeField] private GameObject gachaButtonPrefab;
    [SerializeField] private GameObject itemButtonPrefab;

    [SerializeField] private GachaPanelUI gachaPanelUI;

    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI diaText;

    private void Start()
    {
        backBtn.onClick.AddListener(OnBackBtnClicked);
        gachaPanelUI.gameObject.SetActive(false);
        InitializeShop();

        UpdateCurrencyUI();
        gachaPanelUI.OnGachaCompleted += UpdateCurrencyUI;
    }

    private void OnDestroy()
    {
        gachaPanelUI.OnGachaCompleted -= UpdateCurrencyUI;
    }

    private void OnBackBtnClicked()
    {
        lobbyPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    private void InitializeShop()
    {
        foreach(Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }

        CreateCategory(ShopCategory.Gacha, CategoryName.Gacha, gachaButtonPrefab);
    }

    private void CreateCategory(ShopCategory category, string categoryName, GameObject buttonPrefab)
    {
        var categoryObj = Instantiate(shopCategoryPrefab, scrollViewContent);
        var categoryUI = categoryObj.GetComponent<ShopCategori>();

        categoryUI.Initialize(category, categoryName, buttonPrefab, OnGachaButtonClick);

    }

    private void OnGachaButtonClick((int needCurrencyValue, int drawGroup, string gachaName) info)
    {
        gachaPanelUI.Initialize(info.needCurrencyValue, info.drawGroup, info.gachaName);
        gachaPanelUI.gameObject.SetActive(true);
    }

    private void UpdateCurrencyUI()
    {
        goldText.text = UserData.Gold.ToString();
        diaText.text = $"무료 {UserData.FreeDia} / 유료 {UserData.ChargedDia}";
    }
}
