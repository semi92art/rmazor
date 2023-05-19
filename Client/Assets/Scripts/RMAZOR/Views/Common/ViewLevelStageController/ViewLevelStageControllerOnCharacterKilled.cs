using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.UI.DialogViewers;
using RMAZOR.Constants;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.UI.Panels;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.UI.Game_Logo;
using RMAZOR.Views.Utils;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageControllerOnCharacterKilled
    {
        void OnCharacterKilled(LevelStageArgs _Args, IEnumerable<IViewMazeItem> _MazeItems);
    }
    
    public class ViewLevelStageControllerOnCharacterKilled : 
        ViewLevelStageControllerOnSingleStageBase,
        IViewLevelStageControllerOnCharacterKilled
    {
        #region inject

        private IMazeShaker              MazeShaker              { get; }
        private IContainersGetter        ContainersGetter        { get; }
        private IViewLevelStageSwitcher  LevelStageSwitcher      { get; }
        private IDialogPanelsSet         DialogPanelsSet         { get; }
        private IDialogViewersController DialogViewersController { get; }
        private IAnalyticsManager        AnalyticsManager        { get; }

        public ViewLevelStageControllerOnCharacterKilled(
            IModelGame               _Model,
            IViewUIGameLogo          _GameLogo,
            IMazeShaker              _MazeShaker,
            IContainersGetter        _ContainersGetter,
            IViewLevelStageSwitcher  _LevelStageSwitcher,
            IDialogPanelsSet         _DialogPanelsSet,
            IDialogViewersController _DialogViewersController,
            IAnalyticsManager        _AnalyticsManager) 
            : base(_Model, _GameLogo)
        {
            MazeShaker              = _MazeShaker;
            ContainersGetter        = _ContainersGetter;
            LevelStageSwitcher      = _LevelStageSwitcher;
            DialogPanelsSet         = _DialogPanelsSet;
            DialogViewersController = _DialogViewersController;
            AnalyticsManager        = _AnalyticsManager;
        }

        #endregion

        #region api

        public void OnCharacterKilled(LevelStageArgs _Args, IEnumerable<IViewMazeItem> _MazeItems)
        {
            SendLevelAnalyticEvent(_Args);
            MazeShaker.OnCharacterDeathAnimation(
                ContainersGetter.GetContainer(ContainerNamesMazor.Character).transform.position,
                _MazeItems,
                () =>
                {
                    if (!CommonDataMazor.Release)
                        return;
                    var gravityTraps = Model.GetAllProceedInfos()
                        .Where(_Info => _Info.Type == EMazeItemType.GravityTrap);
                    if (gravityTraps.Any(
                        _Info =>
                            _Info.CurrentPosition == Model.PathItemsProceeder.PathProceeds.First().Key))
                    {
                        InvokeStartUnloadingLevel(ComInComArg.ParameterSourceCharacterDiedPanel);
                    }
                    else
                    {
                        var panel = DialogPanelsSet.GetPanel<ICharacterDiedDialogPanel>();
                        var dv = DialogViewersController.GetViewer(panel.DialogViewerId);
                        LevelStageSwitcher.SwitchLevelStage(EInputCommand.PauseLevel);
                        dv.Show(panel, 3f);
                    }
                });
        }

        #endregion

        #region nonpublic methdos

        private void InvokeStartUnloadingLevel(string _Source)
        {
            var args = new Dictionary<string, object> {{KeySource, _Source}};
            LevelStageSwitcher.SwitchLevelStage(EInputCommand.StartUnloadingLevel, args);
        }
        
        private void SendLevelAnalyticEvent(LevelStageArgs _Args)
        {
            var args = ViewLevelStageSwitcherUtils.GetLevelParametersForAnalytic(_Args);
            AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.CharacterDied, args);
        }
        
        #endregion
    }
}