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
        await loadManager.LoadGamePrefabAsync(AddressLabel.EnemyLazer);
        await loadManager.LoadGameTextureAsync(AddressLabel.Texture);
        await loadManager.LoadGameMeshAsync(AddressLabel.Mesh);
        await loadManager.LoadEnemyPrefabAsync();

        await UniTask.WaitUntil(() => CurrencyManager.Instance.IsInitialized && 
                                        ItemManager.Instance.IsInitialized &&
                                        UserShopItemManager.Instance.IsInitialized);

        await UserShopItemManager.Instance.LoadUserShopItemDataAsync();
        await UserShopItemManager.Instance.EnsureDailyShopFreshAsync();
        // await loadManager.LoadGamePrefabAsync(AddressLabel.PoolObject);
    }
}
