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

    public bool IsLazerHit = false;

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
                amp.AddAbility(abilityId);
                amp.NotifyTargetsAbilityChanged();
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

        if (fromAmp != null)
            fromAmp.RebuildSlotsForNewIndex(toIndex, slotCount);
        if (toAmp != null)
            toAmp.RebuildSlotsForNewIndex(fromIndex, slotCount);
        
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
            }
        }

        for (int i = 0; i < amplifiersSlots.Length; i++)
        {
            var amp = amplifiersSlots[i];
            if (amp == null) continue;
            amp.ResetLocalBuffStateOnly();
        }

        for (int i = 0; i < amplifiersSlots.Length; i++)
        {
            var amp = amplifiersSlots[i];
            if (amp == null || amp.AmplifierTowerData == null) continue;

            var buffSlots = amp.BuffedSlotIndex;
            var randomSlots = amp.RandomAbilitySlotIndex;

            if ((buffSlots == null || buffSlots.Count == 0) &&
                (randomSlots == null || randomSlots.Count == 0))
            {
                continue;
            }

            HashSet<int> targetSlots = new HashSet<int>();

            if (buffSlots != null)
            {
                foreach (var s in buffSlots)
                    targetSlots.Add(s);
            }

            if (randomSlots != null)
            {
                foreach (var s in randomSlots)
                    targetSlots.Add(s);
            }

            foreach (int slotIndex in targetSlots)
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
}