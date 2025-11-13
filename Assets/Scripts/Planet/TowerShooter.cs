using UnityEngine;

[RequireComponent(typeof(TowerTargetingSystem))]
[RequireComponent(typeof(TowerAttack))]
public class TowerShooter : MonoBehaviour
{
    private TowerTargetingSystem targetingSystem;
    private TowerAttack towerAttack;
    private TowerDataSO towerData;

    private float fireTimer = 0f;
    private float fireRate = 1f;
    private float range = 3f;

    private void Awake()
    {
        targetingSystem = GetComponent<TowerTargetingSystem>();
        towerAttack = GetComponent<TowerAttack>();
    }

    private void Start()
    {
        towerData = targetingSystem.GetTowerData();
        if(towerData!=null)
        {
            fireRate = towerData.fireRate;
            range = towerData.rangeData != null ? towerData.rangeData.GetRange() : 3f;
            towerAttack.SetTowerData(towerData);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
