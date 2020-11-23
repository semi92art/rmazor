using System;
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
        IScoreController MainScoreController { get; }
        IGameMenuUi GameMenuUi { get; }
        void Init(int _Level);
    }

    public abstract class GameManagerBase : MonoBehaviour, IGameManager, ISingleton
    {
        #region nonpublic members
    
        private LeanTouch m_MainTouchSystem;
    
        #endregion
    
        #region api
    
        public ILevelController LevelController { get; protected set; }
        public ILifesController LifesController { get; protected set; }
        public IScoreController MainScoreController { get; protected set; }
        public IGameMenuUi GameMenuUi { get; protected set; }

        public virtual void Init(int _Level)
        {
            if (LevelController == null)
                LevelController = new LevelControllerBasedOnTime();
            LevelController.Level = _Level;
            LevelController.OnLevelBeforeStarted += OnBeforeLevelStarted;
            LevelController.OnLevelStarted += OnLevelStarted;
            LevelController.OnLevelFinished += OnLevelFinished;
            
            if (LifesController == null)
                LifesController = new DefaultLifesController();
            LifesController.OnLifesChanged += OnLifesChanged;
            LifesController.Init(5);
            
            if (MainScoreController == null)
                MainScoreController = new ScoreController();
            MainScoreController.OnScoreChanged += OnScoreChanged;
            MainScoreController.OnNecessaryScoreChanged += OnNecessaryScoreChanged;
            MainScoreController.OnNecessaryScoreReached += OnNecessaryScoreReached;
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
            GameMenuUi.StatsPanel.SetLifes(_Args.Lifes);
        }

        protected virtual void OnBeforeLevelStarted(LevelStateChangedArgs _Args)
        {
            GameMenuUi.OnBeforeLevelStarted(
                _Args, 
                _Lifes =>
                {
                    LifesController.SetLifesWithoutNotification(_Lifes);
                    GameMenuUi.StatsPanel.SetLifes(_Lifes, false);
                }, 
                () => LevelController.StartLevel());
        }

        protected virtual void OnLevelStarted(LevelStateChangedArgs _Args)
        {
            GameMenuUi.OnLevelStarted(_Args);
        }

        protected virtual void OnLevelFinished(LevelStateChangedArgs _Args)
        {
            GameMenuUi.OnLevelFinished(_Args);
        }

        protected virtual void OnScoreChanged(int _Score)
        {
            GameMenuUi.StatsPanel.SetScore(_Score);
        }
        
        protected virtual void OnNecessaryScoreChanged(int _Score)
        {
            GameMenuUi.StatsPanel.SetNecessaryScore(_Score);
        }

        protected virtual void OnNecessaryScoreReached()
        {
            LevelController.FinishLevel();
        }
    
        #endregion
    
        #region engine methods
    
        protected virtual void Start()
        {
            InitTouchSystem();
        }

        protected virtual void OnDestroy()
        {
            LevelController.OnLevelBeforeStarted -= OnBeforeLevelStarted;
            LevelController.OnLevelStarted -= OnLevelStarted;
            LevelController.OnLevelFinished -= OnLevelFinished;
            LifesController.OnLifesChanged -= OnLifesChanged;
            MainScoreController.OnScoreChanged -= OnScoreChanged;
            MainScoreController.OnNecessaryScoreChanged -= OnNecessaryScoreChanged;
            MainScoreController.OnNecessaryScoreReached -= OnNecessaryScoreReached;
        }

        #endregion
    }
}