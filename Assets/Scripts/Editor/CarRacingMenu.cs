using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CarRacingMenu
{
    [MenuItem("Tools/Car Racing/Validate Open Scene")]
    public static void ValidateOpenScene()
    {
        var problems = new List<string>();
        TrackLayout.FromScene(problems);
        string scene = EditorSceneManager.GetActiveScene().name;

        if (problems.Count == 0)
            Debug.Log($"'{scene}' has all race objects.");
        else
            Debug.LogWarning($"'{scene}' is missing race objects:\n- " + string.Join("\n- ", problems));
    }

    private static readonly string[] GroupNames = { "Lighting", "Environment", "Track", "Decorations", "Coins", "Obstacles" };

    // Moves the root objects of the open scene into group objects. Cameras and cars stay at the root.
    // Running it again only moves the new objects, so it can be used after adding things to the map.
    [MenuItem("Tools/Car Racing/Organize Open Scene")]
    public static void OrganizeOpenScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        var groups = new Dictionary<string, Transform>();
        foreach (GameObject root in scene.GetRootGameObjects())
            if (System.Array.IndexOf(GroupNames, root.name) >= 0)
                groups[root.name] = root.transform;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (groups.ContainsValue(root.transform)) continue;
            string group = GroupFor(root);
            if (group == null) continue;

            if (!groups.ContainsKey(group))
            {
                var parent = new GameObject(group);
                Undo.RegisterCreatedObjectUndo(parent, "Organize Scene");
                groups[group] = parent.transform;
            }
            Undo.SetTransformParent(root.transform, groups[group], "Organize Scene");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log($"'{scene.name}' organized.");
    }

    private static string GroupFor(GameObject root)
    {
        if (root.GetComponent<Camera>() != null || root.GetComponent<CarController>() != null) return null;
        if (root.GetComponent<Light>() != null) return "Lighting";
        if (root.GetComponent<Terrain>() != null) return "Environment";

        string prefab = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root);
        string prefabName = System.IO.Path.GetFileNameWithoutExtension(prefab);
        if (prefabName == "Coin" || prefabName == "StarCoin") return "Coins";
        if (prefabName == "Sand" || prefabName == "Ice" || prefabName == "Banana") return "Obstacles";
        if (prefab.Contains("Modular_Track_Free") || root.name.StartsWith("Fence")) return "Track";
        return "Decorations";
    }

    [MenuItem("Tools/Car Racing/Reset Save Data")]
    public static void ResetSaveData()
    {
        SaveService.ResetProgress();
        Debug.Log("Save data reset.");
    }
}
