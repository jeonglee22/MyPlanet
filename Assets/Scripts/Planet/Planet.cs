using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

public class Planet : LivingEntity
{
    private List<TowerAttack> planetAttacks;
    private List<GameObject> towers;
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private Transform towerSlotTransform;

    private int towerCount;

    private void Awake()
    {
        planetAttacks = new List<TowerAttack>();

        InitPlanet();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.touchCount != 0)
        {
            foreach (var attack in planetAttacks)
                attack.Shoot(ProjectileType.Normal, transform.forward, true);
        }
#endif
    }
    
    public void InitPlanet(int towerCount = 12)
    {
        towers = new List<GameObject>();
        for (int i = 0; i < towerCount; i++)
            towers.Add(null);
        this.towerCount = towerCount;
    }

    public void SetTower(GameObject tower, int index)
    {
        towers[index] = Instantiate(towerPrefab, towerSlotTransform);

        planetAttacks.Add(towers[index].GetComponent<TowerAttack>());
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
}
