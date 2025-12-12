using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlanetTowerUI : MonoBehaviour
{
    [SerializeField] private Button battleButton;
    [SerializeField] private Button goToTitleButton;
    [SerializeField] private TowerInfoUI towerInfoUI;
    [SerializeField] private TowerInstallControl installControl;
    [SerializeField] private RectTransform planetCenter;
    [SerializeField] private GameObject backConfirmPanel;
    [SerializeField] private Button backYexBtn;
    [SerializeField]  private Button backNoBtn;
    private RectTransform dragAreaRect;
    private TowerUpgradeSlotUI towerUpgradeSlotUI;

    public float Angle { get; set; }
    public int TowerCount { get; set; }
    public bool TowerRotateClock { get; private set; }

    private float dragBeforePosX;
    private bool isStartDrag = false;
    public bool IsStartDrag => isStartDrag;

    private Vector2 circleCenter = new Vector2(0f, -20f);
    private float controlRadius;

    private bool isOpen = false;
    private bool isBackBtnClicked = false;
    public bool IsBackBtnClicked => isBackBtnClicked;

    void Awake()
    {
        towerUpgradeSlotUI = GetComponent<TowerUpgradeSlotUI>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        battleButton.onClick.AddListener(OnStartBattelClicked);
        goToTitleButton?.onClick.AddListener(OnGoToTitleClicked);

        Angle = 0f;
        
        battleButton.gameObject.SetActive(!towerUpgradeSlotUI.IsFirstInstall);
        if(installControl != null)
            dragAreaRect = installControl.gameObject.GetComponent<RectTransform>();

        controlRadius = Screen.width * 0.5f;

        backYexBtn.onClick.AddListener(OnBackYesClicked);
        backNoBtn.onClick.AddListener(OnBackNoClicked);
        backConfirmPanel.SetActive(false);
    }

    void OnEnable()
    {
        if (!isOpen)
        {
            GamePauseManager.Instance.Pause();
            isOpen = true;
        }

        battleButton.gameObject.SetActive(!towerUpgradeSlotUI.IsFirstInstall);
    }

    void OnDisable()
    {
        isOpen = false;
    }

    void Update()
    {
        // if (towerUpgradeSlotUI != null)
        // {
        //     if(battleButton.gameObject.activeSelf == towerUpgradeSlotUI.IsFirstInstall)
        //         battleButton.gameObject.SetActive(!towerUpgradeSlotUI.IsFirstInstall);
        // }

        if(installControl == null || isBackBtnClicked)
            return;

        if(!TouchManager.Instance.IsTouching)
        {
            isStartDrag = false;
            return;
        }

        if (installControl.CurrentDragGhost != null)
            return;

        if (RectTransformUtility.RectangleContainsScreenPoint(dragAreaRect, TouchManager.Instance.StartTouchPos) &&
            RectTransformUtility.RectangleContainsScreenPoint(dragAreaRect, TouchManager.Instance.TouchPos) &&
            Vector2.Distance(TouchManager.Instance.TouchPos, planetCenter.position) < controlRadius &&
            TouchManager.Instance.TouchPos.y > planetCenter.position.y + 20f)
            OnDragTowerSlots();
    }

    private void OnDragTowerSlots()
    {
        if(!TouchManager.Instance.IsDragging)
        {
            isStartDrag = false;
            return;
        }

        if(!isStartDrag)
        {
            dragBeforePosX = TouchManager.Instance.TouchPos.x;
            isStartDrag = true;
        }

        float touchPosX = TouchManager.Instance.TouchPos.x;
        float deltaX = touchPosX - dragBeforePosX;

        if (TowerRotateClock != (deltaX > 0f))
        {
            Angle = installControl.CurrentAngle;
        }
        Angle -= deltaX * 0.1f;
        TowerRotateClock = deltaX > 0f;
        dragBeforePosX = touchPosX;
    }
    
    private void OnStartBattelClicked()
    {
        if (towerInfoUI != null)
            towerInfoUI.gameObject.SetActive(false);
        gameObject.SetActive(false);
        GamePauseManager.Instance.Resume();
    }

    private void OnGoToTitleClicked()
    {
        goToTitleButton.interactable = false;
        battleButton.interactable = false;

        backConfirmPanel.SetActive(true);
        isBackBtnClicked = true;
    }

    private void OnBackYesClicked()
    {
        UserTowerManager.Instance.UpdateUserTowerDataAsync(installControl).Forget();

        SceneControlManager.Instance.LoadScene(SceneName.LobbyScene).Forget();
    }

    private void OnBackNoClicked()
    {
        backConfirmPanel.SetActive(false);
        goToTitleButton.interactable = true;
        battleButton.interactable = true;

        isBackBtnClicked = false;
    }
}
