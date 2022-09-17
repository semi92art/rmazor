using System.Linq;
using Common;
using Common.Constants;
using Common.Ticker;
using Common.UI.DialogViewers;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.UI.Panels;
using RMAZOR.Views.InputConfigurators;
using Random = UnityEngine.Random;

namespace RMAZOR.Views.UI
{
    public class ViewUI : ViewUIBase, IApplicationFocus
    {
        #region nonpublic members

        private int  m_LevelsFinishedThisSession;
        private bool m_RatePanelShownThisSession;

        #endregion
        
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
            GameControls.Init();
            DialogViewersController.Init();
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
                case EInputCommand.RateGamePanel:
                {
                    m_RatePanelShownThisSession = true;
                    var panel = DialogPanelsSet.RateGameDialogPanel;
                    var dv = DialogViewersController.GetViewer(panel.DialogViewerType);
                    dv.Show(panel);
                    int ratePanelShowsCount = SaveUtils.GetValue(SaveKeysRmazor.RatePanelShowsCount);
                    SaveUtils.PutValue(SaveKeysRmazor.RatePanelShowsCount, ratePanelShowsCount + 1);
                }
                    break;
            }
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage == ELevelStage.Finished)
            {
                m_LevelsFinishedThisSession++;
                if (_Args.LevelIndex == ViewSettings.firstLevelToRateGame
                    || m_LevelsFinishedThisSession == 10
                    && !m_RatePanelShownThisSession)
                {
                    CommandsProceeder.RaiseCommand(EInputCommand.RateGamePanel, null);
                }
            }
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
            return !SaveUtils.GetValue(SaveKeysCommon.GameWasRated)
                   && Random.value < 0.0f
                   && Model.LevelStaging.LevelIndex > ViewSettings.firstLevelToRateGame
                   && !m_RatePanelShownThisSession;
        }

        #endregion
    }
}