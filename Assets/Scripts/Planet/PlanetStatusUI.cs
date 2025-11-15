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
    [SerializeField] private GameObject towerSettingUi;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        planet.HpDecreseEvent += HpValueChanged;
        planet.expUpEvent += ExpValueChange;
        planet.levelUpEvent += OpenTowerUpgradeUI;

        towerSettingUi.SetActive(true);
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
        planet.levelUpEvent -= OpenTowerUpgradeUI;
    }

    private void Initialize()
    {
        HpValueChanged(planet.Health);
        ExpValueChange(planet.CurrentExp);
    }

    private void OpenTowerUpgradeUI()
    {
        towerSettingUi.SetActive(true);
            Time.timeScale = 0f;
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
