using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private GameObject gachaMainPanel;

    [SerializeField] private Button exitBtn;
    [SerializeField] private Button setBtn;
    [SerializeField] private Button planetBtn;
    [SerializeField] private Button towerBtn;
    [SerializeField] private Button collectionBtn;
    [SerializeField] private Button storeBtn;
    [SerializeField] private Button playBtn;

    private void Start()
    {
        playBtn.onClick.AddListener(OnPlayBtnClicked);
        storeBtn.onClick.AddListener(OnStoreBtnClicked);
        
        gachaMainPanel.SetActive(false);
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
}
