using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlanetStatusUI : MonoBehaviour
{
    [SerializeField] private Planet planet;

    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider expSlider;
    [SerializeField] private GameObject towerSettingUi;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TowerInstallControl towerInstallControl;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        planet.HpDecreseEvent += HpValueChanged;
        planet.expUpEvent += ExpValueChange;
        planet.levelUpEvent += OpenTowerUpgradeUI;
        planet.levelUpEvent += ChangeLevelText;

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
        planet.levelUpEvent -= ChangeLevelText;
    }

    private void Initialize()
    {
        HpValueChanged(planet.Health);
        ExpValueChange(planet.CurrentExp);
        ChangeLevelText();
    }

     private void ChangeLevelText()
    {
        if(levelText==null) 
            return;
        
        levelText.text = $"Level : {planet.Level}";
    }

    private void OpenTowerUpgradeUI()
    {
        towerSettingUi.SetActive(true);
            GamePauseManager.Instance.Pause();
        towerInstallControl.isInstall = false;
    }

    private void HpValueChanged(float hp)
    {
        hpSlider.value = hp / planet.MaxHealth;
    }

    private void ExpValueChange(float exp)
    {
        expSlider.value = exp / planet.MaxExp;
    }

    public async UniTaskVoid AddExp(float exp = 10f)
    {
        planet.CurrentExp += exp;
    }
}
