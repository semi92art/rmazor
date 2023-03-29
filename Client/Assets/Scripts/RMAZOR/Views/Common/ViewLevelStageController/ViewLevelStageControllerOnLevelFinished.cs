using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common.Helpers;
using Common.Managers.Achievements;
using mazing.common.Runtime;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.UI.Game_Logo;
using RMAZOR.Views.Utils;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageControllerOnLevelFinished : IInit
    {
        void OnLevelFinished(LevelStageArgs _Args);
    }
    
    public class ViewLevelStageControllerOnLevelFinished
        : ViewLevelStageControllerOnSingleStageBase, 
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
        private IManagersGetter             Managers           { get; }
        private IViewInputCommandsProceeder CommandsProceeder  { get; }
        private ILevelsLoader               LevelsLoader       { get; }
        private IViewLevelStageSwitcher     LevelStageSwitcher { get; }
        private IViewGameTicker             ViewGameTicker     { get; }


        public ViewLevelStageControllerOnLevelFinished(
            GlobalGameSettings          _GlobalGameSettings,
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            IManagersGetter             _Managers,
            IViewInputCommandsProceeder _CommandsProceeder,
            ILevelsLoader               _LevelsLoader,
            IViewLevelStageSwitcher     _LevelStageSwitcher,
            IViewGameTicker             _ViewGameTicker,
            IViewUIGameLogo             _GameLogo)
            : base(_Model, _GameLogo)
        {
            GlobalGameSettings = _GlobalGameSettings;
            ViewSettings       = _ViewSettings;
            Managers           = _Managers;
            CommandsProceeder  = _CommandsProceeder;
            LevelsLoader       = _LevelsLoader;
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
            // ViewLevelStageControllerUtils.SaveGame(_Args, Managers.ScoreManager);
            StartUnloadLevelWithDelay(_Args);
            UnlockAchievementOnLevelFinishedIfKeyExist(_Args);
            CheckForAllLevelsPassed(_Args);
            ShowLevelGroupFinishedDialogPanelIfNeed(_Args);
            ProceedSounds(_Args);
            SendAnalyticsEvent(_Args);
        }
        
        #endregion

        #region nonpublic methods

        private void StartUnloadLevelWithDelay(LevelStageArgs _Args)
        {
            
            string gameMode = (string) _Args.Arguments.GetSafe(KeyGameMode, out _);
            const float defaultDelay = 1.5f;
            const float shortDelay = 1f;
            bool IsShortDelayInMainGameMode()
            {
                string levelType = ViewLevelStageSwitcherUtils.GetCurrentLevelType(_Args.Arguments);
                return levelType == ParameterLevelTypeBonus
                    || !GlobalGameSettings.enableExtraLevels
                    || (RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex)
                        && (_Args.LevelIndex % GlobalGameSettings.extraLevelEveryNStage != 0));
            }
            float delay = gameMode switch
            {
                ParameterGameModeMain => IsShortDelayInMainGameMode() ? shortDelay : defaultDelay,
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
                string source = MustLoadBonusLevel(_Args)
                    ? ParameterSourcePlayBonusLevelPanel
                    : ParameterSourceScreenTap;
                InvokeStartUnloadingLevel(source);
            }));
        }
        
        private void InvokeStartUnloadingLevel(string _Source)
        {
            var args = new Dictionary<string, object> {{KeySource, _Source}};
            LevelStageSwitcher.SwitchLevelStage(EInputCommand.StartUnloadingLevel, args);
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
            Cor.Run(Cor.Delay(0.5f, ViewGameTicker, () =>
            {
                CommandsProceeder.RaiseCommand(
                    EInputCommand.FinishLevelGroupPanel,
                    _Args.Arguments, 
                    true);
            }));
        }
        
        private bool MustLoadBonusLevel(LevelStageArgs _Args)
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
                return false;
            }
            return true;
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
            var args = new Dictionary<string, object>
            {
                {AnalyticIds.ParameterLevelIndex, _Args.LevelIndex},
                {AnalyticIdsRmazor.ParameterGameMode, GetGameModeAnalyticParameterValue(gameMode)},
                {AnalyticIds.ParameterLevelType, GetLevelTypeAnalyticParameterValue(levelType)},
            };
            Managers.AnalyticsManager.SendAnalytic(analyticId, args);
            Managers.AnalyticsManager.SendAnalytic(AnalyticIds.GetLevelFinishedAnalyticId(_Args.LevelIndex));
            if (RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex))
                Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.LevelStageFinished);
        }

        #endregion
    }
}