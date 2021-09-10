using Constants;
using GameHelpers;
using Lean.Touch;
using UnityEngine;

namespace Games.RazorMaze.Views.InputConfigurators
{
    public class RazorMazeInputConfigurator : IInputConfigurator
    {
        private LeanTouch m_LeanTouch;
        private bool m_Locked;
        
        public event IntHandler Command;
        public event NoArgsHandler Initialized;

        public void Init()
        {
            var touchSystemObj = new GameObject("Main Touch System");
            var mts = touchSystemObj.AddComponent<LeanTouch>();
            mts.TapThreshold = 0.5f;
            mts.SwipeThreshold = 50f;
            mts.ReferenceDpi = 200;
            mts.GuiLayers = LayerMask.GetMask(LayerNames.Touchable);
            mts.RecordFingers = true;
            mts.RecordThreshold = 5f;
            mts.RecordLimit = 0f;
            mts.SimulateMultiFingers = true;
            mts.PinchTwistKey = KeyCode.LeftControl;
            mts.MovePivotKey = KeyCode.LeftAlt;
            mts.MultiDragKey = KeyCode.LeftAlt;
#if UNITY_EDITOR
            mts.FingerTexture = PrefabUtilsEx.GetObject<Texture2D>("icons", "finger_texture");
#endif
            m_LeanTouch = mts;
            
            Initialized?.Invoke();
        }

        public bool Locked
        {
            get => m_Locked;
            set
            {
                m_Locked = value;
                m_LeanTouch.enabled = value;
            }
        }
    }
}