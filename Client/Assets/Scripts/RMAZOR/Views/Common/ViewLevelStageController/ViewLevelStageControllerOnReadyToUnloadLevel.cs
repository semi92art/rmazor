using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Enums;
using Common.Extensions;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Common.Additional_Background;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageControllerOnReadyToUnloadLevel
    {
        void OnReadyToUnloadLevel(LevelStageArgs _Args, IReadOnlyCollection<IViewMazeItem> _MazeItems);
    }
    
    public class ViewLevelStageControllerOnReadyToUnloadLevel : IViewLevelStageControllerOnReadyToUnloadLevel
    {
        private ViewSettings                        ViewSettings                   { get; }
        private IViewCameraEffectsCustomAnimator    CameraEffectsCustomAnimator    { get; }
        private IViewBetweenLevelAdShower           BetweenLevelAdShower           { get; }
        private IViewFullscreenTransitioner         FullscreenTransitioner         { get; }
        private IViewMazeAdditionalBackgroundDrawer AdditionalBackgroundDrawer     { get; }
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }

        public ViewLevelStageControllerOnReadyToUnloadLevel(
            ViewSettings                        _ViewSettings,
            IViewCameraEffectsCustomAnimator    _CameraEffectsCustomAnimator,
            IViewBetweenLevelAdShower           _BetweenLevelAdShower,
            IViewFullscreenTransitioner         _FullscreenTransitioner,
            IViewMazeAdditionalBackgroundDrawer _AdditionalBackgroundDrawer,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker)
        {
            ViewSettings                   = _ViewSettings;
            CameraEffectsCustomAnimator    = _CameraEffectsCustomAnimator;
            BetweenLevelAdShower           = _BetweenLevelAdShower;
            FullscreenTransitioner         = _FullscreenTransitioner;
            AdditionalBackgroundDrawer     = _AdditionalBackgroundDrawer;
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
        }
        
        public void OnReadyToUnloadLevel(LevelStageArgs _Args, IReadOnlyCollection<IViewMazeItem> _MazeItems)
        {
            string currentLevelType = (string) _Args.Args.GetSafe(
                CommonInputCommandArg.KeyCurrentLevelType, out _);
            bool currentLevelIsBonus = currentLevelType == CommonInputCommandArg.ParameterLevelTypeBonus;
            BetweenLevelAdShower.TryShowAd(
                _Args.LevelIndex,
                currentLevelIsBonus, 
                () => UnloadLevel(_MazeItems));
        }
        
        private void UnloadLevel(IReadOnlyCollection<IViewMazeItem> _MazeItems)
        {
            CameraEffectsCustomAnimator.AnimateCameraEffectsOnBetweenLevelTransition(false);
            FullscreenTransitioner.DoTextureTransition(true, ViewSettings.betweenLevelTransitionTime);
            foreach (var mazeItem in _MazeItems)
                mazeItem.Appear(false);
            AdditionalBackgroundDrawer.Appear(false);
            Cor.Run(Cor.WaitWhile(() =>
                {
                    return _MazeItems.Any(_Item => _Item.AppearingState != EAppearingState.Dissapeared);
                },
                () =>
                {
                    SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.UnloadLevel);
                }));
        }
    }
}