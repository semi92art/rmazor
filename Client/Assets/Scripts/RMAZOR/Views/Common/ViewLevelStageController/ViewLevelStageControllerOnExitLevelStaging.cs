using mazing.common.Runtime;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
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
        
        private bool m_StartLogoShowing = true;

        #endregion

        #region inject
        
        private IViewUIGameLogo             GameLogo               { get; }
        private IAudioManager               AudioManager           { get; }
        private IViewFullscreenTransitioner FullscreenTransitioner { get; }

        public ViewLevelStageControllerOnExitLevelStaging(
            IViewUIGameLogo             _GameLogo,
            IAudioManager               _AudioManager,
            IViewFullscreenTransitioner _FullscreenTransitioner)
        {
            GameLogo               = _GameLogo;
            AudioManager           = _AudioManager;
            FullscreenTransitioner = _FullscreenTransitioner;
        }

        #endregion

        #region api

        public void OnExitLevelStaging(LevelStageArgs _Args)
        {
            Dbg.Log("OnExitLevelStaging");
            if (!AudioManager.IsPlaying(AudioClipArgsMainTheme))
                AudioManager.PlayClip(AudioClipArgsMainTheme);
            FullscreenTransitioner.Enabled = false;
            if (!m_StartLogoShowing) 
                return;
            GameLogo.Show();
            m_StartLogoShowing = false;
        }

        #endregion
    }
}