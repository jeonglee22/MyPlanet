using Cysharp.Threading.Tasks;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private async UniTaskVoid Start()
    {
        await DataTableManager.InitializeAsync();
    }
}
