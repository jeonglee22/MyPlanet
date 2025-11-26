using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlanetTowerUI : MonoBehaviour
{
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button battleButton;
    [SerializeField] private TowerInfoUI towerInfoUI;

    public float Angle { get; private set; }
    public int TowerCount { get; set; }
    public bool TowerRotateClock { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        leftButton.onClick.AddListener(OnLetfMoveClicked);
        rightButton.onClick.AddListener(OnRightMoveClicked);
        battleButton.onClick.AddListener(OnStartBattelClicked);

        Angle = 0f;
        Time.timeScale = 0f;
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
}
