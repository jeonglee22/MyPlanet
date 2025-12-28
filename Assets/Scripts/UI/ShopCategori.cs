using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCategori : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI categoryText;
    [SerializeField] private Transform[] buttonsContainers;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;

    private GameObject buttonPrefab;

    public void Initialize(ShopCategory category, string categoryName, GameObject buttonPrefab, Action<(int, int, string)> onButtonClick)
    {
        categoryText.text = categoryName;
        this.buttonPrefab = buttonPrefab;

        // SetUpGridLayout(category);

        // foreach (Transform child in buttonsContainers)
        // {
        //     Destroy(child.gameObject);
        // }

        switch(category)
        {
            case ShopCategory.Gacha:
                SetUpGachaData(onButtonClick);
                break;
            default:
                break;
        }
    }
    public void Initialize(ShopCategory category, string categoryName, GameObject buttonPrefab, Action<(int, int, int, GameObject)> onButtonClick)
    {
        categoryText.text = categoryName;
        this.buttonPrefab = buttonPrefab;

        switch(category)
        {
            case ShopCategory.DailyShop:
                SetUpDailyShopData(onButtonClick);
                break;
            case ShopCategory.ChargeDiaShop:
                SetUpChargeDiaShopData(onButtonClick);
                break;
            case ShopCategory.PackageShop:
                SetUpPackageShopData(onButtonClick);
                break;
            case ShopCategory.DailyShopRefresh:
                RefreshDailyShopData(onButtonClick);
                break;
            default:
                break;
        }
    }

    private void RefreshDailyShopData(Action<(int, int, int, GameObject)> onButtonClick)
    {
        var currentItems = new List<int>();
        var beforeItems = new List<Transform> {buttonsContainers[3], buttonsContainers[4], buttonsContainers[5]};
        beforeItems.Remove(beforeItems[UnityEngine.Random.Range(0, beforeItems.Count)]);
        for(int i = 3; i < buttonsContainers.Length; i++)
        {
            int index = i;
            var dailyBtnObj = buttonsContainers[index].GetChild(0).gameObject;
            var dailyButton = dailyBtnObj.GetComponent<DailyButton>();

            var currentShopData = UserShopItemManager.Instance.BuyedShopItemData;
            currentShopData.dailyShop[dailyButton.ButtonIndex] = false;
            UserShopItemManager.Instance.SaveUserShopItemDataAsync(currentShopData).Forget();

            dailyButton.RefreshObj(index, onButtonClick, beforeItems, currentItems);
            currentItems.Add(dailyButton.RandomRewardId);
        }
    }

    private void SetUpDailyShopData(Action<(int, int, int, GameObject)> onButtonClick)
    {
        var currentShopData = UserShopItemManager.Instance.BuyedShopItemData;
        var boughtItems = currentShopData.dailyShop;

        var dailyItemKeys = new List<int>();
        for(int i = 0; i < buttonsContainers.Length; i++)
        {
            int index = i;
            GameObject dailyBtnObj;
            if (buttonsContainers[index].childCount > 0)
                dailyBtnObj = buttonsContainers[index].GetChild(0).gameObject;
            else
                dailyBtnObj = Instantiate(buttonPrefab, buttonsContainers[index]);
            var dailyButton = dailyBtnObj.GetComponent<DailyButton>();
            dailyButton.Initialize(index, onButtonClick, dailyItemKeys, boughtItems[index]);
            
            if (index > 2)
                dailyItemKeys.Add(dailyButton.RandomRewardId);
        }
    }

    private void SetUpChargeDiaShopData(Action<(int, int, int, GameObject)> onButtonClick)
    {
        for(int i = 0; i < buttonsContainers.Length; i++)
        {
            int index = i;
            var chargedDiaBtnObj = Instantiate(buttonPrefab, buttonsContainers[index]);
            var chargeDiaButton = chargedDiaBtnObj.GetComponent<ChargeDiaButton>();
            chargeDiaButton.Initialize(index, onButtonClick);
        }
    }

    private void SetUpPackageShopData(Action<(int, int, int, GameObject)> onButtonClick)
    {
        for(int i = 0; i < buttonsContainers.Length; i++)
        {
            int index = i;
            var dailyBtnObj = Instantiate(buttonPrefab, buttonsContainers[index]);
            var packageItemButton = dailyBtnObj.GetComponent<PackageItemButton>();
            packageItemButton.Initialize(index, onButtonClick);
        }
    }

    // private void SetUpGridLayout(ShopCategory category)
    // {
    //     if(category == ShopCategory.Gacha)
    //     {
    //         gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
    //         gridLayoutGroup.constraintCount = 4;
    //         gridLayoutGroup.cellSize = new Vector2(52, 100);
    //     }
    //     else if(category == ShopCategory.Others)
    //     {
    //         gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
    //         gridLayoutGroup.constraintCount = 3;
    //         gridLayoutGroup.cellSize = new Vector2(73, 100);
    //     }
    // }

    private void SetUpGachaData(Action<(int, int, string)> onButtonClick)
    {
        var gachaList = DataTableManager.DrawTable.GetGachaList();

        for(int i = 0; i < gachaList.Count; i++)
        {
            var gachaBtnObj = Instantiate(buttonPrefab, buttonsContainers[i]);
            var gachaBtn = gachaBtnObj.GetComponent<GachaButton>();
            gachaBtn.Initialize(gachaList[i].Item1, gachaList[i].Item2, gachaList[i].Item3, onButtonClick);
        }
    }


}
