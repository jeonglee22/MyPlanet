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

    [SerializeField] private SettingPanel settingPanel;

    private void Start()
    {
        ResetBtn();

        playBtn.onClick.AddListener(OnPlayBtnClicked);
        storeBtn.onClick.AddListener(OnStoreBtnClicked);
        collectionBtn.onClick.AddListener(OnCollectionBtnClicked);
        planetBtn.onClick.AddListener(OnPlanetBtnClicked);
        towerBtn.onClick.AddListener(OnTowerBtnClicked);
        setBtn.onClick.AddListener(OnSettingBtnClicked);

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

    private void OnPlayBtnClicked()
    {
        SceneControlManager.Instance.LoadScene(SceneName.StageSelectScene).Forget();
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
