using System;
using System.Linq;
using Constants;
using Extensions;
using GameHelpers;
using Lean.Touch;
using Network;
using Network.Packets;
using UI;
using UnityEngine;
using Utils;

namespace Managers
{
    public interface IGameManager
    {
        void Init(int _Level);
    }

    public abstract class GameManagerBase : MonoBehaviour, IGameManager, ISingleton
    {
        #region singleton

        private static IGameManager _instance;
        protected static IGameManager GetInstance<T>() where T : GameManagerBase
        {
            if (_instance is T ptm && !ptm.IsNull()) 
                    return _instance;
            var go = new GameObject("Game Manager");
            _instance = go.AddComponent<T>();
            return _instance;
        }

        #endregion
        
        #region nonpublic members
        
        protected ILevelController LevelController;
        protected ILifesController LifesController;
        protected IScoreController MainScoreController;
        protected IRevenueController RevenueController;
        protected IGameMenuUi GameMenuUi;
        private LeanTouch m_MainTouchSystem;
    
        #endregion
    
        #region api
        
        public virtual void Init(int _Level)
        {
            GameTimeProvider.Instance.Reset();
            if (LevelController == null)
                LevelController = new DefaultLevelController();
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

            if (RevenueController == null)
                RevenueController = new DefaultRevenueController();
            RevenueController.OnRevenueIncome += OnRevenueIncome;
            
            LevelController.Level = _Level;
            LevelController.BeforeStartLevel();
            if (GameMenuUi != null)
                LevelController.CountdownController.OnTimeChange += GameMenuUi.StatsMiniPanel.SetTime;
            LevelController.CountdownController.OnTimeEnded += OnTimeEnded;
            LifesController.OnLifesEnded += OnLifesEnded;
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
        
        protected virtual void OnBeforeLevelStarted(LevelStateChangedArgs _Args)
        {
            MainScoreController.Score = 0;
            MainScoreController.NecessaryScore = NecessaryScore(_Args.Level);
            GameMenuUi?.OnBeforeLevelStarted(
                _Args, 
                _Lifes =>
                {
                    LevelController.CountdownController.SetDuration(LevelDuration(_Args.Level));
                    LifesController.SetLifesWithoutNotification(_Lifes);
                    GameMenuUi?.StatsMiniPanel.SetLifes(_Lifes, false);
                }, 
                () => LevelController.StartLevel(
                    LevelDuration(_Args.Level), () => LifesController.Lifes <= 0));
        }

        protected virtual void OnLevelStarted(LevelStateChangedArgs _Args)
        {
            GameMenuUi?.OnLevelStarted(_Args);
        }

        protected virtual void OnLevelFinished(LevelStateChangedArgs _Args)
        {
            var scores = ScoreManager.Instance.GetScores();
            Coroutines.Run(Coroutines.WaitWhile(
                () => !scores.Loaded,
                () =>
            {
                int mainScore = scores.Scores[ScoreType.Main];
                if (mainScore < LevelController.Level)
                    ScoreManager.Instance.SetScore(ScoreType.Main, LevelController.Level);

                GameMenuUi?.OnLevelFinished(_Args, RevenueController.TotalRevenue,
                    _NewRevenue =>
                        RevenueController.TotalRevenue = _NewRevenue,
                    () =>
                    {
                        LevelController.Level++;
                        BankManager.Instance.PlusBankItems(RevenueController.TotalRevenue);
                        RevenueController.TotalRevenue.Clear();
                        LevelController.BeforeStartLevel();
                    },
                    LevelController.Level > mainScore);
            }));
        }

        protected virtual void OnScoreChanged(int _Score)
        {
            GameMenuUi?.StatsMiniPanel.SetScore(_Score);
        }
        
        protected virtual void OnNecessaryScoreChanged(int _Score)
        {
            GameMenuUi?.StatsMiniPanel.SetNecessaryScore(_Score);
        }

        protected virtual void OnNecessaryScoreReached()
        {
            LevelController.FinishLevel();
        }
        
        protected virtual void OnLifesChanged(LifeEventArgs _Args)
        {
            GameMenuUi?.StatsMiniPanel.SetLifes(_Args.Lifes);
        }
        
        protected virtual void OnLifesEnded()
        {
            GameMenuUi?.OnLifesEnded(_AdditionalLifes => 
                LifesController.PlusLifes(_AdditionalLifes), () => LevelController.StartLevel(
                null, () => LifesController.Lifes <= 0));
        }

        protected virtual void OnTimeEnded()
        {
            GameMenuUi?.OnTimeEnded(_AdditionalTime => 
                    LevelController.CountdownController.SetDuration(_AdditionalTime),
                () => LevelController.StartLevel(
                    null, () => LifesController.Lifes <= 0));
        }
        
        protected virtual void OnRevenueIncome(BankItemType _BankItemType, long _Revenue)
        {
            GameMenuUi?.OnRevenueIncome(_BankItemType, _Revenue);
        }
        
        protected abstract float LevelDuration(int _Level);
        protected abstract int NecessaryScore(int _Level);
    
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
            if (GameMenuUi != null)
                LevelController.CountdownController.OnTimeChange -= GameMenuUi.StatsMiniPanel.SetTime;
            LevelController.CountdownController.OnTimeEnded -= OnTimeEnded;
            LifesController.OnLifesEnded -= OnLifesEnded;
        }

        #endregion
    }
}