using System;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Extensions;
using Common.Utils;
using RMAZOR.Models.MazeInfos;
using UnityEngine;

namespace RMAZOR
{
    public static class SaveKeysRmazor
    {
        private static Dictionary<EMazeItemType, SaveKey<bool>> _mazeItemsTutorialsFinished;

        private static SaveKey<bool>                      _allLevelsPassed;
        private static SaveKey<bool>                      _movementTutorialFinished;
        private static SaveKey<bool>                      _sgFromRemoteLoadedOnce;
        private static SaveKey<int>                       _ratePanelShowsCount;
        private static SaveKey<long>                      _currentLevelGroupMoney;
        private static SaveKey<List<long>>                _levelsFinishedOnce;
        private static SaveKey<Dictionary<DateTime, int>> _sessionsCountByDays;
        private static SaveKey<Dictionary<long, float>>   _mainLevelTimeRecordsDict;
        private static SaveKey<Dictionary<long, float>>   _bonusLevelTimeRecordsDict;

        [RuntimeInitializeOnLoadMethod]
        public static void ResetState()
        {
            SetAllToNull();
            CacheFromDisc();
        }
 
        public static SaveKey<bool>  AllLevelsPassed          =>
            _allLevelsPassed ??= new SaveKey<bool>(nameof(AllLevelsPassed));
        public static SaveKey<bool>  MovementTutorialFinished => 
            _movementTutorialFinished ??= new SaveKey<bool>(nameof(MovementTutorialFinished));
        public static SaveKey<bool>  SgFromRemoteLoadedOnce   =>
            _sgFromRemoteLoadedOnce ??= new SaveKey<bool>(nameof(SgFromRemoteLoadedOnce));
        public static SaveKey<int>  RatePanelShowsCount       =>
            _ratePanelShowsCount ??= new SaveKey<int>(nameof(RatePanelShowsCount));
        public static SaveKey<long> CurrentLevelGroupMoney    =>
            _currentLevelGroupMoney ??= new SaveKey<long>(nameof(CurrentLevelGroupMoney));
        public static SaveKey<List<long>> LevelsFinishedOnce  =>
            _levelsFinishedOnce ??= new SaveKey<List<long>>(nameof(LevelsFinishedOnce));
        public static SaveKey<Dictionary<DateTime, int>> SessionCountByDays =>
            _sessionsCountByDays ??= new SaveKey<Dictionary<DateTime, int>>(nameof(SessionCountByDays));
        public static SaveKey<Dictionary<long, float>> MainLevelTimeRecordsDict =>
            _mainLevelTimeRecordsDict ??= new SaveKey<Dictionary<long, float>>(nameof(MainLevelTimeRecordsDict));
        public static SaveKey<Dictionary<long, float>> BonusLevelTimeRecordsDict =>
            _bonusLevelTimeRecordsDict ??= new SaveKey<Dictionary<long, float>>(nameof(BonusLevelTimeRecordsDict));

        public static SaveKey<bool> GetMazeItemTutorialFinished(EMazeItemType _Type)
        {
            _mazeItemsTutorialsFinished ??= new Dictionary<EMazeItemType, SaveKey<bool>>();
            var saveKey = _mazeItemsTutorialsFinished.GetSafe(_Type, out bool containsKey);
            if (containsKey && saveKey != null)
                return saveKey;
            saveKey = new SaveKey<bool>($"MazeItemTutorialFinished_{_Type}");
            _mazeItemsTutorialsFinished.SetSafe(_Type, saveKey);
            return saveKey;
        }

        private static void SetAllToNull()
        {
            _mazeItemsTutorialsFinished = null;
            _allLevelsPassed            = null;
            _movementTutorialFinished   = null;
            _sgFromRemoteLoadedOnce     = null;
            _ratePanelShowsCount        = null;
            _currentLevelGroupMoney     = null;
            _levelsFinishedOnce         = null;
            _sessionsCountByDays        = null;
            _mainLevelTimeRecordsDict   = null;
            _bonusLevelTimeRecordsDict  = null;
        }

        private static void CacheFromDisc()
        {
            const bool onlyCache = true;
            SaveUtils.PutValue(AllLevelsPassed,          SaveUtils.GetValue(AllLevelsPassed),          onlyCache);
            SaveUtils.PutValue(MovementTutorialFinished, SaveUtils.GetValue(MovementTutorialFinished), onlyCache);
            SaveUtils.PutValue(RatePanelShowsCount,      SaveUtils.GetValue(RatePanelShowsCount),      onlyCache);
            SaveUtils.PutValue(SgFromRemoteLoadedOnce,   SaveUtils.GetValue(SgFromRemoteLoadedOnce),   onlyCache);
            SaveUtils.PutValue(CurrentLevelGroupMoney,   SaveUtils.GetValue(CurrentLevelGroupMoney),   onlyCache);
            SaveUtils.PutValue(LevelsFinishedOnce,       SaveUtils.GetValue(LevelsFinishedOnce),       onlyCache);
            // SaveUtils.PutValue(SessionCountByDays,       SaveUtils.GetValue(SessionCountByDays),       onlyCache);
            // SaveUtils.PutValue(LevelTimeRecordsDict,     SaveUtils.GetValue(LevelTimeRecordsDict),     onlyCache);
           
            var mazeItemTypes = Enum.GetValues(typeof(EMazeItemType)).Cast<EMazeItemType>().ToArray();
            foreach (var mazeItemType in mazeItemTypes)
            {
                var saveKey = GetMazeItemTutorialFinished(mazeItemType);
                SaveUtils.PutValue(saveKey, SaveUtils.GetValue(saveKey), onlyCache);
            }
        }
    }
}