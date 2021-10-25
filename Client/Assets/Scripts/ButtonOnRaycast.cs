using Lean.Touch;
using UnityEngine;
using UnityEngine.Events;

public class ButtonOnRaycast : MonoBehaviour
{
    public Collider collider;
    public UnityEvent OnClickEvent;
    
    private int m_State;
    private Ray m_Ray;

    private void Update()
    {
        if (Input.touchCount == 0 && !Input.GetMouseButtonDown(0))
        {
            m_State = 0;
            return;
        }
        m_Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(m_Ray, out var hit)) 
            return;
        if (hit.collider != collider)
            return;
        if (m_State == 0)
            OnClickEvent?.Invoke();
        m_State = 1;
    }
}