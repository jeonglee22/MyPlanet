using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;

public class Planet : LivingEntity
{
    private List<GameObject> towers; 
    private List<TowerAttack> planetAttacks; //Only Attack Tower
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
                Debug.Log($"Next Level Exp : {MaxExp}");
                levelUpCount++;
            }
            levelUps(levelUpCount).Forget();
            expUpEvent?.Invoke(exp);
        }
    }

    public float MaxExp { get; private set; }

    [SerializeField] private Color baseColor = Color.white;
    [SerializeField] private Color hitColor = Color.gray;
    private Material Material;

    private CancellationTokenSource colorResetCts;

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

    protected override void OnEnable()
    {
        base.OnEnable();
        Material = GetComponent<Renderer>().material;
        Cancel();
    }

    private void Update()
    {
#if UNITY_EDITOR

        // if (Input.touchCount != 0)
        // {
        //     foreach (var attack in planetAttacks)
        //         attack.Shoot(ProjectileType.Normal, transform.forward, true);
        // }

       /* if (shootTime > shootInterval)
        {
            foreach (var attack in planetAttacks)
                attack.Shoot(attack.gameObject.transform.forward, true);
            shootTime = 0f;
        }
        shootTime += Time.deltaTime;*/
#endif
    }

    private async UniTaskVoid levelUps(int count)
    {
        Debug.Log("LevelUpCount" + count);
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
            if (abilityId == -1)
            {
                newTowerAttack.SetRandomAbility();
            }
            else
            {
                newTowerAttack.AddAbility(abilityId);
                var ability = AbilityManager.GetAbility(abilityId);
                ability.ApplyAbility(newTowerAttack.gameObject);
            }
            if(!planetAttacks.Contains(newTowerAttack))
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
        var go = towers[index];
        if (go == null) return;

        var attack = go.GetComponent<TowerAttack>();
        if (attack != null)
        {
            attack.SetReinforceLevel(attack.ReinforceLevel + 1);
            Debug.Log($"[Planet.UpgradeTower] Slot {index} AttackTower => Lv {attack.ReinforceLevel}");
            return;
        }

        var amp = go.GetComponent<TowerAmplifier>();
        if (amp != null)
        {
            amp.SetReinforceLevel(amp.ReinforceLevel + 1);
            Debug.Log($"[Planet.UpgradeTower] Slot {index} BuffTower => Lv {amp.ReinforceLevel}");
        }
    }


    public void SetAmplifierTower(
        AmplifierTowerDataSO ampData, 
        int index, 
        int randomAbilityId,
        int[] presetBuffSlots=null)
    {
        //Install Amplifier Tower
        GameObject ampTower = Instantiate(amplifierTowerPrefab, towerSlotTransform);
        towers[index] = ampTower;

        //Rotation System
        SetSlotRotation(index);

        //Enroll Tower Amlifier System
        var amplifier=ampTower.GetComponent<TowerAmplifier>();
        if (amplifier != null)
        {
            amplifiersSlots[index] = amplifier;
            amplifier.AddAmpTower(ampData, index, this, randomAbilityId, presetBuffSlots);
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
}