using UnityEngine;

[DisallowMultipleComponent]
public class NPCId : MonoBehaviour
{
    [SerializeField] private string npcId;
    public string Id => npcId;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(npcId))
        {
            // 首次给个默认值，之后请手动确认/固定
            npcId = $"{gameObject.name}_{System.Guid.NewGuid().ToString("N").Substring(0,8)}";
        }
    }
#endif
}
