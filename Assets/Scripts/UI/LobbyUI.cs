using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private GameObject gachaMainPanel;
    [SerializeField] private GameObject collectionPanel;
    [SerializeField] private GameObject planetPanel;
    [SerializeField] private GameObject towerPanel;

    [SerializeField] private Button exitBtn;
    [SerializeField] private Button setBtn;
    [SerializeField] private Button planetBtn;
    [SerializeField] private Button towerBtn;
    [SerializeField] private Button collectionBtn;
    [SerializeField] private Button storeBtn;
    [SerializeField] private Button playBtn;

    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [SerializeField] private SettingPanel settingPanel;

    [SerializeField] private ScrollRect stageScrollRect;
    [SerializeField] private GameObject stagePanelObject;
    [SerializeField] private GameObject emptyObject;
    [SerializeField] private ScrollSnapToCenter snapToCenter;
    private RectTransform stageContent;
    private bool isSettingPlanets = false;
    public bool isInitialized => isSettingPlanets;

    private int selectedStageIndex = 0;
    private int stageIdBase = 50001;

    private void Start()
    {
        ResetBtn();

        stageContent = stageScrollRect.content;
        var childs = stageContent.transform.childCount;
        for (int i = childs - 1; i >= 0; i--)
        {
            Destroy(stageContent.GetChild(i).gameObject);
        }

        var stageCount = DataTableManager.StageTable.GetStageCount();
        Instantiate(emptyObject, stageContent);
        for (int i = 0; i < stageCount; i++)
        {
            var stagePanel = Instantiate(stagePanelObject, stageContent).GetComponent<StagePanelUI>();
            int stageId = stageIdBase + i;
            var stageData = DataTableManager.StageTable.Get(stageId);
            stagePanel.Initialize(stageData, stageData.UnlockCondition == 0);
        }
        Instantiate(emptyObject, stageContent);

        isSettingPlanets = true;

        playBtn.onClick.AddListener(() => OnPlayBtnClicked().Forget());
        storeBtn.onClick.AddListener(OnStoreBtnClicked);
        collectionBtn.onClick.AddListener(OnCollectionBtnClicked);
        planetBtn.onClick.AddListener(OnPlanetBtnClicked);
        towerBtn.onClick.AddListener(OnTowerBtnClicked);
        setBtn.onClick.AddListener(OnSettingBtnClicked);

        leftButton?.onClick.AddListener(() => OnLeftBtnClicked().Forget());
        rightButton?.onClick.AddListener(() => OnRightBtnClicked().Forget());

        AddBtnSound();
        
        gachaMainPanel.SetActive(false);
        collectionPanel.SetActive(false);
        planetPanel.SetActive(false);

        settingPanel = settingPanel.GetComponent<SettingPanel>();
        if(settingPanel != null)
        {
            settingPanel.gameObject.SetActive(false);
            settingPanel.Initialize();
        }
    }

    private void OnDestroy()
    {
        ResetBtn();
    }

    private void AddBtnSound()
    {
        playBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        storeBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        collectionBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        planetBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        towerBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        setBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
    }

    private void ResetBtn()
    {
        playBtn.onClick.RemoveAllListeners();
        storeBtn.onClick.RemoveAllListeners();
        collectionBtn.onClick.RemoveAllListeners();
        planetBtn.onClick.RemoveAllListeners();
        towerBtn.onClick.RemoveAllListeners();
    }

    public async UniTaskVoid OnLeftBtnClicked()
    {
        leftButton.interactable = false;
        rightButton.interactable = false;

        await snapToCenter.SnapLeftOne();

        leftButton.interactable = true;
        rightButton.interactable = true;
    }

    public async UniTaskVoid OnRightBtnClicked()
    {
        leftButton.interactable = false;
        rightButton.interactable = false;

        await snapToCenter.SnapRightOne();

        leftButton.interactable = true;
        rightButton.interactable = true;
    }

    private async UniTaskVoid OnPlayBtnClicked()
    {
        if (snapToCenter.ChoosedIndex == -1)
        {
            Debug.LogWarning("스테이지가 선택되지 않았습니다.");
            return;
        }

        // SetInteractableBtns(false);

        Variables.Stage = snapToCenter.ChoosedIndex;
        await SceneControlManager.Instance.LoadScene(SceneName.BattleScene);

        // SetInteractableBtns(true);
    }

    private void SetInteractableBtns(bool interactable)
    {
        setBtn.interactable = interactable;
        planetBtn.interactable = interactable;
        towerBtn.interactable = interactable;
        collectionBtn.interactable = interactable;
        storeBtn.interactable = interactable;
        playBtn.interactable = interactable;
    }

    private void OnStoreBtnClicked()
    {
        gachaMainPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    private void OnCollectionBtnClicked()
    {
        collectionPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    private void OnPlanetBtnClicked()
    {
        planetPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    private void OnTowerBtnClicked()
    {
        towerPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    private void OnSettingBtnClicked()
    {
        settingPanel.LoadCurrentSettings();
        settingPanel.gameObject.SetActive(true);
    }
}
