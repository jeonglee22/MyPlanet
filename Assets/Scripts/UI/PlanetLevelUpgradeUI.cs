using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlanetLevelUpgradeUI : MonoBehaviour
{
    [SerializeField] private Image planetIcon;
    [SerializeField] private TextMeshProUGUI planetNameText;
    [SerializeField] private TextMeshProUGUI attackPowerText;

    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI nextLevelText;

    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI defenseText;
    [SerializeField] private TextMeshProUGUI itemCountText;

    [SerializeField] private Button levelUpButton;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color canUpgradeColor;

    [SerializeField] private PlanetPanelUI planetPanelUI;
    [SerializeField] private PlanetInfoUI planetInfoUI;

    private PlanetData currentPlanetData;
    private UserPlanetInfo currentUserPlanetInfo;
    private PlanetLvUpgradeData planetLvUpgradeData;
    private PlanetStats currentStats;

    private CancellationTokenSource holdCts;
    private float repeatInterval = 0.1f;
    private float holdDelay = 0.5f;

    private void Start()
    {
        AddEventTrigger(levelUpButton.gameObject);
    }

    private void OnDestroy()
    {
        Cancel();
    }

    private void OnDisable()
    {
        Cancel();
    }

    private void Cancel()
    {
        holdCts?.Cancel();
        holdCts?.Dispose();
        holdCts = new CancellationTokenSource();
    }

    public void Initialize(PlanetData planetData, UserPlanetInfo userPlanetInfo)
    {
        currentPlanetData = planetData;
        currentUserPlanetInfo = userPlanetInfo;

        planetIcon.sprite = LoadManager.GetLoadedGameTexture(planetData.PlanetImage);

        var planetTextData = DataTableManager.PlanetTextTable.Get(planetData.PlanetText_ID);
        planetNameText.text = planetTextData.PlanetName;

        UpdateAllUI();
    }

    private void UpdateAllUI()
    {
        UpdateLevelDisplay();
        UpdateStatsDisplay();
        UpdateFightingPower();
        UpdateItemCount();
        UpdateLevelUpButton();
    }

    private void UpdateLevelDisplay()
    {
        int maxLevel = currentUserPlanetInfo.starLevel * 10;
        currentLevelText.text = $"Lv. {currentUserPlanetInfo.level} / Lv. {maxLevel}";

        if(currentUserPlanetInfo.level >= maxLevel)
        {
            nextLevelText.text = "MAX";
        }
        else
        {
            nextLevelText.text = $"Lv. {currentUserPlanetInfo.level + 1} / Lv. {maxLevel}";
        }
    }

    private void UpdateStatsDisplay()
    {
        int maxLevel = currentUserPlanetInfo.starLevel * 10;

        var currentStats = PlanetStatManager.Instance.GetPlanetStatsPreview(
            currentPlanetData.Planet_ID, 
            currentUserPlanetInfo.level, 
            currentUserPlanetInfo.starLevel);

        if(currentUserPlanetInfo.level >= maxLevel)
        {
            healthText.text = $"{currentStats.hp:F1}";
            defenseText.text = $"{currentStats.defense:F1}";
        }
        else
        {
            var nextStats = PlanetStatManager.Instance.GetPlanetStatsPreview(
                currentPlanetData.Planet_ID, 
                currentUserPlanetInfo.level + 1, 
                currentUserPlanetInfo.starLevel);

            float hpDiff = nextStats.hp - currentStats.hp;
            float defenseDiff = nextStats.defense - currentStats.defense;

            healthText.text = $"{FormatStat(currentStats.hp)} → {FormatStat(nextStats.hp)} <color=green>(+{FormatStat(hpDiff)})</color>";
            defenseText.text = $"{FormatStat(currentStats.defense)} → {FormatStat(nextStats.defense)} " + $"<color=green>(+{FormatStat(defenseDiff)})</color>";
        }
    }

    private void UpdateFightingPower()
    {
        var currentStats = PlanetStatManager.Instance.GetPlanetStatsPreview(
            currentPlanetData.Planet_ID, 
            currentUserPlanetInfo.level, 
            currentUserPlanetInfo.starLevel);

        var cal = (currentStats.hp * (100 + currentStats.defense) / 100f) + 
                  currentStats.shield + 
                  (currentStats.hpRegeneration * 420) + 
                  (currentStats.drain * 650);

        attackPowerText.text = FormatStat(cal);
    }

    private void UpdateItemCount()
    {
        int maxLevel = currentUserPlanetInfo.starLevel * 10;

        if(currentUserPlanetInfo.level >= maxLevel)
        {
            itemCountText.text = "MAX";
            return;
        }

        int nextLevel = currentUserPlanetInfo.level + 1;
        planetLvUpgradeData = DataTableManager.PlanetLvUpgradeTable.GetCurrentLevelData(currentPlanetData.Planet_ID, nextLevel);

        if(planetLvUpgradeData == null)
        {
            itemCountText.text = "데이터 없음";
            return;
        }

        int currentItem = UserData.PlanetEnhanceItem;
        int requiredItem = planetLvUpgradeData.UpgradeResource;

        if(currentItem >= requiredItem)
        {
            itemCountText.text = $"{currentItem} / {requiredItem}";
        }
        else
        {
            itemCountText.text = $"<color=red>{currentItem}</color> / {requiredItem}";
        }
    }

    private void UpdateLevelUpButton()
    {
        int maxLevel = currentUserPlanetInfo.starLevel * 10;

        if(currentUserPlanetInfo.level >= maxLevel)
        {
            levelUpButton.interactable = false;
            levelUpButton.GetComponent<Image>().color = defaultColor;
            return;
        }

        int nextLevel = currentUserPlanetInfo.level + 1;
        planetLvUpgradeData = DataTableManager.PlanetLvUpgradeTable.GetCurrentLevelData(
            currentPlanetData.Planet_ID, nextLevel);

        if(planetLvUpgradeData == null)
        {
            levelUpButton.interactable = false;
            levelUpButton.GetComponent<Image>().color = defaultColor;
            return;
        }

        int currentItem = UserData.PlanetEnhanceItem;
        int requiredItem = planetLvUpgradeData.UpgradeResource;

        if(currentItem >= requiredItem)
        {
            levelUpButton.interactable = true;
            levelUpButton.GetComponent<Image>().color = canUpgradeColor;
        }
        else
        {
            levelUpButton.interactable = false;
            levelUpButton.GetComponent<Image>().color = defaultColor;
        }
    }

    private async UniTaskVoid OnLevelUpButtonClicked()
    {
        int maxLevel = currentUserPlanetInfo.starLevel * 10;

        if(currentUserPlanetInfo.level >= maxLevel)
        {
            Debug.Log("최대 레벨 도달");
            return;
        }

        int nextLevel = currentUserPlanetInfo.level + 1;
        planetLvUpgradeData = DataTableManager.PlanetLvUpgradeTable.GetCurrentLevelData(currentPlanetData.Planet_ID, nextLevel);

        if(planetLvUpgradeData == null)
        {
            Debug.LogError("레벨 업그레이드 데이터 없음");
            return;
        }

        int currentItem = UserData.PlanetEnhanceItem;
        int requiredItem = planetLvUpgradeData.UpgradeResource;

        if(currentItem < requiredItem)
        {
            Debug.Log("골드 부족");
            return;
        }

        UserData.PlanetEnhanceItem -= requiredItem;
        PlanetManager.Instance.LevelUpPlanet(currentPlanetData.Planet_ID);

        if(currentPlanetData.Planet_ID == PlanetManager.Instance.ActivePlanetId)
        {
            PlanetStatManager.Instance?.UpdateCurrentPlanetStats();
        }

        UpdateAllUI();
        planetPanelUI.RefreshPlanetPanelUI();
        planetInfoUI.Initialize(currentPlanetData, currentUserPlanetInfo);

        SaveDataAsync().Forget();
    }

    private async UniTask SaveDataAsync()
    {
        try
        {
            await CurrencyManager.Instance.SaveCurrencyAsync();
            await PlanetManager.Instance.SavePlanetsAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"PlanetLevelUpgradeUI SaveDataAsync failed: {e.Message}");
        }
    }

    public void RefreshUI()
    {
        if(currentPlanetData != null && currentUserPlanetInfo != null)
        {
            Initialize(currentPlanetData, currentUserPlanetInfo);
        }
    }

    private void AddEventTrigger(GameObject target)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if(trigger == null)
        {
            trigger = target.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => { OnPointerDownButton(); });
        trigger.triggers.Add(pointerDown);

        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => { OnPointerUpButton(); });
        trigger.triggers.Add(pointerUp);

        EventTrigger.Entry pointerExit = new EventTrigger.Entry();
        pointerExit.eventID = EventTriggerType.PointerExit;
        pointerExit.callback.AddListener((data) => { OnPointerUpButton(); });
        trigger.triggers.Add(pointerExit);
    }

    private void OnPointerDownButton()
    {
        Cancel();

        SoundManager.Instance.PlayClickSound();
        OnLevelUpButtonClicked().Forget();
        HoldButtonAsync(holdCts.Token).Forget();
    }

    private void OnPointerUpButton()
    {
        Cancel();
    }

    private async UniTaskVoid HoldButtonAsync(CancellationToken token)
    {
        try
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(holdDelay), cancellationToken: token);

            while (true)
            {
                OnLevelUpButtonClicked().Forget();
                await UniTask.Delay(System.TimeSpan.FromSeconds(repeatInterval), cancellationToken: token);
            }
        }
        catch (OperationCanceledException)
        {
            // 취소 시 예외 무시
        }
    }

    private string FormatStat(float value)
    {
        return value % 1 == 0 ? $"{value:F0}" : $"{value:F1}";
    }
}
