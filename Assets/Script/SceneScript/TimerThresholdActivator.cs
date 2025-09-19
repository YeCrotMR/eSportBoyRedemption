using UnityEngine;
using System.Collections;

/// 方案B：由“唯一控制者”负责暂停/继续计时，其它触发器只做时间比较
/// 用法：
/// - 在需要“玩家在场暂停，不在场继续”的场景里，只给一份 TimerThresholdActivator 勾选 controlGlobalTimer=true。
/// - 其它同类触发器不要勾选 controlGlobalTimer，它们只比较 GlobalTimer.ElapsedTime >= triggerTime 后触发。
public class TimerThresholdActivator : MonoBehaviour
{
    [Header("触发阈值（秒）")]
    [Tooltip("达到这个时间(秒)时触发（使用 GlobalTimer.ElapsedTime 比较）")]
   public static float triggerTime = 20f; // 注意：不要用 static，Inspector 才能生效

    [Tooltip("触发一次后是否停止监听")]
    public bool oneShot = true;

    [Header("触发时要激活的对象")]
    public GameObject[] activateOnTrigger;

    [Header("触发时要隐藏的对象")]
    public GameObject[] deactivateOnTrigger;

    [Tooltip("启用时先把 activateOnTrigger 隐藏、deactivateOnTrigger 显示，等到触发再切换回去")]
    public bool setInitialStateUntilTrigger = false;

    [Header("玩家检测（仅用于‘唯一控制者’暂停/继续计时）")]
    [Tooltip("是否由本实例负责控制 GlobalTimer 的暂停/继续（全场景只勾选一个）")]
    public bool controlGlobalTimer = false;

    [Tooltip("玩家的Tag")]
    public string playerTag = "Player";

    [Tooltip("仅当玩家与本脚本所在同一 Scene 才视为“在场”")]
    public bool sameSceneOnly = true;

    [Tooltip("玩家检测间隔（秒）")]
    public float checkInterval = 0.2f;

    [Tooltip("当控制者被禁用/离场时，是否自动恢复计时")]
    public bool resumeOnDisable = true;

    // 内部状态
    public bool triggered { get; private set; }

    // 以下仅在 controlGlobalTimer=true 的实例上使用
    private bool playerPresent;
    private Coroutine presenceRoutine;

    [Header("可选：触发回调示例")]
    public NPCcontinueMovernormal npc2;

    private void OnEnable()
    {
        triggered = false;

        if (setInitialStateUntilTrigger)
        {
            SetActiveArray(activateOnTrigger, false);
            SetActiveArray(deactivateOnTrigger, true);
        }

        // 仅由“唯一控制者”去暂停/继续全局计时器
        if (controlGlobalTimer)
        {
            GlobalTimer.EnsureInstance(); // 确保有计时器实例（不会重复创建）

            // 初次刷新一次并应用
            UpdatePlayerPresence(force: true);
            ApplyPause();

            // 启动定时检测
            presenceRoutine = StartCoroutine(PresenceCheckLoop());
        }

        // 进场时尝试一次（读档后若时间已恢复，则能立即触发）
        TryTrigger();
    }

    private void OnDisable()
    {
        if (controlGlobalTimer && presenceRoutine != null)
        {
            StopCoroutine(presenceRoutine);
            presenceRoutine = null;
        }

        // 关键：离开这个场景时，确保全局计时继续
        if (controlGlobalTimer && resumeOnDisable)
        {
            GlobalTimer.StartTimer(reset: false);
        }
    }

    private IEnumerator PresenceCheckLoop()
    {
        var wait = new WaitForSeconds(checkInterval);
        while (true)
        {
            UpdatePlayerPresence();
            ApplyPause();
            yield return wait;
        }
    }

    private void Update()
    {
        if (!triggered)
            TryTrigger();
    }

    private void UpdatePlayerPresence(bool force = false)
    {
        bool nowPresent = IsPlayerPresent();
        if (!force && nowPresent == playerPresent) return;
        playerPresent = nowPresent;
    }

    private bool IsPlayerPresent()
    {
        var players = GameObject.FindGameObjectsWithTag(playerTag);
        if (players == null || players.Length == 0) return false;

        if (!sameSceneOnly)
        {
            foreach (var p in players)
                if (p != null && p.activeInHierarchy) return true;
            return false;
        }

        var scene = gameObject.scene;
        foreach (var p in players)
            if (p != null && p.activeInHierarchy && p.scene == scene) return true;

        return false;
    }

    // 仅“唯一控制者”调用：根据是否在场暂停/继续全局计时器
    private void ApplyPause()
    {
        if (!controlGlobalTimer) return;

        if (playerPresent)
        {
            GlobalTimer.StopTimer();
        }
        else
        {
            // 离场继续计时，不重置
            GlobalTimer.StartTimer(reset: false);
        }
    }

    private void TryTrigger()
    {
        if (GlobalTimer.ElapsedTime >= triggerTime)
            DoTrigger();
    }

    private void DoTrigger()
    {
        triggered = true;

        SetActiveArray(activateOnTrigger, true);
        SetActiveArray(deactivateOnTrigger, false);

        // 示例：触发NPC的某个状态（按你的需求设置）
        if (npc2 != null) npc2.isTriggered = true;

        if (oneShot)
            enabled = false; // 停止继续检查
    }

    private static void SetActiveArray(GameObject[] arr, bool active)
    {
        if (arr == null) return;
        foreach (var go in arr)
        {
            if (go != null && go.activeSelf != active)
                go.SetActive(active);
        }
    }

    // 手动复位（如果需要再次允许触发）
    public void ResetTrigger()
    {
        triggered = false;
        enabled = true;

        if (setInitialStateUntilTrigger)
        {
            SetActiveArray(activateOnTrigger, false);
            SetActiveArray(deactivateOnTrigger, true);
        }
    }

    // 读档后如需立刻重算，可调用
    public void ForceRecalc()
    {
        if (controlGlobalTimer)
        {
            UpdatePlayerPresence(force: true);
            ApplyPause();
        }
        TryTrigger();
    }
}
