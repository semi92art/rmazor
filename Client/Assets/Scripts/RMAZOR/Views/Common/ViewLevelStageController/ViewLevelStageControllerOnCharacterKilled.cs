using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.UI.DialogViewers;
using RMAZOR.Constants;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.UI.Panels;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageControllerOnCharacterKilled
    {
        void OnCharacterKilled(LevelStageArgs _Args, IEnumerable<IViewMazeItem> _MazeItems);
    }
    
    public class ViewLevelStageControllerOnCharacterKilled : IViewLevelStageControllerOnCharacterKilled
    {
        #region inject

        private IModelGame                          Model                          { get; }
        private IMazeShaker                         MazeShaker                     { get; }
        private IContainersGetter                   ContainersGetter               { get; }
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }
        private IDialogPanelsSet                    DialogPanelsSet                { get; }
        private IDialogViewersController            DialogViewersController        { get; }
        private IAnalyticsManager                   AnalyticsManager               { get; }

        public ViewLevelStageControllerOnCharacterKilled(
            IModelGame                          _Model,
            IMazeShaker                         _MazeShaker,
            IContainersGetter                   _ContainersGetter,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker,
            IDialogPanelsSet                    _DialogPanelsSet,
            IDialogViewersController            _DialogViewersController,
            IAnalyticsManager                   _AnalyticsManager)
        {
            Model                          = _Model;
            MazeShaker                     = _MazeShaker;
            ContainersGetter               = _ContainersGetter;
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
            DialogPanelsSet                = _DialogPanelsSet;
            DialogViewersController        = _DialogViewersController;
            AnalyticsManager               = _AnalyticsManager;
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
                    if (!MazorCommonData.Release)
                        return;
                    var gravityTraps = Model.GetAllProceedInfos()
                        .Where(_Info => _Info.Type == EMazeItemType.GravityTrap);
                    if (gravityTraps.Any(
                        _Info =>
                            _Info.CurrentPosition == Model.PathItemsProceeder.PathProceeds.First().Key))
                    {
                        InvokeStartUnloadingLevel(CommonInputCommandArg.ParameterCharacterDiedPanel);
                    }
                    else
                    {
                        var panel = DialogPanelsSet.GetPanel<ICharacterDiedDialogPanel>();
                        var dv = DialogViewersController.GetViewer(panel.DialogViewerId);
                        SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.PauseLevel);
                        dv.Show(panel, 3f);
                    }
                });
        }

        #endregion

        #region nonpublic methdos

        private void InvokeStartUnloadingLevel(string _Source)
        {
            var args = new Dictionary<string, object> {{CommonInputCommandArg.KeySource, _Source}};
            SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.StartUnloadingLevel, args);
        }
        
        private void SendLevelAnalyticEvent(LevelStageArgs _Args)
        {
            const string analyticId = AnalyticIdsRmazor.CharacterDied;

            if (string.IsNullOrEmpty(analyticId))
                return;
            AnalyticsManager.SendAnalytic(analyticId, 
                new Dictionary<string, object>
                {
                    {AnalyticIds.ParameterLevelIndex, _Args.LevelIndex},
                });
        }
        
        #endregion
    }
}