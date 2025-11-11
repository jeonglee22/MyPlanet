using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlanetStatusUI : MonoBehaviour
{
    [SerializeField] private Planet planet;

    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider expSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hpSlider.onValueChanged.AddListener(OnHpValueChanged);
        planet.expUpEvent += OnExpValueChange;

        Initialize();
        //test
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDestroy()
    {
        planet.expUpEvent -= OnExpValueChange;
    }

    private void Initialize()
    {
        hpSlider.value = planet.Health;
        expSlider.value = 0f;
    }

    private void OnHpValueChanged(float hpPercent)
    {
        hpSlider.value = hpPercent;
    }

    private void OnExpValueChange(float exp)
    {
        Debug.Log(exp);
        expSlider.value = exp / planet.MaxExp;
    }

    public void AddExp()
    {
        planet.CurrentExp += 10f;
    }
}
