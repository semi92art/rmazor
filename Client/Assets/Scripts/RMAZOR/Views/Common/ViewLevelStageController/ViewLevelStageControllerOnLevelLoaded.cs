using System.Collections.Generic;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Settings;
using RMAZOR.Views.Characters;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.MazeItemGroups;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.UI.Game_Logo;
using RMAZOR.Views.Utils;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageControllerOnLevelLoaded
    {
        void OnLevelLoaded(LevelStageArgs _Args, IEnumerable<IViewMazeItem> _MazeItems);
    }
    
    public class ViewLevelStageControllerOnLevelLoaded 
        : ViewLevelStageControllerOnSingleStageBase, 
          IViewLevelStageControllerOnLevelLoaded
    {
        #region inject
        
        private ViewSettings                     ViewSettings                { get; }
        private IScoreManager                    ScoreManager                { get; }
        private IAudioManager                    AudioManager                { get; }
        private IViewCharacter                   Character                   { get; }
        private IViewFullscreenTransitioner      FullscreenTransitioner      { get; }
        private IViewMazePathItemsGroup          PathItemsGroup              { get; }
        private IViewCameraEffectsCustomAnimator CameraEffectsCustomAnimator { get; }
        private IRetroModeSetting                RetroModeSetting            { get; }

        private ViewLevelStageControllerOnLevelLoaded(
            ViewSettings                     _ViewSettings,
            IModelGame                       _Model,
            IViewUIGameLogo                  _GameLogo,
            IScoreManager                    _ScoreManager,
            IAudioManager                    _AudioManager,
            IViewCharacter                   _Character,
            IViewFullscreenTransitioner      _FullscreenTransitioner,
            IViewMazePathItemsGroup          _PathItemsGroup,
            IViewCameraEffectsCustomAnimator _CameraEffectsCustomAnimator,
            IRetroModeSetting                _RetroModeSetting)
            : base(_Model, _GameLogo)
        {
            ViewSettings                = _ViewSettings;
            ScoreManager                = _ScoreManager;
            AudioManager                = _AudioManager;
            Character                   = _Character;
            FullscreenTransitioner      = _FullscreenTransitioner;
            PathItemsGroup              = _PathItemsGroup;
            CameraEffectsCustomAnimator = _CameraEffectsCustomAnimator;
            RetroModeSetting            = _RetroModeSetting;
        }
        
        #endregion

        #region api
        
        public void OnLevelLoaded(LevelStageArgs _Args, IEnumerable<IViewMazeItem> _MazeItems)
        {
            ProceedAudio(_Args);
            ViewLevelStageSwitcherUtils.SaveGame(_Args, ScoreManager);
            Character.Appear(true);
            ProceedPathAndMazeItems(_MazeItems);
            ProceedBetweenLevelTransition();
            if (ViewSettings.loadMainGameModeOnStart)
                ShowGameLogoIfItWasNot();
            RetroModeSetting.UpdateState();
        }
        
        #endregion

        #region nonpublic methods

        private void ProceedAudio(LevelStageArgs _Args)
        {
            if (_Args.PreviousStage == ELevelStage.None)
                AudioManager.StopClip(AudioClipArgsMainMenuTheme);
            var audioClipArgsLevelTheme = GetAudioClipArgsLevelTheme();
            if (!AudioManager.IsPlaying(audioClipArgsLevelTheme))
                AudioManager.PlayClip(audioClipArgsLevelTheme);
        }

        private void ProceedPathAndMazeItems(IEnumerable<IViewMazeItem> _MazeItems)
        {
            foreach (var pathItem in PathItemsGroup.PathItems)
            {
                bool collect = Model.PathItemsProceeder.PathProceeds[pathItem.Props.Position];
                pathItem.Collect(collect, true);
            }
            foreach (var mazeItem in _MazeItems)
                mazeItem.Appear(true);
        }

        private void ProceedBetweenLevelTransition()
        {
            FullscreenTransitioner.Enabled = true;
            FullscreenTransitioner.DoTextureTransition(false, ViewSettings.betweenLevelTransitionTime);
            CameraEffectsCustomAnimator.AnimateCameraEffectsOnBetweenLevelTransition(true);
        }

        #endregion
    }
}