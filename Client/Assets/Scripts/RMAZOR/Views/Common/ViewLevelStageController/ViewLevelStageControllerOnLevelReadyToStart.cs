using Common.Entities;
using Common.Enums;
using Common.Managers;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.UI.Game_Logo;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageControllerOnLevelReadyToStart
    {
        void OnLevelReadyToStart(LevelStageArgs _Args);
    }
    
    public class ViewLevelStageControllerOnLevelReadyToStart : IViewLevelStageControllerOnLevelReadyToStart
    {
        private static AudioClipArgs AudioClipArgsLevelStart => 
            new AudioClipArgs("level_start", EAudioClipType.GameSound);
        private static AudioClipArgs AudioClipArgsMainTheme =>
            new AudioClipArgs("main_theme", EAudioClipType.Music, _Loop: true);
        
        private bool m_FirstTimeLevelLoaded;
        
        
        private IAudioManager   AudioManager { get; }
        private IViewUIGameLogo GameLogo     { get; }

        public ViewLevelStageControllerOnLevelReadyToStart(
            IAudioManager   _AudioManager,
            IViewUIGameLogo _GameLogo)
        {
            AudioManager = _AudioManager;
            GameLogo     = _GameLogo;
        }
        
        public void OnLevelReadyToStart(LevelStageArgs _Args)
        {
            ProceedSounds(_Args);
        }
        
        private void ProceedSounds(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.ReadyToStart when _Args.PreviousStage == ELevelStage.Loaded:
                    Cor.Run(Cor.WaitWhile(
                        () => !GameLogo.WasShown,
                        () =>
                        {
                            AudioManager.PlayClip(AudioClipArgsLevelStart);
                            if (!m_FirstTimeLevelLoaded)
                            {
                                AudioManager.PlayClip(AudioClipArgsMainTheme);
                                m_FirstTimeLevelLoaded = true;
                            }
                            AudioManager.UnmuteAudio(EAudioClipType.GameSound);
                        }));
                    break;
                case ELevelStage.ReadyToStart:
                    AudioManager.UnmuteAudio(EAudioClipType.GameSound);
                    break;
            }
        }
    }
}