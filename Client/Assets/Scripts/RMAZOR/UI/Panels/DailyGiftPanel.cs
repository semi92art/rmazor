using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.UI;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Entities.UI;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.Panels
{
    public interface IDailyGiftPanel : IDialogPanel
    {
        UnityAction OnClose { set; }
    }
    
    public class DailyGiftPanelFake : IDailyGiftPanel
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public UnityAction OnClose { get; set; }
        public EDialogViewerType DialogViewerType   => default;
        public EAppearingState   AppearingState     { get; set; }
        public RectTransform     PanelRectTransform => null;
        public Animator          Animator           => null;
        
        public void        LoadPanel(RectTransform _Container, ClosePanelAction _OnClose) { }
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
            m_TomorrowText,
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
        
        #endregion
        
        #region inject
        
        private IModelGame                          Model                          { get; }
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }

        public DailyGiftPanel(
            IManagersGetter                     _Managers,
            IUITicker                           _Ticker,
            ICameraProvider                     _CameraProvider,
            IColorProvider                      _ColorProvider,
            IViewTimePauser                     _TimePauser,
            IModelGame                          _Model,
            IViewInputCommandsProceeder         _CommandsProceeder,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker)
            : base(
                _Managers,
                _Ticker,
                _CameraProvider, 
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder)
        {
            Model                          = _Model;
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
        }

        #endregion

        #region api

        public UnityAction OnClose { private get; set; }
        
        public override EDialogViewerType DialogViewerType => EDialogViewerType.Medium1;
        public override Animator          Animator         => m_PanelAnimator;

        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            var go  = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(_Container, RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels, "daily_gift_panel");
            PanelRectTransform = go.RTransform();
            PanelRectTransform.SetGoActive(false);
            GetPrefabContentObjects(go);
            GetTodayAndTomorrowGiftMoneyCount();
            LocalizeTexts();
            var psm = Managers.PrefabSetManager;
            m_SpriteMoney = psm.GetObject<Sprite>(
                "icons", 
                "icon_coin_ui");
            m_SpriteMoneyMultiplied = psm.GetObject<Sprite>(
                "icons", 
                "icon_coin_ui_multiplied");

            m_TodayGiftMoneyCountText.text = m_TodayGiftMoneyCount.ToString();
            m_GetButton.onClick.AddListener(OnGetButtonClick);
            m_MultiplyButton.onClick .AddListener(OnMultiplyButtonClick);
            m_TriggererMoneyIcon.Trigger1 += () => Cor.Run(OnMoneyIconStartDisappearingCoroutine());
            m_TriggererMoneyIcon.Trigger2 += OnMoneyItemAnimTrigger2;
        }

        public override void OnDialogStartAppearing()
        {
            Managers.AudioManager.PlayClip(AudioClipArgsDailyReward);
            Cor.Run(StartIndicatingAdLoadingCoroutine());
            m_MoneyIcon.sprite = m_SpriteMoney;
            base.OnDialogStartAppearing();
        }

        public override void OnDialogDisappeared()
        {
            OnClose?.Invoke();
            base.OnDialogDisappeared();
        }

        #endregion

        #region nonpublic methods
        
        private void GetPrefabContentObjects(GameObject _GameObject)
        {
            var go = _GameObject;
            m_PanelAnimator           = go.GetCompItem<Animator>("animator");
            m_TodayGiftMoneyCountText = go.GetCompItem<TextMeshProUGUI>("money_count_text");
            m_GetButton               = go.GetCompItem<Button>("get_button");
            m_MultiplyButton          = go.GetCompItem<Button>("multiply_button");
            m_DailyGiftText           = go.GetCompItem<TextMeshProUGUI>("daily_gift_text");
            m_TomorrowText            = go.GetCompItem<TextMeshProUGUI>("tomorrow");
            m_GetButtonText           = go.GetCompItem<TextMeshProUGUI>("get_button_text");
            m_MultiplyButtonText      = go.GetCompItem<TextMeshProUGUI>("multiply_button_text");
            m_AnimMoneyIcon           = go.GetCompItem<Animator>("money_icon");
            m_TriggererMoneyIcon      = go.GetCompItem<AnimationTriggerer>("money_icon");
            m_IconWatchAds            = go.GetCompItem<Image>("watch_ads_icon");
            m_MoneyIcon               = go.GetCompItem<Image>("money_icon");
            m_AnimLoadingAds          = go.GetCompItem<Animator>("loading_ads_anim");
        }

        private void LocalizeTexts()
        {
            var locMan = Managers.LocalizationManager;
            locMan.AddTextObject(new LocalizableTextObjectInfo(
                m_DailyGiftText,
                ETextType.MenuUI, 
                "daily_gift",
                _Text => _Text.ToUpper(CultureInfo.CurrentUICulture)));
            locMan.AddTextObject(new LocalizableTextObjectInfo(
                m_TomorrowText,
                ETextType.MenuUI, 
                "tomorrow",
                _Text => _Text.ToUpper(CultureInfo.CurrentUICulture) + ": " + m_TomorrowGiftMoneyCount));
            locMan.AddTextObject(new LocalizableTextObjectInfo(
                m_GetButtonText,
                ETextType.MenuUI, 
                "get",
                _Text => _Text.ToUpper(CultureInfo.CurrentUICulture)));
            locMan.AddTextObject(new LocalizableTextObjectInfo(
                m_MultiplyButtonText,
                ETextType.MenuUI, 
                "multiply",
                _Text => _Text.ToUpper(CultureInfo.CurrentUICulture) + " x2"));
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
                SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.UnPauseLevel);
            });
            Managers.AudioManager.PlayClip(CommonAudioClipArgs.UiButtonClick);
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
                () => !Managers.AdsManager.RewardedAdNonSkippableReady,
                () =>
                {
                    IndicateAdsLoading(false);
                });
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
            var savedGameEntity = Managers.ScoreManager.GetSavedGameProgress(
                MazorCommonData.SavedGameFileName,
                true);
            void SendAnalytic()
            {
                var eventData = new Dictionary<string, object>
                {
                    {AnalyticIds.ParameterLevelIndex, Model.LevelStaging.LevelIndex}
                };
                Managers.AnalyticsManager.SendAnalytic(AnalyticIds.WatchAdInFinishGroupPanelPressed, eventData);
            }
            void SetMoneyInBank()
            {
                bool castSuccess = savedGameEntity.Value.CastTo(out SavedGame savedGame);
                if (savedGameEntity.Result == EEntityResult.Fail || !castSuccess)
                {
                    Dbg.LogWarning("Failed to save new game data");
                    return;
                }
                long reward = m_TodayGiftMoneyCount * m_MultiplyCoefficient;
                var newSavedGame = new SavedGame
                {
                    FileName = MazorCommonData.SavedGameFileName,
                    Money = savedGame.Money + reward,
                    Level = Model.LevelStaging.LevelIndex,
                    Args = Model.LevelStaging.Arguments
                };
                Managers.ScoreManager.SaveGameProgress(newSavedGame, false);
            }
            Cor.Run(Cor.WaitWhile(
                () => savedGameEntity.Result == EEntityResult.Pending,
                () =>
                {
                    SendAnalytic();
                    SetMoneyInBank();
                }));
        }

        #endregion
    }
}