using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlanetStatusUI : MonoBehaviour
{
    [SerializeField] private Planet planet;

    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider expSlider;
    [SerializeField] private Slider barriorSlider;
    [SerializeField] private GameObject towerSettingUi;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TowerInstallControl towerInstallControl;

    private bool isTutorial = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        planet.HpDecreseEvent += HpValueChanged;
        planet.OnHealthChanged += HpValueChanged;
        planet.expUpEvent += ExpValueChange;
        planet.OnBarriorChanged += BarriorValueChange;
        planet.levelUpEvent += OpenTowerUpgradeUI;
        planet.levelUpEvent += ChangeLevelText;

        towerSettingUi.SetActive(true);
        Initialize();

        SetIsTutorial(TutorialManager.Instance.IsTutorialMode);
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
        planet.OnHealthChanged -= HpValueChanged;
        planet.OnBarriorChanged -= BarriorValueChange;
        planet.levelUpEvent -= OpenTowerUpgradeUI;
        planet.levelUpEvent -= ChangeLevelText;
    }

    private void Initialize()
    {
        HpValueChanged(planet.Health);
        ExpValueChange(planet.CurrentExp);
        if (planet.InitShield <= 0f)
            barriorSlider.value = 0f;
        else
            barriorSlider.value = 1f;
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
        
        towerInstallControl.isInstall = false;

        if(isTutorial && Variables.Stage == 1)
        {
            TutorialManager.Instance.ShowTutorialStep(2);
        }
    }

    private void HpValueChanged(float hp)
    {
        hpSlider.value = hp / planet.MaxHealth;
    }

    private void ExpValueChange(float exp)
    {
        expSlider.value = exp / planet.MaxExp;
    }

    private void BarriorValueChange(float barrior)
    {
        if (planet.InitShield <= 0f)
        {
            barriorSlider.value = 0f;
            return;
        }

        barriorSlider.value = barrior / planet.InitShield;
    }

    public void AddExp(float exp = 10f)
    {
        planet.AddExp(exp);
    }

    private void SetIsTutorial(bool isTutorialMode)
    {
        isTutorial = isTutorialMode;
    }
}
