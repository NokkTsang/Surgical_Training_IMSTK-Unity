using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Editor utility to automatically add all scenes under Assets/Scenes/Devices/
/// to the Build Settings. Required for VRSceneMenu runtime scene switching.
/// Access via menu: Tools > Setup Scene Menu > Add All Device Scenes to Build
/// </summary>
public class SceneMenuSetup : EditorWindow
{
    [MenuItem("Tools/Setup Scene Menu/Add All Device Scenes to Build")]
    public static void AddDeviceScenesToBuild()
    {
        string scenesFolder = "Assets/Scenes/Devices";

        if (!Directory.Exists(scenesFolder))
        {
            EditorUtility.DisplayDialog("Error",
                $"Folder not found: {scenesFolder}", "OK");
            return;
        }

        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { scenesFolder });
        var editorScenes = new List<EditorBuildSettingsScene>();

        foreach (string guid in sceneGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            editorScenes.Add(new EditorBuildSettingsScene(path, true));
        }

        EditorBuildSettings.scenes = editorScenes.ToArray();

        string sceneNames = "";
        foreach (var s in editorScenes)
        {
            sceneNames += $"  • {Path.GetFileNameWithoutExtension(s.path)}\n";
        }

        EditorUtility.DisplayDialog("Scenes Added",
            $"Added {editorScenes.Count} scenes to Build Settings:\n\n{sceneNames}", "OK");

        Debug.Log($"[SceneMenuSetup] Added {editorScenes.Count} device scenes to Build Settings.");
    }

    [MenuItem("Tools/Setup Scene Menu/Create VRSceneMenu in Current Scene")]
    public static void CreateVRSceneMenuInScene()
    {
        // Check if one already exists
        var existing = Object.FindObjectOfType<VRSceneMenu>();
        if (existing != null)
        {
            EditorUtility.DisplayDialog("Already Exists",
                "A VRSceneMenu already exists in this scene.", "OK");
            Selection.activeGameObject = existing.gameObject;
            return;
        }

        GameObject menuObj = new GameObject("VRSceneMenu");
        menuObj.AddComponent<VRSceneMenu>();

        Undo.RegisterCreatedObjectUndo(menuObj, "Create VRSceneMenu");
        Selection.activeGameObject = menuObj;

        Debug.Log("[SceneMenuSetup] Created VRSceneMenu GameObject. Press Play to see the floating menu.");
    }
}
