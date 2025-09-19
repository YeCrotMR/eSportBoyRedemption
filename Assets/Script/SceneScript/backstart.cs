using UnityEngine;
using UnityEngine.SceneManagement;

public class backstart : MonoBehaviour
{
    // 设置目标场景名称（在Inspector里填）
    public int index;

    // 按钮点击调用这个方法
    public void ChangeScene()
    {
        DoorStateManager.Instance?.ClearAll();
        doorTeleport.momunHasunlocked = false;
        doorTeleport.previousSceneName = "";

        // 3) 其他重置
        TextPopup.hasShown = false;
        TextPopup2.hasShown = false;
        TextPopup3.hasShown = false;
        //NPCcontinueMovernormal.moveint = 0;
        gameover2.momisrunning = false;
        PlayerMovementMonitor.awake = false;
        GlobalTimer.ResetTimer();
        GlobalTimer.StopTimer();
        Timer.instance?.ResetTimer();
        FadeController.Instance?.FadeAndLoadScene(index);
    
        Debug.Log("切换场景成功");
    }
}