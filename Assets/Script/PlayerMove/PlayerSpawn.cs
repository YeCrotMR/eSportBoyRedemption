using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    void Start()
    {
        if (TeleportInfo.useTargetPosition)
        {
            transform.position = TeleportInfo.targetPosition;
            TeleportInfo.useTargetPosition = false;
        }
    }
}