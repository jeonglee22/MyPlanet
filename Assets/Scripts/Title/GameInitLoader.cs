using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameInitLoader : MonoBehaviour
{
    [SerializeField] private LoadManager loadManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private async UniTaskVoid Start()
    {
        await DataTableManager.InitializeAsync();
        await loadManager.LoadGamePrefabAsync(AddressLabel.Prefab);
        // await loadManager.LoadGamePrefabAsync(AddressLabel.PoolObject);
    }
}
