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

    private void Start()
    {
        ResetBtn();

        playBtn.onClick.AddListener(OnPlayBtnClicked);
        storeBtn.onClick.AddListener(OnStoreBtnClicked);
        collectionBtn.onClick.AddListener(OnCollectionBtnClicked);
        planetBtn.onClick.AddListener(OnPlanetBtnClicked);
        towerBtn.onClick.AddListener(OnTowerBtnClicked);
        
        gachaMainPanel.SetActive(false);
        collectionPanel.SetActive(false);
        planetPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        ResetBtn();
    }

    private void ResetBtn()
    {
        playBtn.onClick.RemoveListener(OnPlayBtnClicked);
        storeBtn.onClick.RemoveListener(OnStoreBtnClicked);
        collectionBtn.onClick.RemoveListener(OnCollectionBtnClicked);
        planetBtn.onClick.RemoveListener(OnPlanetBtnClicked);
        towerBtn.onClick.RemoveListener(OnTowerBtnClicked);
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
}
