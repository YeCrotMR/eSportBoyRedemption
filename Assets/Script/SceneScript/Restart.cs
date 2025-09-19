using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    public int index;

    public void ChangeScene()
    {
        // 1) 结束任何读档流程，并创建全新的内存存档容器
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isLoadingSave = false;
            GameManager.Instance.currentSaveData = new GameSaveData(); // 新游戏：空存档容器（可写入对话记录）

            // 若项目里还有地方在用 SaveSystem.GetCurrentSaveData()，同步一下（若无此方法可忽略
        }

        // 2) 清门状态 + 清静态标志
        DoorStateManager.Instance?.ClearAll();
        doorTeleport.momunHasunlocked = false;
        doorTeleport.previousSceneName = "";

        // 3) 其他重置
        TextPopup.hasShown = false;
        TextPopup2.hasShown = false;
        TextPopup3.hasShown = false;
        TurnTV.tvon = false;
        TVplaying.isplaying = false;
        PasswordSystem.correct = false;
        Timer.stolenkey = false;
        Timer.TimerFinished = false;
        NPCcontinueMover.hasgoout = false;
        NPCcontinueMover.doisMoving = false;
        EquipmentManager.Instance?.reset();
        //NPCcontinueMovernormal.moveint = 0;
        gameover2.momisrunning = false;
        PlayerMovementMonitor.awake = false;
        Timer.instance?.ResetTimer();
        InventoryManager.Instance?.ClearInventory();
        PhoneUIManager.Instance?.ResetPhoneStack();
        GlassStateManager.Instance.ResetAllStates();
        // 计时器重置；若需要新开局立刻计时可下一行打开
        GlobalTimer.ResetTimer();
        GlobalTimer.StopTimer();


        // 4) 切场景
        FadeController.Instance?.FadeAndLoadScene(index);
        Debug.Log("新游戏：切换场景成功");
    }
}
