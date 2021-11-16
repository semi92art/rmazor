using System;
using System.Collections.Generic;
using Constants;
using DI.Extensions;
using Exceptions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Lean.Common;
using Lean.Touch;
using Ticker;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze.Views.InputConfigurators
{
    public interface IViewInputTouchProceeder : IInit, IOnLevelStageChanged
    {
        UnityAction<Vector2> OnTap { get; set; }
    }
    
    public class ViewInputTouchProceeder : IViewInputTouchProceeder, IUpdateTick
    {

        #region nonpublic members

        private          LeanFingerTap       m_LeanFingerTap;
        private readonly List<Vector2>       m_TouchPositionsQueue = new List<Vector2>(500);
        private          EMazeMoveDirection? m_PrevMoveDirection;
        
        #endregion

        #region inject

        protected      IModelGame                  Model             { get; }
        protected      IContainersGetter           ContainersGetter  { get; }
        protected      IViewInputCommandsProceeder CommandsProceeder { get; }
        private        IViewGameTicker                 GameTicker        { get; }
        private static ICameraProvider             CameraProvider    { get; set; }

        protected ViewInputTouchProceeder(
            IModelGame _Model,
            IContainersGetter _ContainersGetter,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewGameTicker _GameTicker,
            ICameraProvider _CameraProvider)
        {
            Model = _Model;
            ContainersGetter = _ContainersGetter;
            CommandsProceeder = _CommandsProceeder;
            GameTicker = _GameTicker;
            CameraProvider = _CameraProvider;
        }

        #endregion

        #region api

        public event UnityAction    Initialized;
        public UnityAction<Vector2> OnTap { get; set; }

        public virtual void Init()
        {
            GameTicker.Register(this);
            InitLeanTouch();
            InitLeanTouchForTapToNext();
            // InitLeanTouchForRotate();
            Initialized?.Invoke();
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Paused)
                m_LeanFingerTap.OnFinger.RemoveListener(MoveNext);
            else if (_Args.PreviousStage == ELevelStage.Paused)
                m_LeanFingerTap.OnFinger.AddListener(MoveNext);
        }
        
        public void UpdateTick()
        {
            ProceedTouchForMove();
        }

        
        #endregion
        
        #region nonpublic methods
        
        private void InitLeanTouch()
        {
            var goLeanTouch = new GameObject("Lean Touch");
            goLeanTouch.SetParent(GetContainer());
            var lt = goLeanTouch.AddComponent<LeanTouch>();
            lt.useGUILayout = false;
            lt.TapThreshold = 0.5f;
            lt.SwipeThreshold = 50f;
            lt.ReferenceDpi = 200;
            lt.GuiLayers = LayerMask.GetMask(LayerNames.Touchable);
            lt.RecordFingers = true;
            lt.RecordThreshold = 5f;
            lt.RecordLimit = 0f;
            lt.SimulateMultiFingers = true;
            lt.PinchTwistKey = KeyCode.LeftControl;
            lt.MovePivotKey = KeyCode.LeftAlt;
            lt.MultiDragKey = KeyCode.LeftAlt;
#if UNITY_EDITOR
            lt.FingerTexture = PrefabUtilsEx.GetObject<Texture2D>("icons", "finger_texture");
#endif
        }

        private void InitLeanTouchForTapToNext()
        {
            var goLeanFingerTap = new GameObject("Lean Finger Tap");
            goLeanFingerTap.SetParent(GetContainer());
            var lft = goLeanFingerTap.AddComponent<LeanFingerTap>();
            lft.OnFinger.AddListener(MoveNext);
            lft.OnFinger.AddListener(_Finger => OnTap?.Invoke(_Finger.ScreenPosition));
            m_LeanFingerTap = lft;
        }
        
        private void InitLeanTouchForRotate()
        {
            var ldt = InitPrefab<LeanDragTrail>("lean_drag_trail");
            var lsc = InitPrefab<LeanShape>("lean_shape_circle");
            var lscDetClockwise = InitPrefab<LeanShapeDetector>("lean_shape_detector_clockwise_circle");
            var lscDetCounter = InitPrefab<LeanShapeDetector>("lean_shape_detector_counter_clockwise_circle");
            lscDetClockwise.Shape = lsc;
            lscDetCounter.Shape = lsc;
            
            lscDetClockwise.OnDetected.AddListener(_Finger => RotateCommand(true));
            lscDetCounter.OnDetected.AddListener(_Finger => RotateCommand(false));
        }
        
        private T InitPrefab<T>(string _Name) where T : Component
        {
            return PrefabUtilsEx
                .InitPrefab(GetContainer(), "ui_game", _Name)
                .GetComponent<T>();
        }

        private void RotateCommand(bool _Clockwise)
        {
            if (LeanInput.GetTouchCount() < 2)
                return;
            CommandsProceeder.RaiseCommand(
                _Clockwise ? EInputCommand.RotateClockwise : EInputCommand.RotateCounterClockwise, 
                null);
        }

        private void ProceedTouchForMove()
        {
            const int maxTactsToCheck = 10;
            if (!IsFingerOnScreen())
            {
                if (m_TactCount == 0)
                    return;
                m_TactCount = 0;
                m_PrevMoveDirection = null;
                m_TouchPositionsQueue.Clear();
                return;
            }
            
            var pos = GetFingerPosition();
            
            for (int i = 0; i < Math.Min(maxTactsToCheck, m_TactCount); i++)
            {
                var prevPos = m_TouchPositionsQueue[m_TactCount - 1 - i];
                var delta = pos - prevPos;
                var moveDir = GetMoveDirection(delta);
                if (!moveDir.HasValue)
                    continue;
                if (m_PrevMoveDirection == moveDir)
                    continue;
                var command = GetMoveCommand(moveDir.Value);
                CommandsProceeder.RaiseCommand(command, null);
                m_PrevMoveDirection = moveDir.Value;
                break;
            }
            
            m_TouchPositionsQueue.Add(pos);
            m_TactCount++;
        }

        private int m_TactCount;

        public static bool IsFingerOnScreen()
        {
#if UNITY_EDITOR
            var view = CameraProvider.MainCamera.ScreenToViewportPoint(LeanInput.GetMousePosition());
            var isOutside = view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;
            return !isOutside && LeanInput.GetMousePressed(0);
#else
            return LeanInput.GetTouchCount() == 1;
#endif
        }
        
        public static Vector2 GetFingerPosition()
        {
#if UNITY_EDITOR
            return LeanInput.GetMousePosition();
#else
            CommonUtils.GetTouch(
                0, out _, out var pos, out _, out _, out _);
            return pos;
#endif
        }

        private EMazeMoveDirection? GetMoveDirection(Vector2 _Delta)
        {
            const float angThreshold = 30f * Mathf.Deg2Rad;
            const float distThreshold = 0.05f;
            EMazeMoveDirection? res = null;
            float absDx = Mathf.Abs(_Delta.x);
            float absDy = Mathf.Abs(_Delta.y);
            float absNormDx = Mathf.Abs( _Delta.x / GraphicUtils.ScreenSize.x);
            float absNormDy = Mathf.Abs(_Delta.y / GraphicUtils.ScreenSize.y);
            if (absDx > absDy && absDy / absDx < Mathf.Tan(angThreshold) && absNormDx > distThreshold)
                res = _Delta.x < 0 ? EMazeMoveDirection.Left : EMazeMoveDirection.Right;
            else if (absDx < absDy && absDx / absDy < Mathf.Tan(angThreshold) && absNormDy > distThreshold)
                res = _Delta.y < 0 ? EMazeMoveDirection.Down : EMazeMoveDirection.Up;
            return res;
        }

        private EInputCommand GetMoveCommand(EMazeMoveDirection _MoveDirection)
        {
            switch (_MoveDirection)
            {
                case EMazeMoveDirection.Left:
                    return EInputCommand.MoveLeft;
                case EMazeMoveDirection.Right:
                    return EInputCommand.MoveRight;
                case EMazeMoveDirection.Down:
                    return EInputCommand.MoveDown;
                case EMazeMoveDirection.Up:
                    return EInputCommand.MoveUp;
                default:
                    throw new SwitchCaseNotImplementedException(_MoveDirection);
            }
        }

        private Transform GetContainer()
        {
            return ContainersGetter.GetContainer(ContainerNames.TouchInput);
        }

        private void MoveNext(LeanFinger _Finger)
        {
            if (!(_Finger.LastScreenPosition.y / GraphicUtils.ScreenSize.y < 0.8f)) 
                return;
            if (Model.LevelStaging.LevelStage != ELevelStage.Finished) 
                return;
            CommandsProceeder.RaiseCommand(EInputCommand.ReadyToUnloadLevel, null);
        }

        #endregion
    }
}