using System;
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
        planet.HpDecreseEvent += HpValueChanged;
        planet.expUpEvent += ExpValueChange;

        Initialize();
        //test
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDestroy()
    {
        planet.expUpEvent -= ExpValueChange;
        planet.HpDecreseEvent -= HpValueChanged;
    }

    private void Initialize()
    {
        HpValueChanged(planet.Health);
        ExpValueChange(planet.CurrentExp);
    }

    private void HpValueChanged(float hp)
    {
        hpSlider.value = hp / planet.MaxHealth;
    }

    private void ExpValueChange(float exp)
    {
        expSlider.value = exp / planet.MaxExp;
    }

    public void AddExp()
    {
        planet.CurrentExp += 10f;
    }
}
