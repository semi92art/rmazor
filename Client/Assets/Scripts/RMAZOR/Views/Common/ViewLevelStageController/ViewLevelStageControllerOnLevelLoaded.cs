using System.Collections.Generic;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Characters;
using RMAZOR.Views.MazeItemGroups;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageControllerOnLevelLoaded
    {
        void OnLevelLoaded(LevelStageArgs _Args, IEnumerable<IViewMazeItem> _MazeItems);
    }
    
    public class ViewLevelStageControllerOnLevelLoaded : IViewLevelStageControllerOnLevelLoaded
    {
        #region nonpublic members

        private static AudioClipArgs AudioClipArgsMainTheme =>
            new AudioClipArgs("main_theme", EAudioClipType.Music, 0.25f, true);

        private bool m_MainThemeSoundPlayFirstTime;

        #endregion
        
        #region inject
        
        private ViewSettings                     ViewSettings                { get; }
        private IModelGame                       Model                       { get; }
        private IScoreManager                    ScoreManager                { get; }
        private IAudioManager                    AudioManager                { get; }
        private IViewCharacter                   Character                   { get; }
        private IViewFullscreenTransitioner      FullscreenTransitioner      { get; }
        private IViewMazePathItemsGroup          PathItemsGroup              { get; }
        private IViewCameraEffectsCustomAnimator CameraEffectsCustomAnimator { get; }

        private ViewLevelStageControllerOnLevelLoaded(
            ViewSettings                     _ViewSettings,
            IModelGame                       _Model,
            IScoreManager                    _ScoreManager,
            IAudioManager                    _AudioManager,
            IViewCharacter                   _Character,
            IViewFullscreenTransitioner      _FullscreenTransitioner,
            IViewMazePathItemsGroup          _PathItemsGroup,
            IViewCameraEffectsCustomAnimator _CameraEffectsCustomAnimator)
        {
            ViewSettings                = _ViewSettings;
            Model                       = _Model;
            ScoreManager                = _ScoreManager;
            AudioManager                = _AudioManager;
            Character                   = _Character;
            FullscreenTransitioner      = _FullscreenTransitioner;
            PathItemsGroup              = _PathItemsGroup;
            CameraEffectsCustomAnimator = _CameraEffectsCustomAnimator;
        }


        #endregion

        #region api
        
        public void OnLevelLoaded(LevelStageArgs _Args, IEnumerable<IViewMazeItem> _MazeItems)
        {
            ProceedAudio();
            ViewLevelStageControllerUtils.SaveGame(_Args, ScoreManager);
            Character.Appear(true);
            ProceedPathAndMazeItems(_MazeItems);
            ProceedBetweenLevelTransition();
        }
        
        #endregion

        #region nonpublic methods

        private void ProceedAudio()
        {
            if (AudioManager.IsPlaying(AudioClipArgsMainTheme)) return;
            if (!m_MainThemeSoundPlayFirstTime)
            {
                AudioManager.PlayClip(AudioClipArgsMainTheme);
                m_MainThemeSoundPlayFirstTime = true;
            }
            else AudioManager.UnpauseClip(AudioClipArgsMainTheme);
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