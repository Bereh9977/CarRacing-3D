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

    [MenuItem("Tools/Car Racing/Reset Save Data")]
    public static void ResetSaveData()
    {
        SaveService.ResetProgress();
        Debug.Log("Save data reset.");
    }
}
