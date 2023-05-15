using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Common;
using Common.Constants;
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
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.UI.Utils;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static RMAZOR.Models.ComInComArg;
using Object = UnityEngine.Object;

namespace RMAZOR.UI.Panels
{
    public interface IFinishLevelGroupDialogPanel : IDialogPanel { }
    
    public class FinishLevelGroupDialogPanelFake 
        : DialogPanelFake, IFinishLevelGroupDialogPanel { }

    public class FinishLevelGroupDialogPanel : DialogPanelBase, IFinishLevelGroupDialogPanel
    {
        #region constants

        private const int MultiplyCoefficientDefault = 1;

        #endregion
        
        #region nonpublic members
        
        private static AudioClipArgs AudioClipArgsPayoutReward =>
            new AudioClipArgs("payout_reward", EAudioClipType.UiSound);

        private MultiplyMoneyWheelPanelView m_WheelPanelView;
        private Animator
            m_PanelAnimator,
            m_AnimLoadingAds,
            m_AnimMoneyIcon,
            m_AnimXpIcon,
            m_AnimLevelIcon;
        private AnimationTriggerer
            m_Triggerer,
            m_TriggererMoneyIcon;
        private Image           
            m_MoneyIcon,
            m_IconWatchAds, 
            m_Background;
        private Button     
            m_ButtonMultiply,
            m_ButtonSkip,
            m_ButtonContinue;
        private TextMeshProUGUI
            m_TitleText,
            m_RewardText,
            m_MoneyGotCountText,
            m_XpGotCountText,
            m_MultiplyButtonText,
            m_GetMoneyButtonText,
            m_ContinueButtonText,
            m_WatchVideoToTheEndText,
            m_CharacterLevelText,
            m_XpSliderText;
        private Sprite 
            m_SpriteMoney,
            m_SpriteMoneyMultiplied;
        private Slider m_XpSlider;
        private int   m_MultiplyCoefficient;
        private string m_MultiplyWord;
        private bool   m_RewardGot;
        
        protected override string PrefabName => "finish_level_group_panel";

        #endregion

        #region inject

        private ViewSettings            ViewSettings       { get; }
        private IViewGameUIPrompt       Prompt             { get; }
        private IModelGame              Model              { get; }
        private IRewardCounter          RewardCounter      { get; }
        private IViewLevelStageSwitcher LevelStageSwitcher { get; }

        private FinishLevelGroupDialogPanel(
            ViewSettings                _ViewSettings,
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewTimePauser             _TimePauser,
            IViewGameUIPrompt           _Prompt,
            IViewInputCommandsProceeder _CommandsProceeder,
            IModelGame                  _Model,
            IRewardCounter              _RewardCounter,
            IViewLevelStageSwitcher     _LevelStageSwitcher)
            : base(
                _Managers,
                _Ticker,
                _CameraProvider,
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder)
        {
            ViewSettings       = _ViewSettings;
            Prompt             = _Prompt;
            Model              = _Model;
            RewardCounter      = _RewardCounter;
            LevelStageSwitcher = _LevelStageSwitcher;
        }

        #endregion

        #region api
        
        public override    int      DialogViewerId => DialogViewerIdsCommon.MediumCommon;
        public override    Animator Animator       => m_PanelAnimator;

        #endregion

        #region nonpublic methods
        
        protected override void LoadPanelCore(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanelCore(_Container, _OnClose);
            var psm = Managers.PrefabSetManager;
            m_SpriteMoney = psm.GetObject<Sprite>(
                "icons", 
                "icon_coin_ui");
            m_SpriteMoneyMultiplied = psm.GetObject<Sprite>(
                "icons", 
                "icon_coin_ui_multiplied");
            m_WheelPanelView.Init(
                Ticker,
                Managers.AudioManager,
                Managers.LocalizationManager);
            const string backgroundSpriteNameRaw = "finish_level_group_panel_background";
            int backVar = ViewSettings.finishLevelGroupPanelBackgroundVariant;
            string backgroundSpriteName = $"{backgroundSpriteNameRaw}_{(backVar < 1 ? 1 : backVar)}";
            m_Background.sprite = psm.GetObject<Sprite>(CommonPrefabSetNames.Views, backgroundSpriteName);
        }
        
        protected override void OnDialogStartAppearing()
        {
            TimePauser.PauseTimeInGame();
            m_MoneyIcon.sprite = m_SpriteMoney;
            m_WatchVideoToTheEndText.enabled = false;
            m_RewardGot = false;
            m_MultiplyCoefficient = MultiplyCoefficientDefault;
            SetMultiplyWordTranslationOnDialogStartAppearing();
            SetStageClearTextOnDialogStartAppearing();
            SetButtonsStateOnDialogStartAppearing();
            SetRewardOnDialogStartAppearing();
            SetLevelAndXpSliderTextOnDialogStartAppearing();
            Cor.Run(StartIndicatingAdLoadingCoroutine());
            m_WheelPanelView.ResetWheel();
            base.OnDialogStartAppearing();
        }

        protected override void OnDialogAppeared()
        {
            //FIXME какогото хуя кнопки не включаются после пропуска уровня
            m_ButtonMultiply.enabled = true;
            m_ButtonSkip    .enabled = true;
            m_ButtonContinue.enabled = true;
            Cor.Run(AnimateXpSliderValueCoroutine());
            base.OnDialogAppeared();
        }

        protected override void OnDialogDisappearing()
        {
            m_ButtonSkip.interactable = false;
            m_ButtonMultiply.interactable = false;
            m_WheelPanelView.StopWheel();
            base.OnDialogDisappearing();
        }

        protected override void OnDialogDisappeared()
        {
            TimePauser.UnpauseTimeInGame();
            Prompt.ShowPrompt(EPromptType.TapToNext);
            base.OnDialogDisappeared();
        }

        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            m_WheelPanelView         = _Go.GetCompItem<MultiplyMoneyWheelPanelView>("wheel_panel_view");
            m_PanelAnimator          = _Go.GetCompItem<Animator>("panel_animator");
            m_ButtonMultiply         = _Go.GetCompItem<Button>("multiply_money_button");
            m_ButtonSkip             = _Go.GetCompItem<Button>("skip_button");
            m_ButtonContinue         = _Go.GetCompItem<Button>("continue_button");
            m_TitleText              = _Go.GetCompItem<TextMeshProUGUI>("title_text");
            m_MoneyGotCountText      = _Go.GetCompItem<TextMeshProUGUI>("money_count_text");
            m_MultiplyButtonText     = _Go.GetCompItem<TextMeshProUGUI>("multiply_button_text");
            m_GetMoneyButtonText     = _Go.GetCompItem<TextMeshProUGUI>("skip_button_text");
            m_ContinueButtonText     = _Go.GetCompItem<TextMeshProUGUI>("continue_button_text");
            m_RewardText             = _Go.GetCompItem<TextMeshProUGUI>("reward_text");
            m_WatchVideoToTheEndText = _Go.GetCompItem<TextMeshProUGUI>("watch_video_to_the_end_text");
            m_AnimLoadingAds         = _Go.GetCompItem<Animator>("loading_ads_anim");
            m_AnimMoneyIcon          = _Go.GetCompItem<Animator>("money_icon");
            m_MoneyIcon              = _Go.GetCompItem<Image>("money_icon");
            m_IconWatchAds           = _Go.GetCompItem<Image>("watch_ads_icon");
            m_Background             = _Go.GetCompItem<Image>("background");
            m_Triggerer              = _Go.GetCompItem<AnimationTriggerer>("triggerer");
            m_TriggererMoneyIcon     = _Go.GetCompItem<AnimationTriggerer>("money_icon");
            m_AnimXpIcon             = _Go.GetCompItem<Animator>("xp_icon_anim");
            m_XpGotCountText         = _Go.GetCompItem<TextMeshProUGUI>("xp_got_count_text");
            m_XpSlider               = _Go.GetCompItem<Slider>("character_xp_slider");
            m_XpSliderText           = _Go.GetCompItem<TextMeshProUGUI>("character_xp_slider_text");
            m_CharacterLevelText     = _Go.GetCompItem<TextMeshProUGUI>("character_level_text");
            m_AnimXpIcon             = _Go.GetCompItem<Animator>("xp_icon_anim");
            m_AnimLevelIcon          = _Go.GetCompItem<Animator>("level_icon_animator");
        }
        
        protected override void LocalizeTextObjectsOnLoad()
        {
            m_WheelPanelView.Init(
                Ticker,
                Managers.AudioManager,
                Managers.LocalizationManager);
            string getMoneyButtonTextLocKey = ViewSettings.finishLevelGroupPanelGetMoneyButtonTextVariant switch
            {
                1 => "get",
                2 => "skip",
                _ => "get"
            };
            static string TextFormula(string _Text) => _Text.ToUpper(CultureInfo.CurrentUICulture);
            static string TextFormula1(string _Text) => _Text.ToUpper(CultureInfo.CurrentUICulture) + ":";
            var locTextInfos = new[]
            {
                new LocTextInfo(m_TitleText,              ETextType.MenuUI_H1, "empty_key"),
                new LocTextInfo(m_RewardText,             ETextType.MenuUI_H1, "reward",         TextFormula1),
                new LocTextInfo(m_MultiplyButtonText,     ETextType.MenuUI_H1, "multiply",       TextFormula),
                new LocTextInfo(m_MoneyGotCountText,      ETextType.MenuUI_H1, "empty_key",      TextFormula),
                new LocTextInfo(m_WatchVideoToTheEndText, ETextType.MenuUI_H1, "watch_video_to_the_end"),
                new LocTextInfo(m_GetMoneyButtonText,     ETextType.MenuUI_H1, getMoneyButtonTextLocKey,       TextFormula),
                new LocTextInfo(m_ContinueButtonText,     ETextType.MenuUI_H1, "continue",       TextFormula)
            };
            foreach (var locTextInfo in locTextInfos)
                Managers.LocalizationManager.AddLocalization(locTextInfo);
        }

        protected override void SubscribeButtonEvents()
        {
            m_ButtonMultiply.onClick.AddListener(          OnMultiplyButtonPressed);
            m_ButtonSkip.onClick.AddListener(              OnSkipButtonPressed);
            m_ButtonContinue.onClick.AddListener(          OnContinueButtonPressed);
            m_Triggerer.Trigger1                        += OnStartAppearingAnimationFinished;
            m_TriggererMoneyIcon.Trigger1               += () => Cor.Run(OnMoneyIconStartDisappearingCoroutine());
            m_TriggererMoneyIcon.Trigger2               += OnMoneyItemAnimTrigger2;
            m_WheelPanelView.MultiplyCoefficientChanged += OnMultiplyCoefficientChanged;
        }
        
        private IEnumerator AnimateXpSliderValueCoroutine()
        {
            yield return Cor.Delay(0.3f, Ticker);
            var savedGame = Managers.ScoreManager.GetSavedGame(CommonDataMazor.SavedGameFileName);
            object xpTotalGotArg = savedGame.Arguments.GetSafe(KeyCharacterXp, out _);
            int xpTotalGotStart = Convert.ToInt32(xpTotalGotArg);
            int prevCharacterLevel = RmazorUtils.GetCharacterLevel(xpTotalGotStart, out _, out _);
            yield return Cor.Lerp(
                Ticker,
                0.5f,
                0f,
                1f,
                _P =>
                {
                    int xpAddict = Mathf.RoundToInt(_P * RewardCounter.CurrentLevelGroupXp * m_MultiplyCoefficient);
                    int characterLevel = RmazorUtils.GetCharacterLevel(
                        xpTotalGotStart + xpAddict, out int xpToNextLevelTotal, out int xpGotOnThisLevel);
                    if (characterLevel > prevCharacterLevel)
                        AnimateLevelUp();
                    m_CharacterLevelText.text = characterLevel.ToString();
                    m_XpSliderText.text = xpGotOnThisLevel + "/" + xpToNextLevelTotal + "XP";
                    m_XpSlider.value = (float) xpGotOnThisLevel / xpToNextLevelTotal;
                    prevCharacterLevel = characterLevel;
                });
        }

        private IEnumerator OnMoneyIconStartDisappearingCoroutine()
        {
            bool isPlaying = false;
            yield return Cor.WaitWhile(() => Ticker.Pause);
            long prevMoneyCount = RewardCounter.CurrentLevelGroupMoney;
            int prevXpCount     = RewardCounter.CurrentLevelGroupXp;
            yield return Cor.Lerp(
                Ticker, 
                0.7f,
                1f,
                m_MultiplyCoefficient,
                _P =>
                {
                    if (!isPlaying)
                    {
                        Managers.AudioManager.PlayClip(AudioClipArgsPayoutReward);
                        isPlaying = true;
                    }
                    long newMoneyCount = Mathf.RoundToInt(_P * RewardCounter.CurrentLevelGroupMoney);
                    int newXpCount = Mathf.RoundToInt(_P * RewardCounter.CurrentLevelGroupXp);
                    if (newMoneyCount == prevMoneyCount && newXpCount == prevXpCount)
                        return;
                    m_MoneyGotCountText.text = newMoneyCount.ToString();
                    m_XpGotCountText.text = newXpCount.ToString();
                    (prevMoneyCount, prevXpCount) = (newMoneyCount, newXpCount);
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
        
        private void AnimateLevelUp()
        {
            m_AnimLevelIcon.SetTrigger(AnimKeys.Anim);
        }
        
        private void OnMoneyItemAnimTrigger2()
        {
            m_MoneyIcon.sprite = m_SpriteMoneyMultiplied;
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
            m_ButtonMultiply.interactable = !_Indicate;
        }
        
        private void OnStartAppearingAnimationFinished()
        {
            m_ButtonSkip.interactable = true;
            m_WheelPanelView.StartWheel();
        }

        private void OnMultiplyCoefficientChanged(int _Coefficient)
        {
            if (!Managers.AdsManager.RewardedAdReady)
                return;
            m_MultiplyButtonText.text = m_MultiplyWord + " x" + _Coefficient;
        }
        
        private void OnMultiplyButtonPressed()
        {
            if (AppearingState != EAppearingState.Appeared)
                return;
            void OnBeforeShown()
            {
                TimePauser.PauseTimeInUi();
            }
            void OnReward()
            {
                m_WheelPanelView.StopWheel();
                m_MultiplyCoefficient = m_WheelPanelView.GetMultiplyCoefficient();
                m_WheelPanelView.SetArrowOnCurrentCoefficientPosition();
                m_WheelPanelView.HighlightCurrentCoefficient();
                Multiply();
                Cor.Run(AnimateXpSliderValueCoroutine());
                m_AnimMoneyIcon.SetTrigger(AnimKeys.Anim);
                m_RewardGot = true;
            }
            void OnClosed()
            {
                TimePauser.UnpauseTimeInUi();
                if (!m_RewardGot)
                    m_WatchVideoToTheEndText.enabled = true;
                m_ButtonMultiply.gameObject.SetActive(false);
                m_ButtonSkip    .gameObject.SetActive(false);
                m_ButtonContinue.gameObject.SetActive(true);
            }
            Managers.AdsManager.ShowRewardedAd(
                OnBeforeShown,
                _OnReward: OnReward,
                _OnClosed: OnClosed);
            SendButtonPressedAnalytic(m_ButtonMultiply);
        }

        private void OnSkipButtonPressed()
        {
            Cor.Run(Cor.WaitNextFrame(() =>
            {
                m_WheelPanelView.StopWheel();
                m_MultiplyCoefficient = MultiplyCoefficientDefault;
                Multiply();
                OnClose(UnpauseLevel);
            }, false, 3U));
            SendButtonPressedAnalytic(m_ButtonSkip);
        }

        private void OnContinueButtonPressed()
        {
            OnClose(UnpauseLevel);
            SendButtonPressedAnalytic(m_ButtonContinue);
        }

        private void UnpauseLevel()
        {
            Cor.Run(Cor.WaitNextFrame(
                () => LevelStageSwitcher.SwitchLevelStage(EInputCommand.UnPauseLevel),
                false,
                3U));
        }

        private void Multiply()
        {
            var savedGame = Managers.ScoreManager.GetSavedGame(CommonDataMazor.SavedGameFileName);
            object moneyArg = savedGame.Arguments.GetSafe(KeyMoneyCount, out _);
            long money = Convert.ToInt64(moneyArg);
            long reward = RewardCounter.CurrentLevelGroupMoney * m_MultiplyCoefficient;
            money += reward;
            savedGame.Arguments.SetSafe(KeyMoneyCount, money);
            object xpArg = savedGame.Arguments.GetSafe(KeyCharacterXp, out _);
            int xp = Convert.ToInt32(xpArg);
            xp += RewardCounter.CurrentLevelGroupXp * m_MultiplyCoefficient;
            savedGame.Arguments.SetSafe(KeyCharacterXp, xp);
            Managers.ScoreManager.SaveGame(savedGame);
        }

        private void SendButtonPressedAnalytic(Object _Button)
        {
            string levelType = (string) Model.LevelStaging.Arguments.GetSafe(KeyCurrentLevelType, out _);
            bool isThisLevelBonus = levelType == ParameterLevelTypeBonus;
            string analyticId = null;
            if (_Button == m_ButtonMultiply)
                analyticId = "button_multiply_in_finish_level_group_pressed";
            else if (_Button == m_ButtonSkip)
                analyticId = "button_skip_in_finish_level_group_pressed";
            else if (_Button == m_ButtonContinue)
                analyticId = "button_continue_in_finish_level_group_pressed";
            var eventData = new Dictionary<string, object>
            {
                {AnalyticIds.ParameterLevelIndex, Model.LevelStaging.LevelIndex},
                {AnalyticIds.ParameterLevelType, isThisLevelBonus ? 2 : 1},
            };
            Managers.AnalyticsManager.SendAnalytic(analyticId, eventData);
        }

        private void SetMultiplyWordTranslationOnDialogStartAppearing()
        {
            m_MultiplyWord = Managers.LocalizationManager
                .GetTranslation("multiply")
                .ToUpper(CultureInfo.CurrentUICulture);
        }

        private void SetButtonsStateOnDialogStartAppearing()
        {
            m_ButtonMultiply.gameObject.SetActive(true);
            m_ButtonSkip    .gameObject.SetActive(true);
            m_ButtonContinue.gameObject.SetActive(false);
            m_ButtonSkip    .interactable = false;
            m_ButtonMultiply.interactable = false;
        }

        private void SetStageClearTextOnDialogStartAppearing()
        {
            string gameMode = (string) Model.LevelStaging.Arguments.GetSafe(KeyGameMode, out _);
            string locKey = gameMode == ParameterGameModeMain ? "stage_finished" : "level_finished";
            m_TitleText.text = Managers.LocalizationManager.GetTranslation(locKey)
                .ToUpper(CultureInfo.CurrentUICulture);
            m_TitleText.font = Managers.LocalizationManager.GetFont(ETextType.MenuUI_H1);
        }

        private void SetRewardOnDialogStartAppearing()
        {
            m_MoneyGotCountText.text = RewardCounter.CurrentLevelGroupMoney.ToString();
            m_XpGotCountText   .text = RewardCounter.CurrentLevelGroupXp.ToString();
        }

        private void SetLevelAndXpSliderTextOnDialogStartAppearing()
        {
            int totalXpGot = MainMenuUtils.GetTotalXpGot(Managers.ScoreManager);
            int characterLevel = RmazorUtils.GetCharacterLevel(
                totalXpGot, out int xpToNextLevelTotal, out int xpGotOnThisLevel);
            m_CharacterLevelText.text = characterLevel.ToString();
            m_XpSliderText.text = xpGotOnThisLevel + "/" + xpToNextLevelTotal + "XP";
            m_XpSlider.value = (float) xpGotOnThisLevel / xpToNextLevelTotal;
        }

        #endregion
    }
}