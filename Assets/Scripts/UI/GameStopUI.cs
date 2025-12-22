using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameStopUI : MonoBehaviour
{
    [SerializeField] private Button gameResumeButton;
    [SerializeField] private Button goToHomeButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button detailButton;

    [SerializeField] private TextMeshProUGUI enemyKillCountText;
    [SerializeField] private TextMeshProUGUI coinCountText;

    [SerializeField] private List<GameObject> attackTowerObjects;
    [SerializeField] private List<GameObject> amplifierTowerObjects;

    [SerializeField] private GameObject confirmPanel;

    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    [SerializeField] private Planet planet;

    public object OnConfirmYe { get; private set; }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameResumeButton.onClick.AddListener(OnGameResumeClicked);
        goToHomeButton.onClick.AddListener(OnGoToHomeClicked);
        settingButton.onClick.AddListener(OnOpenSettingClicked);
        detailButton.onClick.AddListener(OnOpenDetailClicked);

        confirmYesButton.onClick.AddListener(OnConfirmYesClicked);
        confirmNoButton.onClick.AddListener(OnConfirmNoClicked);
    }

    void OnEnable()
    {
        confirmPanel.SetActive(false);
    }

    private void OnConfirmYesClicked()
    {
        confirmPanel.SetActive(false);
        SceneControlManager.Instance.LoadScene(SceneName.LobbyScene).Forget();
    }

    private void OnConfirmNoClicked()
    {
        confirmPanel.SetActive(false);
    }

    private void OnOpenDetailClicked()
    {
    }

    private void OnOpenSettingClicked()
    {
    }

    private void OnGoToHomeClicked()
    {
        confirmPanel.SetActive(true);
    }

    private void OnGameResumeClicked()
    {
        gameObject.SetActive(false);
        GamePauseManager.Instance.Resume();
    }

    public void SettingTowerObjects()
    {
        foreach (var towerObj in attackTowerObjects)
        {
            towerObj.SetActive(false);
        }
        foreach (var towerObj in amplifierTowerObjects)
        {
            towerObj.SetActive(false);
        }

        var towerCount = planet.TowerCount;
        
        int attackTowerIndex = 0;
        int amplifierTowerIndex = 0;

        for (int i = 0; i < towerCount; i++)
        {
            var attackTower = planet.GetAttackTowerToAmpTower(i);
            var amplifierTower = planet.GetAmplifierTower(i);

            if (attackTower != null)
            {
                var attackTowerObject = attackTowerObjects[attackTowerIndex++];
                attackTowerObject.SetActive(true);
                var towerSetting = attackTowerObject.GetComponent<TowerPanelSetttingUI>();
                towerSetting.SetTowerPanel(attackTower.AttackTowerData.towerIdInt);
                towerSetting.SetTowerLevel(attackTower.ReinforceLevel);
            }
            else if (amplifierTower != null)
            {
                var amplifierTowerObject = amplifierTowerObjects[amplifierTowerIndex++];
                amplifierTowerObject.SetActive(true);
                var towerSetting = amplifierTowerObject.GetComponent<TowerPanelSetttingUI>();
                towerSetting.SetTowerPanel(amplifierTower.AmplifierTowerData.BuffTowerId);
                towerSetting.SetTowerLevel(amplifierTower.ReinforceLevel);
            }
        }
    }

    public void SetEnemyKillCountText(int count)
    {
        enemyKillCountText.text = count.ToString();
    }

    public void SetCoinCountText(int count)
    {
        coinCountText.text = count.ToString();
    }


}
