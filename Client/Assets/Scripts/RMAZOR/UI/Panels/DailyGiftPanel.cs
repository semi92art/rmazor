using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using Common;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.Panels
{
    public interface IDailyGiftPanel : IDialogPanel
    {
        UnityAction OnPanelCloseAction                   { get; set; }
        bool        IsDailyGiftAvailableToday { get; }
    }
    
    public class DailyGiftPanelFake : DialogPanelFake, IDailyGiftPanel
    {
#pragma warning disable 0067

        public UnityAction OnPanelCloseAction                   { get; set; }
#pragma warning restore 0067

        public bool        IsDailyGiftAvailableToday => default;
    }
     
    public class DailyGiftPanel : DialogPanelBase, IDailyGiftPanel
    {
        #region nonpublic members
        
        private static readonly Dictionary<int, int> MoneyCount = new Dictionary<int, int>
        {
            {1, 25}, {2, 50}, {3, 75}, {4, 100}, {5, 125}, {6, 150}, {7, 175},
        };
        
        private static AudioClipArgs AudioClipArgsPayoutReward =>
            new AudioClipArgs("payout_reward", EAudioClipType.UiSound);
        private static AudioClipArgs AudioClipArgsDailyReward =>
            new AudioClipArgs("daily_reward", EAudioClipType.UiSound);

        private AnimationTriggerer m_TriggererMoneyIcon;
        private TextMeshProUGUI
            m_TodayGiftMoneyCountText,
            m_DailyGiftText,
            m_TomorrowGiftMoneyCountText,
            m_GetButtonText,
            m_MultiplyButtonText;  
        private Button        
            m_GetButton,        
            m_MultiplyButton;
        private Image    m_IconWatchAds,  m_MoneyIcon;
        private Animator m_PanelAnimator, m_AnimLoadingAds, m_AnimMoneyIcon;
        private Sprite 
            m_SpriteMoney,
            m_SpriteMoneyMultiplied;

        private long m_TodayGiftMoneyCount;
        private long m_TomorrowGiftMoneyCount;
        private long m_MultiplyCoefficient;
        
        private LocTextInfo TodayGiftMoneyCountLocTextInfo => 
            new LocTextInfo(m_TodayGiftMoneyCountText, ETextType.MenuUI_H1, "empty_key", 
                _T => m_TodayGiftMoneyCount.ToString());
        
        protected override string PrefabName => "daily_gift_panel";
        
        #endregion
        
        #region inject

        private IModelGame              Model              { get; }
        private IViewLevelStageSwitcher LevelStageSwitcher { get; }

        private DailyGiftPanel(
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewTimePauser             _TimePauser,
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewLevelStageSwitcher     _LevelStageSwitcher)
            : base(
                _Managers,
                _Ticker,
                _CameraProvider, 
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder)
        {
            Model              = _Model;
            LevelStageSwitcher = _LevelStageSwitcher;
        }

        #endregion

        #region api

        public          UnityAction OnPanelCloseAction        { get; set; }
        public override int         DialogViewerId => DialogViewerIdsCommon.MediumCommon;
        public override Animator    Animator       => m_PanelAnimator;

        public bool IsDailyGiftAvailableToday
        {
            get
            {
                var today = DateTime.Now.Date;
                var dailyRewardGotDict = SaveUtils.GetValue(SaveKeysRmazor.DailyRewardGot)
                                         ?? new Dictionary<DateTime, bool>();
                bool dailyRewardGotToday = dailyRewardGotDict.GetSafe(today, out _);
                return !dailyRewardGotToday;
            }
        }

        #endregion

        #region nonpublic methods
        
        protected override void LoadPanelCore(RectTransform _Container, ClosePanelAction _OnClose)
        {
            GetTodayAndTomorrowGiftMoneyCount();
            base.LoadPanelCore(_Container, _OnClose);
            var psm = Managers.PrefabSetManager;
            m_SpriteMoney = psm.GetObject<Sprite>(
                "icons", 
                "icon_coin_ui");
            m_SpriteMoneyMultiplied = psm.GetObject<Sprite>(
                "icons", 
                "icon_coin_ui_multiplied");
        }
        
        protected override void OnDialogStartAppearing()
        {
            Managers.AudioManager.PlayClip(AudioClipArgsDailyReward);
            Cor.Run(StartIndicatingAdLoadingCoroutine());
            m_MoneyIcon.sprite = m_SpriteMoney;
            base.OnDialogStartAppearing();
        }

        protected override void OnDialogDisappeared()
        {
            OnPanelCloseAction?.Invoke();
            base.OnDialogDisappeared();
        }

        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            m_PanelAnimator              = _Go.GetCompItem<Animator>("animator");
            m_TodayGiftMoneyCountText    = _Go.GetCompItem<TextMeshProUGUI>("money_count_text");
            m_GetButton                  = _Go.GetCompItem<Button>("get_button");
            m_MultiplyButton             = _Go.GetCompItem<Button>("multiply_button");
            m_DailyGiftText              = _Go.GetCompItem<TextMeshProUGUI>("daily_gift_text");
            m_TomorrowGiftMoneyCountText = _Go.GetCompItem<TextMeshProUGUI>("tomorrow");
            m_GetButtonText              = _Go.GetCompItem<TextMeshProUGUI>("get_button_text");
            m_MultiplyButtonText         = _Go.GetCompItem<TextMeshProUGUI>("multiply_button_text");
            m_AnimMoneyIcon              = _Go.GetCompItem<Animator>("money_icon");
            m_TriggererMoneyIcon         = _Go.GetCompItem<AnimationTriggerer>("money_icon");
            m_IconWatchAds               = _Go.GetCompItem<Image>("watch_ads_icon");
            m_MoneyIcon                  = _Go.GetCompItem<Image>("money_icon");
            m_AnimLoadingAds             = _Go.GetCompItem<Animator>("loading_ads_anim");
        }

        protected override void LocalizeTextObjectsOnLoad()
        {
            static string TextFormula(string _Text) => _Text.ToUpper(CultureInfo.CurrentUICulture);
            var locTextInfos = new[]
            {
                new LocTextInfo(m_DailyGiftText,              ETextType.MenuUI_H1, "daily_gift", TextFormula),
                new LocTextInfo(m_TodayGiftMoneyCountText,    ETextType.MenuUI_H1, "empty_key", _T => m_TodayGiftMoneyCount.ToString()), 
                new LocTextInfo(m_TomorrowGiftMoneyCountText, ETextType.MenuUI_H1, "tomorrow", _T => TextFormula(_T) + ": " + m_TomorrowGiftMoneyCount),
                new LocTextInfo(m_GetButtonText,              ETextType.MenuUI_H1, "get", TextFormula),
                new LocTextInfo(m_MultiplyButtonText,         ETextType.MenuUI_H1, "multiply", _T => TextFormula(_T) + " x2"),
                TodayGiftMoneyCountLocTextInfo
            };
            foreach (var locTextInfo in locTextInfos)
                Managers.LocalizationManager.AddLocalization(locTextInfo);
        }

        protected override void SubscribeButtonEvents()
        {
            m_GetButton.onClick     .AddListener(OnGetButtonClick);
            m_MultiplyButton.onClick.AddListener(OnMultiplyButtonClick);
            m_TriggererMoneyIcon.Trigger1 += () => Cor.Run(OnMoneyIconStartDisappearingCoroutine());
            m_TriggererMoneyIcon.Trigger2 += OnMoneyItemAnimTrigger2;
        }

        private void GetTodayAndTomorrowGiftMoneyCount()
        {
            var sessionCountByDaysDict = SaveUtils.GetValue(SaveKeysRmazor.SessionCountByDays);
            int todayMoneyCountIdx = 1;
            var today = DateTime.Now.Date;
            for (int i = 1; i < MoneyCount.Count; i++)
            {
                var iDay = today.AddDays(-i);
                int sessionsCount = sessionCountByDaysDict.GetSafe(iDay, out _);
                if (sessionsCount == 0)
                    break;
                todayMoneyCountIdx++;
            }
            todayMoneyCountIdx = Mathf.Min(todayMoneyCountIdx, MoneyCount.Count - 1);
            int tomorrowMoneyCountIdx = Mathf.Min(todayMoneyCountIdx + 1, MoneyCount.Count - 1);
            m_TodayGiftMoneyCount = MoneyCount[todayMoneyCountIdx];
            m_TomorrowGiftMoneyCount = MoneyCount[tomorrowMoneyCountIdx];
        }

        private void OnGetButtonClick()
        {
            m_MultiplyCoefficient = 1;
            Multiply();
            var today = DateTime.Now.Date;
            var dailyRewardGotDict = SaveUtils.GetValue(SaveKeysRmazor.DailyRewardGot)
                                     ?? new Dictionary<DateTime, bool>();
            dailyRewardGotDict.SetSafe(today, true);
            SaveUtils.PutValue(SaveKeysRmazor.DailyRewardGot, dailyRewardGotDict);
            base.OnClose(() =>
            {
                LevelStageSwitcher.SwitchLevelStage(EInputCommand.UnPauseLevel);
            });
            PlayButtonClickSound();
        }

        private void OnMultiplyButtonClick()
        {
            if (AppearingState != EAppearingState.Appeared)
                return;
            m_MultiplyCoefficient = 2;
            void OnBeforeShown() => TimePauser.PauseTimeInUi();
            void OnReward()
            {
                Multiply();
                m_AnimMoneyIcon.SetTrigger(AnimKeys.Anim);
            }
            void OnClosed()
            {
                TimePauser.UnpauseTimeInUi();
                m_MultiplyButton.interactable = false;
            }

            Managers.AdsManager.ShowRewardedAd(
                OnBeforeShown,
                _OnReward: OnReward,
                _OnClosed: OnClosed);
        }
        
        private IEnumerator OnMoneyIconStartDisappearingCoroutine()
        {
            bool isPlaying = false;
            yield return Cor.WaitWhile(() => Ticker.Pause);
            long prevMoneyCount = m_TodayGiftMoneyCount;
            yield return Cor.Lerp(
                Ticker, 
                0.7f, 
                prevMoneyCount,
                prevMoneyCount * m_MultiplyCoefficient,
                _P =>
                {
                    if (!isPlaying)
                    {
                        Managers.AudioManager.PlayClip(AudioClipArgsPayoutReward);
                        isPlaying = true;
                    }
                    long newMoneyCount = (long) _P;
                    if (newMoneyCount == prevMoneyCount)
                        return;
                    m_TodayGiftMoneyCountText.text = newMoneyCount.ToString();
                    prevMoneyCount = (long) _P;
                },
                () =>
                {
                    Managers.AudioManager.StopClip(AudioClipArgsPayoutReward);
                });
        }
        
        private IEnumerator StartIndicatingAdLoadingCoroutine()
        {
            IndicateAdsLoading(true);
            yield return Cor.WaitWhile(
                () => !Managers.AdsManager.RewardedAdReady,
                () => IndicateAdsLoading(false));
        }
        
        private void IndicateAdsLoading(bool _Indicate)
        {
            if (AppearingState != EAppearingState.Appearing
                && AppearingState != EAppearingState.Appeared)
            {
                return;
            }
            m_AnimLoadingAds.SetGoActive(_Indicate);
            m_IconWatchAds.enabled        = !_Indicate;
            m_MultiplyButton.interactable = !_Indicate;
        }
        
        private void OnMoneyItemAnimTrigger2()
        {
            m_MoneyIcon.sprite = m_SpriteMoneyMultiplied;
        }
        
        private void Multiply()
        {
            var savedGame = Managers.ScoreManager.GetSavedGame(CommonDataMazor.SavedGameFileName);
            object bankMoneyCountArg = savedGame.Arguments.GetSafe(ComInComArg.KeyMoneyCount, out _);
            long money = Convert.ToInt64(bankMoneyCountArg);
            long reward = m_TodayGiftMoneyCount * m_MultiplyCoefficient;
            money += reward;
            savedGame.Arguments.SetSafe(ComInComArg.KeyMoneyCount, money);
            Managers.ScoreManager.SaveGame(savedGame);
            var eventData = new Dictionary<string, object> {{AnalyticIds.ParameterLevelIndex, Model.LevelStaging.LevelIndex}};
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.WatchAdInFinishGroupPanelClick, eventData);
        }
        
        #endregion
    }
}