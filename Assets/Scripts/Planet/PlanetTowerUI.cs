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
    private RectTransform dragAreaRect;
    private TowerUpgradeSlotUI towerUpgradeSlotUI;

    public float Angle { get; private set; }
    public int TowerCount { get; set; }
    public bool TowerRotateClock { get; private set; }

    private float dragBeforePosX;
    private bool isStartDrag = false;
    public bool IsStartDrag => isStartDrag;

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
    }

    void OnEnable()
    {
        Time.timeScale = 0f;
    }

    void Update()
    {
        if (towerUpgradeSlotUI != null)
        {
            if(battleButton.gameObject.activeSelf == towerUpgradeSlotUI.IsFirstInstall)
                battleButton.gameObject.SetActive(!towerUpgradeSlotUI.IsFirstInstall);
        }

        if(installControl == null)
            return;

        if(!TouchManager.Instance.IsTouching)
        {
            isStartDrag = false;
            return;
        }

        if (RectTransformUtility.RectangleContainsScreenPoint(dragAreaRect, TouchManager.Instance.StartTouchPos) &&
            RectTransformUtility.RectangleContainsScreenPoint(dragAreaRect, TouchManager.Instance.TouchPos))
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
        SceneControlManager.Instance.LoadScene(SceneName.StageSelectScene).Forget();
    }
}
