using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TeleportInfo
{
    public static Vector3 targetPosition = Vector3.zero;
    public static bool useTargetPosition = false;

    // 新增字段：是否启用移动
    public static bool shouldEnableMovement = false;
}
