using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlanetPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject planetInfoPanel;

    [SerializeField] private Button backBtn;
    [SerializeField] private Button choosePlanetBtn;

    [SerializeField] private GameObject saveConfirmPanel;
    [SerializeField] private Button saveYesBtn;
    [SerializeField] private Button saveNoBtn;

    [SerializeField] private Button[] planetButtons;
    [SerializeField] private TextMeshProUGUI planetNameText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < planetButtons.Length; i++)
        {
            int index = i;
            planetButtons[i].onClick.AddListener(() => OnPlanetButtonClicked(index));
        }

        backBtn.onClick.AddListener(OnBackBtnClicked);
        choosePlanetBtn.onClick.AddListener(OnChoosePlanetBtnClicked);
        saveYesBtn.onClick.AddListener(() => OnSaveYesBtnClicked().Forget());
        saveNoBtn.onClick.AddListener(OnSaveNoBtnClicked);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBackBtnClicked()
    {
        lobbyPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    public void OnPlanetButtonClicked(int planetIndex)
    {
        // Assuming planet IDs start from 300000 and increment by 1
        int planetId = 300001 + planetIndex;
        Variables.planetId = planetId;

        // Update the planet name display
        SetPlanetName(DataTableManager.PlanetTable.Get(planetId).PlanetName);
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

        var userPlanetData = new UserPlanetData(
            AuthManager.Instance.UserNickName,
            planetId: Variables.planetId,
            planetUpgrade: 0,
            planetLevel: 1,
            planetCollectionStat: 0
        );
        
        await UserPlanetManager.Instance.UpdateUserPlanetAsync(userPlanetData);
        await UserAttackPowerManager.Instance.UpdatePlanetPower(userPlanetData);
    }

    public void OnSaveNoBtnClicked()
    {
        saveConfirmPanel.SetActive(false);
    }

    public void SetPlanetName(string planetName)
    {
        planetNameText.text = planetName;
    }

    private float CalculatePlanetPower(int planetId, int planetLevel, int planetUpgrade)
    {
        var planetData = DataTableManager.PlanetTable.Get(planetId);
        // var planetUpgradeData = DataTableManager.PlanetUpgradeTable.Get(planetUpgrade);
        // var planetLevelData = DataTableManager.PlanetLevelTable.Get(planetLevel);

        var baseAttack = planetData.PlanetHp * (100 + planetData.PlanetArmor) * 0.01f;
        var totalAttackPower = baseAttack + planetData.PlanetShield + planetData.RecoveryHp * 420f + planetData.Drain * 100f;

        return totalAttackPower;
    }
}
