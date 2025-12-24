using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlanetPanelUI : MonoBehaviour
{
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

    private int choosedIndex = -1;


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

        AddBtnSound();

        InitializePlanetPanelUI();
    }

    private void AddBtnSound()
    {
        backBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        choosePlanetBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        saveYesBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        saveNoBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        homeBtn.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());

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

    public void OnBackBtnClicked()
    {
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
    }

    public void OnChoosePlanetBtnClicked()
    {
        saveConfirmPanel.SetActive(true);
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
}
