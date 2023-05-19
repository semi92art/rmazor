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
        private IManagersGetter             Managers           { get; }
        private IViewInputCommandsProceeder CommandsProceeder  { get; }
        private ILevelsLoader               LevelsLoader       { get; }
        private IViewLevelStageSwitcher     LevelStageSwitcher { get; }
        private IViewGameTicker             ViewGameTicker     { get; }


        public ViewLevelStageControllerOnLevelFinished(
            GlobalGameSettings          _GlobalGameSettings,
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
            StartUnloadLevelWithDelay(_Args);
            UnlockAchievementOnLevelFinishedIfKeyExist(_Args);
            CheckForAllLevelsPassed(_Args);
            ShowLevelGroupFinishedDialogPanelIfNeed(_Args);
            ShowPlayExtraLevelDialogPanelIfNeed(_Args);
            ProceedSounds(_Args);
            SendLevelFinishedAnalyticsEvents(_Args);
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
        
        private static void CheckForAllLevelsPassed(LevelStageArgs _Args)
        {
            if (_Args.PreviousStage == ELevelStage.Paused)
                return;
            bool allLevelsPassed = SaveUtils.GetValue(SaveKeysRmazor.AllLevelsPassed);
            if (!allLevelsPassed && _Args.LevelIndex + 1 >= 400)
                SaveUtils.PutValue(SaveKeysRmazor.AllLevelsPassed, true);
        }

        private void ShowPlayExtraLevelDialogPanelIfNeed(LevelStageArgs _Args)
        {
            string gameMode = ViewLevelStageSwitcherUtils.GetGameMode(_Args.Arguments);
            string currentLevelType = ViewLevelStageSwitcherUtils.GetCurrentLevelType(_Args.Arguments);
            bool mustShow = gameMode switch
            {
                ParameterGameModeMain when currentLevelType == ParameterLevelTypeBonus => false,
                ParameterGameModeMain when
                    currentLevelType == ParameterLevelTypeDefault
                    && RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex)
                    && IsNextLevelBonus(_Args) => true,
                ParameterGameModePuzzles when !_Args.Arguments.ContainsKey(KeyShowPuzzleLevelHint) => true,
                _ => false
            };
            if (!mustShow)
                return;
            RunShowPanelCommandWithDelayAndArguments(EInputCommand.PlayBonusLevelPanel, _Args, 0.5f);
        }
        
        private void ShowLevelGroupFinishedDialogPanelIfNeed(LevelStageArgs _Args)
        {
            string gameMode               = ViewLevelStageSwitcherUtils.GetGameMode(_Args.Arguments);
            string currentLevelType       = ViewLevelStageSwitcherUtils.GetCurrentLevelType(_Args.Arguments);
            bool mustShow = gameMode switch
            {
                ParameterGameModeMain when currentLevelType == ParameterLevelTypeBonus => true,
                ParameterGameModeMain when
                    currentLevelType == ParameterLevelTypeDefault
                    && RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex)
                    && !IsNextLevelBonus((_Args)) => true,
                ParameterGameModePuzzles when !_Args.Arguments.ContainsKey(KeyShowPuzzleLevelHint) => true,
                _ => false
            };
            if (!mustShow)
                return;
            RunShowPanelCommandWithDelayAndArguments(EInputCommand.FinishLevelGroupPanel, _Args, 0.5f);
        }

        private bool IsNextLevelBonus(LevelStageArgs _Args)
        {
            var bonusLevelInfoArgs = new LevelInfoArgs
                {GameMode = ParameterGameModeMain, LevelType = ParameterLevelTypeBonus};
            int bonusLevelsCount = LevelsLoader.GetLevelsCount(bonusLevelInfoArgs);
            int levelGroupIndex           = RmazorUtils.GetLevelsGroupIndex(_Args.LevelIndex);
            int nextBonusLevelIndexToLoad = (levelGroupIndex - 1 - GlobalGameSettings.extraLevelFirstStage) 
                                            / GlobalGameSettings.extraLevelEveryNStage;
            return GlobalGameSettings.enableExtraLevels
                   && ((levelGroupIndex - 1 - GlobalGameSettings.extraLevelFirstStage) %
                       GlobalGameSettings.extraLevelEveryNStage == 0)
                   && MathUtils.IsInRange(nextBonusLevelIndexToLoad, 0, bonusLevelsCount);
        }

        private void RunShowPanelCommandWithDelayAndArguments(
            EInputCommand  _Command,
            LevelStageArgs _Args,
            float          _Delay)
        {
            Cor.Run(Cor.Delay(_Delay, ViewGameTicker, () =>
            {
                CommandsProceeder.RaiseCommand(
                    _Command,
                    _Args.Arguments, 
                    true);
            }));
        }

        private bool MustLoadBonusLevel(LevelStageArgs _Args)
        {
            string gameMode               = ViewLevelStageSwitcherUtils.GetGameMode(_Args.Arguments);
            string currentLevelType       = ViewLevelStageSwitcherUtils.GetCurrentLevelType(_Args.Arguments);
            int levelGroupIndex           = RmazorUtils.GetLevelsGroupIndex(_Args.LevelIndex);
            int nextBonusLevelIndexToLoad = (levelGroupIndex - 1 - GlobalGameSettings.extraLevelFirstStage) 
                                            / GlobalGameSettings.extraLevelEveryNStage;
            var levelInfoArgs = new LevelInfoArgs {GameMode = ParameterGameModeMain, LevelType = ParameterLevelTypeBonus};
            int bonusLevelsCount = LevelsLoader.GetLevelsCount(levelInfoArgs);
            if (gameMode != ParameterGameModeMain
                || !MathUtils.IsInRange(nextBonusLevelIndexToLoad, 0, bonusLevelsCount - 1)
                || !GlobalGameSettings.enableExtraLevels
                || !RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex)
                || currentLevelType == ParameterLevelTypeBonus
                || (levelGroupIndex - 1 - GlobalGameSettings.extraLevelFirstStage) % GlobalGameSettings.extraLevelEveryNStage != 0)
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
                        && !(bool)_Args.Arguments[KeyIsDailyChallengeSuccess])
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
        
        private void SendLevelFinishedAnalyticsEvents(LevelStageArgs _Args)
        {
            if (_Args.PreviousStage != ELevelStage.StartedOrContinued)
                return;
            var args = ViewLevelStageSwitcherUtils.GetLevelParametersForAnalytic(_Args);
            Managers.AnalyticsManager.SendAnalytic(AnalyticIds.LevelFinished, args);
            string gameMode = ViewLevelStageSwitcherUtils.GetGameMode(_Args.Arguments);
            string levelFinisgedConcreteGameModeAnalyticId = gameMode switch
            {
                ParameterGameModeMain           => AnalyticIdsRmazor.LevelFinishedGameModeMain,
                ParameterGameModePuzzles        => AnalyticIdsRmazor.LevelFinishedGameModePuzzles,
                ParameterGameModeDailyChallenge => AnalyticIdsRmazor.LevelFinishedGameModeDailyChallenge,
                ParameterGameModeBigLevels      => AnalyticIdsRmazor.LevelFinishedGameModeBigLevels,
                _                               => null
            };
            if (!string.IsNullOrEmpty(levelFinisgedConcreteGameModeAnalyticId))
                Managers.AnalyticsManager.SendAnalytic(levelFinisgedConcreteGameModeAnalyticId);
            if (gameMode == ParameterGameModeMain && RmazorUtils.IsLastLevelInGroup(_Args.LevelIndex))
                Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.LevelStageFinished, args);
        }

        #endregion
    }
}