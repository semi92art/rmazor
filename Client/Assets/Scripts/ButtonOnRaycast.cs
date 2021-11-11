using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ButtonOnRaycast : MonoBehaviour
{
    public                   Collider                 collider;
    [SerializeField] private UnityEvent               OnClickEvent;
    private                  System.Func<ELevelStage> m_LevelStage;
    private                  ICameraProvider          m_CameraProvider;
    private                  bool                     m_Initialized;
    
    private int m_State;
    private Ray m_Ray;

    private void Update()
    {
        if (!m_Initialized
            || !Mouse.current.leftButton.wasPressedThisFrame 
            || m_LevelStage() == ELevelStage.Paused)
        {
            m_State = 0;
            return;
        }
        m_Ray = m_CameraProvider.MainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(m_Ray, out var hit)) 
            return;
        if (hit.collider != collider)
            return;
        if (m_State == 0)
            OnClickEvent?.Invoke();
        m_State = 1;
    }

    public void Init(UnityAction _Action, System.Func<ELevelStage> _LevelStage, ICameraProvider _CameraProvider)
    {
        OnClickEvent.AddListener(_Action);
        m_LevelStage = _LevelStage;
        m_CameraProvider = _CameraProvider;
        m_Initialized = true;
    }
}