using System.Collections.Generic;
using UnityEngine;

public class GlassStateManager : MonoBehaviour
{
    public static GlassStateManager Instance;

    [System.Serializable]
    public class GlassSaveEntry
    {
        public string sceneName;
        public string doorID;
        public bool isOpen;
        public bool isOiled;
    }

    private readonly Dictionary<string, GlassSaveEntry> _glassStates = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 设置门的状态
    /// </summary>
    public void SetGlassState(string scene, string id, bool open, bool oiled)
    {
        string key = $"{scene}:{id}";
        _glassStates[key] = new GlassSaveEntry
        {
            sceneName = scene,
            doorID = id,
            isOpen = open,
            isOiled = oiled
        };
    }

    /// <summary>
    /// 获取门的状态（可能为 null）
    /// </summary>
    public GlassSaveEntry GetGlassState(string scene, string id)
    {
        string key = $"{scene}:{id}";
        return _glassStates.TryGetValue(key, out var entry) ? entry : null;
    }

    /// <summary>
    /// 尝试获取门的状态（带 out 参数）
    /// </summary>
    public bool TryGetGlassState(string scene, string id, out GlassSaveEntry entry)
    {
        return _glassStates.TryGetValue($"{scene}:{id}", out entry);
    }

    /// <summary>
    /// 导出存档
    /// </summary>
    public List<GlassSaveEntry> ExportToList()
    {
        return new List<GlassSaveEntry>(_glassStates.Values);
    }

    /// <summary>
    /// 导入存档
    /// </summary>
    public void ImportFromList(List<GlassSaveEntry> list)
    {
        _glassStates.Clear();
        foreach (var entry in list)
        {
            string key = $"{entry.sceneName}:{entry.doorID}";
            _glassStates[key] = entry;
        }
    }

    // ========== 兼容旧调用方式 ==========

    /// <summary>
    /// 旧的 TryGetDoorState（只判断是否存在）
    /// </summary>
    public bool TryGetDoorState(string scene, string id)
    {
        return _glassStates.ContainsKey($"{scene}:{id}");
    }

    /// <summary>
    /// 旧的 SaveDoorState（只保存开关状态，oiled 默认 false）
    /// </summary>
    public void SaveDoorState(string scene, string id, bool open)
    {
        SaveDoorState(scene, id, open, false);
    }

    /// <summary>
    /// 新的 SaveDoorState（保存开关和上油状态）
    /// </summary>
    public void SaveDoorState(string scene, string id, bool open, bool oiled)
    {
        SetGlassState(scene, id, open, oiled);
    }

    /// <summary>
    /// 重置指定门的状态（移除记录，相当于从未交互过）
    /// </summary>
    public void ResetDoorState(string scene, string id)
    {
        string key = $"{scene}:{id}";
        if (_glassStates.ContainsKey(key))
        {
            _glassStates.Remove(key);
        }
    }

    /// <summary>
    /// 重置某个场景的所有门状态
    /// </summary>
    public void ResetSceneStates(string scene)
    {
        var keysToRemove = new List<string>();
        foreach (var kv in _glassStates)
        {
            if (kv.Value.sceneName == scene)
                keysToRemove.Add(kv.Key);
        }
        foreach (var key in keysToRemove)
        {
            _glassStates.Remove(key);
        }
    }

    /// <summary>
    /// 重置所有门的状态
    /// </summary>
    public void ResetAllStates()
    {
        _glassStates.Clear();
    }
}
