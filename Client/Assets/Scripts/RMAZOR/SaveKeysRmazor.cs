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

        private static SaveKey<bool>       _allLevelsPassed;
        private static SaveKey<bool>       _movementTutorialFinished;
        private static SaveKey<bool>       _sgFromRemoteLoadedOnce;
        private static SaveKey<int>        _ratePanelShowsCount;
        private static SaveKey<long>       _currentLevelGroupMoney;
        private static SaveKey<List<long>> _levelsFinishedOnce;

        [RuntimeInitializeOnLoadMethod]
        public static void ResetState()
        {
            if (Application.isEditor)
            {
                _mazeItemsTutorialsFinished = null;
                _allLevelsPassed            = null;
                _movementTutorialFinished   = null;
                _sgFromRemoteLoadedOnce     = null;
                _ratePanelShowsCount        = null;
                _currentLevelGroupMoney     = null;
                _levelsFinishedOnce         = null;
            }

            const bool onlyCache = true;
            SaveUtils.PutValue(AllLevelsPassed,          SaveUtils.GetValue(AllLevelsPassed),          onlyCache);
            SaveUtils.PutValue(MovementTutorialFinished, SaveUtils.GetValue(MovementTutorialFinished), onlyCache);
            SaveUtils.PutValue(RatePanelShowsCount,      SaveUtils.GetValue(RatePanelShowsCount),      onlyCache);
            SaveUtils.PutValue(SgFromRemoteLoadedOnce,   SaveUtils.GetValue(SgFromRemoteLoadedOnce),   onlyCache);
            SaveUtils.PutValue(CurrentLevelGroupMoney,   SaveUtils.GetValue(CurrentLevelGroupMoney),   onlyCache);
            SaveUtils.PutValue(LevelsFinishedOnce,       SaveUtils.GetValue(LevelsFinishedOnce),       onlyCache);
            
            var mazeItemTypes = Enum.GetValues(typeof(EMazeItemType)).Cast<EMazeItemType>().ToArray();
            foreach (var mazeItemType in mazeItemTypes)
            {
                var saveKey = GetMazeItemTutorialFinished(mazeItemType);
                SaveUtils.PutValue(saveKey, SaveUtils.GetValue(saveKey), true);
            }
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
    }
}