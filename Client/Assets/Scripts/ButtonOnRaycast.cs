using Lean.Common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ButtonOnRaycast : MonoBehaviour
{
    public new               Collider                 collider;
    [SerializeField] private UnityEvent               onClickEvent;
    private                  System.Func<ELevelStage> m_LevelStage;
    private                  ICameraProvider          m_CameraProvider;
    private                  bool                     m_Initialized;
    
    private int m_State;
    private Ray m_Ray;

    private void Update()
    {
        if (!m_Initialized
            || !IsTouch()
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
            onClickEvent?.Invoke();
        m_State = 1;
    }

    public void Init(UnityAction _Action, System.Func<ELevelStage> _LevelStage, ICameraProvider _CameraProvider)
    {
        onClickEvent.AddListener(_Action);
        m_LevelStage = _LevelStage;
        m_CameraProvider = _CameraProvider;
        m_Initialized = true;
    }

    private bool IsTouch()
    {
#if UNITY_EDITOR
        return LeanInput.GetMouseDown(0);
#else
        int touchCount = LeanInput.GetTouchCount();
        if (touchCount == 0)
            return false;
        Utils.CommonUtils.GetTouch(0, out _, out _, out _, out bool began, out _);
        return began;
#endif
    }
}