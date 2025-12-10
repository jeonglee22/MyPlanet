using UnityEngine;
using UnityEngine.UI;

public class FirebaseTest : MonoBehaviour
{
    [SerializeField] private Button testButton;

    private void Start()
    {
        testButton.onClick.AddListener(TestItems);
    }

    private async void TestItems()
    {
        Debug.Log("===== Items 테스트 시작 =====");

        // 1. 현재 값 확인
        Debug.Log($"현재 TowerEnhanceItem: {UserData.TowerEnhanceItem}");
        Debug.Log($"현재 HealthPlanetPiece: {UserData.HealthPlanetPiece}");

        // 2. 값 변경
        UserData.TowerEnhanceItem = 5;
        UserData.HealthPlanetPiece = 20;

        // 3. Firebase 저장
        var result = await ItemManager.Instance.SaveItemsAsync();
        if (result.success)
        {
            Debug.Log("✅ Items Firebase 저장 성공!");
        }
        else
        {
            Debug.LogError($"❌ Items Firebase 저장 실패: {result.error}");
        }

        Debug.Log("===== Firebase Console에서 확인하세요 =====");
    }
}
