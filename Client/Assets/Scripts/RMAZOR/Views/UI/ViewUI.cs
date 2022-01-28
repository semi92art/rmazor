using Common;
using Common.Enums;
using Common.Extensions;
using Common.Ticker;
using Common.Utils;
using DialogViewers;
using Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;

namespace RMAZOR.Views.UI
{
    public class ViewUI : ViewUIBase, IApplicationPause
    {
        #region inject

        private IModelGame                  Model                { get; }
        private IUITicker                   Ticker               { get; }
        private IBigDialogViewer            BigDialogViewer      { get; }
        private IProposalDialogViewer       ProposalDialogViewer { get; }
        private IDialogPanels               DialogPanels         { get; }
        private IViewInputCommandsProceeder CommandsProceeder    { get; }

        public ViewUI(
            IModelGame                  _Model,
            IUITicker                   _UITicker,
            IBigDialogViewer            _BigDialogViewer,
            IProposalDialogViewer       _ProposalDialogViewer,
            IDialogPanels               _DialogPanels,
            IViewUIGameControls         _GameControls,
            IViewInputCommandsProceeder _CommandsProceeder)
            : base(_GameControls)
        {
            Model                = _Model;
            Ticker               = _UITicker;
            BigDialogViewer      = _BigDialogViewer;
            ProposalDialogViewer = _ProposalDialogViewer;
            DialogPanels         = _DialogPanels;
            CommandsProceeder    = _CommandsProceeder;
        }

        #endregion
        
        #region api

        public override void Init()
        {
            Ticker.Register(this);
            CommandsProceeder.Command += OnCommand;
            CreateCanvas();
            var parent = m_Canvas.RTransform();
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
            RaiseInitializedEvent();
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
                    DialogPanels.RateGameDialogPanel.LoadPanel();
                    ProposalDialogViewer.Show(DialogPanels.RateGameDialogPanel, 3f);
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
            Dbg.Log(nameof(OnApplicationPause));
            int ratePanelShowsCount = SaveUtils.GetValue(SaveKeys.RatePanelShowsCount);
            bool mustShowRateGamePanel =
                Random.value < 0.05f
                && !SaveUtils.GetValue(SaveKeys.GameWasRated)
                && ratePanelShowsCount < 3
                && Model.LevelStaging.LevelIndex > 20;
            if (!mustShowRateGamePanel)
                return;
            CommandsProceeder.RaiseCommand(EInputCommand.RateGamePanel, null);
            SaveUtils.PutValue(SaveKeys.RatePanelShowsCount, ratePanelShowsCount + 1);
        }
        
        #endregion
    }
}