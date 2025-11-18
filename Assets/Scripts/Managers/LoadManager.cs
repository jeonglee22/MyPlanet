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
        catch(System.Exception)
        {
            return null;
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

    public async UniTask LoadEnemyPrefabsAsync(HashSet<int> enemyIds)
    {
        List<UniTask> loadTasks = new List<UniTask>();

        foreach (int enemyId in enemyIds)
        {
            if(!loadedEnemyPrefabs.ContainsKey(enemyId))
            {
                loadTasks.Add(LoadEnemyPrefabAsync(enemyId));
            }
        }

        if(loadTasks.Count > 0)
        {
            await UniTask.WhenAll(loadTasks);
        }
    }
}
