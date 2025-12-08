using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
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
    }

    private void OnPlayBtnClicked()
    {
        SceneControlManager.Instance.LoadScene(SceneName.StageSelectScene).Forget();
    }

    private void OnStoreBtnClicked()
    {
        SceneControlManager.Instance.LoadScene(SceneName.StoreScene).Forget();
    }
}
