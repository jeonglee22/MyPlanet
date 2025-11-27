using UnityEngine;
using UnityEngine.UI;

public class DebugButtonsUI : MonoBehaviour
{
    [SerializeField] private Button debugButton;
    [SerializeField] private GameObject debugButtonsPanel;
    [SerializeField] private Toggle infiniteHPToggle;
    [SerializeField] private Toggle infItemToggle;
    [SerializeField] private Button expButton;
    [SerializeField] private PowerUpItemControlUI powerUpItemControlUI;

    private bool isInfiniteHP = false;
    public bool IsInfiniteHP => isInfiniteHP;
    private bool isInfItem = false;
    public bool IsInfItem => isInfItem;

    private float currentHP;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var planet = GameObject.FindWithTag(TagName.Planet).GetComponent<Planet>();
        debugButton.onClick.AddListener(() =>
        {
            debugButtonsPanel.SetActive(!debugButtonsPanel.activeSelf);
        });
        infiniteHPToggle.onValueChanged.AddListener((isOn) =>
        {
            isInfiniteHP = isOn;
            if(isOn)
            {
                currentHP = planet.Health;
                planet.Health = 1000000f;
                planet.MaxHealth = 1000000f;
                planet.OnDamage(0);
            }
            else
            {
                planet.MaxHealth = 200f;
                planet.Health = currentHP;
                planet.OnDamage(0);
            }
        });
        infItemToggle.onValueChanged.AddListener((isOn) =>
        {
            isInfItem = isOn;
            powerUpItemControlUI.IsInfiniteItem = isOn;
            powerUpItemControlUI.SetActiveItemUseButton(!isOn);
        });
        expButton.onClick.AddListener(() =>
        {
            planet.CurrentExp += 5f;
        });
        debugButtonsPanel.SetActive(false);
    }
}
