using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NPCStateManager : MonoBehaviour
{
    public static NPCStateManager Instance { get; private set; }

    [System.Serializable]
    public class NPCState
    {
        public Vector3 position;
        public int     stage;
        public bool    finished;
        public bool    visible;
        public bool    triggered;
        public int     moveint;
    }

    // key 格式："sceneName|npcId"
    private readonly Dictionary<string, NPCState> _states = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("NPCStateManager");
        Instance = go.AddComponent<NPCStateManager>();
        DontDestroyOnLoad(go);
    }

    // private void Awake()
    // {
    //     if (Instance != null && Instance != this)
    //     {
    //         Destroy(gameObject); // 防止重复实例
    //         return;
    //     }
    //     Instance = this;
    //     DontDestroyOnLoad(gameObject);
    // }

    private static string Key(string sceneName, string npcId) => $"{sceneName}|{npcId}";

    public void SaveState(string sceneName, string npcId, NPCState s)
    {
        if (string.IsNullOrEmpty(sceneName) || string.IsNullOrEmpty(npcId)) return;
        _states[Key(sceneName, npcId)] = s;
    }

    public bool TryGetState(string sceneName, string npcId, out NPCState s)
    {
        return _states.TryGetValue(Key(sceneName, npcId), out s);
    }

    [System.Serializable]
    public class NPCSaveEntry
    {
        public string sceneName; // 用 name 而非 path
        public string npcId;
        public float  x, y, z;
        public int    stage;
        public bool   finished;
        public bool   visible;
        public bool   triggered;
        public int   moveint;
    }

    public List<NPCSaveEntry> ExportToList()
    {
        var list = new List<NPCSaveEntry>(_states.Count);
        foreach (var kv in _states)
        {
            var parts = kv.Key.Split('|');
            if (parts.Length != 2) continue;

            var s = kv.Value;
            list.Add(new NPCSaveEntry
            {
                sceneName = parts[0],
                npcId     = parts[1],
                x         = s.position.x,
                y         = s.position.y,
                z         = s.position.z,
                stage     = s.stage,
                finished  = s.finished,
                visible   = s.visible,
                triggered = s.triggered,
                moveint   = s.moveint
            });
        }
        return list;
    }

    public void ImportFromList(List<NPCSaveEntry> list)
    {
        _states.Clear();
        if (list == null) return;

        foreach (var e in list)
        {
            var s = new NPCState
            {
                position  = new Vector3(e.x, e.y, e.z),
                stage     = e.stage,
                finished  = e.finished,
                visible   = e.visible,
                triggered = e.triggered,
                moveint   = e.moveint
            };
            SaveState(e.sceneName, e.npcId, s);
        }
    }

    public void CollectLiveNPCs()
    {
        var npcs = GameObject.FindObjectsOfType<NPCcontinueMovernormal>(true);
        foreach (var npc in npcs)
        {
            var idComp = npc.GetComponent<NPCId>();
            if (idComp == null || string.IsNullOrEmpty(idComp.Id)) 
                continue;

            var s = new NPCState
            {
                position  = npc.transform.position,
                stage     = npc.CurrentStage,
                finished  = npc.hasFinishedMoving,
                visible   = npc.IsVisible,
                triggered = npc.isTriggered,
                moveint   = npc.moveint
            };
            SaveState(npc.gameObject.scene.name, idComp.Id, s);
        }
    }

    // —— 可选的清理/查询 —— 

    public void ClearAllStates()
    {
        _states.Clear();
        Debug.Log("[NPCStateManager] Cleared all NPC states.");
    }

    public int ClearBySceneName(string sceneName)
    {
        var keys = new List<string>();
        foreach (var k in _states.Keys)
            if (k.StartsWith(sceneName + "|"))
                keys.Add(k);
        foreach (var k in keys)
            _states.Remove(k);
        Debug.Log($"[NPCStateManager] Cleared {keys.Count} states for scene {sceneName}");
        return keys.Count;
    }

    public int ClearCurrentScene()
    {
        return ClearBySceneName(SceneManager.GetActiveScene().name);
    }

    public bool RemoveState(string sceneName, string npcId)
    {
        bool rem = _states.Remove(Key(sceneName, npcId));
        if (rem)
            Debug.Log($"[NPCStateManager] Removed state for {sceneName}|{npcId}");
        return rem;
    }

    public int Count => _states.Count;
}
