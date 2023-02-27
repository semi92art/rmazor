using Common;
using Common.Entities;
using Common.Managers.PlatformGameServices;
using mazing.common.Runtime.Extensions;
using RMAZOR.Models;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public static class ViewLevelStageControllerUtils
    {
        public static void SaveGame(LevelStageArgs _Args, IScoreManager _ScoreManager)
        {
            var savedGame = _ScoreManager.GetSavedGame(MazorCommonData.SavedGameFileName) ?? new SavedGameV2();
            RmazorUtils.RemoveMethodArgs(_Args.Arguments);
            foreach ((string key, var value) in _Args.Arguments)
                savedGame.Arguments.SetSafe(key, value);
            string gameMode = (string)_Args.Arguments.GetSafe(KeyGameMode, out _);
            if (gameMode == ParameterGameModeDailyChallenge)
                return;
            string levelIndexKey = gameMode switch
            {
                ParameterGameModeMain      => KeyLevelIndexMainLevels,
                ParameterGameModePuzzles   => KeyLevelIndexPuzzleLevels,
                ParameterGameModeBigLevels => KeyLevelIndexBigLevels,
                _                          => KeyLevelIndex
            };
            savedGame.Arguments.SetSafe(levelIndexKey, _Args.LevelIndex);
            _ScoreManager.SaveGame(savedGame);
        }
    }
}