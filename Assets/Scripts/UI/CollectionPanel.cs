using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    [SerializeField] private GameObject towerInfoPanelObj;
    [SerializeField] private GameObject buffTowerInfoPanelObj;
    [SerializeField] private GameObject randomAbilityInfoPanelObj;

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

        for(int i = 0; i < attackTowerDatas.Count; i += 2)
        {
            var row = Instantiate(collectionItemPrefab, towerPanelContent.transform);
            CollectionItemPanelUI[] panels = row.GetComponentsInChildren<CollectionItemPanelUI>();

            if(panels.Length > 0)
            {
                panels[0].Initialize(attackTowerDatas[i], dataCount, isTowerPanel);
                panels[0].gameObject.SetActive(true);

                panels[0].OnInfoBtn += OnInfoBtnClicked;
            }

            if(panels.Length > 1)
            {
                if(i + 1 < attackTowerDatas.Count)
                {
                    panels[1].Initialize(attackTowerDatas[i + 1], dataCount, isTowerPanel);
                    panels[1].gameObject.SetActive(true);

                    panels[1].OnInfoBtn += OnInfoBtnClicked;
                }
                else
                {
                    panels[1].gameObject.SetActive(false);
                }
            }

            instantiatedItems.Add(row);
        }

        var buffTowerDatas = DataTableManager.BuffTowerTable.GetAllDatas();
        var buffDataCount = buffTowerDatas.Count;

        for (int i = 0; i < buffTowerDatas.Count; i += 2)
        {
            var row = Instantiate(collectionItemPrefab, towerPanelContent.transform);
            CollectionItemPanelUI[] panels = row.GetComponentsInChildren<CollectionItemPanelUI>();

            if (panels.Length > 0)
            {
                panels[0].Initialize(buffTowerDatas[i], buffDataCount, isTowerPanel);
                panels[0].gameObject.SetActive(true);

                panels[0].OnInfoBtn += OnInfoBtnClicked;
            }

            if (panels.Length > 1)
            {
                if (i + 1 < buffTowerDatas.Count)
                {
                    panels[1].Initialize(buffTowerDatas[i + 1], buffDataCount, isTowerPanel);
                    panels[1].gameObject.SetActive(true);

                    panels[1].OnInfoBtn += OnInfoBtnClicked;
                }
                else
                {
                    panels[1].gameObject.SetActive(false);
                }
            }

            instantiatedItems.Add(row);
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

    public void OnInfoBtnClicked(CollectionItemPanelUI panel)
    {
        if(panel.IsTower)
        {
            if(panel.IsAttackTower)
            {
                towerInfoPanelObj.SetActive(true);
                var towerInfoPanel = towerInfoPanelObj.GetComponent<TowerInfoPanelUI>();
                towerInfoPanel.Initialize(panel.AttackTowerData);
            }
            else if(panel.IsBuffTower)
            {
                buffTowerInfoPanelObj.SetActive(true);
                var buffTowerInfoPanel = buffTowerInfoPanelObj.GetComponent<BuffTowerInfoPanelUI>();
                buffTowerInfoPanel.Initialize(panel.BuffTowerData);
            }
        }
    }

}
