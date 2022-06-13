using System;
using Common;
using Common.Constants;
using Common.Enums;
using Common.Extensions;
using Common.Ticker;
using Common.UI;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.UI.Panels;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RMAZOR.Views.UI
{
    public class ViewUI : ViewUIBase, IApplicationFocus
    {
        #region inject

        private ViewSettings                ViewSettings         { get; }
        private IModelGame                  Model                { get; }
        private IUITicker                   Ticker               { get; }
        private IBigDialogViewer            BigDialogViewer      { get; }
        private IProposalDialogViewer       ProposalDialogViewer { get; }
        private IDialogPanelsSet            DialogPanelsSet      { get; }
        private IViewInputCommandsProceeder CommandsProceeder    { get; }
        private IManagersGetter             Managers             { get; }

        private ViewUI(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            IUITicker                   _UITicker,
            IBigDialogViewer            _BigDialogViewer,
            IProposalDialogViewer       _ProposalDialogViewer,
            IDialogPanelsSet            _DialogPanelsSet,
            IViewUIGameControls         _GameControls,
            IViewInputCommandsProceeder _CommandsProceeder,
            IManagersGetter             _Managers)
            : base(_GameControls)
        {
            ViewSettings         = _ViewSettings;
            Model                = _Model;
            Ticker               = _UITicker;
            BigDialogViewer      = _BigDialogViewer;
            ProposalDialogViewer = _ProposalDialogViewer;
            DialogPanelsSet      = _DialogPanelsSet;
            CommandsProceeder    = _CommandsProceeder;
            Managers             = _Managers;
        }

        #endregion
        
        #region api

        public override void Init()
        {
            Ticker.Register(this);
            CommandsProceeder.Command += OnCommand;
            CreateCanvas();
            var parent = Canvas.RTransform();
            BigDialogViewer.Init(parent);
            ProposalDialogViewer.Init(parent);
            BigDialogViewer.IsOtherDialogViewersShowing = () =>
            {
                var panel = ProposalDialogViewer.CurrentPanel;
                return panel != null && 
                       panel.AppearingState != EAppearingState.Dissapeared;
            };
            ProposalDialogViewer.IsOtherDialogViewersShowing = () =>
            {
                var panel = BigDialogViewer.CurrentPanel;
                return panel != null && 
                       panel.AppearingState != EAppearingState.Dissapeared;
            };
            GameControls.Init();
            DialogPanelsSet.Init();
            base.Init();
        }
        
        private void OnCommand(EInputCommand _Key, object[] _Args)
        {
            switch (_Key)
            {
                case EInputCommand.SettingsMenu:
                    Managers.AnalyticsManager.SendAnalytic(AnalyticIds.SettingsButtonPressed);
                    DialogPanelsSet.SettingDialogPanel.LoadPanel();
                    BigDialogViewer.Show(DialogPanelsSet.SettingDialogPanel);
                    CommandsProceeder.RaiseCommand(EInputCommand.PauseLevel, null, true);
                    break;
                case EInputCommand.ShopMenu:
                    Managers.AnalyticsManager.SendAnalytic(AnalyticIds.ShopButtonPressed);
                    DialogPanelsSet.ShopMoneyDialogPanel.LoadPanel();
                    BigDialogViewer.Show(DialogPanelsSet.ShopMoneyDialogPanel);
                    CommandsProceeder.RaiseCommand(EInputCommand.PauseLevel, null, true);
                    break;
                case EInputCommand.RateGamePanel:
                    ShowRateGamePanel(false);
                    // var lastTimeShown = SaveUtils.GetValue(SaveKeysCommon.TimeSinceLastIapReviewDialogShown);
                    // var span = DateTime.Now - lastTimeShown;
                    // ShowRateGamePanel(span.Days > 31);
                    int ratePanelShowsCount = SaveUtils.GetValue(SaveKeysRmazor.RatePanelShowsCount);
                    SaveUtils.PutValue(SaveKeysRmazor.RatePanelShowsCount, ratePanelShowsCount + 1);
                    break;
            }
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage == ELevelStage.Finished && _Args.LevelIndex == ViewSettings.firstLevelToRateGame)
                CommandsProceeder.RaiseCommand(EInputCommand.RateGamePanel, null);
            GameControls.OnLevelStageChanged(_Args);
        }
        
        public void OnApplicationFocus(bool _Focus)
        {
            if (!_Focus)
                return;
            if (MustShowRateGamePanelOnUnPause())
                CommandsProceeder.RaiseCommand(EInputCommand.RateGamePanel, null);
        }
        
        #endregion

        #region nonpublic methods
        
        private bool MustShowRateGamePanelOnUnPause()
        {
            int ratePanelShowsCount = SaveUtils.GetValue(SaveKeysRmazor.RatePanelShowsCount);
            return !Application.isEditor 
                    && !SaveUtils.GetValue(SaveKeysCommon.GameWasRated)
                    && ratePanelShowsCount < 10
                    && Random.value < 0.5f
                    && Model.LevelStaging.LevelIndex > ViewSettings.firstLevelToRateGame;
        }

        private void ShowRateGamePanel(bool _Native)
        {
            if (_Native)
            {
                if (Application.isEditor)
                    Dbg.Log("Native rate app ui was shown.");
                Managers.ShopManager.RateGame(true);
                SaveUtils.PutValue(SaveKeysCommon.TimeSinceLastIapReviewDialogShown, DateTime.Now);
            }
            else
            {
                DialogPanelsSet.RateGameDialogPanel.LoadPanel();
                ProposalDialogViewer.Show(DialogPanelsSet.RateGameDialogPanel, 3f);
            }
        }

        #endregion
    }
}