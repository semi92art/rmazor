using System.Collections.Generic;
using DialogViewers;
using Entities;
using Extensions;
using GameHelpers;
using Managers;
using UI.Entities;
using UI.Factories;
using UI.Panels;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace UI
{
    public interface IGameMenuUi
    {
        IStatsMiniPanel StatsMiniPanel { get; }
        IRevenueMiniPanel RevenueMiniPanel { get; }
        void OnBeforeLevelStarted(LevelStateChangedArgs _Args,
            UnityAction<long> _GetLifes, UnityAction _StartLevel);
        void OnLevelStarted(LevelStateChangedArgs _Args);
        void OnLevelFinished(
            LevelStateChangedArgs _Args,
            Dictionary<BankItemType, long> _Revenue,
            UnityAction<Dictionary<BankItemType, long>> _SetNewRevenue,
            UnityAction _Finish,
            bool _IsPersonalBest);
        void OnTimeEnded(UnityAction<float> _SetAdditionalTime, UnityAction _Continue);
        void OnLifesEnded(UnityAction<long> _SetAdditionalLifes, UnityAction _Continue);
        void OnRevenueIncome(BankItemType _BankItemType, long _Revenue);
    }
    
    public abstract class GameMenuUiBase : GameObservable, IGameMenuUi
    {
        #region api properties
        public IStatsMiniPanel StatsMiniPanel { get; protected set; }
        public IRevenueMiniPanel RevenueMiniPanel { get; protected set; }
        
        #endregion
        
        #region nonpublic members
        
        protected IGameDialogViewer DialogViewer;
        protected Canvas Canvas;
        
        #endregion

        #region constructor

        protected GameMenuUiBase()
        {
            CreateCanvas();
            CreateDialogViewer();
            CreateStatsMiniPanel();
            CreateRevenueMiniPanel();
            CreateGameMenuButton();
        }

        #endregion
        
        #region nonpublic methods
        
        protected virtual void CreateCanvas()
        {
            Canvas = UiFactory.UiCanvas(
                "MenuCanvas",
                RenderMode.ScreenSpaceOverlay,
                false,
                0,
                AdditionalCanvasShaderChannels.None,
                CanvasScaler.ScaleMode.ScaleWithScreenSize,
                new Vector2Int(1920, 1080),
                CanvasScaler.ScreenMatchMode.Shrink,
                0f,
                100,
                true,
                GraphicRaycaster.BlockingObjects.None);
        }

        protected virtual void CreateStatsMiniPanel()
        {
            StatsMiniPanel = Panels.StatsMiniPanel.Create(
                Canvas.RTransform(), StatsPanelPosition.Top);
        }
        
        protected virtual void CreateDialogViewer()
        {
            DialogViewer = GameDialogViewer.Create(Canvas.RTransform());
        }

        protected virtual void CreateRevenueMiniPanel()
        {
            RevenueMiniPanel = Panels.RevenueMiniPanel.Create(
                Canvas.RTransform(), RevenuePanelPosition.TopRight);
            RevenueMiniPanel.Hide();
        }
        
        protected virtual void CreateGameMenuButton()
        {
            var settingsButtonSmall = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    Canvas.RTransform(),
                    UiAnchor.Create(1, 0, 1, 0),
                    new Vector2(-43, 35),
                    Vector2.one * 0.5f,
                    new Vector2(63, 54)),
                "game_menu",
                "game_menu_button");
            
            settingsButtonSmall.GetComponent<Button>().SetOnClick(OnGameMenuButtonClick);
        }
        
        protected virtual void OnGameMenuButtonClick()
        {
            if (GameMenuPanel.PanelState.HasFlag(PanelState.Showing))
            {
                GameMenuPanel.PanelState |= PanelState.NeedToClose;
                return;
            }
            GameTimeProvider.Instance.Pause = true;
            var gameMenuPanel = new GameMenuPanel(DialogViewer,
                () => GameTimeProvider.Instance.Pause = false);
            gameMenuPanel.AddObservers(GetObservers());
            gameMenuPanel.Init();
            DialogViewer.Show(gameMenuPanel);
        }

        [DI.Update]
        protected virtual void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                OnGameMenuButtonClick();
        }
        
        #endregion
        
        #region api methods

        public virtual void OnBeforeLevelStarted(
            LevelStateChangedArgs _Args,
            UnityAction<long> _GetLifes,
            UnityAction _StartLevel)
        {
            var startLevelPanel = new LevelStartPanel(
                DialogViewer, _Args.Level, _GetLifes, _StartLevel);
            startLevelPanel.AddObservers(GetObservers());
            startLevelPanel.Init();
            DialogViewer.Show(startLevelPanel);
        }

        public virtual void OnLevelStarted(LevelStateChangedArgs _Args)
        {
            RevenueMiniPanel.ClearRevenue();
        }

        public virtual void OnLevelFinished(
            LevelStateChangedArgs _Args, 
            Dictionary<BankItemType, long> _Revenue,
            UnityAction<Dictionary<BankItemType, long>> _SetNewRevenue,
            UnityAction _Finish,
            bool _IsPersonalBest)
        {
            var levelFinishPanel = new LevelFinishPanel(
                DialogViewer,
                _Args.Level,
                _Revenue,
                _SetNewRevenue,
                _Finish,
                _IsPersonalBest);
            levelFinishPanel.AddObservers(GetObservers());
            levelFinishPanel.Init();
            DialogViewer.Show(levelFinishPanel);
        }

        public virtual void OnTimeEnded(UnityAction<float> _SetAdditionalTime, UnityAction _Continue)
        {
            var panel = new TimeOrLifesEndedPanel(
                DialogViewer,
                true,
                _Continue,
                _SetAdditionalTime);
            panel.AddObservers(GetObservers());
            panel.Init();
            DialogViewer.Show(panel);
        }

        public virtual void OnLifesEnded(UnityAction<long> _SetAdditionalLifes, UnityAction _Continue)
        {
            var panel = new TimeOrLifesEndedPanel(
                DialogViewer,
                false,
                _Continue,
                _SetAdditionalLife: _SetAdditionalLifes);
            panel.AddObservers(GetObservers());
            panel.Init();
            DialogViewer.Show(panel);
        }

        public virtual void OnRevenueIncome(BankItemType _BankItemType, long _Revenue)
        {
            RevenueMiniPanel.PlusRevenue(_BankItemType, _Revenue);
        }

        #endregion
    }
}