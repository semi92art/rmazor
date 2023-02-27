using System.Collections;
using mazing.common.Runtime;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views.UI.Game_Logo;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageControllerOnExitLevelStaging : IInit
    {
        void OnExitLevelStaging(LevelStageArgs _Args);
    }
    
    public class ViewLevelStageControllerOnExitLevelStaging :
        InitBase, 
        IViewLevelStageControllerOnExitLevelStaging
    {
        #region nonpublic members
        
        private static AudioClipArgs AudioClipArgsMainTheme =>
            new AudioClipArgs("main_theme", EAudioClipType.Music, 0.25f, true);
        
        private bool m_StartLogoWasShown;

        #endregion

        #region inject

        private ViewSettings                ViewSettings           { get; }
        private IViewUIGameLogo             GameLogo               { get; }
        private IAudioManager               AudioManager           { get; }
        private IViewFullscreenTransitioner FullscreenTransitioner { get; }
        private IUITicker                   UiTicker               { get; }

        private ViewLevelStageControllerOnExitLevelStaging(
            ViewSettings                _ViewSettings,
            IViewUIGameLogo             _GameLogo,
            IAudioManager               _AudioManager,
            IViewFullscreenTransitioner _FullscreenTransitioner,
            IUITicker                   _UIUiTicker)
        {
            ViewSettings           = _ViewSettings;
            GameLogo               = _GameLogo;
            AudioManager           = _AudioManager;
            FullscreenTransitioner = _FullscreenTransitioner;
            UiTicker               = _UIUiTicker;
        }

        #endregion

        #region api

        public void OnExitLevelStaging(LevelStageArgs _Args)
        {
            if (_Args.PreviousStage != ELevelStage.None)
                AudioManager.PauseClip(AudioClipArgsMainTheme);
            if (_Args.PreviousStage == ELevelStage.Unloaded)
                Cor.Run(HideFullscreenTransitionTextureCoroutine());
            ShowGameLogoIfItWasNot();
        }
        
        private IEnumerator HideFullscreenTransitionTextureCoroutine()
        {
            FullscreenTransitioner.DoTextureTransition(false, ViewSettings.betweenLevelTransitionTime);
            yield return Cor.Delay(ViewSettings.betweenLevelTransitionTime, UiTicker);
            FullscreenTransitioner.Enabled = false;
        }

        #endregion

        #region nonpublic methods

        private void ShowGameLogoIfItWasNot()
        {
            if (m_StartLogoWasShown) 
                return;
            GameLogo.Show();
            m_StartLogoWasShown = true;
        }

        #endregion
    }
}