using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;

public class Planet : LivingEntity
{
    private List<TowerAttack> planetAttacks;
    private List<GameObject> towers;

    [SerializeField] private GameObject towerPrefab; //Attack Tower
    [SerializeField] private GameObject amplifierTowerPrefab; //Amplier Tower
    [SerializeField] private Transform towerSlotTransform;

    private TowerAmplifier[] amplifierSlots;

    private int towerCount;
    private float exp;
    private float level;

    public event Action levelUpEvent;
    public event Action<float> expUpEvent;
    public float CurrentExp
    {
        get => exp;
        set
        {
            exp = value;
            if (exp >= MaxExp)
            {
                level++;
                levelUpEvent?.Invoke();
                exp = 0f;
                MaxExp *= 1.2f;
            }
            expUpEvent?.Invoke(exp);
        }
    }

    public float MaxExp { get; internal set; } = 100f;

    //test
    // [SerializeField] private float shootInterval = 0.5f;
    // private float shootTime = 0f;

    [SerializeField] private Color baseColor = Color.gray;
    [SerializeField] private Color hitColor = Color.white;
    private Material Material;

    private CancellationTokenSource colorResetCts;

    private void Awake()
    {
        planetAttacks = new List<TowerAttack>();
        InitPlanet();
        exp = 0f;
        MaxExp = 100f;

        if (towerSlotTransform == null) towerSlotTransform = transform;
    }

    private void Start()
    {
    }

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
    
    public void InitPlanet(int towerCount = 12)
    {
        towers = new List<GameObject>();
        for (int i = 0; i < towerCount; i++) towers.Add(null);
        this.towerCount = towerCount;
    }

    public void SetTower(TowerDataSO towerData, int index, IAbility ability = null)
    {
        //Install Tower
        GameObject installTower=Instantiate(towerPrefab, towerSlotTransform);
        towers[index] = installTower;

        //Enroll TowerAttack
        TowerAttack newTowerAttack = installTower.GetComponent<TowerAttack>();
        if(newTowerAttack!=null)
        {
            newTowerAttack.SetTowerData(towerData);
            if (ability == null)
            {
                newTowerAttack.SetRandomAbility();
            }
            else
            {
                newTowerAttack.AddAbility(ability);
            }
            
            planetAttacks.Add(newTowerAttack);
        }

        //Enroll Targeting
        var targeting = installTower.GetComponent<TowerTargetingSystem>();
        if (targeting != null) targeting.SetTowerData(towerData);
        
        //Rotation System
        var rot = new Vector3(0, 90, 0);
        rot.x = 360f * (index / (towers.Count - 1f));

        towers[index].transform.rotation = Quaternion.Euler(rot);
    }

    public void UpgradeTower(int index)
    {
        var tower = towers[index];
        var towerAttack = tower.GetComponent<TowerAttack>();
    
        towerAttack.AddAbility(new ParalyzeAbility());
    }

    public override void OnDamage(float damage)
    {
        base.OnDamage(damage);

        Cancel();

        Material.color = hitColor;
        ResetColorAsync(0.2f, colorResetCts.Token).Forget();
    }
    
    protected override void Die()
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
}