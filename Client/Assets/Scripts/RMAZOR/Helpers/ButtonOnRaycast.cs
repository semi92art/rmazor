using System;
using Common.CameraProviders;
using Common.Managers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Helpers
{
    public class ButtonOnRaycast : MonoBehaviour
    {
        private enum EButtonState { Idle, Down, Hold, Up }
        private enum EFingerState { None, Out, Enter, Stay, Exit }

        [SerializeField] private new Collider   collider;
        [SerializeField] private     UnityEvent onClickEvent;

        private bool         m_Initialized;
        private EButtonState m_ButtonState;
        private EFingerState m_FingerState;
        private EFingerState m_PrevFingerState;
        private bool         m_EnteredOnThisTouchSession;
        private bool         m_ExitedOnThisTimeSession;

        private Func<ELevelStage>        m_LevelStage;
        private ICameraProvider          m_CameraProvider;
        private IHapticsManager          m_HapticsManager;
        private IViewInputTouchProceeder m_ViewInputTouchProceeder;


        public void Init(
            UnityAction              _Action,
            Func<ELevelStage>        _LevelStage, 
            ICameraProvider          _CameraProvider,
            IHapticsManager          _HapticsManager,
            IViewInputTouchProceeder _ViewInputTouchProceeder)
        {
            m_LevelStage              = _LevelStage;
            m_CameraProvider          = _CameraProvider;
            m_HapticsManager          = _HapticsManager;
            m_ViewInputTouchProceeder = _ViewInputTouchProceeder;
            onClickEvent.AddListener(_Action);
            m_Initialized = true;
        }

        private void Update()
        {
            if (!m_Initialized)
                return;
            if (m_LevelStage() == ELevelStage.Paused
                || m_LevelStage() == ELevelStage.CharacterKilled)
                return;
            m_PrevFingerState = m_FingerState;
            if (m_ViewInputTouchProceeder.AreFingersOnScreen(1))
                ProceedButtonNotIdleState();
            else
                ProceedButtonIdleState();
            ProceedButtonClick();
            if (m_ViewInputTouchProceeder.AreFingersOnScreen(1)) 
                return;
            m_EnteredOnThisTouchSession = false;
            m_ExitedOnThisTimeSession = false;
        }

        private void ProceedButtonIdleState()
        {
            if (m_ButtonState == EButtonState.Hold || m_ButtonState == EButtonState.Down)
                m_ButtonState = EButtonState.Up;
            else if (m_ButtonState == EButtonState.Up)
                m_ButtonState = EButtonState.Idle;
            m_FingerState = EFingerState.None;
        }


        private void ProceedButtonNotIdleState()
        {
            var pos = m_ViewInputTouchProceeder.GetFingerPosition(0);
            var ray = m_CameraProvider.Camera.ScreenPointToRay(pos);
            if (!Physics.Raycast(ray, out var hit))
            {
                if (m_FingerState == EFingerState.Stay)
                    m_FingerState = EFingerState.Exit;
                else if (m_FingerState == EFingerState.Exit || m_FingerState == EFingerState.None)
                    m_FingerState = EFingerState.Out;
        
                if (m_ButtonState == EButtonState.Hold || m_ButtonState == EButtonState.Down)
                    m_ButtonState = EButtonState.Up;
                else if (m_ButtonState == EButtonState.Up)
                    m_ButtonState = EButtonState.Idle;
                return;
            }
            if (m_ButtonState == EButtonState.Idle)
                m_ButtonState = EButtonState.Down;
            else if (m_ButtonState == EButtonState.Down)
                m_ButtonState = EButtonState.Hold;

            if (hit.collider != collider)
            {
                if (m_FingerState == EFingerState.Stay)
                    m_FingerState = EFingerState.Exit;
                else if (m_FingerState == EFingerState.Exit || m_FingerState == EFingerState.None)
                    m_FingerState = EFingerState.Out;
                return;
            }
            if (m_FingerState == EFingerState.Out)
                m_FingerState = EFingerState.Enter;
            else if (m_FingerState == EFingerState.None || m_FingerState == EFingerState.Enter)
                m_FingerState = EFingerState.Stay;
        }

        private void ProceedButtonClick()
        {
            if (m_FingerState == EFingerState.Enter)
                m_EnteredOnThisTouchSession = true;
            else if (m_FingerState == EFingerState.Exit)
                m_ExitedOnThisTimeSession = true;
            if (m_EnteredOnThisTouchSession || m_ExitedOnThisTimeSession)
                return;
            if (m_PrevFingerState != EFingerState.Stay || m_ButtonState != EButtonState.Up)
                return;
            m_HapticsManager.PlayPreset(EHapticsPresetType.Selection);
            onClickEvent?.Invoke();
        }
    }
}
