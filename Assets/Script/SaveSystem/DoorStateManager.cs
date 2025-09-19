using System.Collections.Generic;
using UnityEngine;

public class DoorStateManager : MonoBehaviour
{
    public static DoorStateManager Instance;

    [System.Serializable]
    public struct DoorState
    {
        public bool isUnlocked;
        public bool isLocked;
    }

    [System.Serializable]
    public class DoorSaveEntry
    {
        public string sceneName;
        public string doorID;
        public bool isUnlocked;
        public bool isLocked;
    }

    private readonly Dictionary<string, DoorState> _states = new Dictionary<string, DoorState>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private string MakeKey(string sceneName, string doorID) => $"{sceneName}:{doorID}";

    public DoorState Get(string sceneName, string doorID)
    {
        string key = MakeKey(sceneName, doorID);
        if (_states.TryGetValue(key, out var s)) return s;
        return default;
    }

    public void SetUnlocked(string sceneName, string doorID, bool value)
    {
        string key = MakeKey(sceneName, doorID);
        if (_states.TryGetValue(key, out var s)) { s.isUnlocked = value; _states[key] = s; }
        else _states[key] = new DoorState { isUnlocked = value, isLocked = false };
    }

    public void SetLocked(string sceneName, string doorID, bool value)
    {
        string key = MakeKey(sceneName, doorID);
        if (_states.TryGetValue(key, out var s)) { s.isLocked = value; _states[key] = s; }
        else _states[key] = new DoorState { isUnlocked = false, isLocked = value };
    }

    // 新增：清空所有门状态（用于“新游戏”）
    public void ClearAll()
    {
        _states.Clear();
    }

    public List<DoorSaveEntry> ExportToList()
    {
        var list = new List<DoorSaveEntry>();
        foreach (var kv in _states)
        {
            var idx = kv.Key.IndexOf(':');
            string sceneName = idx >= 0 ? kv.Key.Substring(0, idx) : "";
            string doorID = idx >= 0 ? kv.Key.Substring(idx + 1) : kv.Key;

            list.Add(new DoorSaveEntry
            {
                sceneName = sceneName,
                doorID = doorID,
                isUnlocked = kv.Value.isUnlocked,
                isLocked = kv.Value.isLocked
            });
        }
        return list;
    }

    public void ImportFromList(List<DoorSaveEntry> entries)
    {
        _states.Clear();
        if (entries == null) return;
        foreach (var e in entries)
        {
            string key = MakeKey(e.sceneName, e.doorID);
            _states[key] = new DoorState { isUnlocked = e.isUnlocked, isLocked = e.isLocked };
        }
    }
}
