using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class EditorStartInit
{
    static EditorStartInit()
    {
        var firstScene = EditorBuildSettings.scenes[0]; 
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(firstScene.path);
        EditorSceneManager.playModeStartScene = sceneAsset;
    }
}
