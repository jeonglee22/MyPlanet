using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private LoadManager loadManager;
    public static LoadManager LoadManagerInstance { get; private set; }

    private async UniTaskVoid Start()
    {
        LoadManagerInstance = loadManager;

        // await DataTableManager.InitializeAsync();
        // await loadManager.LoadGamePrefabAsync(AddressLabel.Prefab);

        //await loadManager.TestLoadEnemy();
        await BattleRandomAbilityUnlockPatcher.ApplyOnceAsync(); //unlock
        await WaveManager.Instance.InitializeStage(Variables.Stage);

        int enemyLayer = LayerMask.NameToLayer(TagName.Enemy);
        int playerLayer = LayerMask.NameToLayer(TagName.Planet);
        Physics.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
        Physics.IgnoreLayerCollision(playerLayer, playerLayer, true);
    }
}