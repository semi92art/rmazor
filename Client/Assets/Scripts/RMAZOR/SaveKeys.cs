using System;
using System.Collections.Generic;
using Common.Entities;
using Common.Utils;
using UnityEngine;

namespace RMAZOR
{
    public static class SaveKeys
    {
        private static SaveKey<bool?>     _disableAds;
        private static SaveKey<bool>      _settingSoundOn;
        private static SaveKey<bool>      _settingMusicOn;

        private static SaveKey<bool>      _settingNotificationsOn;
        
        private static SaveKey<bool>      _allLevelsPassed;
        private static SaveKey<bool>      _settingHapticsOn;
        private static SaveKey<bool>      _gameWasRated;
        private static SaveKey<bool>      _movementTutorialFinished;
        private static SaveKey<bool>      _rotationTutorialFinished;
        private static SaveKey<bool>      _enableRotation;
      
                     
        private static SaveKey<int>       _dailyBonusLastClickedDay; 
        private static SaveKey<int>       _ratePanelShowsCount;
        
        private static SaveKey<DateTime>  _wheelOfFortuneLastDate; 
        private static SaveKey<List<int>> _boughtPurchaseIds;      
        private static SaveKey<DateTime>  _dailyBonusLastDate;     
        

        private static readonly Dictionary<string, SaveKey<uint>> BundleVersions =
            new Dictionary<string, SaveKey<uint>>();

        [RuntimeInitializeOnLoadMethod]
        public static void ResetState()
        {
            SaveUtils.PutValue(DisableAds, SaveUtils.GetValue(DisableAds), true);
            SaveUtils.PutValue(SettingSoundOn, SaveUtils.GetValue(SettingSoundOn), true);
            SaveUtils.PutValue(SettingMusicOn, SaveUtils.GetValue(SettingMusicOn), true);
            SaveUtils.PutValue(SettingNotificationsOn, SaveUtils.GetValue(SettingNotificationsOn), true);
            SaveUtils.PutValue(AllLevelsPassed, SaveUtils.GetValue(AllLevelsPassed), true);
            SaveUtils.PutValue(SettingHapticsOn, SaveUtils.GetValue(SettingHapticsOn), true);
            SaveUtils.PutValue(GameWasRated, SaveUtils.GetValue(GameWasRated), true);
            SaveUtils.PutValue(MovementTutorialFinished, SaveUtils.GetValue(MovementTutorialFinished), true);
            SaveUtils.PutValue(RotationTutorialFinished, SaveUtils.GetValue(RotationTutorialFinished), true);
            SaveUtils.PutValue(EnableRotation, SaveUtils.GetValue(EnableRotation), true);

            SaveUtils.PutValue(DailyBonusLastClickedDay, SaveUtils.GetValue(DailyBonusLastClickedDay), true);
            SaveUtils.PutValue(RatePanelShowsCount, SaveUtils.GetValue(RatePanelShowsCount), true);
            SaveUtils.PutValue(WheelOfFortuneLastDate, SaveUtils.GetValue(WheelOfFortuneLastDate), true);
            SaveUtils.PutValue(BoughtPurchaseIds, SaveUtils.GetValue(BoughtPurchaseIds), true);
            SaveUtils.PutValue(DailyBonusLastDate, SaveUtils.GetValue(DailyBonusLastDate), true);
        }
        
        public static SaveKey<bool?> DisableAds               => _disableAds ??= new SaveKey<bool?>("disable_ads");
        public static SaveKey<bool>  SettingSoundOn           => _settingSoundOn ??= new SaveKey<bool>("sound_on");
        public static SaveKey<bool>  SettingMusicOn           => _settingMusicOn ??= new SaveKey<bool>("music_on");
        
        public static SaveKey<bool>  SettingNotificationsOn   => _settingNotificationsOn ??= new SaveKey<bool>("notifications_on");

        public static SaveKey<bool>  AllLevelsPassed          => _allLevelsPassed ??= new SaveKey<bool>("all_levels_passed");
        public static SaveKey<bool>  SettingHapticsOn         => _settingHapticsOn ??= new SaveKey<bool>("haptics_on");
        public static SaveKey<bool>  GameWasRated             => _gameWasRated ??= new SaveKey<bool>("game_was_rated");
        public static SaveKey<bool>  MovementTutorialFinished => _movementTutorialFinished ??= new SaveKey<bool>("mov_tut_finished");
        public static SaveKey<bool>  RotationTutorialFinished => _rotationTutorialFinished ??= new SaveKey<bool>("rot_tut_finished");
        public static SaveKey<bool>  EnableRotation           => _enableRotation ??= new SaveKey<bool>("enable_rotation");
        


        public static SaveKey<int>  DailyBonusLastClickedDay => _dailyBonusLastClickedDay ??= new SaveKey<int>("daily_bonus_last_day");
        public static SaveKey<int>  RatePanelShowsCount      => _ratePanelShowsCount ??= new SaveKey<int>("rate_panel_shows_count");
        


        public static SaveKey<DateTime>  WheelOfFortuneLastDate => _wheelOfFortuneLastDate ??= new SaveKey<DateTime>("wof_last_date");
        public static SaveKey<List<int>> BoughtPurchaseIds      => _boughtPurchaseIds ??= new SaveKey<List<int>>("bought_purchase_ids");
        public static SaveKey<DateTime>  DailyBonusLastDate     => _dailyBonusLastDate ??= new SaveKey<DateTime>("daily_bonus_last_date");
        


        public static SaveKey<uint> BundleVersion(string _BundleName)
        {
            if (BundleVersions.ContainsKey(_BundleName))
                return BundleVersions[_BundleName];
            var saveKey = new SaveKey<uint>($"bundle_version_{_BundleName}");
            BundleVersions.Add(_BundleName, saveKey);
            return saveKey;
        } 
    }
}