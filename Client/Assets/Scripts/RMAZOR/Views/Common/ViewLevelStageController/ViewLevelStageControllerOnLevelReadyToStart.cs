using System.Collections.Generic;
using mazing.common.Runtime;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
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
        #region inject
        
        private IAudioManager     AudioManager     { get; }
        private IAnalyticsManager AnalyticsManager { get; }
        private IModelGame        Model            { get; }
        
        public ViewLevelStageControllerOnLevelReadyToStart(
            IAudioManager     _AudioManager,
            IAnalyticsManager _AnalyticsManager,
            IModelGame        _Model)
        {
            AudioManager     = _AudioManager;
            AnalyticsManager = _AnalyticsManager;
            Model            = _Model;
        }

        #endregion

        #region api

        public void OnLevelReadyToStart(LevelStageArgs _Args)
        {
            ProceedSounds(_Args);
        }

        #endregion

        #region nonpublic methods

        private void SendAnalyticLevelReadyToStart()
        {
            string levelType = (string)Model.LevelStaging.Arguments.GetSafe(
                CommonInputCommandArg.KeyCurrentLevelType, out _);
            bool isThisLevelBonus = levelType == CommonInputCommandArg.ParameterLevelTypeBonus;
            var args = new Dictionary<string, object>
            {
                {AnalyticIds.ParameterLevelIndex, Model.LevelStaging.LevelIndex},
                {AnalyticIds.ParameterLevelType, isThisLevelBonus ? 2 : 1}
            };
            AnalyticsManager.SendAnalytic(AnalyticIds.LevelReadyToStart, args);
        }
        
        private void ProceedSounds(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.ReadyToStart when _Args.PreviousStage == ELevelStage.Loaded:
                    SendAnalyticLevelReadyToStart();
                    AudioManager.UnmuteAudio(EAudioClipType.GameSound);
                    break;
                case ELevelStage.ReadyToStart:
                    AudioManager.UnmuteAudio(EAudioClipType.GameSound);
                    break;
            }
        }

        #endregion
    }
}