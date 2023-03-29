using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Common.Constants;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.UI.PanelItems.Daily_Challenge_Panel_Items;
using RMAZOR.UI.Utils;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using static RMAZOR.Models.ComInComArg;

namespace RMAZOR.UI.Panels
{
    public interface IDailyChallengePanel : IDialogPanel
    {
        UnityAction OnReadyToLoadLevelAction { set; }
        int         GetChallengesCountTodayTotal();
        int         GetChallengesCountTodayFinished();
    }
    
    public class DailyChallengePanelFake : DialogPanelFake, IDailyChallengePanel
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public UnityAction OnReadyToLoadLevelAction          { get; set; }
        public int         GetChallengesCountTodayTotal()    => default;
        public int         GetChallengesCountTodayFinished() => default;
    }

    public class DailyChallengePanel : DialogPanelBase, IDailyChallengePanel
    {
        #region constants

        private const int DailyChallengesCountTimer = 1;
        private const int DailyChallengesCountSteps = 2;

        #endregion
        
        #region nonpublic members

        private TextMeshProUGUI m_TitleText;
        private Button          m_ButtonClose;
        
        private readonly List<DailyChallengePanelItemBase> m_DailyChallengeItems 
            = new List<DailyChallengePanelItemBase>();
        private readonly List<DailyChallengeInfo> m_DailyChallengeInfos
            = new List<DailyChallengeInfo>();

        protected override string PrefabName => "daily_challenge_panel";

        #endregion
        
        #region inject

        private ViewSettings                ViewSettings           { get; }
        private IViewFullscreenTransitioner FullscreenTransitioner { get; }
        private ILevelsLoader               LevelsLoader           { get; }
        private IViewLevelStageSwitcher     LevelStageSwitcher     { get; }
        private IViewBetweenLevelAdShower   BetweenLevelAdShower   { get; }

        private DailyChallengePanel(
            ViewSettings                _ViewSettings,
            IViewFullscreenTransitioner _FullscreenTransitioner,
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewTimePauser             _TimePauser,
            IViewInputCommandsProceeder _CommandsProceeder,
            ILevelsLoader               _LevelsLoader,
            IViewLevelStageSwitcher     _LevelStageSwitcher,
            IViewBetweenLevelAdShower   _BetweenLevelAdShower) 
            : base(
                _Managers, 
                _Ticker,
                _CameraProvider,
                _ColorProvider,
                _TimePauser, 
                _CommandsProceeder)
        {
            ViewSettings           = _ViewSettings;
            FullscreenTransitioner = _FullscreenTransitioner;
            LevelsLoader           = _LevelsLoader;
            LevelStageSwitcher     = _LevelStageSwitcher;
            BetweenLevelAdShower   = _BetweenLevelAdShower;
        }

        #endregion

        #region api

        public override int DialogViewerId => DialogViewerIdsCommon.FullscreenCommon;

        public UnityAction OnReadyToLoadLevelAction { private get; set; }

        public int GetChallengesCountTodayTotal()
        {
            return DailyChallengesCountTimer + DailyChallengesCountSteps;
        }

        public int GetChallengesCountTodayFinished()
        {
            GetDailyChallengeInfosFromDisc(
                out _,
                out var challengesTodayAll,
                out _,
                ParameterChallengeTypeTimer);
            return challengesTodayAll.Count(_C => _C.Finished);
        }

        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            InitLevelInfosTimer();
            InitLevelInfosSteps();
            InitPanelItems();
        }

        #endregion

        #region nonpublic methods
        
        protected override void OnDialogStartAppearing()
        {
            base.OnDialogStartAppearing();
            CheckIfDailyChallengesCompleted();
        }

        private void OnButtonCloseClick()
        {
            PlayButtonClickSound();
            OnClose();
        }

        private void OnDailyChallengeItemClick(int _Index, string _ChallengeType)
        {
            PlayButtonClickSound();
            SendAnalyticOnDailyChallengeItemClick(_ChallengeType);
            BetweenLevelAdShower.TryShowAd(() =>
            {
                var infos = m_DailyChallengeInfos
                    .Where(_Info => _Info.ChallengeType == _ChallengeType)
                    .ToList();
                LoadDailyChallengeLevel(infos[_Index]);
            }, EAdvertisingType.Interstitial);
        }

        private void SendAnalyticOnDailyChallengeItemClick(string _ChallengeType)
        {
            string analyticId = _ChallengeType switch
            {
                ParameterChallengeTypeTimer => AnalyticIdsRmazor.DailyChallengeTimerItemClick,
                ParameterChallengeTypeSteps => AnalyticIdsRmazor.DailyChallengeStepsItemClick,
                _                           => null
            };
            if (!string.IsNullOrEmpty(analyticId))
                Managers.AnalyticsManager.SendAnalytic(analyticId);
        }
        
        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            m_TitleText   = _Go.GetCompItem<TextMeshProUGUI>("title_text");
            m_ButtonClose = _Go.GetCompItem<Button>("button_close");
            for (int i = 1; i <= DailyChallengesCountTimer; i++)
            {
                var dailyChallengeItem = _Go.GetCompItem<DailyChallengePanelItemTimer>(
                    $"daily_challenge_item_timer_{i}");
                m_DailyChallengeItems.Add(dailyChallengeItem);
            }
            for (int i = 1; i <= DailyChallengesCountSteps; i++)
            {
                var dailyChallengeItem = _Go.GetCompItem<DailyChallengePanelItemSteps>(
                    $"daily_challenge_item_steps_{i}");
                m_DailyChallengeItems.Add(dailyChallengeItem);
            }
        }

        protected override void LocalizeTextObjectsOnLoad()
        {
            var locInfos = new[]
            {
                new LocTextInfo(m_TitleText, ETextType.MenuUI_H1, "daily_challenges",
                    _T => _T.ToUpper(CultureInfo.CurrentUICulture)),
            };
            foreach (var locInfo in locInfos)
                Managers.LocalizationManager.AddLocalization(locInfo);
        }

        protected override void SubscribeButtonEvents()
        {
            m_ButtonClose.onClick.AddListener(OnButtonCloseClick);
        }

        private void InitLevelInfosTimer()
        {
            InitLevelInfos(
                ParameterChallengeTypeTimer,
                DailyChallengesCountTimer,
                _Idx => 60 - _Idx * 15,
                _Idx => 400 + _Idx * 200,
                _Idx => 500 + _Idx * 250);
        }

        private void InitLevelInfosSteps()
        {
            InitLevelInfos(
                ParameterChallengeTypeSteps,
                DailyChallengesCountSteps,
                _Idx => 40 - _Idx * 5,
                _Idx => 300 + _Idx * 300,
                _Idx => 500 + _Idx * 250);
        }

        private void InitLevelInfos(
            string         _Type,
            int            _Count,
            Func<int, int> _GetGoal,
            Func<int, int> _GetRewardMoney,
            Func<int, int> _GetRewardXp)
        {
            GetDailyChallengeInfosFromDisc(
                out var dailyChallengeInfosFromDisc,
                out _,
                out var todaysChallengesByType,
                _Type);
            todaysChallengesByType = todaysChallengesByType
                .OrderBy(_Dc => _Dc.IndexToday)
                .ToList();
            if (!todaysChallengesByType.Any())
            {
                for (int i = 0; i < _Count; i++)
                {
                    var info = new DailyChallengeInfo
                    {
                        ChallengeType  = _Type,
                        IndexToday     = i,
                        LevelIndex     = GetRandomDailyChallengeLevelIndex(),
                        Goal           = _GetGoal(i),
                        RewardMoney    = _GetRewardMoney(i),
                        RewardXp       = _GetRewardXp(i),
                        Day            = DateTime.Today,
                        Finished       = false
                    };
                    todaysChallengesByType.Add(info);
                }
                dailyChallengeInfosFromDisc.AddRange(todaysChallengesByType);
                SaveUtils.PutValue(SaveKeysRmazor.DailyChallengeInfos, dailyChallengeInfosFromDisc);
            }
            foreach (var todaysChallenge in todaysChallengesByType)
                m_DailyChallengeInfos.Add(todaysChallenge);
        }

        private static void GetDailyChallengeInfosFromDisc(
            out List<DailyChallengeInfo> _ChallengesEverAll,
            out List<DailyChallengeInfo> _ChallengesTodayAll,
            out List<DailyChallengeInfo> _ChallengesTodayByType,
            string _Type)
        {
            _ChallengesEverAll =
                SaveUtils.GetValue(SaveKeysRmazor.DailyChallengeInfos) ?? new List<DailyChallengeInfo>();
            _ChallengesTodayAll = _ChallengesEverAll
                .Where(_Dc => _Dc.Day == DateTime.Today)
                .ToList();
            _ChallengesTodayByType = string.IsNullOrEmpty(_Type)
                ? null
                : _ChallengesTodayAll
                    .Where(_Dc => _Dc.ChallengeType == _Type)
                    .ToList();
        }

        private void CheckIfDailyChallengesCompleted()
        {
            var dailyChallengeInfosFromDisc =
                SaveUtils.GetValue(SaveKeysRmazor.DailyChallengeInfos) ?? new List<DailyChallengeInfo>();
            var dailyChallengesCompletedToday = 
                dailyChallengeInfosFromDisc
                    .Where(_Dc => _Dc.Day == DateTime.Today && _Dc.Finished)
                    .ToList();
            foreach (var dailyChallenge in dailyChallengesCompletedToday)
            {
                m_DailyChallengeItems.FirstOrDefault(_Item =>
                {
                    var itemInfo = _Item.Arguments.Info;
                    return dailyChallenge.ChallengeType == itemInfo.ChallengeType
                           && dailyChallenge.IndexToday == itemInfo.IndexToday;

                })?.CompleteChallenge();    
            }
        }
        
        private void InitPanelItems()
        {
            InitPanelItems(ParameterChallengeTypeTimer);
            InitPanelItems(ParameterChallengeTypeSteps);
        }

        private void InitPanelItems(string _ChallengeType)
        {
            var prefMan = Managers.PrefabSetManager;
            var classType = _ChallengeType switch
            {
                ParameterChallengeTypeTimer => typeof(DailyChallengePanelItemTimer),
                ParameterChallengeTypeSteps => typeof(DailyChallengePanelItemSteps),
                _                           => throw new SwitchExpressionException(_ChallengeType)
            };
            var dailyChallengeItems = m_DailyChallengeItems
                .Where(_Item => _Item.GetType() == classType)
                .ToList();
            var dailyChallengeInfos = m_DailyChallengeInfos
                .Where(_Info => _Info.ChallengeType == _ChallengeType)
                .ToList();
            if (dailyChallengeItems.Count != dailyChallengeInfos.Count)
            {
                Dbg.LogError("Daily challenge items and infos count are not equal each other.");
                return;
            }
            for (int i = 0; i < dailyChallengeItems.Count; i++)
            {
                var dailyChallengeItem = dailyChallengeItems[i];
                var dailyChallengeInfo = dailyChallengeInfos[i];
                int i1 = i;
                var moneyIcon = prefMan.GetObject<Sprite>(
                    CommonPrefabSetNames.Views, $"daily_challenge_money_icon_{i + 1}");
                var args = new DailyChallengePanelItemArgs
                {
                    OnClick     = () => OnDailyChallengeItemClick(i1, _ChallengeType),
                    RewardIcon  = moneyIcon,
                    Info        = dailyChallengeInfo
                };
                dailyChallengeItem.Init(
                    Ticker,
                    Managers.AudioManager,
                    Managers.LocalizationManager,
                    args);
            }
        }
        
        private void LoadDailyChallengeLevel(DailyChallengeInfo _Info)
        {
            var args = new Dictionary<string, object>
            {
                {KeyGameMode,                       ParameterGameModeDailyChallenge},
                {KeyNextLevelType,                  ParameterLevelTypeDefault},
                {KeyDailyChallengeIndex,            _Info.IndexToday},
                {KeyLevelIndex,                     _Info.LevelIndex},
                {KeyChallengeGoal,                  _Info.Goal},
                {KeyOnReadyToLoadLevelFinishAction, OnReadyToLoadLevelAction},
                {KeyIsDailyChallengeSuccess,        true},
                {KeyDailyChallengeRewardMoney,      _Info.RewardMoney},
                {KeyDailyChallengeRewardXp,         _Info.RewardXp},
                {KeyChallengeType,                  _Info.ChallengeType},
                {KeyRemoveTrapsFromLevel,           true}
            };
            OnClose();
            var loadLevelCoroutine = MainMenuUtils.LoadLevelCoroutine(
                args, ViewSettings, FullscreenTransitioner, Ticker, LevelStageSwitcher);
            Cor.Run(loadLevelCoroutine);
        }

        private long GetRandomDailyChallengeLevelIndex()
        {
            var args = new LevelInfoArgs
            {
                LevelType = ParameterLevelTypeDefault,
                GameMode  = ParameterGameModeMain
            };
            long levelsCount = LevelsLoader.GetLevelsCount(args);
            return levelsCount / 2 + Mathf.FloorToInt(Random.value * levelsCount / 2);
        }

        #endregion
    }
}