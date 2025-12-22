using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Planet : LivingEntity
{
    private List<GameObject> towers; 
    private List<TowerAttack> planetAttacks; //Only Attack Tower
    private Dictionary<int, float> towerDamageDict = new Dictionary<int, float>();
    private TowerAmplifier[] amplifiersSlots; //Installed Amplifer Tower

    [SerializeField] private GameObject towerPrefab; //Attack Tower
    [SerializeField] private GameObject amplifierTowerPrefab; //Amplier Tower
    [SerializeField] private Transform towerSlotTransform;
    [SerializeField] private TowerInstallControl towerInstallControl;

    private int towerCount;
    public int TowerCount => towers?.Count ?? 0;
    private float exp;
    private int level = 1;
    public int Level => level;

    public event Action levelUpEvent;
    public event Action<float> expUpEvent;

    private PlanetData planetData;
    public PlanetData PlanetData => planetData;

    private float defense;
    public float Defense => defense;
    private float shield;
    private float initShield;
    public float InitShield => initShield;
    public float Shield 
    { 
        get => shield; 
        set
        {
            if (value == 0)
            {
                shield = 0f;
                OnBarriorChanged?.Invoke(shield);
                return;
            }

            shield = value;
            if (shield < 0f)
                shield = 0f;
            OnBarriorChanged?.Invoke(shield);
        }
    }
    private float drain;
    private float expScale;
    private float recoveryHp;

    private float recoveryInterval = 1f;
    private float recoveryTimer = 0f;

    private float attackPower;
    public float AttackPower => attackPower;

    public override float Health
    {
        get => base.Health;
        set
        {
            base.Health = value;
            
            if (base.Health > MaxHealth)
                base.Health = MaxHealth;
            OnHealthChanged?.Invoke(base.Health);
        }
    }

    public float CurrentExp
    {
        get => exp;
        set
        {
            exp = value;
            int levelUpCount = 0;
            while (exp >= MaxExp)
            {
                level++;
                exp -= MaxExp;
                MaxExp = DataTableManager.PlanetLevelUpTable.Get(level).Exp;
                levelUpCount++;
            }
            levelUps(levelUpCount).Forget();
            expUpEvent?.Invoke(exp);
        }
    }

    public float MaxExp { get; private set; }
    public event Action<float> OnBarriorChanged;

    public event Action<float> OnHealthChanged;

    [SerializeField] private Color baseColor = Color.white;
    [SerializeField] private Color hitColor = Color.gray;
    private Material Material;

    private CancellationTokenSource colorResetCts;

    public bool IsLazerHit = false;

    [Header("SFX")]
    [SerializeField] private AudioSource hitAudioSource;
    [SerializeField] private AudioClip playerHitSfx;
    [SerializeField, Range(0f, 1f)] private float playerHitSfxVolume = 1f;
    [SerializeField] private float playerHitSfxMinInterval = 0.05f;

    private float lastHitSfxTime = -999f;

    private void Awake()
    {
        planetAttacks = new List<TowerAttack>();
        InitPlanet();
        exp = 0f;
        MaxExp = DataTableManager.PlanetLevelUpTable.Get(level).Exp;

        if (towerSlotTransform == null) towerSlotTransform = transform;
    }

    // private async UniTaskVoid Start()
    // {
    //     await UniTask.WaitUntil(() => DataTableManager.IsInitialized);

    //     MaxExp = DataTableManager.PlanetLevelUpTable.Get(level).Exp;
    // }

    private void Start()
    {
        var planetData = DataTableManager.PlanetTable.Get(Variables.planetId);
        if (planetData == null)
            return;

        maxHealth = planetData.PlanetHp;
        Health = maxHealth;
        this.planetData = planetData;
        defense = planetData.PlanetArmor;
        shield = planetData.PlanetShield;
        initShield = planetData.PlanetShield;
        drain = planetData.Drain;
        expScale = planetData.ExpScale == 0f ? 1f : planetData.ExpScale;
        recoveryHp = planetData.RecoveryHp;

        CalculatePlanetAttackPower();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Material = GetComponent<Renderer>().material;
        Cancel();
    }

    private void Update()
    {
        recoveryTimer += Time.deltaTime;
        if (recoveryTimer < recoveryInterval)
            return;

        if (recoveryHp > 0f && !IsDead)
        {
            Health += recoveryHp * Time.deltaTime;
            recoveryTimer = 0f;
        }
    }

    public void CalculatePlanetAttackPower()
    {
        var baseAttack = maxHealth * (100 + defense) * 0.01f;
        var totalAttackPower = baseAttack + initShield + recoveryHp * 420f + drain * 100f;

        attackPower = totalAttackPower;
    }

    public void AddExp(float exp)
    {
        float finalExp = exp * expScale;
        CurrentExp += finalExp;
    }

    private async UniTaskVoid levelUps(int count)
    {
        //Debug.Log("LevelUpCount" + count);
        for (int i = 0; i < count; i++)
        {
            levelUpEvent?.Invoke();
            await UniTask.WaitUntil(() => !towerInstallControl.isInstall);
        }
    }
    
    public void InitPlanet(int towerCount = 12)
    {
        towers = new List<GameObject>();
        for (int i = 0; i < towerCount; i++) 
            towers.Add(null);
        this.towerCount = towerCount;

        amplifiersSlots = new TowerAmplifier[towerCount];
        planetAttacks = new List<TowerAttack>();
    }

    public void SetAttackTower(TowerDataSO towerData, int index, int abilityId = -1)
    {
        //Install Attack Tower
        GameObject installTower=Instantiate(towerPrefab, towerSlotTransform);
        towers[index] = installTower;

        //Enroll Tower Attack System
        TowerAttack newTowerAttack = installTower.GetComponent<TowerAttack>();
        if(newTowerAttack!=null)
        {
            newTowerAttack.SetTowerData(towerData);
            if(abilityId>0)
            {
                newTowerAttack.AddAbility(abilityId);
            }

            if (!planetAttacks.Contains(newTowerAttack))
                planetAttacks.Add(newTowerAttack);
        }

        //Enroll Targeting
        var targeting = installTower.GetComponent<TowerTargetingSystem>();
        if (targeting != null) targeting.SetTowerData(towerData);

        //Rotation System
        SetSlotRotation(index);

        //Apply Buffed Slot To New Tower
        if(newTowerAttack!=null&&amplifiersSlots!=null)
        {
            for(int i=0; i<amplifiersSlots.Length; i++)
            {
                var amp = amplifiersSlots[i];
                if (amp == null) continue;
                amp.ApplyBuffForNewTower(index, newTowerAttack);
            }
        }
    }

    public void UpgradeTower(int index)
    {
        UpgradeTower(index, -1);
    }

    public void UpgradeTower(int index, int abilityId)
    {
        var go = towers[index];
        Debug.Log($"[Planet][UpgradeTower] index={index}, abilityId={abilityId}, go={(go ? go.name : "null")}");
        if (go == null) return;

        // Attack Tower
        var attack = go.GetComponent<TowerAttack>();
        if (attack != null)
        {
            attack.SetReinforceLevel(attack.ReinforceLevel + 1);

            if (abilityId > 0)
            {
                attack.AddAbility(abilityId);
            }
            return;
        }

        // Buff Tower
        var amp = go.GetComponent<TowerAmplifier>();
        if (amp != null)
        {
            amp.SetReinforceLevel(amp.ReinforceLevel + 1);
            if (abilityId > 0)
            {
                amp.AddAbilityAndApplyToCurrentTargets(abilityId);
            }
        }
    }

    public void SetAmplifierTower(
        AmplifierTowerDataSO ampData, 
        int index, 
        int randomAbilityId,
        int[] presetBuffSlots=null,
        int[] presetRandomAbilitySlots = null
        )
    {
        //Install Amplifier Tower
        GameObject ampTower = Instantiate(amplifierTowerPrefab, towerSlotTransform);
        towers[index] = ampTower;

        //Rotation System
        SetSlotRotation(index);

        //Enroll Tower Amlifier System
        var amplifier = ampTower.GetComponent<TowerAmplifier>();
        if (amplifier != null)
        {
            amplifiersSlots[index] = amplifier;
            amplifier.AddAmpTower(
                ampData,
                index,
                this,
                randomAbilityId,
                presetBuffSlots,
                presetRandomAbilitySlots 
            );
        }
    }

    private void SetSlotRotation(int index)
    {
        var rot = new Vector3(0, 90, 0);
        rot.x = 360f * (index / (towers.Count - 1f));

        towers[index].transform.rotation = Quaternion.Euler(rot);
    }

    public override void OnDamage(float damage)
    {
        base.OnDamage(damage);
        TryPlayHitSfx();

        Cancel();
        Material.color = hitColor;
        ResetColorAsync(0.2f, colorResetCts.Token).Forget();
    }

    public override void Die()
    {
        base.Die();
        
        Destroy(gameObject);
    }

    //test
    private void Cancel()
    {
        colorResetCts?.Cancel();
        colorResetCts?.Dispose();
        colorResetCts = new CancellationTokenSource();
    }

    private async UniTaskVoid ResetColorAsync(float delay, CancellationToken token = default)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: colorResetCts.Token);
        Material.color = baseColor;
    }

    public TowerAttack GetAttackTowerToAmpTower(int index)
    {
        if (towers == null || index < 0 || index >= towers.Count) return null;

        var tower = towers[index];
        if (tower == null) return null;

        return tower.GetComponent<TowerAttack>();
    }

    public TowerAmplifier GetAmplifierTower(int index)
    {
        if (towers == null || index < 0 || index >= towers.Count) return null;

        var tower = towers[index];
        if (tower == null) return null;

        return tower.GetComponent<TowerAmplifier>();
    }

    //Move Tower ---------------------------------------
    public void MoveTower(int fromIndex, int toIndex)
    {
        if (towers == null) return;
        if (fromIndex == toIndex) return;
        if (fromIndex < 0 || fromIndex >= towers.Count) return;
        if (toIndex < 0 || toIndex >= towers.Count) return;

        var fromGo = towers[fromIndex];
        var toGo = towers[toIndex];
        if (fromGo == null && toGo == null) return;
        var fromAmp = fromGo != null ? fromGo.GetComponent<TowerAmplifier>() : null;
        var toAmp = toGo != null ? toGo.GetComponent<TowerAmplifier>() : null;
        towers[fromIndex] = toGo;
        towers[toIndex] = fromGo;

        if (fromGo != null) SetSlotRotation(toIndex);
        if (toGo != null) SetSlotRotation(fromIndex);

        int slotCount = towers.Count;
        if (amplifiersSlots != null && amplifiersSlots.Length == slotCount)
        {
            amplifiersSlots[fromIndex] = towers[fromIndex]?.GetComponent<TowerAmplifier>();
            amplifiersSlots[toIndex] = towers[toIndex]?.GetComponent<TowerAmplifier>();
        }

        if (fromAmp != null) fromAmp.RebuildSlotIndicesOnly(toIndex, slotCount);
        if (toAmp != null) toAmp.RebuildSlotIndicesOnly(fromIndex, slotCount);

        ReapplyAllAmplifierBuffs();
    }
    public void ReapplyAllAmplifierBuffs()
    {
        if (amplifiersSlots == null || towers == null) return;

        int slotCount = towers.Count;

        if (planetAttacks != null)
        {
            foreach (var atk in planetAttacks)
            {
                if (atk == null) continue;
                atk.ClearAllAmplifierBuffs();
                atk.ClearAllAmplifierAbilityStates();  // ← 이 줄 추가
            }
        }

        for (int i = 0; i < amplifiersSlots.Length; i++)
        {
            var amp = amplifiersSlots[i];
            if (amp == null || amp.AmplifierTowerData == null) continue;

            var buffSlots = amp.BuffedSlotIndex;
            if (buffSlots == null || buffSlots.Count == 0) continue;

            foreach (int slotIndex in buffSlots)
            {
                if (slotIndex < 0 || slotIndex >= slotCount) continue;

                var attack = GetAttackTowerToAmpTower(slotIndex);
                if (attack == null) continue;

                amp.ApplyBuffForNewTower(slotIndex, attack);
            }
        }
    }

    //--------------------------------------------------
    //Remove Tower--------------------------------------
    public int GetAttackTowerCount()
    {
        if (planetAttacks == null) return 0;
        return planetAttacks.Count;
    }

    public void RemoveTowerAt(int index)
    {
        if (towers == null) return;
        if (index < 0 || index >= towers.Count) return;

        var go = towers[index];
        if (go == null) return;

        var attack = go.GetComponent<TowerAttack>();
        if (attack != null)
        {
            if (planetAttacks != null)
            {
                planetAttacks.Remove(attack);
            }
        }

        var amp = go.GetComponent<TowerAmplifier>();
        if (amp != null && amplifiersSlots != null && index >= 0 && index < amplifiersSlots.Length)
        {
            amplifiersSlots[index] = null;
        }

        Destroy(go);
        towers[index] = null;

        ReapplyAllAmplifierBuffs();
    }
    //--------------------------------------------------
    //audio --------------------------------------------
    private void EnsureHitAudioSource()
    {
        if (hitAudioSource != null) return;

        hitAudioSource = GetComponent<AudioSource>();
        if (hitAudioSource == null) hitAudioSource = gameObject.AddComponent<AudioSource>();

        hitAudioSource.playOnAwake = false;
        hitAudioSource.loop = false;
        hitAudioSource.spatialBlend = 0f; 
    }
    private void TryPlayHitSfx()
    {
        if (playerHitSfx == null) return;
        if (Time.time - lastHitSfxTime < playerHitSfxMinInterval) return;

        lastHitSfxTime = Time.time;
        EnsureHitAudioSource();
        hitAudioSource.PlayOneShot(playerHitSfx, playerHitSfxVolume);
    }
    //--------------------------------------------------

}