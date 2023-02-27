using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using Common.Helpers;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Common.Additional_Background;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageControllerOnReadyToUnloadLevel : IInit
    {
        void OnReadyToUnloadLevel(LevelStageArgs _Args, IReadOnlyCollection<IViewMazeItem> _MazeItems);
    }
    
    public class ViewLevelStageControllerOnReadyToUnloadLevel
        : InitBase, 
          IViewLevelStageControllerOnReadyToUnloadLevel
    {
        private ViewSettings                        ViewSettings                   { get; }
        private IViewCameraEffectsCustomAnimator    CameraEffectsCustomAnimator    { get; }
        private IViewBetweenLevelAdShower           BetweenLevelAdShower           { get; }
        private IViewFullscreenTransitioner         FullscreenTransitioner         { get; }
        private IViewMazeAdditionalBackgroundDrawer AdditionalBackgroundDrawer     { get; }
        private IViewLevelStageSwitcher LevelStageSwitcher { get; }

        public ViewLevelStageControllerOnReadyToUnloadLevel(
            ViewSettings                        _ViewSettings,
            IViewCameraEffectsCustomAnimator    _CameraEffectsCustomAnimator,
            IViewBetweenLevelAdShower           _BetweenLevelAdShower,
            IViewFullscreenTransitioner         _FullscreenTransitioner,
            IViewMazeAdditionalBackgroundDrawer _AdditionalBackgroundDrawer,
            IViewLevelStageSwitcher             _LevelStageSwitcher)
        {
            ViewSettings                   = _ViewSettings;
            CameraEffectsCustomAnimator    = _CameraEffectsCustomAnimator;
            BetweenLevelAdShower           = _BetweenLevelAdShower;
            FullscreenTransitioner         = _FullscreenTransitioner;
            AdditionalBackgroundDrawer     = _AdditionalBackgroundDrawer;
            LevelStageSwitcher             = _LevelStageSwitcher;
        }

        public override void Init()
        {
            BetweenLevelAdShower.Init();
            base.Init();
        }

        public void OnReadyToUnloadLevel(LevelStageArgs _Args, IReadOnlyCollection<IViewMazeItem> _MazeItems)
        {
            string currentLevelType = (string) _Args.Arguments.GetSafe(
                ComInComArg.KeyCurrentLevelType, out _);
            bool currentLevelIsBonus = currentLevelType == ComInComArg.ParameterLevelTypeBonus;
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
                    LevelStageSwitcher.SwitchLevelStage(EInputCommand.UnloadLevel);
                }));
        }
    }
}