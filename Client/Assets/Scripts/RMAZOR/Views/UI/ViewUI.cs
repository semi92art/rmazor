using System.Linq;
using Common.Constants;
using Common.UI.DialogViewers;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.UI.Panels;
using RMAZOR.Views.InputConfigurators;

namespace RMAZOR.Views.UI
{
    public class ViewUI : ViewUIBase
    {
        #region inject

        private IDialogViewersController       DialogViewersController { get; }
        private IDialogPanelsSet               DialogPanelsSet         { get; }
        private IViewInputCommandsProceeder    CommandsProceeder       { get; }
        private IManagersGetter                Managers                { get; }
        private IViewUIRateGamePanelController RateGamePanelController { get; }

        private ViewUI(
            IDialogViewersController       _DialogViewersController,
            IViewUIGameControls            _GameControls,
            IDialogPanelsSet               _DialogPanelsSet,
            IViewInputCommandsProceeder    _CommandsProceeder,
            IManagersGetter                _Managers,
            IViewUIRateGamePanelController _RateGamePanelController)
            : base(_GameControls)
        {
            DialogViewersController = _DialogViewersController;
            DialogPanelsSet         = _DialogPanelsSet;
            CommandsProceeder       = _CommandsProceeder;
            Managers                = _Managers;
            RateGamePanelController = _RateGamePanelController;
        }

        #endregion
        
        #region api

        public override void Init()
        {
            CommandsProceeder.Command += OnCommand;
            GameControls.Init();
            DialogViewersController.Init();
            RateGamePanelController.Init();
            Cor.Run(Cor.WaitNextFrame(() => DialogPanelsSet.Init()));
            base.Init();
        }
        
        private void OnCommand(EInputCommand _Key, object[] _Args)
        {
            switch (_Key)
            {
                case EInputCommand.SettingsMenu:
                {
                    var panel = DialogPanelsSet.SettingDialogPanel;
                    var dv = DialogViewersController.GetViewer(panel.DialogViewerType);
                    dv.Show(panel);
                    CommandsProceeder.RaiseCommand(EInputCommand.PauseLevel, null, true);
                    Managers.AnalyticsManager.SendAnalytic(AnalyticIds.SettingsButtonPressed);
                }
                    break;
                case EInputCommand.ShopMenu:
                {
                    var panel = DialogPanelsSet.ShopDialogPanel;
                    var dv = DialogViewersController.GetViewer(panel.DialogViewerType);
                    panel.SetOnCloseFinishAction(() =>
                    {
                        if (_Args != null && _Args.Contains(CommonInputCommandArgs.LoadShopPanelFromCharacterDiedPanel))
                        {
                            DialogPanelsSet.CharacterDiedDialogPanel.ReturnFromShopPanel();
                        }
                        else
                        {
                            CommandsProceeder.RaiseCommand(
                                EInputCommand.UnPauseLevel, null, true);
                        }
                    });
                    dv.Show(panel);
                    if (_Args == null || !_Args.Contains(CommonInputCommandArgs.LoadShopPanelFromCharacterDiedPanel))
                    {
                        CommandsProceeder.RaiseCommand(EInputCommand.PauseLevel, null, true);
                        Managers.AnalyticsManager.SendAnalytic(AnalyticIds.ShopButtonPressed);
                    }
                }
                    break;
            }
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            RateGamePanelController.OnLevelStageChanged(_Args);
            GameControls.OnLevelStageChanged(_Args);
        }

        #endregion
    }
}