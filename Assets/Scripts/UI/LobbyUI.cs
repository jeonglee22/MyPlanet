using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private GameObject gachaMainPanel;
    [SerializeField] private GameObject collectionPanel;

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
        
        gachaMainPanel.SetActive(false);
        collectionPanel.SetActive(false);
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
}
