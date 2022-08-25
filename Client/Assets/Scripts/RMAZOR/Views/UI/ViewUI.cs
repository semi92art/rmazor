using Common;
using Common.Constants;
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

        private IDialogViewersController    DialogViewersController { get; }
        private ViewSettings                ViewSettings            { get; }
        private IModelGame                  Model                   { get; }
        private IUITicker                   Ticker                  { get; }
        private IDialogPanelsSet            DialogPanelsSet         { get; }
        private IViewInputCommandsProceeder CommandsProceeder       { get; }
        private IManagersGetter             Managers                { get; }

        private ViewUI(
            IDialogViewersController    _DialogViewersController,
            IViewUIGameControls         _GameControls,
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            IUITicker                   _UITicker,
            IDialogPanelsSet            _DialogPanelsSet,
            IViewInputCommandsProceeder _CommandsProceeder,
            IManagersGetter             _Managers)
            : base(_GameControls)
        {
            DialogViewersController = _DialogViewersController;
            ViewSettings            = _ViewSettings;
            Model                   = _Model;
            Ticker                  = _UITicker;
            DialogPanelsSet         = _DialogPanelsSet;
            CommandsProceeder       = _CommandsProceeder;
            Managers                = _Managers;
        }

        #endregion
        
        #region api

        public override void Init()
        {
            Ticker.Register(this);
            CommandsProceeder.Command += OnCommand;
            DialogViewersController.Init();
            GameControls.Init();
            DialogPanelsSet.Init();
            base.Init();
        }
        
        private void OnCommand(EInputCommand _Key, object[] _Args)
        {
            var dv = DialogViewersController.GetViewer(EDialogViewerType.Fullscreen);
            switch (_Key)
            {
                case EInputCommand.SettingsMenu:
                    Managers.AnalyticsManager.SendAnalytic(AnalyticIds.SettingsButtonPressed);
                    DialogPanelsSet.SettingDialogPanel.LoadPanel();
                    dv.Show(DialogPanelsSet.SettingDialogPanel);
                    CommandsProceeder.RaiseCommand(EInputCommand.PauseLevel, null, true);
                    break;
                case EInputCommand.ShopMenu:
                    Managers.AnalyticsManager.SendAnalytic(AnalyticIds.ShopButtonPressed);
                    DialogPanelsSet.ShopDialogPanel.LoadPanel();
                    dv.Show(DialogPanelsSet.ShopDialogPanel);
                    CommandsProceeder.RaiseCommand(EInputCommand.PauseLevel, null, true);
                    break;
                case EInputCommand.RateGamePanel:
                    ShowRateGamePanel();
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

        private void ShowRateGamePanel()
        {
            var dv = DialogViewersController.GetViewer(EDialogViewerType.Proposal);
            DialogPanelsSet.RateGameDialogPanel.LoadPanel();
            dv.Show(DialogPanelsSet.RateGameDialogPanel, 3f);
        }

        #endregion
    }
}