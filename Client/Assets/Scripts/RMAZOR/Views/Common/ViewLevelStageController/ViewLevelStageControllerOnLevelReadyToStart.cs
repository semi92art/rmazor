using System.Collections.Generic;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Managers;
using RMAZOR.Constants;
using RMAZOR.Models;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.UI.Game_Logo;
using RMAZOR.Views.Utils;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageControllerOnLevelReadyToStart : IInit
    {
        void OnLevelReadyToStart(LevelStageArgs _Args);
    }
    
    public class ViewLevelStageControllerOnLevelReadyToStart : 
        ViewLevelStageControllerOnSingleStageBase,
        IViewLevelStageControllerOnLevelReadyToStart
    {
        #region inject
        
        private IAudioManager     AudioManager     { get; }
        private IAnalyticsManager AnalyticsManager { get; }
        private IScoreManager     ScoreManager     { get; }
        
        public ViewLevelStageControllerOnLevelReadyToStart(
            IAudioManager     _AudioManager,
            IAnalyticsManager _AnalyticsManager,
            IScoreManager     _ScoreManager,
            IModelGame        _Model,
            IViewUIGameLogo   _GameLogo) 
            : base(_Model, _GameLogo)
        {
            AudioManager     = _AudioManager;
            AnalyticsManager = _AnalyticsManager;
            ScoreManager     = _ScoreManager;
        }

        #endregion

        #region api

        public void OnLevelReadyToStart(LevelStageArgs _Args)
        {
            SaveGame(_Args);
            ProceedSounds(_Args);
            SendAnalyticLevelReadyToStart(_Args);
        }

        #endregion

        #region nonpublic methods

        private void SaveGame(LevelStageArgs _Args)
        {
            if (_Args.PreviousStage == ELevelStage.Loaded)
                ViewLevelStageSwitcherUtils.SaveGame(_Args, ScoreManager);
        }

        private void SendAnalyticLevelReadyToStart(LevelStageArgs _Args)
        {
            if (_Args.PreviousStage != ELevelStage.Loaded)
                return;
            string gameMode = ViewLevelStageSwitcherUtils.GetGameMode(_Args.Arguments);
            string levelType = ViewLevelStageSwitcherUtils.GetCurrentLevelType(_Args.Arguments);
            var args = new Dictionary<string, object>
            {
                {AnalyticIds.ParameterLevelIndex, _Args.LevelIndex},
                {AnalyticIdsRmazor.ParameterGameMode, GetGameModeAnalyticParameterValue(gameMode)},
                {AnalyticIds.ParameterLevelType, GetLevelTypeAnalyticParameterValue(levelType)},
            };
            AnalyticsManager.SendAnalytic(AnalyticIds.LevelReadyToStart, args);
        }
        
        private void ProceedSounds(LevelStageArgs _Args)
        {
            AudioManager.UnmuteAudio(EAudioClipType.GameSound);
        }

        #endregion
    }
}