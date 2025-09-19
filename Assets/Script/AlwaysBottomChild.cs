using UnityEngine;

public class AlwaysBottomChild : MonoBehaviour
{
    void LateUpdate()
    {
        // 每帧把自己放到兄弟节点的最下方
        transform.SetAsLastSibling();
    }
}
