using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class StoreUI : MonoBehaviour
{
    [SerializeField] private Button backBtn;
    
    [SerializeField] private Transform scrollViewContent;
    [SerializeField] private GameObject shopCategoryPrefab;

    [SerializeField] private GameObject gachaButtonPrefab;
    [SerializeField] private GameObject itemButtonPrefab;

    [SerializeField] private BuyPanelUI buyPanelUI;

    private void Start()
    {
        backBtn.onClick.AddListener(OnBackBtnClicked);
        buyPanelUI.gameObject.SetActive(false);
        InitializeShop();
    }

    private void OnBackBtnClicked()
    {
        SceneControlManager.Instance.LoadScene(SceneName.LobbyScene).Forget();
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
        buyPanelUI.Initialize(info.needCurrencyValue, info.drawGroup, info.gachaName);
        buyPanelUI.gameObject.SetActive(true);
    }
}
