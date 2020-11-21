using Constants;
using Helpers;
using Lean.Touch;
using UI;
using UnityEngine;

namespace Managers
{
    public interface IGameManager
    {
        ILevelController LevelController { get; }
        ILifesController LifesController { get; }
        IGameMenuUi GameMenuUi { get; }
        void Init(int _Level, long _LifesOnStart);
    }

    public abstract class GameManagerBase : MonoBehaviour, IGameManager, ISingleton
    {
        #region nonpublic members
    
        private LeanTouch m_MainTouchSystem;
    
        #endregion
    
        #region api
    
        public ILevelController LevelController { get; protected set; }
        public ILifesController LifesController { get; protected set; }
        public IGameMenuUi GameMenuUi { get; protected set; }

        public virtual void Init(int _Level, long _LifesOnStart)
        {
            if (LevelController == null)
                LevelController = new LevelControllerBasedOnTime();
            LevelController.Level = _Level;
            LevelController.OnLevelStarted += OnLevelStarted;
            LevelController.OnLevelFinished += OnLevelFinished;
            if (LifesController == null)
                LifesController = new DefaultLifesController();
            LifesController.OnLifesChanged += OnLifesChanged;
            LifesController.Init(_LifesOnStart);
            
        }
        
        #endregion
    
        #region protected methods
    
        protected virtual void InitTouchSystem()
        {
            var touchSystemObj = new GameObject("Main Touch System");
            m_MainTouchSystem = touchSystemObj.AddComponent<LeanTouch>();
            var mts = m_MainTouchSystem;
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
            mts.FingerTexture = PrefabInitializer.GetObject<Texture2D>("icons", "finger_texture");
        }

        protected virtual void OnLifesChanged(LifeEventArgs _Args)
        {
            GameMenuUi.LifesPanel.SetLifes(_Args.Lifes);
        }
        protected abstract void OnLevelStarted(LevelStateChangedArgs _Args);
        protected abstract void OnLevelFinished(LevelStateChangedArgs _Args);
    
        #endregion
    
        #region engine methods
    
        protected virtual void Start()
        {
            InitTouchSystem();
        }
    
        #endregion
    }
}