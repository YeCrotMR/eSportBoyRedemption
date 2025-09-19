using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static int buttonindex;

    public bool isLoadingSave = false;
    public GameSaveData currentSaveData; // 存放当前内存中的存档数据
    

    void Awake()
    {
        Debug.Log("GameManager Awake: " + gameObject.name);

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("销毁多余的 GameManager: " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 在任何切场景前手动调用，确保把当前场景 NPC 的实时状态写回字典
    public static void BeforeSceneChange()
    {
        NPCStateManager.Instance?.CollectLiveNPCs();
    }

    public void setbuttonindex(int number)
    {
        buttonindex = number;
    }

    public void buttonSaveGame(int slot)
    {
        bool hasSave = SaveSystem.HasSave(slot);
        if (!hasSave)
        {
            SaveGame();
        }
    }

    // 手动保存
    public void SaveGame()
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("找不到 Player，保存失败");
            return;
        }

        // 先收集场景中所有 NPC 状态到 NPCStateManager
        NPCStateManager.Instance?.CollectLiveNPCs();

        // 构建存档数据
        GameSaveData data = new GameSaveData
        {
            sceneName             = SceneManager.GetActiveScene().name,
            playerPosX            = player.transform.position.x,
            playerPosY            = player.transform.position.y,
            playerPosZ            = player.transform.position.z,
            inventoryItemNames    = new List<string>(),
            selectedIndex         = InventoryManager.Instance.selectedIndex,
            //pickedUpItemIDs       = new List<string>(PickupStateManager.Instance.pickedItems),
            saveTime              = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            triggeredDialogueIDs  = new List<string>(),
            playerMovementMonitorAwake = PlayerMovementMonitor.awake,
            TelepreviousSceneName = doorTeleport.previousSceneName,
            GlobalTime            = GlobalTimer.ElapsedTime,
            livingroomrunning = gameover2.momisrunning,
            t1hasShown = TextPopup.hasShown,
            t2hasShown = TextPopup2.hasShown,
            t3hasShown = TextPopup3.hasShown,
            tv = TurnTV.tvon,
            TVisplaying = TVplaying.isplaying,
            Passwordcorrect = PasswordSystem.correct,
            keystolen = Timer.stolenkey,
            TimerhasFinished = Timer.TimerFinished,
            momhasgoout = NPCcontinueMover.hasgoout,
            TimerhasStarted = Timer.hasStarted,
            TimerIsRunning  = Timer.instance != null && Timer.instance.isRunning,
            TimerCurrentTime = Timer.instance != null ? Timer.instance.currentTime : 0f,
            npcDoismoving = NPCcontinueMover.doisMoving
        };

        Debug.Log($"[Save] Timer.hasStarted={Timer.hasStarted}, Timer.isRunning={(Timer.instance!=null ? Timer.instance.isRunning : false)}");


        var npcList = new List<NPCcontinueMoverSaveEntry>();
        foreach (var mover in FindObjectsOfType<NPCcontinueMover>())
        {
            Vector3 pos = mover.transform.position;
            npcList.Add(new NPCcontinueMoverSaveEntry
            {
                npcID = mover.npcID,
                currentStage = mover.GetCurrentStage(),
                isMoving = mover.GetIsMoving(),
                isTriggered = mover.GetIsTriggered(),
                hassetdisvisable = mover.GetHasSetDisvisable(),
                hasFinishedMoving = mover.GetHasFinishedMoving(),
                posX = pos.x,
                posY = pos.y,
                posZ = pos.z
            });
        }
        data.npcMoves = npcList;

        

        // 继承上一次会话的对话触发列表
        if (currentSaveData != null && currentSaveData.triggeredDialogueIDs != null)
            data.triggeredDialogueIDs = new List<string>(currentSaveData.triggeredDialogueIDs);

            // ... 你原有的构建 data 代码之后
        if (DoorStateManager.Instance != null)
            data.doorStates = DoorStateManager.Instance.ExportToList();


        // 保存玩家动画 / 朝向
        var animator = player.GetComponent<Animator>();
        if (animator != null)
            data.playerAnimIntState = animator.GetInteger("state");
        var movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            data.lastMoveDirX = movement.LastMoveDir.x;
            data.lastMoveDirY = movement.LastMoveDir.y;
        }

        // 保存当前装备状态（需有 EquipmentManager）
        if (EquipmentManager.Instance != null)
        {
            data.hasEquipped      = EquipmentManager.IsEquippedAny();
            data.equippedItemName = EquipmentManager.GetEquippedName();
        }
        else
        {
            data.hasEquipped      = false;
            data.equippedItemName = null;
        }

        var doors = GameObject.FindObjectsOfType<DoorToggle2D>(true);
data.doorToggleStates.Clear();
foreach (var d in doors)
{
    if (!string.IsNullOrEmpty(d.doorID))
    {
        data.doorToggleStates.Add(new DoorToggleSaveEntry
        {
            doorID = d.doorID,
            isOpen = d.IsOpen
        });
    }
}


        // 保存背包列表
        foreach (var item in InventoryManager.Instance.inventory)
            if (item != null)
                data.inventoryItemNames.Add(item.itemName);

        // 最后把所有 NPCState 导出到存档对象里
        if (NPCStateManager.Instance != null)
            data.npcStates = NPCStateManager.Instance.ExportToList();

            // 保存玻璃门状态
if (GlassStateManager.Instance != null)
    data.glassStates = GlassStateManager.Instance.ExportToList();


        currentSaveData = data;
        SaveSystem.SaveGame(data, buttonindex);
        Debug.Log($"游戏已保存至槽位 {buttonindex}");
    }

    // 读取存档（默认用当前 buttonindex）
    public void LoadGame()
    {
        LoadGame(buttonindex);
    }

    // 读取并异步恢复
    public void LoadGame(int slot)
    {
        isLoadingSave = true;

        GameSaveData data = SaveSystem.LoadGame(slot);
        if (data == null)
        {
            Debug.LogWarning($"槽位 {slot} 无存档可读取");
            isLoadingSave = false;
            return;
        }

        currentSaveData = data;

        // 先把门状态导入到内存（在切场景前）
        DoorStateManager.Instance?.ImportFromList(data.doorStates);

        GlassStateManager.Instance?.ImportFromList(data.glassStates);

        // 导入 NPC 存档到内存，确保场景里 NPC Awake 能拿到
        NPCStateManager.Instance?.ImportFromList(data.npcStates);

        PhoneUIManager.Instance?.ResetPhoneStack();

        // 切场景（由你的 GameLoader 负责）
        GameLoader.Instance.LoadGame(slot);

        // 等待新场景完成后再恢复玩家、背包、对话等
        StartCoroutine(RestoreGame(data));
    }

    private IEnumerator RestoreGame(GameSaveData data)
    {
        // 等待场景名字对上
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == data.sceneName);
        // 再等一帧，确保所有 Awake/Start 执行完
        yield return null;

        DialogueSystem.isInDialogue = false;
        InventoryManager.Instance?.SetInventoryVisible(true);

        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(
                data.playerPosX, data.playerPosY, data.playerPosZ);

            var animator = player.GetComponent<Animator>();
            if (animator != null)
                animator.SetInteger("state", data.playerAnimIntState);

            var movement = player.GetComponent<PlayerMovement>();
            movement?.SetLastMoveDir(
                new Vector2(data.lastMoveDirX, data.lastMoveDirY));
        }

        // 恢复玻璃门
        var oilDoors = GameObject.FindObjectsOfType<DoorToggleOil>(true);
        foreach (var entry in data.glassStates)
        {
            foreach (var d in oilDoors)
            {
                if (d.doorID == entry.doorID && SceneManager.GetActiveScene().name == entry.sceneName)
                {
                    d.SetOpen(entry.isOpen);
                    d.SetOiled(entry.isOiled);

                    // interactStep 修正：涂过油直接跳过 step=0
                    d.interactStep = entry.isOiled ? 1 : 0;
                }
            }
        }


        PickupStateManager.Instance?.RefreshWorldItems();

        GlobalTimer.ElapsedTime = data.GlobalTime;
        GlobalTimer.StartTimer(reset: false);

        PlayerMovementMonitor.awake    = data.playerMovementMonitorAwake;
        doorTeleport.previousSceneName = data.TelepreviousSceneName;
        gameover2.momisrunning = data.livingroomrunning;
        TextPopup.hasShown = data.t1hasShown;
        TextPopup2.hasShown = data.t2hasShown;
        TextPopup3.hasShown = data.t3hasShown;
        TurnTV.tvon = data.tv;
        TVplaying.isplaying = data.TVisplaying;

        PasswordSystem.correct = data.Passwordcorrect;
        Timer.stolenkey = data.keystolen;
        Timer.TimerFinished = data.TimerhasFinished;
        NPCcontinueMover.hasgoout = data.momhasgoout;
        NPCcontinueMover.doisMoving = data.npcDoismoving;
        Timer.hasStarted = data.TimerhasStarted;
        if (Timer.instance != null)
        {
            Timer.instance.isRunning   = data.TimerIsRunning;
            Timer.instance.currentTime = data.TimerCurrentTime;
        }

        // 还原装备
        if (data.hasEquipped && !string.IsNullOrEmpty(data.equippedItemName))
        {
            EquipmentManager.RestoreEquipFromSave(data.equippedItemName);
        }
        else
    {
        // 存档里没有装备，但运行时可能有，需要卸下
        if (EquipmentManager.Instance != null && EquipmentManager.IsEquippedAny())
        {
            EquipmentManager.Instance.reset();
        }
    }

        // 读取时（RestoreGame 里）
    var doors = GameObject.FindObjectsOfType<DoorToggle2D>(true);
    foreach (var entry in data.doorToggleStates)
    {
        foreach (var d in doors)
        {
            if (d.doorID == entry.doorID)
            {
                d.SetOpen(entry.isOpen);
            }
        }
    }

    //         foreach (var mover in FindObjectsOfType<NPCcontinueMover>())
    // {
    //     var entry = data.npcMoves.Find(e => e.npcID == mover.npcID);
    //     if (entry != null)
    //     {
    //         // 恢复状态
    //         mover.SetState(entry.currentStage, entry.isMoving, entry.isTriggered,
    //                     entry.hassetdisvisable, entry.hasFinishedMoving);

    //         // 恢复坐标
    //         Vector3 pos = new Vector3(entry.posX, entry.posY, entry.posZ);
    //         var rb2d = mover.GetComponent<Rigidbody2D>();
    //         if (rb2d != null)
    //         {
    //             rb2d.position        = pos;
    //             rb2d.velocity        = Vector2.zero;
    //             rb2d.angularVelocity = 0f;
    //         }
    //         else
    //         {
    //             mover.transform.position = pos;
    //         }
    //     }
    // }




        // 4) 最后，按存档把 NPC 全部贴回去（使用 sceneName 匹配）
        if (data.npcStates != null && data.npcStates.Count > 0)
        {
            var allNPCs = GameObject.FindObjectsOfType<NPCcontinueMovernormal>(true);
            string curName = SceneManager.GetActiveScene().name;

            foreach (var entry in data.npcStates)
            {
                if (entry.sceneName != curName)
                    continue;

                foreach (var npc in allNPCs)
                {
                    var idc = npc.GetComponent<NPCId>();
                    if (idc != null && idc.Id == entry.npcId)
                    {
                        Vector3 pos = new Vector3(entry.x, entry.y, entry.z);
                        var rb2d = npc.GetComponent<Rigidbody2D>();
                        if (rb2d != null)
                        {
                            rb2d.position        = pos;
                            rb2d.velocity        = Vector2.zero;
                            rb2d.angularVelocity = 0f;
                        }
                        else
                        {
                            npc.transform.position = pos;
                        }

                        npc.currentStage      = Mathf.Clamp(entry.stage, 0, npc.movePoints.Length);
                        npc.hasFinishedMoving = entry.finished;
                        npc.SetVisible(entry.visible);
                        npc.isTriggered       = entry.triggered;
                        NPCcontinueMovernormal.momroomisTriggered = entry.triggered;
                        npc.isMoving          = false;
                        npc.moveint           = entry.moveint;
                        break;
                    }
                }
            }

            Debug.Log("NPC 已按存档贴回正确的位置/状态");
        }
        // 在 RestoreGame 的末尾（或在 isLoadingSave -> false 之前）：
        
        
    yield return new WaitUntil(() => SceneManager.GetActiveScene().name == data.sceneName);
    yield return null; // 等 1 帧，保证 Awake/Start 都执行了

    // ……恢复玩家、背包、门、Timer 等逻辑……

    // 再延迟几帧恢复 NPC
    yield return StartCoroutine(ApplyNPCStatesWithDelay(data, 0));

    StartCoroutine(ApplyGlassStatesWithDelay(data, 0)); // 3 帧更保守，可改成 1-3

        //Debug.Log("游戏数据已恢复");
        isLoadingSave = false;
    }

    public void ClearCurrentTriggeredDialogueRecords()
    {
        if (currentSaveData?.triggeredDialogueIDs != null)
        {
            currentSaveData.triggeredDialogueIDs.Clear();
            Debug.Log("已清除当前游戏内存中对话触发记录");
        }
        else
        {
            Debug.LogWarning("当前存档数据为空，无法清除对话触发记录");
        }
    }

        private IEnumerator ApplyGlassStatesWithDelay(GameSaveData data, int delayFrames = 2)
    {
        // 等待几帧，确保场景内所有物体的 Start 都执行完（有时需要多等一帧）
        for (int i = 0; i < delayFrames; i++)
            yield return null;

        string curScene = SceneManager.GetActiveScene().name;
        int savedCount = data?.glassStates?.Count ?? 0;

        var oilDoors = GameObject.FindObjectsOfType<DoorToggleOil>(true);

        // 列出场内 doorIDs（诊断）
        foreach (var d in oilDoors)
        {
        }

        if (data?.glassStates == null)
        {
            yield break;
        }

        foreach (var entry in data.glassStates)
        {
            bool matched = false;
            foreach (var d in oilDoors)
            {
                if (d == null) continue;

                // 精确匹配：同 sceneName 且 doorID 相同
                if (entry.sceneName == curScene && d.doorID == entry.doorID)
                {
                    matched = true;
                    // 使用强制恢复，避免被其它系统覆盖
                    d.ForceRestoreState(entry.isOpen, entry.isOiled);
                    break;
                }
            }
            
        }
    }

    private IEnumerator ApplyNPCStatesWithDelay(GameSaveData data, int delayFrames = 2)
{
    for (int i = 0; i < delayFrames; i++)
        yield return null;

    foreach (var mover in FindObjectsOfType<NPCcontinueMover>())
    {
        var entry = data.npcMoves.Find(e => e.npcID == mover.npcID);
        if (entry != null)
        {
            mover.SetState(entry.currentStage, entry.isMoving, entry.isTriggered,
                           entry.hassetdisvisable, entry.hasFinishedMoving);

            // ✅ 延迟后强制恢复坐标
            Vector3 pos = new Vector3(entry.posX, entry.posY, entry.posZ);
            var rb2d = mover.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                rb2d.position        = pos;
                rb2d.velocity        = Vector2.zero;
                rb2d.angularVelocity = 0f;
            }
            else
            {
                mover.transform.position = pos;
            }
        }
    }
    
    Debug.Log("NPC 状态 + 坐标恢复完毕");
}


}