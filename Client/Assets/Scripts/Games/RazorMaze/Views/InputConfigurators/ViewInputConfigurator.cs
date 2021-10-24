﻿using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using DI.Extensions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Lean.Touch;
using UnityEngine;
using UnityEngine.Events;

namespace Games.RazorMaze.Views.InputConfigurators
{
    public class ViewInputConfigurator : IViewInputConfigurator
    {
        #region nonpublic members

        private LeanTouch m_LeanTouch;
        private LeanMultiSwipe m_LeanMultiSwipe;
        private LeanFingerTap m_LeanFingerTap;
        private readonly List<int> m_LockedCommands = new List<int>();

        #endregion

        #region inject

        private IModelGame Model { get; }
        private IContainersGetter ContainersGetter { get; }

        protected ViewInputConfigurator(IModelGame _Model, IContainersGetter _ContainersGetter)
        {
            Model = _Model;
            ContainersGetter = _ContainersGetter;
        }

        #endregion
        

        #region api

        public event UnityAction<int, object[]> Command;
        public event UnityAction<int, object[]> InternalCommand;
        public event UnityAction Initialized;
        
        public void LockCommand(int _Key)
        {
            if (!m_LockedCommands.Contains(_Key))
                m_LockedCommands.Add(_Key);
        }

        public void UnlockCommand(int _Key)
        {
            if (m_LockedCommands.Contains(_Key))
                m_LockedCommands.Remove(_Key);
        }

        public void LockCommands(IEnumerable<int> _Keys)
        {
            foreach (var key in _Keys)
                LockCommand(key);
        }

        public void UnlockCommands(IEnumerable<int> _Keys)
        {
            foreach (var key in _Keys)
                UnlockCommand(key);
        }

        public void LockAllCommands()
        {
            LockCommands(Enumerable.Range(1, 50));
        }

        public void UnlockAllCommands()
        {
            UnlockCommands(Enumerable.Range(1, 50));
        }
        
        public virtual void Init()
        {
            InitLeanTouch();
            InitLeanTouchForMoveAndRotate();
            InitLeanTouchForTapToNext();
            Initialized?.Invoke();
        }
        
        public void RaiseCommand(int _Key, object[] _Args, bool _Forced = false)
        {
            InternalCommand?.Invoke(_Key, _Args);
            if (!m_LockedCommands.Contains(_Key) || _Forced)
                Command?.Invoke(_Key, _Args);
        }

        #endregion

        #region nonpublic methods
        
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

        private void InitLeanTouchForMoveAndRotate()
        {
            var goLeanFingerSwipe = new GameObject("Lean Finger Touch");
            goLeanFingerSwipe.SetParent(GetContainer());
            var lms = goLeanFingerSwipe.AddComponent<LeanMultiSwipe>();
            lms.OnFingers.AddListener(OnSwipeForMove);
            lms.OnFingers.AddListener(OnSwipeForRotate);
            m_LeanMultiSwipe = lms;
        }

        private void InitLeanTouchForTapToNext()
        {
            var goLeanFingerTap = new GameObject("Lean Finger Tap");
            goLeanFingerTap.SetParent(GetContainer());
            var lft = goLeanFingerTap.AddComponent<LeanFingerTap>();
            lft.OnFinger.AddListener(_Finger =>
            {
                if (Model.LevelStaging.LevelStage == ELevelStage.Finished)
                    RaiseCommand(InputCommands.ReadyToUnloadLevel, null, true);
            });
        }

        private void OnSwipeForMove(List<LeanFinger> _Fingers)
        {
            if (_Fingers.Count > 1)
                return;
            var distance = _Fingers[0].ScreenDelta;
            int key;
            if (Mathf.Abs(distance.x) > Mathf.Abs(distance.y))
                key = distance.x < 0 ? InputCommands.MoveLeft : InputCommands.MoveRight;
            else
                key = distance.y < 0 ? InputCommands.MoveDown : InputCommands.MoveUp;
            RaiseCommand(key, null);
        }

        private void OnSwipeForRotate(List<LeanFinger> _Fingers)
        {
            if (_Fingers.Count < 2)
                return;
            float delta1 = _Fingers[_Fingers.Count - 2].ScaledDelta.x;
            float delta2 = _Fingers[_Fingers.Count - 1].ScaledDelta.x;
            if (Math.Abs(Mathf.Sign(delta1) - Mathf.Sign(delta2)) > float.Epsilon)
                return;
            float tolerance = 0.2f;
            if (delta1 < -tolerance && delta2 < -tolerance)
                RaiseCommand(InputCommands.RotateClockwise, null);
            else if (delta1 > tolerance && delta2 > tolerance)
                RaiseCommand(InputCommands.RotateCounterClockwise, null);
                
        }

        private Transform GetContainer()
        {
            return ContainersGetter.GetContainer(ContainerNames.TouchInput);
        }

        #endregion
    }
}