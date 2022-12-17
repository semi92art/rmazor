using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Extensions;
using Common.Managers.Achievements;
using Common.Utils;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;

namespace RMAZOR.Views.Common.ViewLevelStageController
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

        private ViewSettings                        ViewSettings                   { get; }
        private IModelGame                          Model                          { get; }
        private IManagersGetter                     Managers                       { get; }
        private IViewInputCommandsProceeder         CommandsProceeder              { get; }
        private ILevelsLoader                       LevelsLoader                   { get; }
        private IViewUILevelSkipper                 LevelSkipper                   { get; }
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }


        public ViewLevelStageControllerOnLevelFinished(
            ViewSettings                        _ViewSettings,
            IModelGame                          _Model,
            IManagersGetter                     _Managers,
            IViewInputCommandsProceeder         _CommandsProceeder,
            ILevelsLoader                       _LevelsLoader,
            IViewUILevelSkipper                 _LevelSkipper,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker)
        {
            ViewSettings                   = _ViewSettings;
            Model                          = _Model;
            Managers                       = _Managers;
            CommandsProceeder              = _CommandsProceeder;
            LevelsLoader                   = _LevelsLoader;
            LevelSkipper                   = _LevelSkipper;
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
        }

        #endregion
        
        #region api

        public void OnLevelFinished(LevelStageArgs _Args)
        {
            if (_Args.PreviousStage == ELevelStage.Paused)
                return;
            SetLevelTimeRecord(_Args);
            UnlockAchievementOnLevelFinishedIfKeyExist(_Args);
            CheckForAllLevelsPassed(_Args);
            CheckForLevelGroupOrBonusLevelFinished(_Args);
            ProceedSounds(_Args);
            SendAnalyticsEvent(_Args);
            if (!MustStartUnloadingLevel(_Args)) 
                return;
            var args = new Dictionary<string, object>
            {
                {CommonInputCommandArg.KeySource, CommonInputCommandArg.ParameterLevelSkipper}
            };
            SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.StartUnloadingLevel, args);
        }
        
        #endregion

        #region nonpublic methods

        private static void SetLevelTimeRecord(LevelStageArgs _Args)
        {
            long levelIndex = _Args.LevelIndex;
            string levelType = (string) _Args.Args.GetSafe(CommonInputCommandArg.KeyCurrentLevelType, out _);
            bool isThisLevelBonus = levelType == CommonInputCommandArg.ParameterLevelTypeBonus;
            var saveKey = isThisLevelBonus
                ? SaveKeysRmazor.BonusLevelTimeRecordsDict
                : SaveKeysRmazor.MainLevelTimeRecordsDict;
            var levelTimeRecordsDict = SaveUtils.GetValue(saveKey) ?? new Dictionary<long, float>();
            float timeRecord = levelTimeRecordsDict.GetSafe(levelIndex, out bool containsKey);
            if (!containsKey || _Args.LevelTime < timeRecord)
                levelTimeRecordsDict.SetSafe(levelIndex, _Args.LevelTime);
            SaveUtils.PutValue(saveKey, levelTimeRecordsDict);
        }
        
        private bool MustStartUnloadingLevel(LevelStageArgs _Args)
        {
            string currentLevelType = (string)_Args.Args.GetSafe(CommonInputCommandArg.KeyCurrentLevelType, out _);
            bool isLastLevelInMainGroup = RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex);
            bool isCurrentLevelBonus = currentLevelType == CommonInputCommandArg.ParameterLevelTypeBonus;
            return LevelSkipper.LevelSkipped && (isCurrentLevelBonus || !isLastLevelInMainGroup);
        }

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
                int bonusLevelsCount = LevelsLoader.GetLevelsCount(CommonData.GameId, true);
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
            }
        }
        
        private void SendAnalyticsEvent(LevelStageArgs _Args)
        {
            if (_Args.PreviousStage != ELevelStage.StartedOrContinued)
                return;
            const string analyticId = AnalyticIds.LevelFinished;
            if (CheckIfLevelWasFinishedAtLeastOnce(_Args.LevelIndex))
                return;
            Managers.AnalyticsManager.SendAnalytic(analyticId, 
                new Dictionary<string, object>
                {
                    {AnalyticIds.ParameterLevelIndex, _Args.LevelIndex},
                });
            Managers.AnalyticsManager.SendAnalytic(AnalyticIds.GetLevelFinishedAnalyticId(_Args.LevelIndex));
            if (RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex))
                Managers.AnalyticsManager.SendAnalytic(AnalyticIds.LevelStageFinished);
        }
        
        private static bool CheckIfLevelWasFinishedAtLeastOnce(long _LevelIndex)
        {
            bool wasFinishedAtLeastOnce = false;
            var finishedOnceDict = SaveUtils.GetValue(SaveKeysRmazor.LevelsFinishedOnce);
            if (finishedOnceDict != null && finishedOnceDict.Contains(_LevelIndex))
                wasFinishedAtLeastOnce = true;
            if (wasFinishedAtLeastOnce) 
                return true;
            finishedOnceDict ??= new List<long>();
            finishedOnceDict.Add(_LevelIndex);
            finishedOnceDict = finishedOnceDict.Distinct().ToList();
            SaveUtils.PutValue(SaveKeysRmazor.LevelsFinishedOnce, finishedOnceDict);
            return false;
        }

        #endregion
    }
}