using UnityEngine;
using UnityEngine.UI;

public class PlanetTowerUI : MonoBehaviour
{
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    public float Angle { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        leftButton.onClick.AddListener(OnLetfMoveClicked);
        rightButton.onClick.AddListener(OnRightMoveClicked);

        Angle = 0f;
    }

    private void OnLetfMoveClicked()
    {
        Angle += 30f;
    }
    
    private void OnRightMoveClicked()
    {
        Angle -= 30f;   
    }
}
