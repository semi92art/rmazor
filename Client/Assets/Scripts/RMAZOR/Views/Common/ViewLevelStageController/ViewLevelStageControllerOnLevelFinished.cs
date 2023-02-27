using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.Common.ViewUILevelSkippers;
using RMAZOR.Views.InputConfigurators;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageControllerOnLevelFinished : IInit
    {
        void OnLevelFinished(LevelStageArgs _Args);
    }
    
    public class ViewLevelStageControllerOnLevelFinished
        : InitBase, 
          IViewLevelStageControllerOnLevelFinished
    {
        #region nonpublic members

        private static AudioClipArgs AudioClipArgsLevelComplete => 
            new AudioClipArgs("level_complete", EAudioClipType.GameSound);
        private static AudioClipArgs AudioClipArgsLevelFail => 
            new AudioClipArgs("sound_fail", EAudioClipType.GameSound);

        #endregion
        
        #region inject

        private GlobalGameSettings          GlobalGameSettings { get; }
        private ViewSettings                ViewSettings       { get; }
        private IModelGame                  Model              { get; }
        private IManagersGetter             Managers           { get; }
        private IViewInputCommandsProceeder CommandsProceeder  { get; }
        private ILevelsLoader               LevelsLoader       { get; }
        private IViewGameUiLevelSkipper     LevelSkipper       { get; }
        private IViewLevelStageSwitcher     LevelStageSwitcher { get; }
        private IViewGameTicker             ViewGameTicker     { get; }


        public ViewLevelStageControllerOnLevelFinished(
            GlobalGameSettings          _GlobalGameSettings,
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            IManagersGetter             _Managers,
            IViewInputCommandsProceeder _CommandsProceeder,
            ILevelsLoader               _LevelsLoader,
            IViewGameUiLevelSkipper     _LevelSkipper,
            IViewLevelStageSwitcher     _LevelStageSwitcher,
            IViewGameTicker             _ViewGameTicker)
        {
            GlobalGameSettings = _GlobalGameSettings;
            ViewSettings       = _ViewSettings;
            Model              = _Model;
            Managers           = _Managers;
            CommandsProceeder  = _CommandsProceeder;
            LevelsLoader       = _LevelsLoader;
            LevelSkipper       = _LevelSkipper;
            LevelStageSwitcher = _LevelStageSwitcher;
            ViewGameTicker     = _ViewGameTicker;
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
            ViewLevelStageControllerUtils.SaveGame(_Args, Managers.ScoreManager);
            StartUnloadLevelWithDelay(_Args);
            SetLevelTimeRecord(_Args);
            UnlockAchievementOnLevelFinishedIfKeyExist(_Args);
            CheckForAllLevelsPassed(_Args);
            ShowLevelGroupFinishedDialogPanelIfNeed(_Args);
            ShowPlayBonusLevelDialogPanelIfNeed(_Args);
            ProceedSounds(_Args);
            CheckIfLevelWasFinishedAtLeastOnce(_Args);
            SendAnalyticsEvent(_Args);
        }
        
        #endregion

        #region nonpublic methods

        private void StartUnloadLevelWithDelay(LevelStageArgs _Args)
        {
            string gameMode = (string) _Args.Arguments.GetSafe(KeyGameMode, out _);
            string levelType = (string) _Args.Arguments.GetSafe(KeyCurrentLevelType, out _);
            const float defaultDelay = 1.5f;
            const float shortDelay = 0.5f;
            float delay = gameMode switch
            {
                ParameterGameModeMain => levelType == ParameterLevelTypeBonus
                                         || RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex)
                    ? shortDelay
                    : defaultDelay,
                ParameterGameModeRandom         => defaultDelay,
                ParameterGameModeDailyChallenge => defaultDelay,
                ParameterGameModePuzzles        => shortDelay,
                ParameterGameModeBigLevels      => defaultDelay,
                _                               => throw new SwitchExpressionException(gameMode)
            };
            Cor.Run(Cor.Delay(delay, ViewGameTicker, () =>
            {
                if (Model.LevelStaging.LevelStage != ELevelStage.Finished)
                    return;
                InvokeStartUnloadingLevel(ParameterSourceScreenTap);
            }));
        }
        
        private void InvokeStartUnloadingLevel(string _Source)
        {
            var args = new Dictionary<string, object> {{KeySource, _Source}};
            LevelStageSwitcher.SwitchLevelStage(EInputCommand.StartUnloadingLevel, args);
        }

        private void SetLevelTimeRecord(LevelStageArgs _Args)
        {
            long levelIndex = _Args.LevelIndex;
            string levelType = (string) _Args.Arguments.GetSafe(KeyCurrentLevelType, out _);
            bool isThisLevelBonus = levelType == ParameterLevelTypeBonus;
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
            string additionalInfoArgs = Model.Data.Info.AdditionalInfo.Arguments;
            var args = string.IsNullOrEmpty(additionalInfoArgs) ?
                new string[0] : additionalInfoArgs.Split(';');
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

        private void ShowLevelGroupFinishedDialogPanelIfNeed(LevelStageArgs _Args)
        {
            string gameMode               = ViewLevelStageSwitcherUtils.GetGameMode(_Args.Arguments);
            string currentLevelType       = ViewLevelStageSwitcherUtils.GetCurrentLevelType(_Args.Arguments);
            int levelGroupIndex           = RmazorUtils.GetLevelsGroupIndex(_Args.LevelIndex);
            int nextBonusLevelIndexToLoad = (levelGroupIndex - 1) / GlobalGameSettings.extraLevelEveryNStage;
            var levelInfoArgs = new LevelInfoArgs {GameMode = ParameterGameModeMain, LevelType = ParameterLevelTypeBonus};
            int bonusLevelsCount = LevelsLoader.GetLevelsCount(levelInfoArgs);
            bool mustShow = gameMode switch
            {
                ParameterGameModeMain when currentLevelType == ParameterLevelTypeBonus             => true,
                ParameterGameModeMain when
                    currentLevelType == ParameterLevelTypeDefault
                    && RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex)
                    && (!GlobalGameSettings.enableExtraLevels
                        || ((levelGroupIndex - 1) % GlobalGameSettings.extraLevelEveryNStage != 0)
                        || nextBonusLevelIndexToLoad >= bonusLevelsCount)                          => true,
                ParameterGameModePuzzles when !_Args.Arguments.ContainsKey(KeyShowPuzzleLevelHint) => true,
                _                                                                                  => false
            };
            if (!mustShow)
                return;
            CommandsProceeder.RaiseCommand(
                EInputCommand.FinishLevelGroupPanel,
                _Args.Arguments, 
                true);
        }
        
        private void ShowPlayBonusLevelDialogPanelIfNeed(LevelStageArgs _Args)
        {
            string gameMode               = ViewLevelStageSwitcherUtils.GetGameMode(_Args.Arguments);
            string currentLevelType       = ViewLevelStageSwitcherUtils.GetCurrentLevelType(_Args.Arguments);
            int levelGroupIndex           = RmazorUtils.GetLevelsGroupIndex(_Args.LevelIndex);
            int nextBonusLevelIndexToLoad = (levelGroupIndex - 1) / GlobalGameSettings.extraLevelEveryNStage;
            var levelInfoArgs = new LevelInfoArgs {GameMode = ParameterGameModeMain, LevelType = ParameterLevelTypeBonus};
            int bonusLevelsCount = LevelsLoader.GetLevelsCount(levelInfoArgs);
            if (gameMode != ParameterGameModeMain
                || !GlobalGameSettings.enableExtraLevels
                || !RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex)
                || currentLevelType == ParameterLevelTypeBonus
                || nextBonusLevelIndexToLoad >= bonusLevelsCount
                || (levelGroupIndex - 1) % GlobalGameSettings.extraLevelEveryNStage != 0)
            {
                return;
            }
            CommandsProceeder.RaiseCommand(
                EInputCommand.PlayBonusLevelPanel,
                _Args.Arguments, 
                true);
        }
        
        private void ProceedSounds(LevelStageArgs _Args)
        {
            var audioManager = Managers.AudioManager;
            switch (_Args.LevelStage)
            {
                case ELevelStage.Finished when _Args.PreviousStage != ELevelStage.Paused:
                    var audioClipArgs = AudioClipArgsLevelComplete;
                    if ((string)_Args.Arguments[KeyGameMode] == ParameterGameModeDailyChallenge
                        && (bool)_Args.Arguments[KeyIsDailyChallengeSuccess])
                    {
                        audioClipArgs = AudioClipArgsLevelFail;
                    }
                    audioManager.PlayClip(audioClipArgs);
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
            string levelType = (string)Model.LevelStaging.Arguments.GetSafe(KeyCurrentLevelType, out _);
            string gameMode  = (string) _Args.Arguments.GetSafe(KeyGameMode, out _);
            Managers.AnalyticsManager.SendAnalytic(analyticId, 
                new Dictionary<string, object>
                {
                    {AnalyticIdsRmazor.ParameterGameMode, GetGameModeAnalyticParameterValue(gameMode)},
                    {AnalyticIds.ParameterLevelIndex,     _Args.LevelIndex},
                    {AnalyticIds.ParameterLevelType,      GetLevelTypeAnalyticParameterValue(levelType)},
                });
            Managers.AnalyticsManager.SendAnalytic(AnalyticIds.GetLevelFinishedAnalyticId(_Args.LevelIndex));
            if (RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex))
                Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.LevelStageFinished);
        }
        
        private static int GetGameModeAnalyticParameterValue(string _GameMode)
        {
            return _GameMode switch
            {
                ParameterGameModeMain           => 1,
                ParameterGameModeDailyChallenge => 2,
                ParameterGameModeRandom         => 3,
                ParameterGameModePuzzles        => 4,
                ParameterGameModeBigLevels      => 5,
                _                               => 0
            };
        }

        private static int GetLevelTypeAnalyticParameterValue(string _LevelType)
        {
            return _LevelType switch
            {
                ParameterLevelTypeDefault => 1,
                ParameterLevelTypeBonus   => 2,
                _                         => 0
            };
        }

        private static bool CheckIfLevelWasFinishedAtLeastOnce(LevelStageArgs _Args)
        {
            string gameMode = (string) _Args.Arguments.GetSafe(KeyGameMode, out _);
            if (gameMode == ParameterGameModePuzzles && _Args.Arguments.ContainsKey(KeyShowPuzzleLevelHint))
                return false;
            SaveKey<List<long>> levelsFinishedOnceSaveKey = gameMode switch
            {
                ParameterGameModeMain           => SaveKeysRmazor.LevelsFinishedOnce,
                ParameterGameModePuzzles        => SaveKeysRmazor.LevelsFinishedOncePuzzles,
                ParameterGameModeRandom         => null,
                ParameterGameModeBigLevels      => null,
                ParameterGameModeDailyChallenge => null,
                _                               => null
            };
            if (levelsFinishedOnceSaveKey == null)
                return false;
            bool wasFinishedAtLeastOnce = false;
            var finishedOnceDict = SaveUtils.GetValue(levelsFinishedOnceSaveKey);
            if (finishedOnceDict != null && finishedOnceDict.Contains(_Args.LevelIndex))
                wasFinishedAtLeastOnce = true;
            if (wasFinishedAtLeastOnce) 
                return true;
            finishedOnceDict ??= new List<long>();
            finishedOnceDict.Add(_Args.LevelIndex);
            finishedOnceDict = finishedOnceDict.Distinct().ToList();
            SaveUtils.PutValue(levelsFinishedOnceSaveKey, finishedOnceDict);
            return false;
        }

        #endregion
    }
}