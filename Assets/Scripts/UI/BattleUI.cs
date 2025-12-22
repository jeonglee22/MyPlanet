using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    [SerializeField] private Button statusUIButton;
    [SerializeField] private GameObject towerInstallUiObj;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI stageText;

    [SerializeField] private List<Toggle> waveToggles;
    [SerializeField] private List<Toggle> stageOneToggles;
    [SerializeField] private List<Toggle> stageTwoToggles;
    private List<Toggle> currentStageToggles;
    [SerializeField] private List<GameObject> toggleObjects;

    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private Slider bossHpSlider;

    [SerializeField] private TextMeshProUGUI enemyKillCountText;
    [SerializeField] private TextMeshProUGUI coinGainText;
    [SerializeField] private Button gamePauseButton;
    [SerializeField] private GameObject gamePausePanel;

    private float battleTime = 0f;

    private bool isTutorial = false;

    private int enemyKiilCount = 0;
    public int EnemyKiilCount { get => enemyKiilCount; set => enemyKiilCount = value; }

    private int coinGain = 0;
    public int CoinGain { get => coinGain; set => coinGain = value; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start()
    {
        statusUIButton.onClick.AddListener(OnOpenTowerStatusClicked);
        gamePauseButton.onClick.AddListener(OnGamePauseButtonClicked);
        statusUIButton.onClick.AddListener(() => SoundManager.Instance.PlayDeployOpen());
        gamePauseButton.onClick.AddListener(() => SoundManager.Instance.PlayClickSound());
        battleTime = 0f;

        stageText.text = $"STAGE {Variables.Stage}";

        WaveManager.Instance.WaveChange += OnWaveChanged;
        SpawnManager.Instance.OnBossSpawn += OnWaveChanged;

        SetCoinGainText();
        SetEnemyKillCountText();

        foreach(var toggleObj in toggleObjects)
        {
            toggleObj.SetActive(false);
        }

        switch(Variables.Stage)
        {
            case 1:
                toggleObjects[1].SetActive(true);
                currentStageToggles = stageOneToggles;
                break;
            case 2:
                toggleObjects[2].SetActive(true);
                currentStageToggles = stageTwoToggles;
                break;
            default:
                toggleObjects[0].SetActive(true);
                currentStageToggles = waveToggles;
                break;
        }

        currentStageToggles[0].isOn = true;

        bossNameText.gameObject.SetActive(false);
        bossHpSlider.gameObject.SetActive(false);

        SetIsTutorial(TutorialManager.Instance.IsTutorialMode);
    }

    private void OnGamePauseButtonClicked()
    {
        var gamePauseSetting = gamePausePanel.GetComponent<GameStopUI>();
        gamePauseSetting.SettingTowerObjects();

        gamePauseSetting.SetCoinCountText(coinGain);
        gamePauseSetting.SetEnemyKillCountText(enemyKiilCount);

        GamePauseManager.Instance.Pause();
        gamePausePanel.SetActive(true);
    }

    public void OnDestroy()
    {
        WaveManager.Instance.WaveChange -= OnWaveChanged;
        SpawnManager.Instance.OnBossSpawn -= OnWaveChanged;

        statusUIButton.onClick.RemoveAllListeners();
        gamePauseButton.onClick.RemoveAllListeners();
    }

    // Update is called once per frame
    void Update()
    {
        if(WaveManager.Instance.IsBossBattle)
        {
            return;
        }

        battleTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(battleTime / 60f);
        int seconds = Mathf.FloorToInt(battleTime % 60f);
        SetBattleTimeText(minutes, seconds);
    }
    
    private void OnOpenInstallUIClicked()
    {
        towerInstallUiObj.SetActive(true);
        
        GamePauseManager.Instance.Pause();
    }

    private void OnOpenTowerStatusClicked()
    {
        towerInstallUiObj.GetComponent<TowerUpgradeSlotUI>().IsNotUpgradeOpen = true;
        towerInstallUiObj.SetActive(true);
        GamePauseManager.Instance.Pause();
    }

    private void SetBattleTimeText(int minutes, int seconds)
    {
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void UpdateWaveToggles()
    {
        int currentStage = Variables.Stage;
        int currentWave = WaveManager.Instance.WaveCount;
        bool hasMiddleBoss = Variables.MiddleBossEnemy != null;
        bool hasLastBoss = Variables.LastBossEnemy != null;

        foreach(var toggle in currentStageToggles)
        {
            toggle.isOn = false;
        }

        int toggleIndex = -1;

        switch (currentStage)
        {
            case 1:
                if(currentWave == 3 && hasLastBoss)
                {
                    toggleIndex = 3;
                }
                else
                {
                    toggleIndex = currentWave - 1;
                }
                break;
            case 2:
                if(currentWave == 2 && hasMiddleBoss)
                {
                    toggleIndex = 2;
                }
                else if(currentWave == 3 && hasLastBoss)
                {
                    toggleIndex = 4;
                }
                else if(currentWave == 3)
                {
                    toggleIndex = 3;
                }
                else
                {
                    toggleIndex = currentWave - 1;
                }
                break;
            default:
                if(currentWave == 3 && hasMiddleBoss)
                {
                    toggleIndex = 3;
                }
                else if(currentWave == 5 && hasLastBoss)
                {
                    toggleIndex = 6;
                }
                else if(currentWave <= 3)
                {
                    toggleIndex = currentWave - 1;
                }
                else
                {
                    toggleIndex = currentWave;
                }
                break;
        }

        currentStageToggles[toggleIndex].isOn = true;
    }

    public void OnWaveChanged()
    {
        UpdateWaveToggles();
    }

    public void SetBossHp(string name, float currentHp, float maxHp)
    {
        if(!bossHpSlider.gameObject.activeSelf)
        {
            if(isTutorial && Variables.Stage == 1)
            {
                TutorialManager.Instance.ShowTutorialStep(5);
            }

            bossNameText.gameObject.SetActive(true);
            bossHpSlider.gameObject.SetActive(true);
        }
        bossNameText.text = name;
        bossHpSlider.value = currentHp / maxHp;
    }

    private void SetIsTutorial(bool isTutorialMode)
    {
        isTutorial = isTutorialMode;
    }

    public void AddEnemyKillCountText(int killCount)
    {
        enemyKiilCount += killCount;
        SetEnemyKillCountText();
    }

    public void AddEnemyKillCountText()
    {
        enemyKiilCount += 1;
        SetEnemyKillCountText();
    }

    public void SetEnemyKillCountText()
    {
        enemyKillCountText.text = $"{enemyKiilCount}";
    }

    public void AddCoinGainText(int coinGain)
    {
        this.coinGain += coinGain;
        SetCoinGainText();
    }

    public void SetCoinGainText()
    {
        coinGainText.text = $"{coinGain}";
    }
}
