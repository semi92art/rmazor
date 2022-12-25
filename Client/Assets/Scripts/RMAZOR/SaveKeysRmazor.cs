using System;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Extensions;
using Common.Utils;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Utils;
using UnityEngine;

namespace RMAZOR
{
    public static class SaveKeysRmazor
    {
        private static Dictionary<string, SaveKey<bool>> _mazeItemsTutorialsFinished;

        private static SaveKey<bool>                       _allLevelsPassed;
        private static SaveKey<bool>                       _sgFromRemoteLoadedOnce;
        private static SaveKey<int>                        _ratePanelShowsCount;
        private static SaveKey<long>                       _currentLevelGroupMoney;
        private static SaveKey<List<long>>                 _levelsFinishedOnce;
        private static SaveKey<Dictionary<DateTime, int>>  _sessionsCountByDays;
        private static SaveKey<Dictionary<long, float>>    _mainLevelTimeRecordsDict;
        private static SaveKey<Dictionary<long, float>>    _bonusLevelTimeRecordsDict;
        private static SaveKey<Dictionary<DateTime, bool>> _dailyRewardGot;

        [RuntimeInitializeOnLoadMethod]
        public static void ResetState()
        {
            SetAllToNull();
            CacheFromDisc();
        }
 
        public static SaveKey<bool>  AllLevelsPassed          =>
            _allLevelsPassed ??= new SaveKey<bool>(nameof(AllLevelsPassed));
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
        public static SaveKey<Dictionary<DateTime, bool>> DailyRewardGot =>
            _dailyRewardGot ??= new SaveKey<Dictionary<DateTime, bool>>(nameof(DailyRewardGot));

        public static SaveKey<bool> IsTutorialFinished(string _TutorialName)
        {
            _mazeItemsTutorialsFinished ??= new Dictionary<string, SaveKey<bool>>();
            var saveKey = _mazeItemsTutorialsFinished.GetSafe(_TutorialName, out bool containsKey);
            if (containsKey && saveKey != null)
                return saveKey;
            saveKey = new SaveKey<bool>($"tutorial_{_TutorialName}");
            _mazeItemsTutorialsFinished.SetSafe(_TutorialName, saveKey);
            return saveKey;
        }

        private static void SetAllToNull()
        {
            _mazeItemsTutorialsFinished = null;
            _allLevelsPassed            = null;
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
            SaveUtils.PutValue(RatePanelShowsCount,      SaveUtils.GetValue(RatePanelShowsCount),      onlyCache);
            SaveUtils.PutValue(SgFromRemoteLoadedOnce,   SaveUtils.GetValue(SgFromRemoteLoadedOnce),   onlyCache);
            SaveUtils.PutValue(CurrentLevelGroupMoney,   SaveUtils.GetValue(CurrentLevelGroupMoney),   onlyCache);
            SaveUtils.PutValue(LevelsFinishedOnce,       SaveUtils.GetValue(LevelsFinishedOnce),       onlyCache);
            SaveUtils.PutValue(SessionCountByDays,       SaveUtils.GetValue(SessionCountByDays),       onlyCache);
            SaveUtils.PutValue(MainLevelTimeRecordsDict, SaveUtils.GetValue(MainLevelTimeRecordsDict), onlyCache);
            SaveUtils.PutValue(BonusLevelTimeRecordsDict,SaveUtils.GetValue(BonusLevelTimeRecordsDict),onlyCache);
            SaveUtils.PutValue(DailyRewardGot,           SaveUtils.GetValue(DailyRewardGot),           onlyCache);
           
            foreach (var saveKey in GetAllTutorialNames().Select(IsTutorialFinished))
                SaveUtils.PutValue(saveKey, SaveUtils.GetValue(saveKey), onlyCache);
        }
        
        public static IEnumerable<string> GetAllTutorialNames()
        {
            return new List<string>
            {
                "movement",
                "timer",
                "gravity_block",
                "shredinger",
                "portal",
                "trap_react",
                "trap_increasing",
                "trap_moving",
                "gravity_trap",
                "turret",
                "gravity_block_free",
                "springboard",
                "hammer",
                "spear",
                "diode",
                "key_lock"
            };
        }

    }
}