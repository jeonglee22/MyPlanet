using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectionPanel : MonoBehaviour
{
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject collectionItemPrefab;

    [SerializeField] private Button backBtn;
    [SerializeField] private Button saveBtn;

    [SerializeField] private Button towerPanelBtn;
    [SerializeField] private Button abilityPanelBtn;
    [SerializeField] private Button planetPanelBtn;

    [SerializeField] private Image coreImg;
    [SerializeField] private TextMeshProUGUI coreText;

    [SerializeField] private GameObject towerPanelObj;
    [SerializeField] private GameObject towerPanelContent;
    [SerializeField] private GameObject abilityPanelObj;
    [SerializeField] private GameObject abilityPanelContent;

    private List<GameObject> instantiatedItems = new List<GameObject>();

    private bool isTowerPanel = true;
    private bool isAbilityPanel = false;

    private void Start()
    {
        ResetBtn();

        backBtn.onClick.AddListener(OnBackBtn);
        towerPanelBtn.onClick.AddListener(OnTowerPanelBtn);
        abilityPanelBtn.onClick.AddListener(OnAbilityPanelBtn);

        Initialize();
        towerPanelObj.SetActive(true);
    }

    private void OnDestroy()
    {
        ResetBtn();
    }

    private void ResetBtn()
    {
        backBtn.onClick.RemoveListener(OnBackBtn);
        towerPanelBtn.onClick.RemoveListener(OnTowerPanelBtn);
        abilityPanelBtn.onClick.RemoveListener(OnAbilityPanelBtn);
    }


    public void OnBackBtn()
    {
        lobbyPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnTowerPanelBtn()
    {
        towerPanelObj.SetActive(true);
        //abilityPanelObj.SetActive(false);
    }

    public void OnAbilityPanelBtn()
    {
        towerPanelObj.SetActive(false);
        //abilityPanelObj.SetActive(true);
    }

    public void Initialize()
    {
        foreach(var item in instantiatedItems)
        {
            Destroy(item);
        }
        instantiatedItems.Clear();

        InitializeTowerPanel();

        isTowerPanel = true;
        isAbilityPanel = false;

        UpdateCoreText();
    }

    private void InitializeTowerPanel()
    {
        var attackTowerDatas = DataTableManager.AttackTowerTable.GetAllDatas();
        int dataCount = attackTowerDatas.Count;

        foreach(var data in attackTowerDatas)
        {
            GameObject itemObj = Instantiate(collectionItemPrefab, towerPanelContent.transform);
            CollectionItemPanelUI itemUI = itemObj.GetComponent<CollectionItemPanelUI>();
            itemUI.Initialize(data, dataCount, isTowerPanel);

            instantiatedItems.Add(itemObj);
            itemUI.OnWeightChanged += UpdateCoreText;
        }
    }

    private void UpdateCoreText()
    {
        if (isTowerPanel)
        {
            coreText.text = UserData.CollectionTowerCore.ToString();
        }
        else if (isAbilityPanel)
        {
            coreText.text = UserData.CollectionRandomAbilityCore.ToString();
        }
    }

}
