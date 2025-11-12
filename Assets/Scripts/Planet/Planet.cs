using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Purchasing;
using UnityEngine;
using UnityEngine.Timeline;

public class Planet : LivingEntity
{
    private List<TowerAttack> planetAttacks;
    private List<GameObject> towers;

    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private Transform towerSlotTransform;

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
    [SerializeField] private float shootInterval = 0.5f;
    private float shootTime = 0f;

    private void Awake()
    {
        planetAttacks = new List<TowerAttack>();
        InitPlanet();
        exp = 0f;
        MaxExp = 100f;
    }

    private void Update()
    {
#if UNITY_EDITOR
        // if (Input.touchCount != 0)
        // {
        //     foreach (var attack in planetAttacks)
        //         attack.Shoot(ProjectileType.Normal, transform.forward, true);
        // }
        if (shootTime > shootInterval)
        {
            foreach (var attack in planetAttacks)
                attack.Shoot(ProjectileType.Normal, attack.gameObject.transform.forward, true);
            shootTime = 0f;
        }
        shootTime += Time.deltaTime;
#endif
    }
    
    public void InitPlanet(int towerCount = 12)
    {
        towers = new List<GameObject>();
        for (int i = 0; i < towerCount; i++)
            towers.Add(null);
        this.towerCount = towerCount;
    }

    public void SetTower(TowerDataSO towerData, int index)
    {
        //Delete Tower
        if(towers[index]!=null)
        {
            Destroy(towers[index]);
            towers[index] = null;
        }

        //Install Tower
        GameObject installTower=Instantiate(towerPrefab, towerSlotTransform);
        towers[index] = installTower;

        //Enroll Tower
        TowerAttack newTowerAttack = installTower.GetComponent<TowerAttack>();
        if(newTowerAttack!=null)
        {
            newTowerAttack.SetTowerData(towerData);
            planetAttacks.Add(newTowerAttack);
        }
        
        //Rotation System
        var rot = new Vector3(0, 90, 0);
        rot.x = 360f * (index / (towers.Count - 1f));

        towers[index].transform.rotation = Quaternion.Euler(rot);
    }

    public override void OnDamage(float damage)
    {
        base.OnDamage(damage);
    }
    
    protected override void Die()
    {
        base.Die();
        
        Destroy(gameObject);
    }

    internal int GetTowerSlotCount()
    {
        return towerCount;
    }
}