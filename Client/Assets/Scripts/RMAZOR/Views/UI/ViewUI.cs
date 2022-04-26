﻿using System;
using Common;
using Common.Constants;
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
using UnityEngine;
using Random = UnityEngine.Random;

namespace RMAZOR.Views.UI
{
    public class ViewUI : ViewUIBase, IApplicationFocus
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
                    Managers.AnalyticsManager.SendAnalytic(AnalyticIds.SettingsButtonPressed);
                    DialogPanels.SettingDialogPanel.LoadPanel();
                    BigDialogViewer.Show(DialogPanels.SettingDialogPanel);
                    CommandsProceeder.RaiseCommand(EInputCommand.PauseLevel, null, true);
                    break;
                case EInputCommand.ShopMenu:
                    Managers.AnalyticsManager.SendAnalytic(AnalyticIds.ShopButtonPressed);
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
                        SaveUtils.PutValue(SaveKeysCommon.TimeSinceLastIapReviewDialogShown, DateTime.Now);
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
            if (_Args.Stage == ELevelStage.Finished && _Args.LevelIndex == ViewSettings.firstLevelToRateGame)
                CommandsProceeder.RaiseCommand(EInputCommand.RateGamePanel, null);
            GameControls.OnLevelStageChanged(_Args);
        }
        
        public void OnApplicationFocus(bool _Focus)
        {
            if (!_Focus)
                return;
            if (CommonData.PausedByAdvertisingOrPurchasing)
            {
                CommonData.PausedByAdvertisingOrPurchasing = false;
                return;
            }
            if (MustShowRateGamePanel())
                CommandsProceeder.RaiseCommand(EInputCommand.RateGamePanel, null);
            else if (MustShowAdvertising())
            {
                if (CommonGameSettings.showRewardedInsteadOfInterstitialOnUnpause)
                    Managers.AdsManager.ShowRewardedAd(null, null);
                else
                    Managers.AdsManager.ShowInterstitialAd(null, null);
            }
        }
        
        #endregion

        #region nonpublic methods
        
        private bool MustShowRateGamePanel()
        {
            int ratePanelShowsCount = SaveUtils.GetValue(SaveKeysRmazor.RatePanelShowsCount);
            return !SaveUtils.GetValue(SaveKeysCommon.GameWasRated)
                   && ratePanelShowsCount < 10
                   && Random.value < 0.5f
                   && Model.LevelStaging.LevelIndex > ViewSettings.firstLevelToRateGame;
        }

        private bool MustShowAdvertising()
        {
            return Model.LevelStaging.LevelIndex > CommonGameSettings.firstLevelToShowAds
                   && !Application.isEditor;
        }

        #endregion
    }
}