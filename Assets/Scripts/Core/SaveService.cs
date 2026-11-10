using System.IO;
using UnityEngine;

// Keeps SaveData in a JSON file. Usage: SaveService.Data.coins += 100; SaveService.Save();
public static class SaveService
{
    private static SaveData data;

    private static string FilePath => Path.Combine(Application.persistentDataPath, "save.json");

    public static SaveData Data
    {
        get
        {
            if (data == null)
                data = File.Exists(FilePath) ? JsonUtility.FromJson<SaveData>(File.ReadAllText(FilePath)) : new SaveData();
            return data;
        }
    }

    public static void Save()
    {
        File.WriteAllText(FilePath, JsonUtility.ToJson(Data, true));
    }

    public static void ResetProgress()
    {
        data = new SaveData();
        File.Delete(FilePath);
    }
}
