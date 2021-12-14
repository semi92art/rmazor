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
        bool                 AreFingersOnScreen(int _Count);
        Vector2              GetFingerPosition(int _Index);
    }
    
    public class ViewInputTouchProceeder : IViewInputTouchProceeder
    {
        #region nonpublic members

        private          LeanFingerTap         m_LeanFingerTap;
        private readonly List<Vector2>         m_TouchPositionsQueue  = new List<Vector2>();
        private readonly List<Vector2>         m_TouchPositionsQueue2 = new List<Vector2>();
        private          EMazeMoveDirection?   m_PrevMoveDirection;
        private          float                 m_TouchForMoveTimer;
        private          EMazeRotateDirection? m_PrevRotateDirection;
        private          bool                  m_EnableRotation = true;
        private          bool                  m_IsOnFingerUp;
        
        #endregion

        #region inject

        private ViewSettings                ViewSettings      { get; }
        private IModelGame                  Model             { get; }
        private IContainersGetter           ContainersGetter  { get; }
        private IViewInputCommandsProceeder CommandsProceeder { get; }
        private IViewGameTicker             GameTicker        { get; }
        private ICameraProvider             CameraProvider    { get; }
        private IPrefabSetManager           PrefabSetManager  { get; }

        protected ViewInputTouchProceeder(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            IContainersGetter _ContainersGetter,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewGameTicker _GameTicker,
            ICameraProvider _CameraProvider,
            IPrefabSetManager _PrefabSetManager)
        {
            ViewSettings = _ViewSettings;
            Model = _Model;
            ContainersGetter = _ContainersGetter;
            CommandsProceeder = _CommandsProceeder;
            GameTicker = _GameTicker;
            CameraProvider = _CameraProvider;
            PrefabSetManager = _PrefabSetManager;
        }

        #endregion

        #region api

        public event UnityAction    Initialized;
        public UnityAction<Vector2> OnTap { get; set; }

        public virtual void Init()
        {
            InitLeanTouch();
            InitLeanTouchForMoveAndRotate();
            InitLeanTouchForTapToNext();
            Initialized?.Invoke();
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Paused)
                m_LeanFingerTap.OnFinger.RemoveListener(MoveNext);
            else if (_Args.PreviousStage == ELevelStage.Paused)
                m_LeanFingerTap.OnFinger.AddListener(MoveNext);
        }

        #endregion
        
        #region nonpublic methods
        
        private void InitLeanTouch()
        {
            var goLeanTouch = new GameObject(nameof(LeanTouch));
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
            lt.FingerTexture = PrefabSetManager.GetObject<Texture2D>("icons", "finger_texture");
#endif
            LeanTouch.OnFingerUp += LeanTouchOnOnFingerUp;
            var goLeanMultiUpdate = new GameObject("Lean Multi Update");
            goLeanMultiUpdate.SetParent(GetContainer());
            var lmu = goLeanMultiUpdate.AddComponent<LeanMultiUpdate>();
            lmu.OnFingers.AddListener(OnLeanMultiUpdateFingers);
        }

        private void InitLeanTouchForMoveAndRotate()
        {
            var goLeanMultiUpdate = new GameObject("Lean Multi Update");
            goLeanMultiUpdate.SetParent(GetContainer());
            var lmu = goLeanMultiUpdate.AddComponent<LeanMultiUpdate>();
            lmu.OnFingers.AddListener(OnLeanMultiUpdateFingers);
            LeanTouch.OnFingerUp += LeanTouchOnOnFingerUp;
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

        private void RotateCommand(EMazeRotateDirection _Direction)
        {
            var cmd = _Direction switch
            {
                EMazeRotateDirection.Clockwise        => EInputCommand.RotateClockwise,
                EMazeRotateDirection.CounterClockwise => EInputCommand.RotateCounterClockwise,
                _                                     => throw new SwitchCaseNotImplementedException(_Direction)
            };
            CommandsProceeder.RaiseCommand(cmd, null);
        }
        
        private void OnLeanMultiUpdateFingers(List<LeanFinger> _Fingers)
        {
            if (m_IsOnFingerUp)
            {
                m_IsOnFingerUp = false;
                return;
            }
            switch (_Fingers.Count)
            {
                case 1: ProceedTouchForMove(_Fingers[0]); break;
                case 2: ProceedTouchForRotate(_Fingers[1]); break;
            }
        }
        
        private void LeanTouchOnOnFingerUp(LeanFinger _Finger)
        {
            m_IsOnFingerUp = true;
            m_PrevMoveDirection = null;
            m_TouchPositionsQueue.Clear();
            m_TouchForMoveTimer = 0;
            m_PrevRotateDirection = null;
            m_TouchPositionsQueue2.Clear();
            m_EnableRotation = true;
        }
        
        private void ProceedTouchForMove(LeanFinger _Finger)
        {
            if (m_TouchForMoveTimer > ViewSettings.MoveSwipeThreshold && m_TouchForMoveTimer != 0f)
            {
                m_TouchForMoveTimer = 0;
                m_TouchPositionsQueue.Clear();
                return;
            }
            var pos = _Finger.ScreenPosition;
            for (int i = Mathf.Min(m_TouchPositionsQueue.Count - 1, 10); i >= 0; i--)
            {
                var prevPos = m_TouchPositionsQueue[i];
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
            m_TouchForMoveTimer += GameTicker.DeltaTime;
        }

        private void ProceedTouchForRotate(LeanFinger _Finger)
        {
            if (!m_EnableRotation)
                return;
            var pos = _Finger.ScreenPosition;
            for (int i = m_TouchPositionsQueue2.Count - 1; i >= 0; i--)
            {
                var prevPos = m_TouchPositionsQueue2[i];
                var rotDir = GetRotateDirection(pos - prevPos);
                if (!rotDir.HasValue || rotDir == m_PrevRotateDirection) 
                    continue;
                RotateCommand(rotDir.Value);
                m_PrevRotateDirection = rotDir;
                m_EnableRotation = false;
                break;
            }
            m_TouchPositionsQueue2.Add(pos);
        }

        public bool AreFingersOnScreen(int _Count)
        {
#if UNITY_EDITOR
            if (_Count > 1)
                return false;
            var view = CameraProvider.MainCamera.ScreenToViewportPoint(LeanInput.GetMousePosition());
            var isOutside = view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;
            return !isOutside && LeanInput.GetMousePressed(0);
#else
            return LeanInput.GetTouchCount() == _Count;
#endif
        }
        
        public Vector2 GetFingerPosition(int _Index)
        {
#if UNITY_EDITOR
            return LeanInput.GetMousePosition();
#else
            CommonUtils.GetTouch(_Index, out _, out var pos, out _, out _, out _);
            return pos;
#endif
        }
        

        private static EMazeMoveDirection? GetMoveDirection(Vector2 _Delta)
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

        private static EMazeRotateDirection? GetRotateDirection(Vector2 _Delta)
        {
            const float angThreshold = 30f * Mathf.Deg2Rad;
            const float distThreshold = 0.15f;
            EMazeRotateDirection? res = null;
            float absDx = Mathf.Abs(_Delta.x);
            float absDy = Mathf.Abs(_Delta.y);
            float absNormDx = Mathf.Abs( _Delta.x / GraphicUtils.ScreenSize.x);
            if (absDx > absDy && absDy / absDx < Mathf.Tan(angThreshold) && absNormDx > distThreshold)
                res = _Delta.x > 0 ? EMazeRotateDirection.CounterClockwise : EMazeRotateDirection.Clockwise;
            return res;
        }

        private static EInputCommand GetMoveCommand(EMazeMoveDirection _MoveDirection)
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