using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlanetPanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject planetInfoPanel;

    [SerializeField] private Button backBtn;
    [SerializeField] private Button choosePlanetBtn;
    [SerializeField] private Button homeBtn;

    [SerializeField] private GameObject saveConfirmPanel;
    [SerializeField] private Button saveYesBtn;
    [SerializeField] private Button saveNoBtn;

    [SerializeField] private Button[] planetButtons;
    [SerializeField] private TextMeshProUGUI planetNameText;

    [SerializeField] private Button starLevelUpBtn;
    [SerializeField] private Button levelUpBtn;
    [SerializeField] private GameObject starUpgradePanel;
    [SerializeField] private GameObject levelUpgradePanel;

    [SerializeField] private GameObject selectPlanetIcons;

    [SerializeField] private GameObject planetInstallSuccessPanel;
    [SerializeField] private Button ConfirmBtn;

    private int choosedIndex = -1;

    private CancellationTokenSource autoCloseCts;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < planetButtons.Length; i++)
        {
            int index = i;
            planetButtons[i].onClick.AddListener(() => OnPlanetButtonClicked(index));
        }

        backBtn.onClick.AddListener(OnBackBtnClicked);
        //choosePlanetBtn?.onClick.AddListener(OnChoosePlanetBtnClicked);
        choosePlanetBtn?.onClick.AddListener(() => OnInstallBtnClicked().Forget());
        saveYesBtn.onClick.AddListener(() => OnSaveYesBtnClicked().Forget());
        saveNoBtn.onClick.AddListener(OnSaveNoBtnClicked);
        homeBtn.onClick.AddListener(OnHomeBtnClicked);
        starLevelUpBtn.onClick.AddListener(OnStarUpgradeButtonClicked);
        levelUpBtn.onClick.AddListener(OnLevelUpgradeButtonClicked);
        ConfirmBtn.onClick.AddListener(OnConfirmBtnClicked);

        AddBtnSound();

        InitializePlanetPanelUI();

        planetInfoPanel.SetActive(false);
        starUpgradePanel.SetActive(false);
        levelUpgradePanel.SetActive(false);
        selectPlanetIcons.SetActive(false);
        planetInstallSuccessPanel.SetActive(false);
    }

    private void AddBtnSound()
    {
        backBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        choosePlanetBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        saveYesBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        saveNoBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        homeBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        starLevelUpBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        levelUpBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        ConfirmBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());

        for (int i = 0; i < planetButtons.Length; i++)
        {
            int index = i;
            planetButtons[i].onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnEnable()
    {
        choosedIndex = -1;
        planetInfoPanel.SetActive(false);

        RefreshPlanetPanelUI();
    }

    private void OnDisable()
    {
        Cancel();
    }

    private void OnDestroy()
    {
        Cancel();
    }

    private void Cancel()
    {
        autoCloseCts?.Cancel();
        autoCloseCts?.Dispose();
        autoCloseCts = new CancellationTokenSource();
    }

    public void OnBackBtnClicked()
    {
        if(planetInfoPanel.activeSelf)
        {
            planetInfoPanel.SetActive(false);
            titleText.text = "행성";
            selectPlanetIcons.SetActive(false);
            return;
        }
        else if(starUpgradePanel.activeSelf)
        {
            starUpgradePanel.SetActive(false);
            planetInfoPanel.SetActive(true);
            titleText.text = "행성 강화";
            selectPlanetIcons.SetActive(true);
            return;
        }
        else if(levelUpgradePanel.activeSelf)
        {
            levelUpgradePanel.SetActive(false);
            planetInfoPanel.SetActive(true);
            titleText.text = "행성 강화";
            selectPlanetIcons.SetActive(true);
            return;
        }

        lobbyPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnPlanetButtonClicked(int planetIndex)
    {
        // Assuming planet IDs start from 300000 and increment by 1
        choosedIndex = planetIndex + 1;

        int planetId = 300000 + planetIndex + 1;
        // Update the planet name display
        SetPlanetName(DataTableManager.PlanetTable.Get(planetId).PlanetName);
        planetInfoPanel.GetComponent<PlanetInfoUI>().Initialize(DataTableManager.PlanetTable.Get(planetId),PlanetManager.Instance.GetPlanetInfo(planetId));
        planetInfoPanel.SetActive(true);

        titleText.text = "행성 강화";

        selectPlanetIcons.SetActive(true);
    }

    public void OnStarUpgradeButtonClicked()
    {
        starUpgradePanel.SetActive(true);
        planetInfoPanel.SetActive(false);
        selectPlanetIcons.SetActive(false);

        int planetId = 300000 + choosedIndex;
        starUpgradePanel.GetComponent<PlanetStarUpgradePanelUI>().Initialize(DataTableManager.PlanetTable.Get(planetId),PlanetManager.Instance.GetPlanetInfo(planetId));

        titleText.text = "행성 승급";
    }

    public void OnLevelUpgradeButtonClicked()
    {
        levelUpgradePanel.SetActive(true);
        planetInfoPanel.SetActive(false);
        selectPlanetIcons.SetActive(false);

        int planetId = 300000 + choosedIndex;
        levelUpgradePanel.GetComponent<PlanetLevelUpgradeUI>().Initialize(DataTableManager.PlanetTable.Get(planetId),PlanetManager.Instance.GetPlanetInfo(planetId));

        titleText.text = "행성 레벨업";
    }

    public async UniTaskVoid OnSaveYesBtnClicked()
    {
        // Save planet choice logic here
        planetInfoPanel.SetActive(false);
        saveConfirmPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        gameObject.SetActive(false);

        if (AuthManager.Instance == null || !AuthManager.Instance.IsSignedIn)
            return;

        int planetId = 300000 + choosedIndex;
        Variables.planetId = planetId;

        var userPlanetInfo = PlanetManager.Instance.GetPlanetInfo(planetId);

        var userPlanetData = new UserPlanetData(
            AuthManager.Instance.UserNickName,
            planetId: Variables.planetId,
            planetUpgrade: userPlanetInfo.starLevel,
            planetLevel: userPlanetInfo.level,
            planetCollectionStat: 0
        );

        PlanetManager.Instance.SetActivePlanet(planetId);
        if(PlanetStatManager.Instance != null)
        {
            PlanetStatManager.Instance.UpdateCurrentPlanetStats();
        }
        
        await PlanetManager.Instance.SavePlanetsAsync();
        await UserPlanetManager.Instance.UpdateUserPlanetAsync(userPlanetData);
        await UserAttackPowerManager.Instance.UpdatePlanetPower(userPlanetData);
    }

    public void OnSaveNoBtnClicked()
    {
        saveConfirmPanel.SetActive(false);
    }

    private void OnHomeBtnClicked()
    {
        lobbyPanel.SetActive(true);
        planetInfoPanel.SetActive(false);
        gameObject.SetActive(false);
    }

    public void SetPlanetName(string planetName)
    {
        planetNameText.text = planetName;
    }

    public void InitializePlanetPanelUI()
    {
        var planetDatas = DataTableManager.PlanetTable.GetAll();
        var allPlanetInfo = PlanetManager.Instance.GetAllPlanets();
        
        for(int i = 0; i < planetButtons.Length; i++)
        {
            if(i < planetDatas.Count)
            {
                var planetData = planetDatas[i];
                string planetKey = planetData.Planet_ID.ToString();

                if(allPlanetInfo.TryGetValue(planetKey, out var userPlanetInfo))
                {
                    planetButtons[i].gameObject.SetActive(true);

                    PlanetItemUI planetItemUI = planetButtons[i].GetComponent<PlanetItemUI>();

                    if(planetItemUI != null)
                    {
                        planetItemUI.Initialize(planetData, userPlanetInfo);
                    }
                }
            }
            else
            {
                planetButtons[i].gameObject.SetActive(false);
            }
        }   
    }

    public void RefreshPlanetPanelUI()
    {
        if(PlanetManager.Instance == null || !PlanetManager.Instance.IsInitialized)
        {
            return;
        }

        InitializePlanetPanelUI();
    }

    private  async UniTaskVoid OnInstallBtnClicked()
    {
        if (AuthManager.Instance == null || !AuthManager.Instance.IsSignedIn)
            return;

        int planetId = 300000 + choosedIndex;
        Variables.planetId = planetId;

        var userPlanetInfo = PlanetManager.Instance.GetPlanetInfo(planetId);

        var userPlanetData = new UserPlanetData(
            AuthManager.Instance.UserNickName,
            planetId: Variables.planetId,
            planetUpgrade: userPlanetInfo.starLevel,
            planetLevel: userPlanetInfo.level,
            planetCollectionStat: 0
        );

        PlanetManager.Instance.SetActivePlanet(planetId);
        if(PlanetStatManager.Instance != null)
        {
            PlanetStatManager.Instance.UpdateCurrentPlanetStats();
        }

        RefreshPlanetPanelUI();

        if (planetInfoPanel.activeSelf)
        {
            var planetData = DataTableManager.PlanetTable.Get(planetId);
            planetInfoPanel.GetComponent<PlanetInfoUI>().Initialize(planetData, userPlanetInfo);
        }

        planetInstallSuccessPanel.SetActive(true);

        AutoClosePanel().Forget();

        SaveInstallDataAsync(userPlanetData).Forget();
    }

    private void OnConfirmBtnClicked()
    {
        Cancel();

        planetInstallSuccessPanel.SetActive(false);
    }

    private async UniTaskVoid AutoClosePanel()
    {
        Cancel();

        try
        {
            await UniTask.Delay(3000, cancellationToken: autoCloseCts.Token);
            planetInstallSuccessPanel.SetActive(false);
        }
        catch (System.OperationCanceledException)
        {
            // Handle cancellation if needed
        }
    }  

    private async UniTaskVoid SaveInstallDataAsync(UserPlanetData userPlanetData)
    {
        try
        {
            await PlanetManager.Instance.SavePlanetsAsync();
            await UserPlanetManager.Instance.UpdateUserPlanetAsync(userPlanetData);
            await UserAttackPowerManager.Instance.UpdatePlanetPower(userPlanetData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SaveInstallDataAsync failed: {e.Message}");
        }
    }

    public void SetChoosedIndex(int planetIndex)
    {
        choosedIndex = planetIndex;

        int planetId = 300000 + planetIndex + 1;
        SetPlanetName(DataTableManager.PlanetTable.Get(planetId).PlanetName);
    }
}
