using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class ShopCategori : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI categoryText;
    [SerializeField] private Transform[] buttonsContainers;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;

    private GameObject buttonPrefab;

    public void InitializePreview(string categoryName, GameObject buttonPrefab)
    {
        categoryText.text = categoryName;
        this.buttonPrefab = buttonPrefab;

        ClearButtons();

        for (int i = 0; i < buttonsContainers.Length; i++)
        {
            if (buttonsContainers[i] == null) continue;

            var obj = Instantiate(this.buttonPrefab, buttonsContainers[i]);
            obj.transform.localScale = Vector3.one; 
        }
    }

    private void ClearButtons()
    {
        if (buttonsContainers == null) return;

        foreach (var container in buttonsContainers)
        {
            if (container == null) continue;

            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }
        }
    }

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
            case ShopCategory.Others:
                break;
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
