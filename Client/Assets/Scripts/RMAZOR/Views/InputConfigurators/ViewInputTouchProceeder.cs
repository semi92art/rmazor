using System.Collections.Generic;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Ticker;
using Common.Utils;
using Lean.Common;
using Lean.Touch;
using RMAZOR.Models;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.InputConfigurators
{
    public interface IViewInputTouchProceeder : IInit, IOnLevelStageChanged
    {
        event UnityAction<LeanFinger> Tap;
        bool                          ProceedRotation { set; }
        bool                          AreFingersOnScreen(int                   _Count);
        Vector2                       GetFingerPosition(int                    _Index);
        void                          OnRotationFinished(MazeRotationEventArgs _Args);
    }
    
    public class ViewInputTouchProceeder : InitBase, IViewInputTouchProceeder
    {
        #region constants

        private const float SwipeThresholdSeconds     = 0.1f;
        private const float SwipeThresholdCentimeters = 0.05f;

        #endregion
        
        #region nonpublic members

        private          LeanFingerTap         m_LeanFingerTap;
        private readonly List<Vector2>         m_TouchPositionsQueue  = new List<Vector2>();
        private readonly List<Vector2>         m_TouchPositionsQueue2 = new List<Vector2>();
        private          EMazeMoveDirection?   m_PrevMoveDirection;
        private          float                 m_TouchForMoveTimer;
        private          EMazeRotateDirection? m_PrevRotateDirection;
        private          bool                  m_EnableRotation = true;
        private          bool                  m_FingerOnScreen;
        
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
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            IContainersGetter           _ContainersGetter,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewGameTicker             _GameTicker,
            ICameraProvider             _CameraProvider,
            IPrefabSetManager           _PrefabSetManager)
        {
            ViewSettings       = _ViewSettings;
            Model              = _Model;
            ContainersGetter   = _ContainersGetter;
            CommandsProceeder  = _CommandsProceeder;
            GameTicker         = _GameTicker;
            CameraProvider     = _CameraProvider;
            PrefabSetManager   = _PrefabSetManager;
        }

        #endregion

        #region api

        public event UnityAction<LeanFinger> Tap;
        public bool                          ProceedRotation { get; set; }
        
        public override void Init()
        {
            InitLeanTouch();
            InitLeanTouchForMoveAndRotate();
            InitLeanTouchForTapToNext();
            base.Init();
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage != ELevelStage.ReadyToStart || _Args.PreviousStage != ELevelStage.Loaded) 
                return;
            if (RmazorUtils.MazeContainsGravityItems(Model.GetAllProceedInfos()))
            {
                CommandsProceeder.UnlockCommands(
                    RmazorUtils.RotateCommands, 
                    nameof(IViewInputTouchProceeder));
            }
            else
            {
                CommandsProceeder.LockCommands(
                    RmazorUtils.RotateCommands, 
                    nameof(IViewInputTouchProceeder));
            }
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
            if (Application.isEditor)
            {
                lt.FingerTexture = PrefabSetManager.GetObject<Texture2D>(
                    "icons", "finger_texture");
            }
            var goLeanMultiUpdate = new GameObject("Lean Multi Update");
            goLeanMultiUpdate.SetParent(GetContainer());
            var lmu = goLeanMultiUpdate.AddComponent<LeanMultiUpdate>();
            lmu.OnFingers.AddListener(OnLeanMultiUpdateFingers);
            
            LeanTouch.OnFingerUp   -= OnFingerUp;
            LeanTouch.OnFingerUp   += OnFingerUp;
            LeanTouch.OnFingerDown -= LeanTouchOnOnFingerDown;
            LeanTouch.OnFingerDown += LeanTouchOnOnFingerDown;
            LeanTouch.OnFingerUp   -= LeanTouchOnOnFingerUp;
            LeanTouch.OnFingerUp   += LeanTouchOnOnFingerUp;
        }
        
        private void LeanTouchOnOnFingerDown(LeanFinger _Obj)
        {
            m_FingerOnScreen = true;
        }
        
        private void LeanTouchOnOnFingerUp(LeanFinger _Obj)
        {
            m_FingerOnScreen = false;
        }
        
        private void InitLeanTouchForMoveAndRotate()
        {
            var goLeanMultiUpdate = new GameObject("Lean Multi Update");
            goLeanMultiUpdate.SetParent(GetContainer());
            var lmu = goLeanMultiUpdate.AddComponent<LeanMultiUpdate>();
            lmu.Coordinate = LeanMultiUpdate.CoordinateType.ScreenPixels;
            lmu.OnFingers.AddListener(OnLeanMultiUpdateFingers);
            LeanTouch.OnFingerUp -= OnFingerUp;
            LeanTouch.OnFingerUp += OnFingerUp;
        }

        private void InitLeanTouchForTapToNext()
        {
            var goLeanFingerTap = new GameObject("Lean Finger Tap");
            goLeanFingerTap.SetParent(GetContainer());
            var lft = goLeanFingerTap.AddComponent<LeanFingerTap>();
            lft.OnFinger.AddListener(OnTapCore);
            m_LeanFingerTap = lft;
        }
        
        private void OnLeanMultiUpdateFingers(List<LeanFinger> _Fingers)
        {
            for (int i = 0; i < _Fingers.Count; i++)
            {
                if (_Fingers[i].Up)
                    return;
            }
            switch (_Fingers.Count)
            {
                case 1: ProceedTouchForMove(_Fingers[0]); break;
                case 2: ProceedTouchForRotate(_Fingers[0]); break;
            }
        }
        
        private void OnFingerUp(LeanFinger _Finger)
        {
            m_PrevMoveDirection = null;
            m_TouchPositionsQueue.Clear();
            m_TouchForMoveTimer = 0;
            m_PrevRotateDirection = null;
            m_TouchPositionsQueue2.Clear();
            m_EnableRotation = true;
        }
        
        private void ProceedTouchForMove(LeanFinger _Finger)
        {
            if (_Finger.LastScreenPosition.y / GraphicUtils.ScreenSize.y > 0.9f) 
                return;
            if (m_TouchForMoveTimer > SwipeThresholdSeconds && Mathf.Abs(m_TouchForMoveTimer) > MathUtils.Epsilon)
            {
                m_TouchForMoveTimer = 0;
                m_TouchPositionsQueue.Clear();
                return;
            }
            var pos = _Finger.ScreenPosition;
            for (int i = Mathf.Min(m_TouchPositionsQueue.Count - 1, 100); i >= 0; i--)
            {
                var prevPos = m_TouchPositionsQueue[i];
                var delta = pos - prevPos;
                var moveDir = GetMoveDirection(delta);
                if (!moveDir.HasValue)
                    continue;
                if (m_PrevMoveDirection == moveDir)
                    continue;
                Move(moveDir.Value);
                m_PrevMoveDirection = moveDir.Value;
                break;
            }
            m_TouchPositionsQueue.Add(pos);
            m_TouchForMoveTimer += GameTicker.DeltaTime;
        }

        private void ProceedTouchForRotate(LeanFinger _Finger)
        {
            if (!ProceedRotation)
                return;
            if (!m_EnableRotation)
                return;
            if (_Finger.LastScreenPosition.y / GraphicUtils.ScreenSize.y > 0.9f) 
                return;
            var pos = _Finger.ScreenPosition;
            for (int i = m_TouchPositionsQueue2.Count - 1; i >= 0; i--)
            {
                var prevPos = m_TouchPositionsQueue2[i];
                var rotDir = GetRotateDirection(pos - prevPos);
                if (!rotDir.HasValue || rotDir == m_PrevRotateDirection) 
                    continue;
                Rotate(rotDir.Value);
                m_PrevRotateDirection = rotDir;
                m_EnableRotation = false;
                break;
            }
            m_TouchPositionsQueue2.Add(pos);
        }

        public bool AreFingersOnScreen(int _Count)
        {
            if (_Count == 1)
                return m_FingerOnScreen;
            if (!Application.isEditor) 
                return LeanInput.GetTouchCount() == _Count;
            if (_Count > 1)
                return false;
            var view = CameraProvider.Camera.ScreenToViewportPoint(LeanInput.GetMousePosition());
            bool isOutside = view.x < 0 || view.x > 1 || view.y < 0 || view.y > 1;
            return !isOutside && LeanInput.GetMousePressed(0);
        }
        
        public Vector2 GetFingerPosition(int _Index)
        {
            if (Application.isEditor)
                return LeanInput.GetMousePosition();
            CommonUtils.GetTouch(_Index, out _, out var pos, out _, out _, out _);
            return pos;
        }

        public void OnRotationFinished(MazeRotationEventArgs _Args)
        {
            Cor.Run(Cor.Delay(
                ViewSettings.afterRotationEnableMoveDelay,
                GameTicker,
                UnlockCommandsOnRotationFinished));
        }
        
        private static EMazeMoveDirection? GetMoveDirection(Vector2 _Delta)
        {
            _Delta *= LeanTouch.ScalingFactor;
            const float angThresholdRadians = 30f * Mathf.Deg2Rad;
            EMazeMoveDirection? res = null;
            float absDx = Mathf.Abs(_Delta.x);
            float absDy = Mathf.Abs(_Delta.y);
            float absReadDx = absDx / Screen.dpi * 2.54f;
            float absReadDy = absDy / Screen.dpi * 2.54f;
            if (absDx > absDy
                && absDy / absDx < Mathf.Tan(angThresholdRadians)
                && absReadDx > SwipeThresholdCentimeters)
            {
                res = _Delta.x < 0 ? EMazeMoveDirection.Left : EMazeMoveDirection.Right;
            }
            else if (absDx < absDy
                     && absDx / absDy < Mathf.Tan(angThresholdRadians)
                     && absReadDy > SwipeThresholdCentimeters)
            {
                res = _Delta.y < 0 ? EMazeMoveDirection.Down : EMazeMoveDirection.Up;
            }
            return res;
        }

        private static EMazeRotateDirection? GetRotateDirection(Vector2 _Delta)
        {
            _Delta *= LeanTouch.ScalingFactor;
            const float angThreshold = 30f * Mathf.Deg2Rad;
            EMazeRotateDirection? res = null;
            float absDx = Mathf.Abs(_Delta.x);
            float absDy = Mathf.Abs(_Delta.y);
            float absReadDx = absDx / Screen.dpi * 2.54f;
            if (absDx > absDy
                && absDy / absDx < Mathf.Tan(angThreshold)
                && absReadDx > SwipeThresholdCentimeters)
            {
                res = _Delta.x > 0 ? EMazeRotateDirection.CounterClockwise : EMazeRotateDirection.Clockwise;
            }
            return res;
        }

        private Transform GetContainer()
        {
            return ContainersGetter.GetContainer(ContainerNames.TouchInput);
        }

        private void Move(EMazeMoveDirection _Direction)
        {
            var command = _Direction switch
            {
                EMazeMoveDirection.Left  => EInputCommand.MoveLeft,
                EMazeMoveDirection.Right => EInputCommand.MoveRight,
                EMazeMoveDirection.Down  => EInputCommand.MoveDown,
                EMazeMoveDirection.Up    => EInputCommand.MoveUp,
                _                        => throw new SwitchCaseNotImplementedException(_Direction)
            };
            CommandsProceeder.RaiseCommand(command, null);
        }

        private void Rotate(EMazeRotateDirection _Direction)
        {
            var command = _Direction switch
            {
                EMazeRotateDirection.Clockwise        => EInputCommand.RotateClockwise,
                EMazeRotateDirection.CounterClockwise => EInputCommand.RotateCounterClockwise,
                _                                     => throw new SwitchCaseNotImplementedException(_Direction)
            };
            if (CommandsProceeder.IsCommandLocked(command))
                return;
            CommandsProceeder.RaiseCommand(command, null);
            LockCommandsOnRotationStarted();
        }

        protected virtual void OnTapCore(LeanFinger _Finger)
        {
            Tap?.Invoke(_Finger);
        }
        
        private void LockCommandsOnRotationStarted()
        {
            CommandsProceeder.LockCommands(RmazorUtils.MoveAndRotateCommands, GetGroupName());
        }

        private void UnlockCommandsOnRotationFinished()
        {
            CommandsProceeder.UnlockCommands(RmazorUtils.MoveAndRotateCommands, GetGroupName());
        }

        private static string GetGroupName()
        {
            return nameof(IViewInputTouchProceeder);
        }
        
        #endregion
    }
}