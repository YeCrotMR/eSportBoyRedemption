using UnityEngine;

public class AlwaysBottomChild : MonoBehaviour
{
    void LateUpdate()
    {
    
        transform.SetAsLastSibling();
    }
}
