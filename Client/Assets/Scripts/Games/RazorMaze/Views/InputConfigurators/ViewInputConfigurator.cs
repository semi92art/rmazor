using System.Collections.Generic;
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
    public class ViewInputConfigurator : IViewInputConfigurator
    {

        #region nonpublic members

        private LeanTouch m_LeanTouch;
        private LeanFingerSwipe m_LeanFingerSwipe;
        private LeanDragTrail m_LeanDragTrail;
        private bool m_Locked;

        #endregion

        #region inject
        
        protected IContainersGetter ContainersGetter { get; }

        protected ViewInputConfigurator(IContainersGetter _ContainersGetter)
        {
            ContainersGetter = _ContainersGetter;
        }

        #endregion
        

        #region api

        public event UnityAction<int, object[]> Command;
        public event UnityAction Initialized;
        
        public bool Locked
        {
            get => m_Locked;
            set
            {
                m_Locked = value;
                m_LeanTouch.enabled = !value;
            }
        }
        
        public virtual void Init()
        {
            InitLeanTouch();
            InitLeanTouchForMove();
            InitLeanTouchForRotate();
            Initialized?.Invoke();
        }
        
        public void RaiseCommand(int _Key, object[] _Args)
        {
            Command?.Invoke(_Key, _Args);
        }

        #endregion

        #region nonpublic methods

        protected bool IsCommandEventNull()
        {
            return Command == null;
        }
        
        private void InitLeanTouch()
        {
            var goLeanTouch = new GameObject("Lean Touch");
            goLeanTouch.SetParent(GetContainer());
            var lt = goLeanTouch.AddComponent<LeanTouch>();
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
            m_LeanTouch = lt;
        }

        private void InitLeanTouchForMove()
        {
            var goLeanFingerSwipe = new GameObject("Lean Finger Touch");
            goLeanFingerSwipe.SetParent(GetContainer());
            var lfs = goLeanFingerSwipe.AddComponent<LeanFingerSwipe>();
            m_LeanFingerSwipe = lfs;
            lfs.OnDelta.AddListener(OnSwipeForMove);
        }

        private void InitLeanTouchForRotate()
        {
            m_LeanDragTrail = InitPrefab<LeanDragTrail>("lean_drag_trail");
            var lsc = InitPrefab<LeanShape>("lean_shape_circle");
            var lscDetClockwise = InitPrefab<LeanShapeDetector>("lean_shape_detector_clockwise_circle");
            var lscDetCounter = InitPrefab<LeanShapeDetector>("lean_shape_detector_counter_clockwise_circle");
            lscDetClockwise.Shape = lsc;
            lscDetCounter.Shape = lsc;
            lscDetClockwise.OnDetected.AddListener(_Finger => Command?.Invoke(InputCommands.RotateClockwise, null));
            lscDetCounter.OnDetected.AddListener(_Finger => Command?.Invoke(InputCommands.RotateCounterClockwise, null));
        }

        private void OnSwipeForMove(Vector2 _Distance)
        {
            int key;
            if (Mathf.Abs(_Distance.x) > Mathf.Abs(_Distance.y))
                key = _Distance.x < 0 ? InputCommands.MoveLeft : InputCommands.MoveRight;
            else
                key = _Distance.y < 0 ? InputCommands.MoveDown : InputCommands.MoveUp;
            Command?.Invoke(key, null);
        }

        private T InitPrefab<T>(string _Name) where T : Component
        {
            return PrefabUtilsEx
                .InitPrefab(GetContainer(), "ui_game", _Name)
                .GetComponent<T>();
        }
        
        private Transform GetContainer()
        {
            return ContainersGetter.GetContainer(ContainerNames.TouchInput);
        }

        #endregion
    }
}