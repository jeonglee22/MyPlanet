using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCategori : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI categoryText;
    [SerializeField] private Transform buttonsContainer;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;

    private GameObject buttonPrefab;

    public void Initialize(ShopCategory category, string categoryName, GameObject buttonPrefab)
    {
        categoryText.text = categoryName;
        this.buttonPrefab = buttonPrefab;

        SetUpGridLayout(category);

        foreach (Transform child in buttonsContainer)
        {
            Destroy(child.gameObject);
        }

        switch(category)
        {
            case ShopCategory.Gacha:
                SetUpGachaData();
                break;
            case ShopCategory.Others:
                break;
        }
    }

    private void SetUpGridLayout(ShopCategory category)
    {
        if(category == ShopCategory.Gacha)
        {
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = 4;
            gridLayoutGroup.cellSize = new Vector2(52, 100);
        }
        else if(category == ShopCategory.Others)
        {
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = 3;
            gridLayoutGroup.cellSize = new Vector2(73, 100);
        }
    }

    private void SetUpGachaData()
    {
        var gachaList = DataTableManager.DrawTable.GetGachaList();

        foreach(var gacha in gachaList)
        {
            var gachaBtnObj = Instantiate(buttonPrefab, buttonsContainer);
            var gachaBtn = gachaBtnObj.GetComponent<GachaButton>();
            gachaBtn.Initialize(gacha.Item1, gacha.Item2);
        }
    }
}
