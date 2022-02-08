using System;
using Common.Entities;
using Common.Utils;
using UnityEngine;

namespace RMAZOR
{
    public static class SaveKeysRmazor
    {
        private static SaveKey<bool> _allLevelsPassed;

        private static SaveKey<bool> _movementTutorialFinished;
        private static SaveKey<bool> _rotationTutorialFinished;
        private static SaveKey<bool> _enableRotation;
        private static SaveKey<bool> _darkThemeAvailable;
        private static SaveKey<bool> _moneyFromServerLoadedFirstTime;

        private static SaveKey<int> _dailyBonusLastClickedDay; 
        private static SaveKey<int> _ratePanelShowsCount;
        
        private static SaveKey<DateTime>  _wheelOfFortuneLastDate; 
            
        private static SaveKey<DateTime>  _dailyBonusLastDate;
        
        
        [RuntimeInitializeOnLoadMethod]
        public static void ResetState()
        {
            SaveUtils.PutValue(AllLevelsPassed,          SaveUtils.GetValue(AllLevelsPassed),          true);
            SaveUtils.PutValue(MovementTutorialFinished, SaveUtils.GetValue(MovementTutorialFinished), true);
            SaveUtils.PutValue(RotationTutorialFinished, SaveUtils.GetValue(RotationTutorialFinished), true);
            SaveUtils.PutValue(EnableRotation,           SaveUtils.GetValue(EnableRotation),           true);
            SaveUtils.PutValue(DarkThemeAvailable,       SaveUtils.GetValue(DarkThemeAvailable),       true);
            
            SaveUtils.PutValue(DailyBonusLastClickedDay, SaveUtils.GetValue(DailyBonusLastClickedDay), true);
            SaveUtils.PutValue(RatePanelShowsCount,      SaveUtils.GetValue(RatePanelShowsCount),      true);
            SaveUtils.PutValue(WheelOfFortuneLastDate,   SaveUtils.GetValue(WheelOfFortuneLastDate),   true);
            
            SaveUtils.PutValue(DailyBonusLastDate,       SaveUtils.GetValue(DailyBonusLastDate),       true);
            
            SaveUtils.PutValue(MoneyFromServerLoadedFirstTime, SaveUtils.GetValue(MoneyFromServerLoadedFirstTime), true);
        }
 
        public static SaveKey<bool>  AllLevelsPassed          =>
            _allLevelsPassed ??= new SaveKey<bool>(nameof(AllLevelsPassed));
        public static SaveKey<bool>  MovementTutorialFinished => 
            _movementTutorialFinished ??= new SaveKey<bool>(nameof(MovementTutorialFinished));
        public static SaveKey<bool>  RotationTutorialFinished =>
            _rotationTutorialFinished ??= new SaveKey<bool>(nameof(RotationTutorialFinished));
        public static SaveKey<bool>  EnableRotation           => 
            _enableRotation ??= new SaveKey<bool>(nameof(EnableRotation));
        public static SaveKey<bool>  DarkThemeAvailable       => 
            _darkThemeAvailable ??= new SaveKey<bool>(nameof(DarkThemeAvailable));
        public static SaveKey<bool>  MoneyFromServerLoadedFirstTime =>
            _moneyFromServerLoadedFirstTime ??= new SaveKey<bool>(nameof(MoneyFromServerLoadedFirstTime));
        public static SaveKey<int>  DailyBonusLastClickedDay =>
            _dailyBonusLastClickedDay ??= new SaveKey<int>(nameof(DailyBonusLastClickedDay));
        public static SaveKey<int>  RatePanelShowsCount      =>
            _ratePanelShowsCount ??= new SaveKey<int>(nameof(RatePanelShowsCount));
        public static SaveKey<DateTime>     WheelOfFortuneLastDate =>
            _wheelOfFortuneLastDate ??= new SaveKey<DateTime>(nameof(WheelOfFortuneLastDate));
        public static SaveKey<DateTime>     DailyBonusLastDate     => 
            _dailyBonusLastDate ??= new SaveKey<DateTime>(nameof(DailyBonusLastDate));

    }
}