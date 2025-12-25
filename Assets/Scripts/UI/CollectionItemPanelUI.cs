using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CollectionItemPanelUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI towerNameText;
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI rateText;
    [SerializeField] private TextMeshProUGUI weightText;
    [SerializeField] private Button weightUpBtn;
    [SerializeField] private Button weightDownBtn;
    [SerializeField] private Button infoBtn;

    private bool isTower = false;
    private bool isAttackTower = false;
    private bool isBuffTower = false;

    public bool IsTower => isTower;
    public bool IsAttackTower => isAttackTower;
    public bool IsBuffTower => isBuffTower;

    private int weightId;

    public event Action OnWeightChanged;
    public event Action<CollectionItemPanelUI> OnInfoBtn;

    public AttackTowerTableRow AttackTowerData { get; private set; }
    public BuffTowerData BuffTowerData { get; private set; }
    public RandomAbilityData AbilityData { get; private set; }

    private CancellationTokenSource holdCts;
    [SerializeField] private float repeatInterval = 0.1f;
    [SerializeField] private float holdDelay = 0.5f;

    private void Awake()
    {
        AddEventTrigger(weightUpBtn.gameObject, true);
        AddEventTrigger(weightDownBtn.gameObject, false);
    }

    private void OnDestroy()
    {
        holdCts?.Cancel();
        holdCts?.Dispose();
        holdCts = null;

        infoBtn.onClick.RemoveListener(OnInfoBtnClicked);
    }

    private void OnDisable()
    {
        holdCts?.Cancel();
        holdCts?.Dispose();
        holdCts = null;
    }

    public void Initialize(AttackTowerTableRow data, int dataCount, bool isTower)
    {
        var textData = DataTableManager.TowerExplainTable.Get(data.TowerText_ID);

        towerNameText.text = textData.TowerName;

        weightId = data.AttackTower_Id;
        this.isTower = isTower;
        isAttackTower = true;

        AttackTowerData = data;

        UpdateWeightDisplay();

        infoBtn.onClick.AddListener(OnInfoBtnClicked);

        var towerId = data.AttackTower_Id;
        var towerData = DataTableManager.AttackTowerTable.GetById(towerId);
        var towerIconName = towerData.AttackTowerAsset;
        var iconSprite = LoadManager.GetLoadedGameTexture(towerIconName);
        iconImg.sprite = iconSprite;
    }

    public void Initialize(BuffTowerData data, int dataCount, bool isTower)
    {
        var textData = DataTableManager.TowerExplainTable.Get(data.TowerText_ID);

        towerNameText.text = textData.TowerName;

        weightId = data.BuffTower_ID;
        this.isTower = isTower;
        isBuffTower = true;

        BuffTowerData = data;

        UpdateWeightDisplay();

        infoBtn.onClick.AddListener(OnInfoBtnClicked);

        var towerId = data.BuffTower_ID;
        var towerData = DataTableManager.BuffTowerTable.Get(towerId);
        var towerIconName = towerData.BuffTowerAsset;
        var iconSprite = LoadManager.GetLoadedGameTexture(towerIconName);
        iconImg.sprite = iconSprite;
    }

    public void Initialize(RandomAbilityData data, int dataCount, bool isTower)
    {
        var textData = DataTableManager.RandomAbilityTextTable.Get(data.RandomAbilityText_ID);

        towerNameText.text = textData.RandomAbilityName;

        weightId = data.RandomAbility_ID;
        this.isTower = isTower;

        AbilityData = data;

        UpdateWeightDisplay();

        infoBtn.onClick.AddListener(OnInfoBtnClicked);

        var abilityId = data.RandomAbility_ID;
        var abilityData = DataTableManager.RandomAbilityTable.Get(abilityId);
        var specialEffectId = abilityData.SpecialEffect_ID;
        var effectData = DataTableManager.SpecialEffectTable.Get(specialEffectId);
        var abilityIconName = effectData.SpecialEffectIcon;
        var iconSprite = LoadManager.GetLoadedGameTexture(abilityIconName);
        iconImg.sprite = iconSprite;
    }

    public void UpdateWeightDisplay()
    {
        float currentWeight = CollectionManager.Instance.GetWeight(weightId);

        float totalWeight = 0f;

        if(isTower)
        {
            totalWeight = CollectionManager.Instance.GetTotalWeightAttackTowers() + CollectionManager.Instance.GetTotalWeightBuffTowers();
        }
        else
        {
            totalWeight = CollectionManager.Instance.GetTotalWeight(false);
        }

        float probability = totalWeight > 0 ? (currentWeight / totalWeight) * 100f : 0f;
        
        rateText.text = $"확률\n{probability:F1}%";
        weightText.text = $"가중치\n{currentWeight}";
    }

    public void OnWeightUpBtn()
    {
        if(CollectionManager.Instance.TryIncreaseWeight(weightId, isTower))
        {
            OnWeightChanged?.Invoke();
        }
    }

    public void OnWeightDownBtn()
    {
        if(CollectionManager.Instance.TryDecreaseWeight(weightId, isTower))
        {
            OnWeightChanged?.Invoke();
        }
    }

    public void OnInfoBtnClicked()
    {
        OnInfoBtn?.Invoke(this);
    }

    private void AddEventTrigger(GameObject target, bool isUpButton)
    {
        EventTrigger trigger = target.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = target.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => { OnPointerDownButton(isUpButton); });
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

    private void OnPointerDownButton(bool isUpButton)
    {
        holdCts?.Cancel();
        holdCts?.Dispose();
        holdCts = new CancellationTokenSource();

        if (isUpButton)
        {
            OnWeightUpBtn();
            HoldButtonAsync(true, holdCts.Token).Forget();
        }
        else
        {
            OnWeightDownBtn();
            HoldButtonAsync(false, holdCts.Token).Forget();
        }
    }

    private void OnPointerUpButton()
    {
        holdCts?.Cancel();
        holdCts?.Dispose();
        holdCts = null;
    }

    private async UniTaskVoid HoldButtonAsync(bool isUpButton, CancellationToken token)
    {
        try
        {
            await UniTask.Delay(500, cancellationToken: token);

            while (true)
            {
                if (isUpButton)
                {
                    OnWeightUpBtn();
                }
                else
                {
                    OnWeightDownBtn();
                }

                await UniTask.Delay(TimeSpan.FromSeconds(repeatInterval), cancellationToken: token);
            }
        }
        catch (OperationCanceledException)
        {
            
        }
    }
}
