using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using Common.Managers.Achievements;
using mazing.common.Runtime;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common.ViewUILevelSkippers;
using RMAZOR.Views.InputConfigurators;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageControllerOnLevelFinished : IInit
    {
        void OnLevelFinished(LevelStageArgs _Args);
    }
    
    public class ViewLevelStageControllerOnLevelFinished : InitBase, IViewLevelStageControllerOnLevelFinished
    {
        #region nonpublic members

        private static AudioClipArgs AudioClipArgsLevelComplete => 
            new AudioClipArgs("level_complete", EAudioClipType.GameSound);

        #endregion
        
        #region inject

        private GlobalGameSettings                  GlobalGameSettings             { get; }
        private ViewSettings                        ViewSettings                   { get; }
        private IModelGame                          Model                          { get; }
        private IManagersGetter                     Managers                       { get; }
        private IViewInputCommandsProceeder         CommandsProceeder              { get; }
        private ILevelsLoader                       LevelsLoader                   { get; }
        private IViewUILevelSkipper                 LevelSkipper                   { get; }
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }
        private IViewGameTicker                     ViewGameTicker                 { get; }


        public ViewLevelStageControllerOnLevelFinished(
            GlobalGameSettings                  _GlobalGameSettings,
            ViewSettings                        _ViewSettings,
            IModelGame                          _Model,
            IManagersGetter                     _Managers,
            IViewInputCommandsProceeder         _CommandsProceeder,
            ILevelsLoader                       _LevelsLoader,
            IViewUILevelSkipper                 _LevelSkipper,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker,
            IViewGameTicker                     _ViewGameTicker)
        {
            GlobalGameSettings             = _GlobalGameSettings;
            ViewSettings                   = _ViewSettings;
            Model                          = _Model;
            Managers                       = _Managers;
            CommandsProceeder              = _CommandsProceeder;
            LevelsLoader                   = _LevelsLoader;
            LevelSkipper                   = _LevelSkipper;
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
            ViewGameTicker                 = _ViewGameTicker;
        }

        #endregion
        
        #region api

        public override void Init()
        {
            Managers.AudioManager.InitClip(AudioClipArgsLevelComplete);
            base.Init();
        }

        public void OnLevelFinished(LevelStageArgs _Args)
        {
            if (_Args.PreviousStage == ELevelStage.Paused)
                return;
            UnloadLevelWithDelay(_Args);
            SetLevelTimeRecord(_Args);
            UnlockAchievementOnLevelFinishedIfKeyExist(_Args);
            CheckForAllLevelsPassed(_Args);
            CheckForLevelGroupOrBonusLevelFinished(_Args);
            ProceedSounds(_Args);
            SendAnalyticsEvent(_Args);
        }
        
        #endregion

        #region nonpublic methods

        private void UnloadLevelWithDelay(LevelStageArgs _Args)
        {
            string levelType = (string) _Args.Args.GetSafe(CommonInputCommandArg.KeyCurrentLevelType, out _);
            bool isThisLevelBonus = levelType == CommonInputCommandArg.ParameterLevelTypeBonus;
            bool isThisLevelLastInGroup = !isThisLevelBonus && RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex);
            float delay = isThisLevelBonus || isThisLevelLastInGroup ? 0.5f : 1.5f;
            Cor.Run(Cor.Delay(delay, ViewGameTicker, () =>
            {
                if (Model.LevelStaging.LevelStage != ELevelStage.Finished)
                    return;
                InvokeStartUnloadingLevel(CommonInputCommandArg.ParameterScreenTap);
            }));
        }
        
        private void InvokeStartUnloadingLevel(string _Source)
        {
            var args = new Dictionary<string, object> {{CommonInputCommandArg.KeySource, _Source}};
            SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.StartUnloadingLevel, args);
        }

        private void SetLevelTimeRecord(LevelStageArgs _Args)
        {
            long levelIndex = _Args.LevelIndex;
            string levelType = (string) _Args.Args.GetSafe(CommonInputCommandArg.KeyCurrentLevelType, out _);
            bool isThisLevelBonus = levelType == CommonInputCommandArg.ParameterLevelTypeBonus;
            var saveKey = isThisLevelBonus
                ? SaveKeysRmazor.BonusLevelTimeRecordsDict
                : SaveKeysRmazor.MainLevelTimeRecordsDict;
            var levelTimeRecordsDict = SaveUtils.GetValue(saveKey) ?? new Dictionary<long, float>();
            float timeRecord = levelTimeRecordsDict.GetSafe(levelIndex, out bool containsKey);
            if ((!containsKey || _Args.LevelTime < timeRecord) && !LevelSkipper.LevelSkipped)
                levelTimeRecordsDict.SetSafe(levelIndex, _Args.LevelTime);
            SaveUtils.PutValue(saveKey, levelTimeRecordsDict);
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
                int levelGroupIndex = RmazorUtils.GetLevelsGroupIndex(_Args.LevelIndex);
                int nextBonusLevelIndexToLoad = (levelGroupIndex - 1) / GlobalGameSettings.extraLevelEveryNStage;
                int bonusLevelsCount = LevelsLoader.GetLevelsCount(CommonData.GameId, true);
                var inputCommand = GlobalGameSettings.enableExtraLevels
                                   && (levelGroupIndex - 1) % GlobalGameSettings.extraLevelEveryNStage == 0
                                   && nextBonusLevelIndexToLoad < bonusLevelsCount
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
            string levelType = (string)Model.LevelStaging.Arguments.GetSafe(
                CommonInputCommandArg.KeyCurrentLevelType, out _);
            bool isThisLevelBonus = levelType == CommonInputCommandArg.ParameterLevelTypeBonus;
            Managers.AnalyticsManager.SendAnalytic(analyticId, 
                new Dictionary<string, object>
                {
                    {AnalyticIds.ParameterLevelIndex, _Args.LevelIndex},
                    {AnalyticIds.ParameterLevelType, isThisLevelBonus ? 2 : 1}
                });
            Managers.AnalyticsManager.SendAnalytic(AnalyticIds.GetLevelFinishedAnalyticId(_Args.LevelIndex));
            if (RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex))
                Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.LevelStageFinished);
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