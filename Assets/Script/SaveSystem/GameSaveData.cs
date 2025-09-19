using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    public string sceneName;
    public float playerPosX, playerPosY, playerPosZ;

    public List<string> inventoryItemNames = new List<string>();
    //public List<string> pickedUpItemIDs = new List<string>();

    public int selectedIndex;
    public string saveTime;

    public string playerAnimState;

    public int playerAnimIntState;
    public float lastMoveDirX;
    public float lastMoveDirY;

    public List<string> triggeredDialogueIDs = new List<string>();

    public bool playerMovementMonitorAwake;
    public string TelepreviousSceneName;
    public float GlobalTime;
    public bool livingroomrunning;
    public bool t1hasShown;
    public bool t2hasShown;
    public bool t3hasShown;

    public bool hasEquipped;
    public string equippedItemName;
    public bool tv;
    public bool TVisplaying;
    public List<DoorStateManager.DoorSaveEntry> doorStates = new List<DoorStateManager.DoorSaveEntry>();
    public List<DoorToggleSaveEntry> doorToggleStates = new List<DoorToggleSaveEntry>();


    // 新增：NPC 状态列表（跨重启持久）
    public List<NPCStateManager.NPCSaveEntry> npcStates = new List<NPCStateManager.NPCSaveEntry>();
    public Dictionary<string, string> saveableStates = new Dictionary<string, string>();
    public List<GlassStateManager.GlassSaveEntry> glassStates = new List<GlassStateManager.GlassSaveEntry>();
    public List<NPCcontinueMoverSaveEntry> npcMoves = new List<NPCcontinueMoverSaveEntry>();

    public bool Passwordcorrect;
    public bool keystolen;
    public bool TimerhasFinished;
    public bool momhasgoout;
    public bool TimerhasStarted;
    public bool TimerIsRunning;
    public float TimerCurrentTime;
    public bool npcDoismoving;
}

[System.Serializable]
public class DoorToggleSaveEntry
{
    public string doorID;
    public bool isOpen;
    public bool isOiled; // 新增：是否已涂油
}

[System.Serializable]
public class NPCcontinueMoverSaveEntry
{
    public string npcID;
    public int currentStage;
    public bool isMoving;
    public bool isTriggered;
    public bool hassetdisvisable;
    public bool hasFinishedMoving;
    public float posX;
    public float posY;
    public float posZ;
}