using Controllers;
using DI.Extensions;
using DialogViewers;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.InputConfigurators;
using Ticker;

namespace Games.RazorMaze.Views.UI
{
    public class BoolArgs
    {
        public bool Value { get; set; }
    }
    
    public class ViewUI : ViewUIBase
    {
        #region inject

        private IBigDialogViewer          DialogViewer       { get; }
        private IProposalDialogViewer    ProposalDialogViewer { get; }
        private IDialogPanels          DialogPanels       { get; }
        private ILoadingController     LoadingController  { get; }
        private IViewInputConfigurator InputConfigurator  { get; }

        public ViewUI(
            IUITicker _UITicker,
            IBigDialogViewer _DialogViewer,
            IProposalDialogViewer _ProposalDialogViewer,
            IDialogPanels _DialogPanels,
            ILoadingController _LoadingController,
            IViewUIGameControls _ViewUIGameControls,
            IViewInputConfigurator _InputConfigurator)
            : base(_ViewUIGameControls)
        {
            DialogViewer = _DialogViewer;
            ProposalDialogViewer = _ProposalDialogViewer;
            DialogPanels = _DialogPanels;
            LoadingController = _LoadingController;
            InputConfigurator = _InputConfigurator;
            _UITicker.Register(this);
        }

        #endregion
        
        #region api

        public override void Init()
        {
            InputConfigurator.Command += OnCommand;
            CreateCanvas();
            var parent = m_Canvas.RTransform();
            DialogViewer.Init(parent);
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
                    DialogViewer.Show(DialogPanels.SettingDialogPanel);
                    InputConfigurator.RaiseCommand(InputCommands.PauseLevel, null, true);
                    break;
                case InputCommands.ShopMenu:
                    DialogPanels.ShopDialogPanel.Init();
                    DialogViewer.Show(DialogPanels.ShopDialogPanel);
                    InputConfigurator.RaiseCommand(InputCommands.PauseLevel, null, true);
                    break;
            }
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            UIGameControls.OnLevelStageChanged(_Args);
        }
        
        #endregion
    }
}