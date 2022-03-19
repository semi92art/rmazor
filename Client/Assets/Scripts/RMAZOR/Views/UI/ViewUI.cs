using System;
using Common;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Ticker;
using Common.UI;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.UI.Panels;
using RMAZOR.Views.InputConfigurators;
using Random = UnityEngine.Random;

namespace RMAZOR.Views.UI
{
    public class ViewUI : ViewUIBase, IApplicationPause
    {
        #region inject

        private CommonGameSettings          CommonGameSettings   { get; }
        private ViewSettings                ViewSettings         { get; }
        private IModelGame                  Model                { get; }
        private IUITicker                   Ticker               { get; }
        private IBigDialogViewer            BigDialogViewer      { get; }
        private IProposalDialogViewer       ProposalDialogViewer { get; }
        private IDialogPanels               DialogPanels         { get; }
        private IViewInputCommandsProceeder CommandsProceeder    { get; }
        private IManagersGetter             Managers             { get; }

        public ViewUI(
            CommonGameSettings          _CommonGameSettings,
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            IUITicker                   _UITicker,
            IBigDialogViewer            _BigDialogViewer,
            IProposalDialogViewer       _ProposalDialogViewer,
            IDialogPanels               _DialogPanels,
            IViewUIGameControls         _GameControls,
            IViewInputCommandsProceeder _CommandsProceeder,
            IManagersGetter             _Managers)
            : base(_GameControls)
        {
            CommonGameSettings   = _CommonGameSettings;
            ViewSettings         = _ViewSettings;
            Model                = _Model;
            Ticker               = _UITicker;
            BigDialogViewer      = _BigDialogViewer;
            ProposalDialogViewer = _ProposalDialogViewer;
            DialogPanels         = _DialogPanels;
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
            DialogPanels.Init();
            base.Init();
        }
        
        private void OnCommand(EInputCommand _Key, object[] _Args)
        {
            switch (_Key)
            {
                case EInputCommand.SettingsMenu:
                    DialogPanels.SettingDialogPanel.LoadPanel();
                    BigDialogViewer.Show(DialogPanels.SettingDialogPanel);
                    CommandsProceeder.RaiseCommand(EInputCommand.PauseLevel, null, true);
                    break;
                case EInputCommand.ShopMenu:
                    DialogPanels.ShopMoneyDialogPanel.LoadPanel();
                    BigDialogViewer.Show(DialogPanels.ShopMoneyDialogPanel);
                    CommandsProceeder.RaiseCommand(EInputCommand.PauseLevel, null, true);
                    break;
                case EInputCommand.RateGamePanel:
                    var lastTimeShown = SaveUtils.GetValue(SaveKeysCommon.TimeSinceLastIapReviewDialogShown);
                    var span = DateTime.Now - lastTimeShown;
                    if (span.Days > 31)
                    {
                        Managers.ShopManager.RateGame(true);
                    }
                    else
                    {
                        DialogPanels.RateGameDialogPanel.LoadPanel();
                        ProposalDialogViewer.Show(DialogPanels.RateGameDialogPanel, 3f);
                    }
                    int ratePanelShowsCount = SaveUtils.GetValue(SaveKeysRmazor.RatePanelShowsCount);
                    SaveUtils.PutValue(SaveKeysRmazor.RatePanelShowsCount, ratePanelShowsCount + 1);
                    break;
            }
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            GameControls.OnLevelStageChanged(_Args);
        }
        
        public void OnApplicationPause(bool _Pause)
        {
            if (_Pause)
                return;
            int ratePanelShowsCount = SaveUtils.GetValue(SaveKeysRmazor.RatePanelShowsCount);
            bool MustShowRateGamePanel()
            {
                return Random.value < 0.5f
                       && !SaveUtils.GetValue(SaveKeysCommon.GameWasRated)
                       && ratePanelShowsCount > 10
                       && Model.LevelStaging.LevelIndex > ViewSettings.firstLevelToRateGame;
            }
            if (MustShowRateGamePanel())
                CommandsProceeder.RaiseCommand(EInputCommand.RateGamePanel, null);
            else if (Model.LevelStaging.LevelIndex > CommonGameSettings.firstLevelToShowAds)
                Managers.AdsManager.ShowInterstitialAd(null);
        }
        
        #endregion
    }
}