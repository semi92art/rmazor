using System;

namespace ClickersAPI.Helpers
{
    public static class AnalyticIds
    {
        public const string ParameterGameMode  = "game_mode";
        public const string ParameterLevelType = "level_type";
        
        public const string ParameterGameModeMain           = "main";
        public const string ParameterGameModeRandom         = "random";
        public const string ParameterGameModeDailyChallenge = "daily_challenge";
        public const string ParameterGameModePuzzles        = "puzzles";
        public const string ParameterGameModeBigLevels      = "big_levels";
        
        public const string ParameterLevelTypeBonus   = "bonus";
        public const string ParameterLevelTypeDefault = "default";

        public static string GetGameParameterValueByAnalyticIdParameterValue(string _AnalyticParameterId, object _AnalyticParameterValue)
        {
            return _AnalyticParameterId switch
            {
                ParameterGameMode  => GetGameModeGameParameterValue(Convert.ToInt32(_AnalyticParameterValue)),
                ParameterLevelType => GetLevelTypeAnalyticParameterValue(Convert.ToInt32(_AnalyticParameterValue)),
                _                  => _AnalyticParameterValue.ToString()
            };
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