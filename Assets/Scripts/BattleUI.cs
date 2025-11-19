using Cysharp.Threading.Tasks.Triggers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    [SerializeField] private Button statusUIButton;
    [SerializeField] private GameObject towerInstallUiObj;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI enemyCountText;
    private float battleTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        statusUIButton.onClick.AddListener(OnOpenTowerStatusClicked);
        battleTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        battleTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(battleTime / 60f);
        int seconds = Mathf.FloorToInt(battleTime % 60f);
        SetBattleTimeText(minutes, seconds);
        SetEnemyCountText(SpawnManager.Instance.CurrentEnemyCount);
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

    public void SetEnemyCountText(int currentCount)
    {
        enemyCountText.text = $"{currentCount}";
    }
}
