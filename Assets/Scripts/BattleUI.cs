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
    private int lastDisplayedWave = 0;
    private float battleTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        statusUIButton.onClick.AddListener(OnOpenTowerStatusClicked);
        battleTime = 0f;

        stageText.text = $"STAGE {Variables.Stage}";

        WaveManager.Instance.WaveChange += OnWaveChanged;
        SpawnManager.Instance.OnBossSpawn += OnWaveChanged;

        waveToggles[0].isOn = true;
    }

    public void OnDestroy()
    {
        WaveManager.Instance.WaveChange -= OnWaveChanged;
        SpawnManager.Instance.OnBossSpawn -= OnWaveChanged;
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
        Time.timeScale = 0f;
    }

    private void OnOpenTowerStatusClicked()
    {
        towerInstallUiObj.GetComponent<TowerUpgradeSlotUI>().IsNotUpgradeOpen = true;
        towerInstallUiObj.SetActive(true);
        Time.timeScale = 0f;
    }

    private void SetBattleTimeText(int minutes, int seconds)
    {
        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void UpdateWaveToggles()
    {
        int currentWave = WaveManager.Instance.WaveCount;

        foreach(var toggle in waveToggles)
        {
            toggle.isOn = false;
        }

        int toggleIndex = -1;

        switch (currentWave)
        {
            case 1:
                toggleIndex = 0;
                break;
            case 2:
                toggleIndex = 1;
                break;
            case 3:
                toggleIndex = Variables.MiddleBossEnemy != null ? 3 : 2;
                break;
            case 4:
                toggleIndex = 4;
                break;
            case 5:
                toggleIndex = (Variables.MiddleBossEnemy != null || Variables.LastBossEnemy != null) ? 6 : 5;;
                break;
        }

        if(toggleIndex >= 0 && toggleIndex < waveToggles.Count)
        {
            waveToggles[toggleIndex].isOn = true;
        }
    }

    public void OnWaveChanged()
    {
        UpdateWaveToggles();
    }
}
