using UnityEngine;
using System.IO;

public static class SaveSystem
{
    public static string GetSavePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");
    }

    public static bool HasSave(int slot)
    {
        return File.Exists(GetSavePath(slot));
    }

    public static GameSaveData GetCurrentSaveData()
    {
        return GameManager.Instance?.currentSaveData;
    }


    public static GameSaveData LoadGame(int slot)
    {
        string path = GetSavePath(slot);
        if (!File.Exists(path)) return null;
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<GameSaveData>(json);
    }

    public static void SaveGame(GameSaveData data, int slot)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(slot), json);
    }
}
