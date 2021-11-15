using Controllers;
using Games.RazorMaze.Views.InputConfigurators;
using UnityEngine;
using UnityEngine.Events;

public class ButtonOnRaycast : MonoBehaviour
{
    public new               Collider                 collider;
    [SerializeField] private UnityEvent               onClickEvent;
    private                  System.Func<ELevelStage> m_LevelStage;
    private                  ICameraProvider          m_CameraProvider;
    private                  IHapticsManager          m_HapticsManager;
    private                  bool                     m_Initialized;

    // private Vector2? m_ScreenPosition;
    private int      m_State;
    private Ray      m_Ray;

    private void Update()
    {
        if (!m_Initialized 
            || !ViewInputTouchProceeder.IsFingerOnScreen()
            ||  m_LevelStage() == ELevelStage.Paused)
        {
            m_State = 0;
            return;
        }
        var pos = ViewInputTouchProceeder.GetFingerPosition();
        m_Ray = m_CameraProvider.MainCamera.ScreenPointToRay(pos);
        if (!Physics.Raycast(m_Ray, out var hit)) 
            return;
        if (hit.collider != collider)
            return;
        if (m_State == 0)
        {
            m_HapticsManager.PlayPreset(EHapticsPresetType.Selection);
            onClickEvent?.Invoke();
        }
        m_State = 1;
    }

    public void Init(
        UnityAction _Action,
        System.Func<ELevelStage> _LevelStage, 
        ICameraProvider _CameraProvider,
        IHapticsManager _HapticsManager)
    {
        onClickEvent.AddListener(_Action);
        m_LevelStage = _LevelStage;
        m_CameraProvider = _CameraProvider;
        m_HapticsManager = _HapticsManager;
        m_Initialized = true;
    }

    public void OnTap(Vector2 _ScreenPosition)
    {
        // m_ScreenPosition = _ScreenPosition;
    }
}