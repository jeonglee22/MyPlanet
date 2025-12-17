using System.Collections.Generic;
using Cysharp.Threading.Tasks;
// using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class LoadManager : MonoBehaviour
{

    private Dictionary<int, GameObject> loadedEnemyPrefabs = new Dictionary<int, GameObject>();

    private static Dictionary<string, GameObject> loadedGamePrefabs = new Dictionary<string, GameObject>();

    public async UniTask LoadGamePrefabAsync(string labelName)
    {
        try
        {
            var prefabLoaded = await Addressables.LoadAssetsAsync<GameObject>(labelName).ToUniTask();
            foreach (var prefab in prefabLoaded)
            {
                loadedGamePrefabs.Add(prefab.name, prefab);
            }
        }
        catch (System.Exception)
        {
        }
    }

    public static GameObject GetLoadedGamePrefab(string prefabName)
    {
        if(loadedGamePrefabs.TryGetValue(prefabName, out GameObject prefab))
        {
            var newObj = Instantiate(prefab);
            return newObj;
        }

        return null;
    }

    public static GameObject GetLoadedGamePrefabOriginal(string prefabName)
    {
        if(loadedGamePrefabs.TryGetValue(prefabName, out GameObject prefab))
        {
            return prefab;
        }

        return null;
    }

    public async UniTask<GameObject> LoadEnemyPrefabAsync(int enemyId)
    {
        if (loadedEnemyPrefabs.ContainsKey(enemyId))
        {
            return loadedEnemyPrefabs[enemyId];
        }

        string addressKey = DataTableManager.EnemyTable.Get(enemyId).VisualAsset;

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

    public async UniTask LoadEnemyPrefabAsync()
    {
        if (loadedEnemyPrefabs.Count > 0)
        {
            return;
        }

        var enemyIds = DataTableManager.EnemyTable.GetEnemyIds();
        foreach(int enemyId in enemyIds)
        {
            string addressKey = DataTableManager.EnemyTable.Get(enemyId).VisualAsset;

            try
            {
                GameObject prefab = await Addressables.LoadAssetAsync<GameObject>(addressKey).ToUniTask();
                loadedEnemyPrefabs.Add(enemyId, prefab);
            }
            catch(System.Exception)
            {
                
            }
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
