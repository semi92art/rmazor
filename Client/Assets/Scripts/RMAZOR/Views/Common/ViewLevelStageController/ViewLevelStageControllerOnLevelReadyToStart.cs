using System.Collections.Generic;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Helpers;
using Common.Managers;
using Common.Managers.Analytics;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views.UI.Game_Logo;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageControllerOnLevelReadyToStart : IInit
    {
        void OnLevelReadyToStart(LevelStageArgs _Args);
    }
    
    public class ViewLevelStageControllerOnLevelReadyToStart : 
        InitBase,
        IViewLevelStageControllerOnLevelReadyToStart
    {
        #region nonpublic members
        
        private static AudioClipArgs AudioClipArgsLevelStart => 
            new AudioClipArgs("level_start", EAudioClipType.GameSound);
        private static AudioClipArgs AudioClipArgsMainTheme =>
            new AudioClipArgs("main_theme", EAudioClipType.Music, _Loop: true);

        #endregion

        #region inject
        
        private IAudioManager     AudioManager     { get; }
        private IAnalyticsManager AnalyticsManager { get; }
        private IViewUIGameLogo   GameLogo         { get; }
        private IModelGame        Model            { get; }
        
        public ViewLevelStageControllerOnLevelReadyToStart(
            IAudioManager     _AudioManager,
            IAnalyticsManager _AnalyticsManager,
            IViewUIGameLogo   _GameLogo,
            IModelGame        _Model)
        {
            AudioManager     = _AudioManager;
            AnalyticsManager = _AnalyticsManager;
            GameLogo         = _GameLogo;
            Model            = _Model;
        }

        #endregion

        #region api
        
        public override void Init()
        {
            GameLogo.Shown += GameLogoOnShown;
            base.Init();
        }
        
        public void OnLevelReadyToStart(LevelStageArgs _Args)
        {
            ProceedSounds(_Args);
        }

        #endregion

        #region nonpublic methods
        
        private void GameLogoOnShown()
        {
            AudioManager.PlayClip(AudioClipArgsMainTheme);
            SendAnalyticFirstLevelReadyToStart();
        }
        
        private void SendAnalyticFirstLevelReadyToStart()
        {
            var args = new Dictionary<string, object>
            {
                {AnalyticIds.ParameterLevelIndex, Model.LevelStaging.LevelIndex + 1}
            };
            AnalyticsManager.SendAnalytic(AnalyticIds.LevelReadyToStart, args);
        }
        
        private void ProceedSounds(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.ReadyToStart when _Args.PreviousStage == ELevelStage.Loaded:
                    if (!GameLogo.WasShown)
                        WaitUntilGameLogoWillBeShown();
                    AudioManager.UnmuteAudio(EAudioClipType.GameSound);
                    break;
                case ELevelStage.ReadyToStart:
                    AudioManager.UnmuteAudio(EAudioClipType.GameSound);
                    break;
            }
        }

        private void WaitUntilGameLogoWillBeShown()
        {
            Cor.Run(Cor.WaitWhile(
                () => !GameLogo.WasShown,
                () => AudioManager.PlayClip(AudioClipArgsLevelStart)));
        }
        
        #endregion
    }
}