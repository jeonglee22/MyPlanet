using Cysharp.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private async UniTaskVoid Start()
    {
        await DataTableManager.InitializeAsync();
    }
}
