using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTowerManager : MonoBehaviour
{
    [SerializeField] private Planet planet;
    [SerializeField] private List<TowerDataSO> towerDatas;

    private TowerDataSO[] assignedTowers;

    private void Awake()
    {
        int towerCount = planet.GetTowerSlotCount();
        assignedTowers = new TowerDataSO[towerCount];
    }

    private void Start()
    {
        AssignRandomTower();
    }

    private void AssignRandomTower()
    {
        int count = planet.GetTowerSlotCount();
        for(int i=0; i<count; i++)
        {
            TowerDataSO randomData = towerDatas[UnityEngine.Random.Range(0, towerDatas.Count)];
            InstallTower(i, randomData);
        }
    }

    private void InstallTower(int index, TowerDataSO randomData)
    {
        assignedTowers[index] = randomData;
        planet.SetTowerInPlayerTowerMGR(index, randomData);
    }

    public TowerDataSO GetTowerData(int index)
    {
        return assignedTowers[index];
    }
}