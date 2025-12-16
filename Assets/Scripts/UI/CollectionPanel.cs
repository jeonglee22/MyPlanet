using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
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

    [SerializeField] private GameObject saveConfirmPanel;
    [SerializeField] private Button saveYesBtn;
    [SerializeField] private Button saveNoBtn;

    private List<GameObject> instantiatedItems = new List<GameObject>();
    private List<CollectionItemPanelUI> allPanels = new List<CollectionItemPanelUI>();

    private bool isTowerPanel = true;
    private bool isAbilityPanel = false;

    private bool isFromBackButton = false;
    private event Action OnPendingPanelSwitch;

    private void Start()
    {
        ResetBtn();

        backBtn.onClick.AddListener(OnBackBtn);
        towerPanelBtn.onClick.AddListener(OnTowerPanelBtn);
        abilityPanelBtn.onClick.AddListener(OnAbilityPanelBtn);
        saveBtn.onClick.AddListener(OnSaveBtnClicked);
        saveYesBtn.onClick.AddListener(OnConfirmYesBtnClicked);
        saveNoBtn.onClick.AddListener(OnConfirmNoBtnClicked);

        Initialize().Forget();
        towerPanelObj.SetActive(true);
        abilityPanelObj.SetActive(false);

        saveConfirmPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        ResetBtn();
    }

    private void OnDisable()
    {
        InfoPanelClosed();
    }

    private void ResetBtn()
    {
        backBtn.onClick.RemoveListener(OnBackBtn);
        towerPanelBtn.onClick.RemoveListener(OnTowerPanelBtn);
        abilityPanelBtn.onClick.RemoveListener(OnAbilityPanelBtn);
        saveBtn.onClick.RemoveListener(OnSaveBtnClicked);
        saveYesBtn.onClick.RemoveListener(OnConfirmYesBtnClicked);
        saveNoBtn.onClick.RemoveListener(OnConfirmNoBtnClicked);
    }

    public void OnBackBtn()
    {
        if(CollectionManager.Instance.HasUnsavedChanges())
        {
            isFromBackButton = true;
            OnPendingPanelSwitch = null;

            towerPanelBtn.interactable = false;
            abilityPanelBtn.interactable = false;
            planetPanelBtn.interactable = false;

            saveBtn.interactable = false;
            backBtn.interactable = false;

            saveConfirmPanel.SetActive(true);
            return;
        }

        lobbyPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnTowerPanelBtn()
    {
        if (CollectionManager.Instance.HasUnsavedChanges())
        {
            isFromBackButton = false;
            OnPendingPanelSwitch = SwitchToTowerPanel;

            towerPanelBtn.interactable = false;
            abilityPanelBtn.interactable = false;
            planetPanelBtn.interactable = false;

            saveBtn.interactable = false;
            backBtn.interactable = false;

            saveConfirmPanel.SetActive(true);
        }
        else
        {
            SwitchToTowerPanel();
        }
    }

    public void OnAbilityPanelBtn()
    {
        if(CollectionManager.Instance.HasUnsavedChanges())
        {
            isFromBackButton = false;
            OnPendingPanelSwitch = SwitchToAbilityPanel;

            towerPanelBtn.interactable = false;
            abilityPanelBtn.interactable = false;
            planetPanelBtn.interactable = false;

            saveBtn.interactable = false;
            backBtn.interactable = false;

            saveConfirmPanel.SetActive(true);
        }
        else
        {
            SwitchToAbilityPanel();
        }
    }

    private void SwitchToTowerPanel()
    {
        towerPanelObj.SetActive(true);
        abilityPanelObj.SetActive(false);

        isTowerPanel = true;
        isAbilityPanel = false;

        UpdateCoreText();
        RefreshAllWeights();
        SortPanelsWeight();

        InfoPanelClosed();
    }

    private void SwitchToAbilityPanel()
    {
        towerPanelObj.SetActive(false);
        abilityPanelObj.SetActive(true);

        isTowerPanel = false;
        isAbilityPanel = true;

        UpdateCoreText();
        RefreshAllWeights();
        SortPanelsWeight();

        InfoPanelClosed();
    }

    public void OnSaveBtnClicked()
    {
        InfoPanelClosed();

        CollectionManager.Instance.SaveCollectionAsync().Forget();
    }

    private void OnConfirmYesBtnClicked()
    {
        CollectionManager.Instance.SaveCollectionAsync().Forget();
        saveConfirmPanel.SetActive(false);

        towerPanelBtn.interactable = true;
        abilityPanelBtn.interactable = true;
        planetPanelBtn.interactable = true;

        saveBtn.interactable = true;
        backBtn.interactable = true;

        UpdateCoreText();

        SortPanelsWeight();

        if(isFromBackButton)
        {
            isFromBackButton = false;

            lobbyPanel.SetActive(true);
            gameObject.SetActive(false);
        }

        else if(OnPendingPanelSwitch != null)
        {
            var action = OnPendingPanelSwitch;
            OnPendingPanelSwitch = null;
            action?.Invoke();
        }
    }

    private void OnConfirmNoBtnClicked()
    {
        CollectionManager.Instance.DiscardChangesAsync().Forget();
        saveConfirmPanel.SetActive(false);

        towerPanelBtn.interactable = true;
        abilityPanelBtn.interactable = true;
        planetPanelBtn.interactable = true;

        saveBtn.interactable = true;
        backBtn.interactable = true;

        UpdateCoreText();
        RefreshAllWeights();

        if(isFromBackButton)
        {
            isFromBackButton = false;

            lobbyPanel.SetActive(true);
            gameObject.SetActive(false);
        }

        else if(OnPendingPanelSwitch != null)
        {
            var action = OnPendingPanelSwitch;
            OnPendingPanelSwitch = null;
            action?.Invoke();
        }
    }

    public async UniTaskVoid Initialize()
    {
        if(CollectionManager.Instance != null)
        {
            await UniTask.WaitUntil(() => CollectionManager.Instance.IsInitialized);
        }

        foreach(var item in instantiatedItems)
        {
            Destroy(item);
        }
        instantiatedItems.Clear();
        allPanels.Clear();

        InitializeTowerPanel();
        InitializeAbilityPanel();

        isTowerPanel = true;
        isAbilityPanel = false;

        UpdateCoreText();

        SortPanelsWeight();
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
                panels[0].OnWeightChanged += OnWeightChanged;

                allPanels.Add(panels[0]);
            }

            if(panels.Length > 1)
            {
                if(i + 1 < attackTowerDatas.Count)
                {
                    panels[1].Initialize(attackTowerDatas[i + 1], dataCount, isTowerPanel);
                    panels[1].gameObject.SetActive(true);

                    panels[1].OnInfoBtn += OnInfoBtnClicked;
                    panels[1].OnWeightChanged += OnWeightChanged;

                    allPanels.Add(panels[1]);
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
                panels[0].OnWeightChanged += OnWeightChanged;

                allPanels.Add(panels[0]);
            }

            if (panels.Length > 1)
            {
                if (i + 1 < buffTowerDatas.Count)
                {
                    panels[1].Initialize(buffTowerDatas[i + 1], buffDataCount, isTowerPanel);
                    panels[1].gameObject.SetActive(true);

                    panels[1].OnInfoBtn += OnInfoBtnClicked;
                    panels[1].OnWeightChanged += OnWeightChanged;

                    allPanels.Add(panels[1]);
                }
                else
                {
                    panels[1].gameObject.SetActive(false);
                }
            }

            instantiatedItems.Add(row);
        }
    }

    private void InitializeAbilityPanel()
    {
        var randomAbilityDatas = DataTableManager.RandomAbilityTable.GetAllAbilityDatas();
        int dataCount = randomAbilityDatas.Count;

        for(int i = 0; i < randomAbilityDatas.Count; i += 2)
        {
            var row = Instantiate(collectionItemPrefab, abilityPanelContent.transform);
            CollectionItemPanelUI[] panels = row.GetComponentsInChildren<CollectionItemPanelUI>();

            if(panels.Length > 0)
            {
                panels[0].Initialize(randomAbilityDatas[i], dataCount, false);
                panels[0].gameObject.SetActive(true);

                panels[0].OnInfoBtn += OnInfoBtnClicked;
                panels[0].OnWeightChanged += OnWeightChanged;

                allPanels.Add(panels[0]);
            }

            if(panels.Length > 1)
            {
                if(i + 1 < randomAbilityDatas.Count)
                {
                    panels[1].Initialize(randomAbilityDatas[i + 1], dataCount, false);
                    panels[1].gameObject.SetActive(true);

                    panels[1].OnInfoBtn += OnInfoBtnClicked;
                    panels[1].OnWeightChanged += OnWeightChanged;

                    allPanels.Add(panels[1]);
                }
                else
                {
                    panels[1].gameObject.SetActive(false);
                }
            }

            instantiatedItems.Add(row);
        }
    }

    private void OnWeightChanged()
    {
        UpdateCoreText();
        RefreshAllWeights();
    }

    private void RefreshAllWeights()
    {
        foreach(var panel in allPanels)
        {
            if(panel.IsTower == isTowerPanel)
            {
                panel.UpdateWeightDisplay();
            }
        }
    }

    private void UpdateCoreText()
    {
        if (isTowerPanel)
        {
            coreText.text = CollectionManager.Instance.TowerCore.ToString();
        }
        else if (isAbilityPanel)
        {
            coreText.text = CollectionManager.Instance.AbilityCore.ToString();
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
        else
        {
            randomAbilityInfoPanelObj.SetActive(true);
            var abilityInfoPanel = randomAbilityInfoPanelObj.GetComponent<RandomAbilityInfoUI>();
            abilityInfoPanel.Initialize(panel.AbilityData);
        }
    }

    private void InfoPanelClosed()
    {
        towerInfoPanelObj.SetActive(false);
        buffTowerInfoPanelObj.SetActive(false);
        randomAbilityInfoPanelObj.SetActive(false);
    }

    private void SortPanelsWeight()
    {
        if (isTowerPanel)
        {
            SortTowerPanels();
        }
        else if (isAbilityPanel)
        {
            SortAbilityPanels();
        }
    }

    private void SortTowerPanels()
    {
        var towerPanels = new List<CollectionItemPanelUI>();

        foreach(var panel in allPanels)
        {
            if(panel.IsTower)
            {
                towerPanels.Add(panel);
            }
        }

        towerPanels.Sort((a, b) =>
        {
            int aId = a.IsAttackTower ? a.AttackTowerData.AttackTower_Id : a.BuffTowerData.BuffTower_ID;
            int bId = b.IsAttackTower ? b.AttackTowerData.AttackTower_Id : b.BuffTowerData.BuffTower_ID;

            float aWeight = CollectionManager.Instance.GetWeight(aId);
            float bWeight = CollectionManager.Instance.GetWeight(bId);

            return bWeight.CompareTo(aWeight);
        });

        var rows = new List<Transform>();
        foreach(var item in instantiatedItems)
        {
            if(item.transform.parent == towerPanelContent.transform)
            {
                rows.Add(item.transform);
            }
        }

        for(int i = 0; i < towerPanels.Count; i++)
        {
            int rowIndex = i / 2;
            int colIndex = i % 2;

            if(rowIndex < rows.Count)
            {
                var emptyGameObject = towerPanels[i].transform.parent;
                emptyGameObject.SetParent(rows[rowIndex]);
                emptyGameObject.SetSiblingIndex(colIndex);
            }
        }

        for(int i = 0; i < rows.Count; i++)
        {
            rows[i].SetSiblingIndex(i);
        }
    }

    private void SortAbilityPanels()
    {
        var abilityPanels = new List<CollectionItemPanelUI>();

        foreach(var panel in allPanels)
        {
            if(!panel.IsTower)
            {
                abilityPanels.Add(panel);
            }
        }

        abilityPanels.Sort((a, b) =>
        {
            int aId = a.AbilityData.RandomAbility_ID;
            int bId = b.AbilityData.RandomAbility_ID;

            float aWeight = CollectionManager.Instance.GetWeight(aId);
            float bWeight = CollectionManager.Instance.GetWeight(bId);

            return bWeight.CompareTo(aWeight);
        });

        var rows = new List<Transform>();
        foreach(var item in instantiatedItems)
        {
            if(item.transform.parent == abilityPanelContent.transform)
            {
                rows.Add(item.transform);
            }
        }

        for(int i = 0; i < abilityPanels.Count; i++)
        {
            int rowIndex = i / 2;
            int colIndex = i % 2;

            if(rowIndex < rows.Count)
            {
                var emptyGameObject = abilityPanels[i].transform.parent;
                emptyGameObject.SetParent(rows[rowIndex]);
                emptyGameObject.SetSiblingIndex(colIndex);
            }
        }

        for(int i = 0; i < rows.Count; i++)
        {
            rows[i].SetSiblingIndex(i);
        }
    }

}
