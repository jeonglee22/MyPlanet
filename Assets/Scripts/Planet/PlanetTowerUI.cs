using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlanetTowerUI : MonoBehaviour
{
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button battleButton;
    [SerializeField] private Button goToTitleButton;
    [SerializeField] private TowerInfoUI towerInfoUI;
    private TowerUpgradeSlotUI towerUpgradeSlotUI;

    public float Angle { get; private set; }
    public int TowerCount { get; set; }
    public bool TowerRotateClock { get; private set; }

    void Awake()
    {
        towerUpgradeSlotUI = GetComponent<TowerUpgradeSlotUI>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        leftButton.onClick.AddListener(OnLetfMoveClicked);
        rightButton.onClick.AddListener(OnRightMoveClicked);
        battleButton.onClick.AddListener(OnStartBattelClicked);
        goToTitleButton?.onClick.AddListener(OnGoToTitleClicked);

        Angle = 0f;
        Time.timeScale = 0f;
        battleButton.gameObject.SetActive(!towerUpgradeSlotUI.IsFirstInstall);
    }

    void Update()
    {
        if (towerUpgradeSlotUI != null)
        {
            if(battleButton.gameObject.activeSelf != towerUpgradeSlotUI.IsFirstInstall)
                return;

            battleButton.gameObject.SetActive(!towerUpgradeSlotUI.IsFirstInstall);
        }
    }

    private void OnLetfMoveClicked()
    {
        Angle -= 360f / TowerCount;
        TowerRotateClock = true;
    }

    private void OnRightMoveClicked()
    {
        Angle += 360f / TowerCount;
        TowerRotateClock = false;
    }
    
    private void OnStartBattelClicked()
    {
        if (towerInfoUI != null)
            towerInfoUI.gameObject.SetActive(false);
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnGoToTitleClicked()
    {
        SceneControlManager.Instance.LoadScene(SceneName.LoginScene).Forget();
    }
}
