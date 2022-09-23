using Common;
using Common.Helpers;
using Common.UI.DialogViewers;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.UI.Panels;
using RMAZOR.Views.InputConfigurators;

namespace RMAZOR.Views.UI
{
    public interface IViewUIRateGamePanelController : IInit, IOnLevelStageChanged { }
    
    public class ViewUIRateGamePanelController : InitBase, IViewUIRateGamePanelController
    {
        #region nonpublic members

        private bool m_CanShowPanelThisSession;
        private bool m_RatePanelShownThisSession;
        private int  m_LevelsFinishedThisSession;

        #endregion
        
        #region inject

        private ViewSettings                ViewSettings            { get; }
        private IViewInputCommandsProceeder CommandsProceeder       { get; }
        private IDialogPanelsSet            DialogPanelsSet         { get; }
        private IDialogViewersController    DialogViewersController { get; }

        public ViewUIRateGamePanelController(
            ViewSettings                _ViewSettings,
            IViewInputCommandsProceeder _CommandsProceeder,
            IDialogPanelsSet            _DialogPanelsSet,
            IDialogViewersController    _DialogViewersController)
        {
            ViewSettings            = _ViewSettings;
            CommandsProceeder       = _CommandsProceeder;
            DialogPanelsSet         = _DialogPanelsSet;
            DialogViewersController = _DialogViewersController;
        }

        #endregion

        #region api

        public override void Init()
        {
            SetThisSessionPanelShowPossibility();
            CommandsProceeder.Command += OnCommand;
            base.Init();
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage != ELevelStage.Finished)
                return;
            m_LevelsFinishedThisSession++;
            if (MustShowPanelOnLevelFinished(_Args.LevelIndex))
            {
                CommandsProceeder.RaiseCommand(EInputCommand.RateGamePanel, null);
            }
        }

        #endregion

        #region nonpulic methods

        private void OnCommand(EInputCommand _Key, object[] _Args)
        {
            if (_Key != EInputCommand.RateGamePanel)
                return;
            m_RatePanelShownThisSession = true;
            var panel = DialogPanelsSet.RateGameDialogPanel;
            var dv = DialogViewersController.GetViewer(panel.DialogViewerType);
            dv.Show(panel);
        }

        private void SetThisSessionPanelShowPossibility()
        {
            float randVal = UnityEngine.Random.value;
            Dbg.Log("SetThisSessionPanelShowPossibility rand val: " + randVal);
            m_CanShowPanelThisSession = randVal < 0.33f;
        }

        private bool MustShowPanelOnLevelFinished(long _LevelIndex)
        {
            if (m_RatePanelShownThisSession || RmazorUtils.IsLastLevelInGroup(_LevelIndex))
                return false;
            if (_LevelIndex == ViewSettings.firstLevelToRateGame)
                return true;
            return m_LevelsFinishedThisSession >= 3
                   && m_CanShowPanelThisSession;
        }

        #endregion
    }
}