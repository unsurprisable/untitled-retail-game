using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class DefaultSceneEditorScript 
{
    static DefaultSceneEditorScript()
    {
        var pathOfFirstScene = EditorBuildSettings.scenes[0].path;
        var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(pathOfFirstScene);
        EditorSceneManager.playModeStartScene = sceneAsset;
        Debug.Log(pathOfFirstScene + " was set as default play mode scene");
    }
}