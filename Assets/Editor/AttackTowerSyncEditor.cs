#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class AttackTowerSyncEditor
{
    [MenuItem("Tools/Data/Sync AttackTower CSV -> TowerDataSO")]
    private static void SyncMenu()
    {
        SyncAsync().Forget();
    }

    private static async UniTask SyncAsync()
    {
        //Load AttackTowerTable
        var table = new AttackTowerTable();
        await table.LoadAsync(DataTableIds.AttackTower);

        if (table == null || table.Rows == null || table.Rows.Count == 0) return;

        //Find TowerDataSO In Project
        string[] guids = AssetDatabase.FindAssets("t:TowerDataSO");
        int updateCount = 0;

        foreach(var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<TowerDataSO>(path);
            if (so == null) continue;
            if (so.towerIdInt == 0) continue;

            AttackTowerRow row = table.GetById(so.towerIdInt);
            if (row == null) continue;

            so.towerId = row.AttackTowerName;       
            so.fireType = (FireType)row.FireType;       
            so.fireRate = row.AttackSpeed;             
            so.Accuracy = row.Accuracy;                 
            so.grouping = row.grouping;            
            so.projectileCount = Mathf.RoundToInt(row.ProjectileNum); 
            so.randomAbilityGroupId = row.RandomAbilityGroup_ID;
            so.projectileIdFromTable = row.Projectile_ID;
            //Not Yet (20251124 11:58, only passivity)
            //AttackRange 

            EditorUtility.SetDirty(so);
            updateCount++;
        }
        AssetDatabase.SaveAssets();
        //Debug.Log($"[AttackTowerSync] 동기화 완료. 갱신된 TowerDataSO 개수: {updateCount}");
    }
}
#endif