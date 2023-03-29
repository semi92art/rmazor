using System;
using System.Linq;

namespace ClickersAPI.Helpers
{
    public static class AnalyticIds
    {
        public const string SessionStart       = "session_start";
        public const string LevelReadyToStart  = "level_ready_to_start";
        public const string LevelStarted       = "level_started";
        public const string LevelFinished      = "level_finished";
        public const string LevelStageFinished = "level_stage_finished";
        
        public const string AdShown        = "ad_shown";
        public const string AdClicked      = "ad_clicked";
        public const string AdReward       = "ad_reward";
        public const string AdClosed       = "ad_closed";
        public const string AdFailedToShow = "ad_failed_to_show";
        
        public const string PlayMainLevelsButtonClick     = "play_main_levels_button_click";
        public const string PlayDailyChallengeButtonClick = "play_daily_challenges_button_click";
        public const string PlayRandomLevelsButtonClick   = "play_random_levels_button_click";
        public const string PlayPuzzleLevelsButtonClick   = "play_puzzle_levels_button_click";
        
        private const string ParameterGameMode  = "game_mode";
        private const string ParameterLevelType = "level_type";
        
        private const string ParameterGameModeMain           = "main";
        private const string ParameterGameModeRandom         = "random";
        private const string ParameterGameModeDailyChallenge = "daily_challenge";
        private const string ParameterGameModePuzzles        = "puzzles";
        private const string ParameterGameModeBigLevels      = "big_levels";
        
        private const string ParameterLevelTypeBonus   = "bonus";
        private const string ParameterLevelTypeDefault = "default";

        public static string GetGameParameterValueByAnalyticIdParameterValue(string _AnalyticParameterId, object _AnalyticParameterValue)
        {
            return _AnalyticParameterId switch
            {
                ParameterGameMode  => GetGameModeGameParameterValue(Convert.ToInt32(_AnalyticParameterValue)),
                ParameterLevelType => GetLevelTypeAnalyticParameterValue(Convert.ToInt32(_AnalyticParameterValue)),
                _                  => _AnalyticParameterValue.ToString()
            };
        }
        
        public static string GetLevelFinishedAnalyticId(long _LevelIndex)
        {
            var validLevelsForAnalytic = new long[]
            {
                1, 2, 3, 4, 5, 6, 7, 8, 9,
                10, 20, 30, 40, 50, 60, 70, 80, 90,
                100, 200, 300, 400, 500, 600, 700, 800, 900, 1000
            };
            string anId = validLevelsForAnalytic.Contains(_LevelIndex) ?
                $"level_{_LevelIndex}_finished" : null;
            return anId;
        }
        
        
        private static string GetGameModeGameParameterValue(int _AnalyticParameterValue)
        {
            return _AnalyticParameterValue switch
            {
                1 => ParameterGameModeMain,
                2 => ParameterGameModeDailyChallenge,
                3 => ParameterGameModeRandom,
                4 => ParameterGameModePuzzles,
                5 => ParameterGameModeBigLevels,
                _ => string.Empty
            };
        }

        private static string GetLevelTypeAnalyticParameterValue(int _LevelType)
        {
            return _LevelType switch
            {
                1 => ParameterLevelTypeDefault,
                2 => ParameterLevelTypeBonus,
                _ => string.Empty
            };
        }
    }
}