using System.Collections.Generic;
using Common.Constants;
using Common.Extensions;
using Common.UI.DialogViewers;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.UI.Panels;
using RMAZOR.UI.Panels.ShopPanels;
using RMAZOR.Views.Common;
using RMAZOR.Views.InputConfigurators;

namespace RMAZOR.Views.UI
{
    public class ViewUI : ViewUIBase
    {
        #region inject

        private IDialogViewersController            DialogViewersController        { get; }
        private IDialogPanelsSet                    DialogPanelsSet                { get; }
        private IViewInputCommandsProceeder         CommandsProceeder              { get; }
        private IManagersGetter                     Managers                       { get; }
        private IViewUIRateGamePanelController      RateGamePanelController        { get; }
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }

        private ViewUI(
            IDialogViewersController            _DialogViewersController,
            IViewUIGameControls                 _GameControls,
            IDialogPanelsSet                    _DialogPanelsSet,
            IViewInputCommandsProceeder         _CommandsProceeder,
            IManagersGetter                     _Managers,
            IViewUIRateGamePanelController      _RateGamePanelController,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker)
            : base(_GameControls)
        {
            DialogViewersController        = _DialogViewersController;
            DialogPanelsSet                = _DialogPanelsSet;
            CommandsProceeder              = _CommandsProceeder;
            Managers                       = _Managers;
            RateGamePanelController        = _RateGamePanelController;
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
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
        
        private void OnCommand(EInputCommand _Key, Dictionary<string, object> _Args)
        {
            switch (_Key)
            {
                case EInputCommand.SettingsPanel:
                {
                    var panel = DialogPanelsSet.GetPanel<ISettingDialogPanel>();
                    var dv = DialogViewersController.GetViewer(panel.DialogViewerType);
                    dv.Show(panel);
                    SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.PauseLevel, true);
                    Managers.AnalyticsManager.SendAnalytic(AnalyticIds.SettingsButtonPressed);
                }
                    break;
                case EInputCommand.ShopPanel:
                {
                    var panel = DialogPanelsSet.GetPanel<IShopDialogPanel>();
                    var dv = DialogViewersController.GetViewer(panel.DialogViewerType);
                    panel.SetOnCloseFinishAction(() =>
                    {
                        object loadShopPanelFromCharDiedPanelArg1 = _Args.GetSafe(
                            CommonInputCommandArg.KeyLoadShopPanelFromCharacterDiedPanel, out bool keyExist1);
                        if (keyExist1 && (bool)loadShopPanelFromCharDiedPanelArg1)
                        {
                            DialogPanelsSet.GetPanel<ICharacterDiedDialogPanel>().ReturnFromShopPanel();
                        }
                        else
                        {
                            SwitchLevelStageCommandInvoker.SwitchLevelStage(
                                EInputCommand.UnPauseLevel, true);
                        }
                    });
                    dv.Show(panel);
                    object loadShopPanelFromCharDiedPanelArg = _Args.GetSafe(
                        CommonInputCommandArg.KeyLoadShopPanelFromCharacterDiedPanel, out bool keyExist);
                    if (!keyExist || (bool)loadShopPanelFromCharDiedPanelArg == false)
                    {
                        SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.PauseLevel, true);
                        Managers.AnalyticsManager.SendAnalytic(AnalyticIds.ShopButtonPressed);
                    }
                }
                    break;
                case EInputCommand.PlayBonusLevelPanel:
                {
                    var playBonusLevelPanel = DialogPanelsSet.GetPanel<IPlayBonusLevelDialogPanel>();
                    var dv = DialogViewersController.GetViewer(playBonusLevelPanel.DialogViewerType);
                    dv.Show(playBonusLevelPanel);
                }
                    break;
                case EInputCommand.FinishLevelGroupPanel:
                {
                    var finishLevelGroupDialogPanel = DialogPanelsSet.GetPanel<IFinishLevelGroupDialogPanel>();
                    var dv = DialogViewersController.GetViewer(finishLevelGroupDialogPanel.DialogViewerType);
                    dv.Show(finishLevelGroupDialogPanel);
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