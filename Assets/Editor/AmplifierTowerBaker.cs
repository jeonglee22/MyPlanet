#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Cysharp.Threading.Tasks;

public static class AmplifierTowerBaker
{
    [MenuItem("Tools/Amplifier/Bake All Amplifier Towers From Tables")]
    private static async void BakeAllAmplifiersFromTables()
    {
        await DataTableManager.InitializeAsync();

        string[] guids = AssetDatabase.FindAssets("t:AmplifierTowerDataSO");
        int bakeCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var amp = AssetDatabase.LoadAssetAtPath<AmplifierTowerDataSO>(path);
            if (amp == null) continue;

            amp.RefreshFromTables();

            EditorUtility.SetDirty(amp);
            bakeCount++;
        }

        AssetDatabase.SaveAssets();

        Debug.Log($"[AmplifierTowerBaker] Baked {bakeCount} AmplifierTowerDataSO assets from tables.");
    }

    [MenuItem("Tools/Amplifier/Bake Selected Amplifier(s) From Tables")]
    private static async void BakeSelectedAmplifiersFromTables()
    {
        await DataTableManager.InitializeAsync();

        Object[] selected = Selection.objects;
        int bakeCount = 0;

        foreach (var obj in selected)
        {
            var amp = obj as AmplifierTowerDataSO;
            if (amp == null) continue;

            amp.RefreshFromTables();
            EditorUtility.SetDirty(amp);
            bakeCount++;
        }

        if (bakeCount > 0)
        {
            AssetDatabase.SaveAssets();
        }

        Debug.Log($"[AmplifierTowerBaker] Baked {bakeCount} selected AmplifierTowerDataSO assets from tables.");
    }
}
#endif