using Controllers;
using DI.Extensions;
using DialogViewers;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.InputConfigurators;
using Ticker;
using Utils;

namespace Games.RazorMaze.Views.UI
{
    public class ViewUI : ViewUIBase
    {
        #region inject

        private IUITicker                   UITicker             { get; }
        private IBigDialogViewer            BigDialogViewer      { get; }
        private IProposalDialogViewer       ProposalDialogViewer { get; }
        private IDialogPanels               DialogPanels         { get; }
        private ILoadingController          LoadingController    { get; }
        private IViewInputCommandsProceeder CommandsProceeder    { get; }

        public ViewUI(
            IUITicker _UITicker,
            IBigDialogViewer _BigDialogViewer,
            IProposalDialogViewer _ProposalDialogViewer,
            IDialogPanels _DialogPanels,
            ILoadingController _LoadingController,
            IViewUIGameControls _ViewUIGameControls,
            IViewInputCommandsProceeder _CommandsProceeder)
            : base(_ViewUIGameControls)
        {
            UITicker = _UITicker;
            BigDialogViewer = _BigDialogViewer;
            ProposalDialogViewer = _ProposalDialogViewer;
            DialogPanels = _DialogPanels;
            LoadingController = _LoadingController;
            CommandsProceeder = _CommandsProceeder;
        }

        #endregion
        
        #region api

        public override void Init()
        {
            UITicker.Register(this);
            CommandsProceeder.Command += OnCommand;
            CreateCanvas();
            var parent = m_Canvas.RTransform();
            BigDialogViewer.Init(parent);
            ProposalDialogViewer.Init(parent);
            BigDialogViewer.IsOtherDialogViewersShowing = () => ProposalDialogViewer.IsShowing;
            ProposalDialogViewer.IsOtherDialogViewersShowing = () => BigDialogViewer.IsShowing;
            UIGameControls.Init();
            RaiseInitializedEvent();
        }

        private void OnCommand(EInputCommand _Key, object[] _Args)
        {
            switch (_Key)
            {
                case EInputCommand.SettingsMenu:
                    DialogPanels.SettingDialogPanel.Init();
                    BigDialogViewer.Show(DialogPanels.SettingDialogPanel);
                    CommandsProceeder.RaiseCommand(EInputCommand.PauseLevel, null, true);
                    break;
                case EInputCommand.ShopMenu:
                    DialogPanels.ShopMoneyDialogPanel.Init();
                    BigDialogViewer.Show(DialogPanels.ShopMoneyDialogPanel);
                    CommandsProceeder.RaiseCommand(EInputCommand.PauseLevel, null, true);
                    break;
            }
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            UIGameControls.OnLevelStageChanged(_Args);
            ShowRateGamePanel(_Args);
        }
        
        #endregion

        #region nonpublic methdos

        private void ShowRateGamePanel(LevelStageArgs _Args)
        {
            if (_Args.Stage != ELevelStage.Finished) 
                return;
            // FIXME
            // if (!GameClientUtils.InternetConnection)
            //     return;
            bool mustShowRateGamePanel = _Args.LevelIndex % 10 == 0 && _Args.LevelIndex != 0;
            if (!mustShowRateGamePanel)
                return;
            bool gameWasRatedAlready = SaveUtils.GetValue(SaveKeys.GameWasRated);
            if (gameWasRatedAlready)
                return;
            var panel = DialogPanels.RateGameDialogPanel;
            panel.Init();
            ProposalDialogViewer.Show(panel);
        }

        #endregion
    }
}