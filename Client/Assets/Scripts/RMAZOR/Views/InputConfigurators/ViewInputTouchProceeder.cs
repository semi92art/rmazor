using System.Collections.Generic;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Exceptions;
using Common.Extensions;
using Common.Ticker;
using Common.Utils;
using GameHelpers;
using Lean.Common;
using Lean.Touch;
using RMAZOR.Models;
using RMAZOR.Views.ContainerGetters;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.InputConfigurators
{
    public interface IViewInputTouchProceeder : IInit, IOnLevelStageChanged
    {
        UnityAction<Vector2> OnTap { get; set; }
        bool                 AreFingersOnScreen(int _Count);
        Vector2              GetFingerPosition(int _Index);
        void                 OnRotationFinished(MazeRotationEventArgs _Args);
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
        private          bool                  m_FingerOnScreen;
        

        
        #endregion

        #region inject

        protected CommonGameSettings          CommonGameSettings { get; }
        protected ViewSettings                ViewSettings       { get; }
        protected IModelGame                  Model              { get; }
        protected IContainersGetter           ContainersGetter   { get; }
        protected IViewInputCommandsProceeder CommandsProceeder  { get; }
        protected IViewGameTicker             GameTicker         { get; }
        protected ICameraProvider             CameraProvider     { get; }
        protected IPrefabSetManager           PrefabSetManager   { get; }

        protected ViewInputTouchProceeder(
            CommonGameSettings          _CommonGameSettings,
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            IContainersGetter           _ContainersGetter,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewGameTicker             _GameTicker,
            ICameraProvider             _CameraProvider,
            IPrefabSetManager           _PrefabSetManager)
        {
            CommonGameSettings = _CommonGameSettings;
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

        public bool                 Initialized { get; private set; }
        public event UnityAction    Initialize;
        public UnityAction<Vector2> OnTap { get; set; }

        public virtual void Init()
        {
            InitLeanTouch();
            InitLeanTouchForMoveAndRotate();
            InitLeanTouchForTapToNext();
            Initialize?.Invoke();
            Initialized = true;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Paused)
                m_LeanFingerTap.OnFinger.RemoveListener(MoveNext);
            else if (_Args.PreviousStage == ELevelStage.Paused)
                m_LeanFingerTap.OnFinger.AddListener(MoveNext);
            if (_Args.Stage == ELevelStage.ReadyToStart && _Args.PreviousStage == ELevelStage.Loaded)
            {
                if (RazorMazeUtils.MazeContainsGravityItems(Model.GetAllProceedInfos()))
                {
                    CommandsProceeder.UnlockCommands(
                        RazorMazeUtils.GetRotateCommands(), 
                        nameof(IViewInputTouchProceeder));
                }
                else
                {
                    CommandsProceeder.LockCommands(
                        RazorMazeUtils.GetRotateCommands(), 
                        nameof(IViewInputTouchProceeder));
                }
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
#if UNITY_EDITOR
            lt.FingerTexture = PrefabSetManager.GetObject<Texture2D>("icons", "finger_texture");
#endif
            var goLeanMultiUpdate = new GameObject("Lean Multi Update");
            goLeanMultiUpdate.SetParent(GetContainer());
            var lmu = goLeanMultiUpdate.AddComponent<LeanMultiUpdate>();
            lmu.OnFingers.AddListener(OnLeanMultiUpdateFingers);
            
            LeanTouch.OnFingerUp -= OnFingerUp;
            LeanTouch.OnFingerUp += OnFingerUp;
            LeanTouch.OnFingerDown -= LeanTouchOnOnFingerDown;
            LeanTouch.OnFingerDown += LeanTouchOnOnFingerDown;
            LeanTouch.OnFingerUp -= LeanTouchOnOnFingerUp;
            LeanTouch.OnFingerUp += LeanTouchOnOnFingerUp;
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
            if (m_TouchForMoveTimer > ViewSettings.MoveSwipeThreshold && m_TouchForMoveTimer != 0f)
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
            if (!m_EnableRotation)
                return;
            if (!SaveUtils.GetValue(SaveKeys.EnableRotation))
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
            {
                return m_FingerOnScreen;
            }
            
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

        public void OnRotationFinished(MazeRotationEventArgs _Args)
        {
            Cor.Run(Cor.Delay(
                ViewSettings.AfterRotationEnableMoveDelay,
                UnlockCommandsOnRotationFinished));
        }
        
        private static EMazeMoveDirection? GetMoveDirection(Vector2 _Delta)
        {
            const float angThreshold = 30f * Mathf.Deg2Rad;
            const float distThreshold = 0.02f;
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
            OnTap?.Invoke(_Finger.ScreenPosition);
            MoveNext(_Finger);
        }

        private void MoveNext(LeanFinger _Finger)
        {
            if (_Finger.LastScreenPosition.y / GraphicUtils.ScreenSize.y > 0.8f) 
                return;
            if (Model.LevelStaging.LevelStage != ELevelStage.Finished) 
                return;
            CommandsProceeder.RaiseCommand(EInputCommand.ReadyToUnloadLevel, null);
        }
        
        private void LockCommandsOnRotationStarted()
        {
            CommandsProceeder.LockCommands(RazorMazeUtils.GetMoveCommands(), GetGroupName());
            CommandsProceeder.LockCommands(RazorMazeUtils.GetRotateCommands(), GetGroupName());
        }

        private void UnlockCommandsOnRotationFinished()
        {
            CommandsProceeder.UnlockCommands(RazorMazeUtils.GetMoveCommands(), GetGroupName());
            CommandsProceeder.UnlockCommands(RazorMazeUtils.GetRotateCommands(), GetGroupName());
        }

        private static string GetGroupName()
        {
            return nameof(IViewInputTouchProceeder);
        }
        
        #endregion
    }
}