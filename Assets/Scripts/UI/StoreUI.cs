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

    [SerializeField] private Button inventoryBtn;
    [SerializeField] private GameObject inventory;
    [SerializeField] private Transform scrollContent;
    [SerializeField] private GameObject inventoryItemPrefab;
    private List<GameObject> instantiatedInventoryItems = new List<GameObject>();

    private void Start()
    {
        backBtn.onClick.AddListener(OnBackBtnClicked);
        gachaPanelUI.gameObject.SetActive(false);
        InitializeShop();

        UpdateCurrencyUI();
        gachaPanelUI.OnGachaCompleted += UpdateCurrencyUI;

        inventory.SetActive(false);
        inventoryBtn.onClick.AddListener(OnInventoryBtnClicked);

        InitializeInventory();

        gachaPanelUI.OnGachaPanelClosed += OnInventoryBtnActive;
    }

    private void OnDestroy()
    {
        gachaPanelUI.OnGachaCompleted -= UpdateCurrencyUI;
        backBtn.onClick.RemoveListener(OnBackBtnClicked);
        inventoryBtn.onClick.RemoveListener(OnInventoryBtnClicked);

        gachaPanelUI.OnGachaPanelClosed -= OnInventoryBtnActive;
    }

    private void OnBackBtnClicked()
    {
        lobbyPanel.SetActive(true);
        gachaPanelUI.gameObject.SetActive(false);
        inventory.SetActive(false);
        OnInventoryBtnActive();
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

        inventory.SetActive(false);
        inventoryBtn.interactable = false;
    }

    private void UpdateCurrencyUI()
    {
        goldText.text = UserData.Gold.ToString();
        diaText.text = $"무료 {UserData.FreeDia} / 유료 {UserData.ChargedDia}";
    }

    private void InitializeInventory()
    {
        var items = DataTableManager.ItemTable.GetAllItemsExceptCollectionItem();
        foreach(var item in items)
        {
            var itemObj = Instantiate(inventoryItemPrefab, scrollContent);
            var itemText = itemObj.GetComponentInChildren<TextMeshProUGUI>();
            var itemCount = ItemManager.Instance.GetItem(item.Item_Id);
            itemText.text = $"{item.ItemNameText} : {itemCount}";
            instantiatedInventoryItems.Add(itemObj);
        }
    }

    public void OnInventoryBtnClicked()
    {
        var items = DataTableManager.ItemTable.GetAllItemsExceptCollectionItem();
        var index = 0;
        foreach(var item in items)
        {
            var itemObj = instantiatedInventoryItems[index];
            var itemText = itemObj.GetComponentInChildren<TextMeshProUGUI>();
            var itemCount = ItemManager.Instance.GetItem(item.Item_Id);
            itemText.text = $"{item.ItemNameText} : {itemCount}";
            index++;
        }

        inventory.SetActive(!inventory.activeSelf);
    }

    public void OnInventoryBtnActive() => inventoryBtn.interactable = true;
}
