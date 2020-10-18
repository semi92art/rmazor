using UnityEngine;

public class StayLastChild : MonoBehaviour
{
    private int m_ChidCount;
    
    private void LateUpdate()
    {
        if (m_ChidCount < transform.parent.childCount)
            transform.SetAsLastSibling();
        
        m_ChidCount = transform.parent.childCount;
    }
}