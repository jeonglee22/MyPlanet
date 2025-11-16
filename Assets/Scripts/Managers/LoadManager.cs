using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LoadManager : MonoBehaviour
{
    private Dictionary<int, GameObject> loadedEnemyPrefabs = new Dictionary<int, GameObject>();

    private async UniTask<GameObject> LoadEnemyPrefabAsync(int enemyId)
    {
        if (loadedEnemyPrefabs.ContainsKey(enemyId))
        {
            return loadedEnemyPrefabs[enemyId];
        }

        string addressKey = enemyId.ToString();

        try
        {
            GameObject prefab = await Addressables.LoadAssetAsync<GameObject>(addressKey).ToUniTask();
            loadedEnemyPrefabs.Add(enemyId, prefab);

            return prefab;
        }
        catch(System.Exception ex)
        {
            Debug.LogError($"Failed to load enemy prefab with ID {enemyId} from Addressables. Exception: {ex}");
            return null;
        }
    }

    public async UniTask TestLoadEnemy400101()
    {
        GameObject prefab = await LoadEnemyPrefabAsync(400101);

        if(prefab != null)
        {
            Debug.Log($"Load Success: {prefab.name}");
        }
        else
        {
            Debug.Log("Load Failed");
        }
    }

    public GameObject GetLoadedPrefab(int enemyId)
    {
        if(loadedEnemyPrefabs.TryGetValue(enemyId, out GameObject prefab))
        {
            return prefab;
        }

        return null;
    }
}
