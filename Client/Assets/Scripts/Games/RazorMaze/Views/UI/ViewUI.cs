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

        private IBigDialogViewer       BigDialogViewer      { get; }
        private IProposalDialogViewer  ProposalDialogViewer { get; }
        private IDialogPanels          DialogPanels         { get; }
        private ILoadingController     LoadingController    { get; }
        private IViewInput Input    { get; }

        public ViewUI(
            IUITicker _UITicker,
            IBigDialogViewer _BigDialogViewer,
            IProposalDialogViewer _ProposalDialogViewer,
            IDialogPanels _DialogPanels,
            ILoadingController _LoadingController,
            IViewUIGameControls _ViewUIGameControls,
            IViewInput _Input)
            : base(_ViewUIGameControls)
        {
            BigDialogViewer = _BigDialogViewer;
            ProposalDialogViewer = _ProposalDialogViewer;
            DialogPanels = _DialogPanels;
            LoadingController = _LoadingController;
            Input = _Input;
            _UITicker.Register(this);
        }

        #endregion
        
        #region api

        public override void Init()
        {
            Input.Command += OnCommand;
            CreateCanvas();
            var parent = m_Canvas.RTransform();
            BigDialogViewer.Init(parent);
            ProposalDialogViewer.Init(parent);
            UIGameControls.Init();
            RaiseInitializedEvent();
        }

        private void OnCommand(int _Key, object[] _Args)
        {
            switch (_Key)
            {
                case InputCommands.SettingsMenu:
                    DialogPanels.SettingDialogPanel.Init();
                    BigDialogViewer.Show(DialogPanels.SettingDialogPanel);
                    Input.RaiseCommand(InputCommands.PauseLevel, null, true);
                    break;
                case InputCommands.ShopMenu:
                    DialogPanels.ShopDialogPanel.Init();
                    BigDialogViewer.Show(DialogPanels.ShopDialogPanel);
                    Input.RaiseCommand(InputCommands.PauseLevel, null, true);
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
            if (!GameClientUtils.InternetConnection)
                return;
            bool mustShowRateGamePanel = _Args.LevelIndex % 10 == 0 && _Args.LevelIndex != 0;
            if (!mustShowRateGamePanel)
                return;
            bool gameWasRatedAlready = SaveUtils.GetValue<bool>(SaveKey.GameWasRated);
            if (gameWasRatedAlready)
                return;
            var panel = DialogPanels.RateGameDialogPanel;
            panel.Init();
            ProposalDialogViewer.Show(panel);
        }

        #endregion
    }
}