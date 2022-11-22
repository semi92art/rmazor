using System.Collections.Generic;
using Common;
using Common.Entities;
using Common.Enums;
using Common.Extensions;
using Common.Managers.Achievements;
using Common.Utils;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;

namespace RMAZOR.Views.Common
{
    public interface IViewLevelStageControllerOnLevelFinished
    {
        void OnLevelFinished(LevelStageArgs _Args);
    }
    
    public class ViewLevelStageControllerOnLevelFinished : IViewLevelStageControllerOnLevelFinished
    {
        #region nonpublic members

        private static AudioClipArgs AudioClipArgsLevelComplete => 
            new AudioClipArgs("level_complete", EAudioClipType.GameSound);

        #endregion
        
        #region inject

        private ViewSettings                ViewSettings      { get; }
        private IModelGame                  Model             { get; }
        private IManagersGetter             Managers          { get; }
        private IViewInputCommandsProceeder CommandsProceeder { get; }
        private ILevelsLoader               LevelsLoader      { get; }


        public ViewLevelStageControllerOnLevelFinished(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            IManagersGetter             _Managers,
            IViewInputCommandsProceeder _CommandsProceeder,
            ILevelsLoader               _LevelsLoader)
        {
            ViewSettings      = _ViewSettings;
            Model             = _Model;
            Managers          = _Managers;
            CommandsProceeder = _CommandsProceeder;
            LevelsLoader      = _LevelsLoader;
        }

        #endregion
        
        #region api

        public void OnLevelFinished(LevelStageArgs _Args)
        {
            if (_Args.PreviousStage == ELevelStage.Paused)
                return;
            UnlockAchievementOnLevelFinishedIfKeyExist(_Args);
            CheckForAllLevelsPassed(_Args);
            CheckForLevelGroupOrBonusLevelFinished(_Args);
            ProceedSounds(_Args);
        }
        
        #endregion

        #region nonpublic methods

        private void UnlockAchievementOnLevelFinishedIfKeyExist(LevelStageArgs _Args)
        {
            UnlockAchievementLevelFinishedByIndex(_Args);
            UnlockSpecificAchievementOnLevelFinished();
        }
        
        private void UnlockAchievementLevelFinishedByIndex(LevelStageArgs _Args)
        {
            if (_Args.PreviousStage == ELevelStage.Paused)
                return;
            var achievementKey = AchievementKeys.GetLevelFinishedAchievementKey(_Args.LevelIndex + 1);
            if (!achievementKey.HasValue)
                return;
            Managers.ScoreManager.UnlockAchievement(achievementKey.Value);
        }
        
        private void UnlockSpecificAchievementOnLevelFinished()
        {
            var args = Model.Data.Info.AdditionalInfo.Arguments.Split(';');
            foreach (string arg in args)
            {
                if (!arg.Contains("achievement"))
                    continue;
                ushort id = ushort.Parse(arg.Split(':')[1]);
                Managers.ScoreManager.UnlockAchievement(id);
            }
        }
        
        private void CheckForAllLevelsPassed(LevelStageArgs _Args)
        {
            if (_Args.PreviousStage == ELevelStage.Paused)
                return;
            bool allLevelsPassed = SaveUtils.GetValue(SaveKeysRmazor.AllLevelsPassed);
            if (!allLevelsPassed && _Args.LevelIndex + 1 >= ViewSettings.levelsCountMain)
                SaveUtils.PutValue(SaveKeysRmazor.AllLevelsPassed, true);
        }
        
        private void CheckForLevelGroupOrBonusLevelFinished(LevelStageArgs _Args)
        {
            if (_Args.PreviousStage == ELevelStage.Paused)
                return;
            string currentLevelType = (string) _Args.Args.GetSafe(
                CommonInputCommandArg.KeyCurrentLevelType, out _);
            bool currentLevelIsBonus = currentLevelType == CommonInputCommandArg.ParameterLevelTypeBonus;
            if (_Args.Args != null && currentLevelIsBonus)
            {
                CommandsProceeder.RaiseCommand(
                    EInputCommand.FinishLevelGroupPanel,
                    _Args.Args, 
                    true);
            }
            else if (RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex) 
                     && _Args.Args != null 
                     && !currentLevelIsBonus)
            {
                int nextBonusLevelIndexToLoad = RmazorUtils.GetLevelsGroupIndex(_Args.LevelIndex) - 1;
                var args = new Dictionary<string, object>
                {
                    {CommonInputCommandArg.KeyNextLevelType, CommonInputCommandArg.ParameterLevelTypeBonus}
                };
                int bonusLevelsCount = LevelsLoader.GetLevelsCount(CommonData.GameId, args);
                var inputCommand = nextBonusLevelIndexToLoad < bonusLevelsCount
                    ? EInputCommand.PlayBonusLevelPanel
                    : EInputCommand.FinishLevelGroupPanel;
                CommandsProceeder.RaiseCommand(
                    inputCommand,
                    _Args.Args, 
                    true);
            }
        }
        
        private void ProceedSounds(LevelStageArgs _Args)
        {
            var audioManager = Managers.AudioManager;
            switch (_Args.LevelStage)
            {
                case ELevelStage.Finished when _Args.PreviousStage != ELevelStage.Paused:
                    audioManager.PlayClip(AudioClipArgsLevelComplete);
                    break;
                case ELevelStage.Finished:
                    audioManager.UnmuteAudio(EAudioClipType.GameSound);
                    break;
                case ELevelStage.ReadyToUnloadLevel:
                case ELevelStage.Unloaded:
                case ELevelStage.CharacterKilled:
                    break;
            }
        }

        #endregion
        

    }
}