using System;
using System.Collections.Generic;
using mazing.common.Runtime.Extensions;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.Views.Common.ViewLevelStageSwitchers
{
    public static class ViewLevelStageSwitcherUtils
    {
        public static string GetGameMode(Dictionary<string, object> _Arguments)
        {
            string gameMode = (string) _Arguments.GetSafe(KeyGameMode, out _);
            return gameMode;
        }
        
        public static string GetCurrentLevelType(Dictionary<string, object> _Arguments)
        {
            string currentLevelType = (string)_Arguments.GetSafe(
                KeyCurrentLevelType, out _);
            return currentLevelType;
        }
        
        public static string GetNextLevelType(Dictionary<string, object> _Arguments)
        {
            string nextLevelType = (string)_Arguments.GetSafe(
                KeyNextLevelType, out _);
            return nextLevelType;
        }

        public static long GetLevelIndex(Dictionary<string, object> _Arguments)
        {
            object nextLevelArg = _Arguments.GetSafe(KeyLevelIndex, out bool keyExist);
            return keyExist ? Convert.ToInt64(nextLevelArg) : -1;
        }

        public static void SetLevelIndex(Dictionary<string, object> _Arguments, long _LevelIndex)
        {
            _Arguments.SetSafe(KeyLevelIndex, _LevelIndex);
        }

        public static long GetNextLevelOfBonusTypeAfterLevelOfDefaultTypeIndex(long _CurrentMainLevelIndex, int _ExtraLevelEveryNStage)
        {
            long levelsGroupIndex = RmazorUtils.GetLevelsGroupIndex(_CurrentMainLevelIndex);
            return (levelsGroupIndex - 1) / _ExtraLevelEveryNStage;
        }

        public static long GetNExtLevelOfDefaultTypeAfterLEvelOfBonusTypeIndex(long _CurrentBonusLevelIndex, int _ExtraLevelEveryNStage)
        {
            long levelsGroupIndex = _CurrentBonusLevelIndex * _ExtraLevelEveryNStage + 1;
            return RmazorUtils.GetFirstLevelInGroupIndex((int)levelsGroupIndex) +
                   RmazorUtils.GetLevelsInGroup((int)levelsGroupIndex);
        }
    }
}