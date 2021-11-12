using Constants;
using DI.Extensions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Lean.Touch;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze.Views.InputConfigurators
{
    public interface IViewInputTouchProceeder : IInit, IOnLevelStageChanged { }
    
    public class ViewInputTouchProceeder : IViewInputTouchProceeder
    {
        #region nonpublic members

        private LeanFingerTap   m_LeanFingerTap;
        
        #endregion

        #region inject

        protected IModelGame                  Model             { get; }
        protected IContainersGetter           ContainersGetter  { get; }
        protected IViewInputCommandsProceeder CommandsProceeder { get; }

        protected ViewInputTouchProceeder(
            IModelGame _Model,
            IContainersGetter _ContainersGetter,
            IViewInputCommandsProceeder _CommandsProceeder)
        {
            Model = _Model;
            ContainersGetter = _ContainersGetter;
            CommandsProceeder = _CommandsProceeder;
        }

        #endregion

        #region api
        
        public event UnityAction Initialized;

        public virtual void Init()
        {
            InitLeanTouch();
            InitLeanTouchForMove();
            InitLeanTouchForTapToNext();
            Initialized?.Invoke();
        }
        
        #endregion
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Paused)
                m_LeanFingerTap.OnFinger.RemoveListener(MoveNext);
            else if (_Args.PreviousStage == ELevelStage.Paused)
                m_LeanFingerTap.OnFinger.AddListener(MoveNext);
        }

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

        private void InitLeanTouchForMove()
        {
            var goLeanFingerFlick = new GameObject("Lean Finger Flick");
            goLeanFingerFlick.SetParent(GetContainer());
            var lfs = goLeanFingerFlick.AddComponent<LeanFingerSwipe>();
            lfs.OnDelta.AddListener(OnSwipeForMove);
        }

        private void InitLeanTouchForTapToNext()
        {
            var goLeanFingerTap = new GameObject("Lean Finger Tap");
            goLeanFingerTap.SetParent(GetContainer());
            var lft = goLeanFingerTap.AddComponent<LeanFingerTap>();
            lft.OnFinger.AddListener(MoveNext);
            m_LeanFingerTap = lft;
        }

        private void OnSwipeForMove(Vector2 _Delta)
        {
            const float angThreshold = 30f * Mathf.Deg2Rad;
            const float distThreshold = 0.01f;
            EInputCommand? key = null;
            float absDx = Mathf.Abs(_Delta.x);
            float absDy = Mathf.Abs(_Delta.y);
            float absNormDx = Mathf.Abs( _Delta.x / GraphicUtils.ScreenSize.x);
            float absNormDy = Mathf.Abs(_Delta.y / GraphicUtils.ScreenSize.y);
            if (absDx > absDy && absDy / absDx < Mathf.Tan(angThreshold) && absNormDx > distThreshold)
                key = _Delta.x < 0 ? EInputCommand.MoveLeft : EInputCommand.MoveRight;
            else if (absDx < absDy && absDx / absDy < Mathf.Tan(angThreshold) && absNormDy > distThreshold)
                key = _Delta.y < 0 ? EInputCommand.MoveDown : EInputCommand.MoveUp;
            if (key.HasValue)
                CommandsProceeder.RaiseCommand(key.Value, null);
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