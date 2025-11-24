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
        await WaveManager.Instance.InitializeStage(Variables.Stage);
    }
}
