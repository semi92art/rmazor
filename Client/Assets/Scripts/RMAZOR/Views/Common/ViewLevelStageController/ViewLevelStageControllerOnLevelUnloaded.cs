using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.UI.Utils;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.Common.ViewUILevelSkippers;
using RMAZOR.Views.InputConfigurators;
using UnityEngine.Events;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageControllerOnLevelUnloaded
    {
        void OnLevelUnloaded(LevelStageArgs _Args);
    }
    
    public class ViewLevelStageControllerOnLevelUnloaded : IViewLevelStageControllerOnLevelUnloaded
    {
        #region inject

        private IModelGame                  Model              { get; }
        private IScoreManager               ScoreManager       { get; }
        private IViewLevelStageSwitcher     LevelStageSwitcher { get; }
        private IViewGameTicker             ViewGameTicker     { get; }
        private IViewInputCommandsProceeder CommandsProceeder  { get; }
        private ICameraProvider             CameraProvider     { get; }
        private IUITicker                   UiTicker           { get; }
        private IViewGameUiLevelSkipper     LevelSkipper       { get; }

        public ViewLevelStageControllerOnLevelUnloaded(
            IModelGame                  _Model,
            IScoreManager               _ScoreManager,
            IViewLevelStageSwitcher     _LevelStageSwitcher,
            IViewGameTicker             _ViewGameTicker,
            IViewInputCommandsProceeder _CommandsProceeder,
            ICameraProvider             _CameraProvider,
            IUITicker                   _UIUiTicker,
            IViewGameUiLevelSkipper     _LevelSkipper)
        {
            Model              = _Model;
            ScoreManager       = _ScoreManager;
            LevelStageSwitcher = _LevelStageSwitcher;
            ViewGameTicker     = _ViewGameTicker;
            CommandsProceeder  = _CommandsProceeder;
            CameraProvider     = _CameraProvider;
            UiTicker           = _UIUiTicker;
            LevelSkipper       = _LevelSkipper;
        }

        #endregion

        #region api

        public void OnLevelUnloaded(LevelStageArgs _Args)
        {
            GC.Collect();
            SetLevelTimeRecord(_Args);
            CheckIfLevelWasFinishedAtLeastOnce(_Args);
            var scoreEntity = ScoreManager.GetScoreFromLeaderboard(DataFieldIds.Level, false);
            Cor.Run(Cor.WaitWhile(
                () => scoreEntity.Result == EEntityResult.Pending,
                () =>
                {
                    switch (scoreEntity.Result)
                    {
                        case EEntityResult.Pending:
                            Dbg.LogWarning("Timeout when getting score from leaderboard");
                            return;
                        case EEntityResult.Fail:
                            Dbg.LogError("Failed to get score from leaderboard");
                            return;
                        case EEntityResult.Success:
                        {
                            var score = scoreEntity.GetFirstScore();
                            if (!score.HasValue)
                            {
                                Dbg.LogError("Failed to get score from leaderboard");
                                return;
                            }
                            Dbg.Log("Level score from server leaderboard: " + score.Value);
                            ScoreManager.SetScoreToLeaderboard(
                                DataFieldIds.Level, 
                                score.Value + 1, 
                                false);
                            break;
                        }
                        default:
                            throw new SwitchCaseNotImplementedException(scoreEntity.Result);
                    }
                },
                _Seconds: 3f,
                _Ticker: ViewGameTicker));
            OnLevelUnloadedFinishAction(_Args);
        }

        #endregion

        #region nonpublic methods

        private void OnLevelUnloadedFinishAction(LevelStageArgs _Args)
        {
            string gameMode = (string) _Args.Arguments.GetSafe(KeyGameMode, out _);
            switch (gameMode)
            {
                case ParameterGameModePuzzles:
                    OnLevelUnloadedFinishActionGameModePuzzles(_Args);
                    break;
                case ParameterGameModeRandom:
                    OnLevelUnloadedFinishActionGameModeRandom(_Args);
                    break;
                case ParameterGameModeDailyChallenge:
                    OnLevelUnloadedFinishActionGameModeDailyChallenge(_Args);
                    break;
                default:
                {
                    if (SaveUtils.GetValue(SaveKeysRmazor.AllLevelsPassed))
                    {
                        // TODO сделать отдельное сообщение на окончание всех уровней
                        var args = new Dictionary<string, object> {{KeyLevelIndex, 0L}};
                        LevelStageSwitcher.SwitchLevelStage(EInputCommand.LoadLevel, args);
                    }
                    else
                    {
                        var args = new Dictionary<string, object> {{KeyLevelIndex, Model.LevelStaging.LevelIndex + 1}}; 
                        LevelStageSwitcher.SwitchLevelStage(EInputCommand.LoadLevel, args);
                    }
                    break;
                }
            }
        }

        private void OnLevelUnloadedFinishActionGameModePuzzles(LevelStageArgs _Args)
        {
            long levelIndex = Model.LevelStaging.LevelIndex;
            if (!_Args.Arguments.ContainsKey(KeyShowPuzzleLevelHint))
                levelIndex += 1;
            var args = new Dictionary<string, object> {{KeyLevelIndex, levelIndex}}; 
            LevelStageSwitcher.SwitchLevelStage(EInputCommand.LoadLevel, args);
        }

        private void OnLevelUnloadedFinishActionGameModeRandom(LevelStageArgs _Args)
        {
            var args = new Dictionary<string, object>
            {
                {KeyLevelIndex,   0L},
                {KeyAiSimulation, _Args.Arguments[KeyAiSimulation]}
            };
            LevelStageSwitcher.SwitchLevelStage(EInputCommand.LoadLevel, args);
        }

        private void OnLevelUnloadedFinishActionGameModeDailyChallenge(LevelStageArgs _Args)
        {
            int dcIndex = (int)_Args.Arguments.GetSafe(KeyDailyChallengeIndex, out _);
            string dcType = (string) _Args.Arguments.GetSafe(KeyChallengeType, out _);
            bool isDailyChallengeSuccess = (bool) _Args.Arguments.GetSafe(KeyIsDailyChallengeSuccess, out _);
            if (isDailyChallengeSuccess)
            {
                var dailyChallengeInfosFromDisc =
                    SaveUtils.GetValue(SaveKeysRmazor.DailyChallengeInfos);
                var thisDailyChallenge = dailyChallengeInfosFromDisc.FirstOrDefault(
                    _Dc => _Dc.IndexToday == dcIndex 
                           && _Dc.ChallengeType == dcType
                           && _Dc.Day == DateTime.Today);
                if (thisDailyChallenge != null)
                    thisDailyChallenge.Finished = true;
                else Dbg.LogError("Failed to save daily challenge on disk");
                SaveUtils.PutValue(SaveKeysRmazor.DailyChallengeInfos, dailyChallengeInfosFromDisc);
                AddDailyChallengeRewardToBank(_Args);
            }
            var args = new Dictionary<string, object>
            {
                {KeyDailyChallengeIndex,     dcIndex},
                {KeyIsDailyChallengeSuccess, isDailyChallengeSuccess},
                {KeyAdditionalCameraEffectAction, 
                    (UnityAction<bool, float>)AdditionalCameraEffectsActionDefaultCoroutine}
            };
            CommandsProceeder.RaiseCommand(EInputCommand.MainMenuPanel, args);
        }
        
        private void AdditionalCameraEffectsActionDefaultCoroutine(bool _Appear, float _Time)
        {
            Cor.Run(MainMenuUtils.SubPanelsAdditionalCameraEffectsActionCoroutine(_Appear, _Time,
                CameraProvider, UiTicker));
        }

        private void AddDailyChallengeRewardToBank(LevelStageArgs _Args)
        {
            var savedGame = ScoreManager.GetSavedGame(MazorCommonData.SavedGameFileName);
            var bankMoneyCountArg = savedGame.Arguments.GetSafe(KeyMoneyCount, out _);
            long money = Convert.ToInt64(bankMoneyCountArg);
            int rewardMoney = (int) _Args.Arguments.GetSafe(KeyDailyChallengeRewardMoney, out _);
            money += rewardMoney;
            savedGame.Arguments.SetSafe(KeyMoneyCount, money);
            var characterXpArg = savedGame.Arguments.GetSafe(KeyCharacterXp, out _);
            int charXp = Convert.ToInt32(characterXpArg);
            int rewardXp = (int) _Args.Arguments.GetSafe(KeyDailyChallengeRewardXp, out _);
            charXp += rewardXp;
            savedGame.Arguments.SetSafe(KeyCharacterXp, charXp);
            ScoreManager.SaveGame(savedGame);
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