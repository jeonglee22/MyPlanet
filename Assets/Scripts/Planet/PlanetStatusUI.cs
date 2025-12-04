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

    private bool isTutorial = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        planet.HpDecreseEvent += HpValueChanged;
        planet.expUpEvent += ExpValueChange;
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

    public void AddExp(float exp = 10f)
    {
        planet.CurrentExp += exp;
    }

    private void SetIsTutorial(bool isTutorialMode)
    {
        isTutorial = isTutorialMode;
    }
}
