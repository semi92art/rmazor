using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using mazing.common.Runtime;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Common.Additional_Background;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.Utils;
using static RMAZOR.Models.ComInComArg;

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
        private GlobalGameSettings                  GlobalGameSettings          { get; }
        private ViewSettings                        ViewSettings                { get; }
        private IViewCameraEffectsCustomAnimator    CameraEffectsCustomAnimator { get; }
        private IViewBetweenLevelAdShower           BetweenLevelAdShower        { get; }
        private IViewFullscreenTransitioner         FullscreenTransitioner      { get; }
        private IViewMazeAdditionalBackgroundDrawer AdditionalBackgroundDrawer  { get; }
        private IViewLevelStageSwitcher             LevelStageSwitcher          { get; }

        public ViewLevelStageControllerOnReadyToUnloadLevel(
            GlobalGameSettings                  _GlobalGameSettings,
            ViewSettings                        _ViewSettings,
            IViewCameraEffectsCustomAnimator    _CameraEffectsCustomAnimator,
            IViewBetweenLevelAdShower           _BetweenLevelAdShower,
            IViewFullscreenTransitioner         _FullscreenTransitioner,
            IViewMazeAdditionalBackgroundDrawer _AdditionalBackgroundDrawer,
            IViewLevelStageSwitcher             _LevelStageSwitcher)
        {
            GlobalGameSettings             = _GlobalGameSettings;
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
            string gameMode = ViewLevelStageSwitcherUtils.GetGameMode(_Args.Arguments);
            string currentLevelType = ViewLevelStageSwitcherUtils.GetCurrentLevelType(_Args.Arguments);
            if (gameMode == ParameterGameModeMain
                && (currentLevelType == ParameterLevelTypeBonus 
                    || _Args.LevelIndex < GlobalGameSettings.firstLevelToShowAds))
            {
                UnloadLevel(_MazeItems);
            }
            else
            {
                BetweenLevelAdShower.TryShowAd(() => UnloadLevel(_MazeItems));
            }
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