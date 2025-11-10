using UnityEngine;
using UnityEngine.UI;

public class PlanetTowerUI : MonoBehaviour
{
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    public float Angle { get; private set; }
    public int TowerCount { get; set; }
    public bool TowerRotateClock { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        leftButton.onClick.AddListener(OnLetfMoveClicked);
        rightButton.onClick.AddListener(OnRightMoveClicked);

        Angle = 0f;
    }

    private void OnLetfMoveClicked()
    {
        Angle += 360f / TowerCount;
        TowerRotateClock = false;
    }
    
    private void OnRightMoveClicked()
    {
        Angle -= 360f / TowerCount;
        TowerRotateClock = true;
    }
}
