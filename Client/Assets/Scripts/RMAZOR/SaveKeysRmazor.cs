using System;
using System.Collections.Generic;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Utils;
using RMAZOR.UI.PanelItems.Customoze_Character_Panel_Items;
using RMAZOR.UI.PanelItems.Daily_Challenge_Panel_Items;
using UnityEngine;

namespace RMAZOR
{
    public static class SaveKeysRmazor
    {
        private static Dictionary<string, SaveKey<bool>>   _mazeItemsTutorialsFinished;
        private static SaveKey<bool>                       _allLevelsPassed;
        private static SaveKey<bool>                       _sgFromRemoteLoadedOnce;
        private static SaveKey<int>                        _ratePanelShowsCount;
        private static SaveKey<int>                        _currentLevelGroupMoney;
        private static SaveKey<int>                        _currentLevelGroupXp;
        private static SaveKey<List<long>>                 _levelsFinishedOnce;
        private static SaveKey<List<long>>                 _levelsFinishedOncePuzzles;
        private static SaveKey<Dictionary<DateTime, int>>  _sessionsCountByDays;
        private static SaveKey<Dictionary<long, float>>    _mainLevelTimeRecordsDict;
        private static SaveKey<Dictionary<long, float>>    _bonusLevelTimeRecordsDict;
        private static SaveKey<Dictionary<DateTime, bool>> _dailyRewardGot;
        private static SaveKey<List<DailyChallengeInfo>>   _dailyChallengeInfos;
        private static SaveKey<string>                     _characterIdV2;
        private static SaveKey<string>                     _characterColorSetIdV2;
        private static SaveKey<bool>                       _retroModeOn;
        private static SaveKey<bool>                       _mainGameModeLoadedAtLeastOnce;
        private static SaveKey<List<string>>               _idsOfBoughtCharacters;
        private static SaveKey<List<string>>               _idsOfBoughtColorSets;
        private static SaveKey<TimeSpan>                   _specialOfferTimePassedInMinutes;
        private static SaveKey<bool>                       _retroModeUnlocked; 
        private static SaveKey<bool>                       _fullCustomizationUnlocked;
        private static SaveKey<float>                      _multiplyNewCoinsCoefficient;

        private static SaveKey<Dictionary<string, List<Badge>>> _tabBadgesDict;

        private static SaveKey<bool> _mainMenuButtonDailyChallengeBadgeNewMustBeHidden;
        private static SaveKey<bool> _mainMenuButtonRandomBadgeNewMustBeHidden;
        private static SaveKey<bool> _mainMenuButtonPuzzlesBadgeNewMustBeHidden;

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
        public static SaveKey<int> CurrentLevelGroupMoney    =>
            _currentLevelGroupMoney ??= new SaveKey<int>(nameof(CurrentLevelGroupMoney));
        public static SaveKey<int> CurrentLevelGroupXp =>
            _currentLevelGroupXp ??= new SaveKey<int>(nameof(CurrentLevelGroupXp));
        public static SaveKey<List<long>> LevelsFinishedOnce  =>
            _levelsFinishedOnce ??= new SaveKey<List<long>>(nameof(LevelsFinishedOnce));
        public static SaveKey<List<long>> LevelsFinishedOncePuzzles =>
            _levelsFinishedOncePuzzles ??= new SaveKey<List<long>>(nameof(LevelsFinishedOncePuzzles));
        public static SaveKey<Dictionary<DateTime, int>> SessionCountByDays =>
            _sessionsCountByDays ??= new SaveKey<Dictionary<DateTime, int>>(nameof(SessionCountByDays));
        public static SaveKey<Dictionary<long, float>> MainLevelTimeRecordsDict =>
            _mainLevelTimeRecordsDict ??= new SaveKey<Dictionary<long, float>>(nameof(MainLevelTimeRecordsDict));
        public static SaveKey<Dictionary<long, float>> BonusLevelTimeRecordsDict =>
            _bonusLevelTimeRecordsDict ??= new SaveKey<Dictionary<long, float>>(nameof(BonusLevelTimeRecordsDict));
        public static SaveKey<Dictionary<DateTime, bool>> DailyRewardGot =>
            _dailyRewardGot ??= new SaveKey<Dictionary<DateTime, bool>>(nameof(DailyRewardGot));
        public static SaveKey<List<DailyChallengeInfo>> DailyChallengeInfos =>
            _dailyChallengeInfos ??= new SaveKey<List<DailyChallengeInfo>>(nameof(DailyChallengeInfos));
        public static SaveKey<string> CharacterIdV2 =>
            _characterIdV2 ??= new SaveKey<string>(nameof(CharacterIdV2));
        public static SaveKey<string> CharacterColorSetIdV2 =>
            _characterColorSetIdV2 ??= new SaveKey<string>(nameof(CharacterColorSetIdV2));
        public static SaveKey<bool> RetroModeOn =>
            _retroModeOn ??= new SaveKey<bool>(nameof(RetroModeOn));

        public static SaveKey<bool> MainMenuButtonDailyChallengeBadgeNewMustBeHidden =>
            _mainMenuButtonDailyChallengeBadgeNewMustBeHidden ??=
                new SaveKey<bool>(nameof(MainMenuButtonDailyChallengeBadgeNewMustBeHidden));
        public static SaveKey<bool> MainMenuButtonRandomBadgeNewMustBeHidden =>
            _mainMenuButtonRandomBadgeNewMustBeHidden ??=
                new SaveKey<bool>(nameof(MainMenuButtonRandomBadgeNewMustBeHidden));
        public static SaveKey<bool> MainMenuButtonPuzzlesBadgeNewMustBeHidden =>
            _mainMenuButtonPuzzlesBadgeNewMustBeHidden ??=
                new SaveKey<bool>(nameof(MainMenuButtonPuzzlesBadgeNewMustBeHidden));

        public static SaveKey<Dictionary<string, List<Badge>>> TabBadgesDict => 
            _tabBadgesDict ??= new SaveKey<Dictionary<string, List<Badge>>>(nameof(TabBadgesDict));
        public static SaveKey<bool> MainGameModeLoadedAtLeastOnce =>
            _mainGameModeLoadedAtLeastOnce ??= new SaveKey<bool>(nameof(MainGameModeLoadedAtLeastOnce));

        public static SaveKey<List<string>> IdsOfBoughtCharacters =>
            _idsOfBoughtCharacters ??= new SaveKey<List<string>>(nameof(IdsOfBoughtCharacters));
        public static SaveKey<List<string>> IdsOfBoughtColorSets =>
            _idsOfBoughtColorSets ??= new SaveKey<List<string>>(nameof(IdsOfBoughtColorSets));

        public static SaveKey<TimeSpan> SpecialOfferTimePassedInMinutes =>
            _specialOfferTimePassedInMinutes ??= new SaveKey<TimeSpan>(nameof(SpecialOfferTimePassedInMinutes));

        public static SaveKey<bool> RetroModeUnlocked =>
            _retroModeUnlocked ?? new SaveKey<bool>(nameof(RetroModeUnlocked));
        public static SaveKey<bool> FullCustomizationUnlocked =>
            _fullCustomizationUnlocked ?? new SaveKey<bool>(nameof(FullCustomizationUnlocked));

        public static SaveKey<float> MultiplyNewCoinsCoefficient =>
            _multiplyNewCoinsCoefficient ?? new SaveKey<float>(nameof(MultiplyNewCoinsCoefficient));

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
            _levelsFinishedOncePuzzles  = null;
            _sessionsCountByDays        = null;
            _mainLevelTimeRecordsDict   = null;
            _bonusLevelTimeRecordsDict  = null;
            _dailyChallengeInfos        = null;
            _characterIdV2              = null;
            _characterColorSetIdV2      = null;
            _retroModeOn                = null;
            _tabBadgesDict              = null;
            _idsOfBoughtCharacters      = null;
            _idsOfBoughtColorSets       = null;
            _retroModeUnlocked          = null;
            _fullCustomizationUnlocked  = null;
            
            _specialOfferTimePassedInMinutes                  = null;
            _mainGameModeLoadedAtLeastOnce                    = null;
            _mainMenuButtonDailyChallengeBadgeNewMustBeHidden = null; 
            _mainMenuButtonRandomBadgeNewMustBeHidden         = null;
            _mainMenuButtonPuzzlesBadgeNewMustBeHidden        = null;
        }

        private static void CacheFromDisc()
        {
            const bool onlyCache = true;
            SaveUtils.PutValue(AllLevelsPassed,          SaveUtils.GetValue(AllLevelsPassed),          onlyCache);
            SaveUtils.PutValue(RatePanelShowsCount,      SaveUtils.GetValue(RatePanelShowsCount),      onlyCache);
            SaveUtils.PutValue(SgFromRemoteLoadedOnce,   SaveUtils.GetValue(SgFromRemoteLoadedOnce),   onlyCache);
            SaveUtils.PutValue(CurrentLevelGroupMoney,   SaveUtils.GetValue(CurrentLevelGroupMoney),   onlyCache);
            SaveUtils.PutValue(LevelsFinishedOnce,       SaveUtils.GetValue(LevelsFinishedOnce),       onlyCache);
            SaveUtils.PutValue(LevelsFinishedOncePuzzles,SaveUtils.GetValue(LevelsFinishedOncePuzzles),onlyCache);
            SaveUtils.PutValue(SessionCountByDays,       SaveUtils.GetValue(SessionCountByDays),       onlyCache);
            SaveUtils.PutValue(MainLevelTimeRecordsDict, SaveUtils.GetValue(MainLevelTimeRecordsDict), onlyCache);
            SaveUtils.PutValue(BonusLevelTimeRecordsDict,SaveUtils.GetValue(BonusLevelTimeRecordsDict),onlyCache);
            SaveUtils.PutValue(DailyRewardGot,           SaveUtils.GetValue(DailyRewardGot),           onlyCache);
            SaveUtils.PutValue(DailyChallengeInfos,      SaveUtils.GetValue(DailyChallengeInfos),      onlyCache);
            SaveUtils.PutValue(CharacterColorSetIdV2,    SaveUtils.GetValue(CharacterColorSetIdV2),    onlyCache);
            SaveUtils.PutValue(CharacterIdV2,            SaveUtils.GetValue(CharacterIdV2),            onlyCache);
            SaveUtils.PutValue(RetroModeOn,              SaveUtils.GetValue(RetroModeOn),              onlyCache);
            SaveUtils.PutValue(TabBadgesDict,            SaveUtils.GetValue(TabBadgesDict),            onlyCache);
            SaveUtils.PutValue(IdsOfBoughtCharacters,    SaveUtils.GetValue(IdsOfBoughtCharacters),    onlyCache);
            SaveUtils.PutValue(IdsOfBoughtColorSets,     SaveUtils.GetValue(IdsOfBoughtColorSets),     onlyCache);
            SaveUtils.PutValue(RetroModeUnlocked,        SaveUtils.GetValue(RetroModeUnlocked),        onlyCache);
            SaveUtils.PutValue(FullCustomizationUnlocked,SaveUtils.GetValue(FullCustomizationUnlocked),onlyCache);
            
            SaveUtils.PutValue(MultiplyNewCoinsCoefficient,
                SaveUtils.GetValue(MultiplyNewCoinsCoefficient),
                onlyCache);
            SaveUtils.PutValue(SpecialOfferTimePassedInMinutes,   
                SaveUtils.GetValue(SpecialOfferTimePassedInMinutes),  
                onlyCache);
            SaveUtils.PutValue(MainGameModeLoadedAtLeastOnce,
                SaveUtils.GetValue(MainGameModeLoadedAtLeastOnce), 
                onlyCache);
            SaveUtils.PutValue(MainMenuButtonDailyChallengeBadgeNewMustBeHidden,
                SaveUtils.GetValue(MainMenuButtonDailyChallengeBadgeNewMustBeHidden),      
                onlyCache);
            SaveUtils.PutValue(
                MainMenuButtonRandomBadgeNewMustBeHidden,         
                SaveUtils.GetValue(MainMenuButtonRandomBadgeNewMustBeHidden), 
                onlyCache);
            SaveUtils.PutValue(MainMenuButtonPuzzlesBadgeNewMustBeHidden,     
                SaveUtils.GetValue(MainMenuButtonPuzzlesBadgeNewMustBeHidden),       
                onlyCache);
            
            _mazeItemsTutorialsFinished = new Dictionary<string, SaveKey<bool>>();
            foreach (string tutorialName in GetAllTutorialNames())
            {
                var saveKey = IsTutorialFinished(tutorialName);
                SaveUtils.PutValue(saveKey, SaveUtils.GetValue(saveKey), onlyCache);
                _mazeItemsTutorialsFinished.SetSafe(tutorialName, saveKey);
            }
            
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
                "key_lock",
                "main_menu"
            };
        }

    }
}