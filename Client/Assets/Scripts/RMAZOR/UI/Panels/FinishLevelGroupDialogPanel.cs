using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Common;
using Common.Constants;
using Common.Entities;
using Common.UI;
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
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.UI.Panels
{
    public interface IFinishLevelGroupDialogPanel : IDialogPanel { }
    
    public class FinishLevelGroupDialogPanelFake 
        : DialogPanelFake, IFinishLevelGroupDialogPanel { }

    public class FinishLevelGroupDialogPanel : DialogPanelBase, IFinishLevelGroupDialogPanel
    {
        #region nonpublic members
        
        private static AudioClipArgs AudioClipArgsPayoutReward =>
            new AudioClipArgs("payout_reward", EAudioClipType.UiSound);

        private MultiplyMoneyWheelPanelView m_WheelPanelView;
        private Animator
            m_PanelAnimator,
            m_AnimLoadingAds,
            m_AnimMoneyIcon;
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
            m_MoneyCountText,
            m_MultiplyButtonText,
            m_GetMoneyButtonText,
            m_ContinueButtonText,
            m_WatchVideoToTheEndText;
        private Sprite 
            m_SpriteMoney,
            m_SpriteMoneyMultiplied;
        private long   m_MultiplyCoefficient;
        private string m_MultiplyWord;
        private bool   m_RewardGot;

        #endregion

        #region inject

        private ViewSettings                        ViewSettings                   { get; }
        private IViewUIPrompt                       Prompt                         { get; }
        private IModelGame                          Model                          { get; }
        private IMoneyCounter                       MoneyCounter                   { get; }
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }

        private FinishLevelGroupDialogPanel(
            ViewSettings                        _ViewSettings,
            IManagersGetter                     _Managers,
            IUITicker                           _Ticker,
            ICameraProvider                     _CameraProvider,
            IColorProvider                      _ColorProvider,
            IViewTimePauser                     _TimePauser,
            IViewUIPrompt                       _Prompt,
            IViewInputCommandsProceeder         _CommandsProceeder,
            IModelGame                          _Model,
            IMoneyCounter                       _MoneyCounter,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker)
            : base(
                _Managers,
                _Ticker,
                _CameraProvider,
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder)
        {
            ViewSettings                   = _ViewSettings;
            Prompt                         = _Prompt;
            Model                          = _Model;
            MoneyCounter                   = _MoneyCounter;
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
        }

        #endregion

        #region api

        public override int      DialogViewerId => DialogViewerIdsCommon.MediumCommon;
        public override Animator Animator       => m_PanelAnimator;

        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            var go = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    _Container,
                    RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels, "finish_level_group_panel");
            PanelRectTransform = go.RTransform();
            go.SetActive(false);
            GetPrefabContentObjects(go);
            LocalizeTextObjectsOnLoad();
            SubscribeEventObjectsOnActions();
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
            string backgroundSpriteName = ViewSettings.finishLevelGroupPanelBackgroundVariant switch
            {
                1 => $"{backgroundSpriteNameRaw}_1",
                2 => $"{backgroundSpriteNameRaw}_2",
                3 => $"{backgroundSpriteNameRaw}_3",
                4 => $"{backgroundSpriteNameRaw}_4",
                5 => $"{backgroundSpriteNameRaw}_5",
                _ => $"{backgroundSpriteNameRaw}_1",
            };
            m_Background.sprite = psm.GetObject<Sprite>(CommonPrefabSetNames.Views, backgroundSpriteName);
        }
        
        public override void OnDialogStartAppearing()
        {
            TimePauser.PauseTimeInGame();
            m_MoneyIcon.sprite = m_SpriteMoney;
            m_MultiplyWord = Managers.LocalizationManager
                .GetTranslation("multiply")
                .ToUpper(CultureInfo.CurrentUICulture);
            m_ButtonMultiply.gameObject.SetActive(true);
            m_ButtonSkip         .gameObject.SetActive(true);
            m_ButtonContinue     .gameObject.SetActive(false);
            m_ButtonSkip         .interactable = false;
            m_ButtonMultiply.interactable = false;
            m_WatchVideoToTheEndText.enabled = false;
            m_RewardGot = false;
            m_MoneyCountText.text = MoneyCounter.CurrentLevelGroupMoney.ToString();
            Cor.Run(StartIndicatingAdLoadingCoroutine());
            m_WheelPanelView.ResetWheel();
            base.OnDialogStartAppearing();
        }

        public override void OnDialogAppeared()
        {
            //FIXME какогото хуя кнопки не включаются после пропуска уровня
            m_ButtonMultiply.enabled = true;
            m_ButtonSkip.enabled = true;
            m_ButtonContinue.enabled = true;
            base.OnDialogAppeared();
        }

        public override void OnDialogDisappearing()
        {
            m_ButtonSkip.interactable = false;
            m_ButtonMultiply.interactable = false;
            m_WheelPanelView.StopWheel();
            base.OnDialogDisappearing();
        }
        
        public override void OnDialogDisappeared()
        {
            TimePauser.UnpauseTimeInGame();
            Prompt.ShowPrompt(EPromptType.TapToNext);
            base.OnDialogDisappeared();
        }

        #endregion

        #region nonpublic methods

        private void GetPrefabContentObjects(GameObject _Go)
        {
            m_WheelPanelView         = _Go.GetCompItem<MultiplyMoneyWheelPanelView>("wheel_panel_view");
            m_PanelAnimator          = _Go.GetCompItem<Animator>("panel_animator");
            m_ButtonMultiply    = _Go.GetCompItem<Button>("multiply_money_button");
            m_ButtonSkip             = _Go.GetCompItem<Button>("skip_button");
            m_ButtonContinue         = _Go.GetCompItem<Button>("continue_button");
            m_TitleText              = _Go.GetCompItem<TextMeshProUGUI>("title_text");
            m_MoneyCountText         = _Go.GetCompItem<TextMeshProUGUI>("money_count_text");
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
        }
        
        private void LocalizeTextObjectsOnLoad()
        {
            var locMan = Managers.LocalizationManager;
            m_WheelPanelView.Init(
                Ticker,
                Managers.AudioManager,
                locMan);
            locMan.AddTextObject(new LocalizableTextObjectInfo(
                m_TitleText, ETextType.MenuUI, "stage_finished",
                _T => _T.ToUpper(CultureInfo.CurrentUICulture)));
            locMan.AddTextObject(new LocalizableTextObjectInfo(
                m_RewardText, ETextType.MenuUI, "reward",
                _S => _S.ToUpper(CultureInfo.CurrentUICulture) + ":"));
            locMan.AddTextObject(new LocalizableTextObjectInfo(
                m_MultiplyButtonText, ETextType.MenuUI, "multiply",
                _S => _S.ToUpper(CultureInfo.CurrentUICulture)));
            locMan.AddTextObject(new LocalizableTextObjectInfo(
                m_MoneyCountText, ETextType.MenuUI, "empty_key",
                _S => _S.ToUpper(CultureInfo.CurrentUICulture)));
            locMan.AddTextObject(new LocalizableTextObjectInfo(
                m_WatchVideoToTheEndText, ETextType.MenuUI, "watch_video_to_the_end"));
            string getMoneyButtonTextLocKey = ViewSettings.finishLevelGroupPanelGetMoneyButtonTextVariant switch
            {
                1 => "get",
                2 => "skip",
                _ => "get"
            };
            locMan.AddTextObject(new LocalizableTextObjectInfo(
                m_GetMoneyButtonText, ETextType.MenuUI, getMoneyButtonTextLocKey,
                _S => _S.ToUpper(CultureInfo.CurrentUICulture)));
            locMan.AddTextObject(new LocalizableTextObjectInfo(
                m_ContinueButtonText, ETextType.MenuUI, "continue",
                _S => _S.ToUpper(CultureInfo.CurrentUICulture)));
        }

        private void SubscribeEventObjectsOnActions()
        {
            m_ButtonMultiply.onClick.AddListener(     OnMultiplyButtonPressed);
            m_ButtonSkip.onClick.AddListener(              OnSkipButtonPressed);
            m_ButtonContinue.onClick.AddListener(          OnContinueButtonPressed);
            m_Triggerer.Trigger1                        += OnStartAppearingAnimationFinished;
            m_TriggererMoneyIcon.Trigger1               += () => Cor.Run(OnMoneyIconStartDisappearingCoroutine());
            m_TriggererMoneyIcon.Trigger2               += OnMoneyItemAnimTrigger2;
            m_WheelPanelView.MultiplyCoefficientChanged += OnMultiplyCoefficientChanged;
        }

        private IEnumerator OnMoneyIconStartDisappearingCoroutine()
        {
            bool isPlaying = false;
            yield return Cor.WaitWhile(() => Ticker.Pause);
            long prevMoneyCount = MoneyCounter.CurrentLevelGroupMoney;
            yield return Cor.Lerp(
                Ticker, 
                0.7f, 
                MoneyCounter.CurrentLevelGroupMoney,
                MoneyCounter.CurrentLevelGroupMoney * m_MultiplyCoefficient,
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
                    m_MoneyCountText.text = newMoneyCount.ToString();
                    prevMoneyCount = (long) _P;
                },
                () =>
                {
                    Managers.AudioManager.StopClip(AudioClipArgsPayoutReward);
                });
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
            if (!Managers.AdsManager.RewardedAdNonSkippableReady)
                return;
            m_MultiplyButtonText.text = m_MultiplyWord + " x" + _Coefficient;
        }
        
        private void OnMultiplyButtonPressed()
        {
            if (AppearingState != EAppearingState.Appeared)
                return;
            m_WheelPanelView.StopWheel();
            m_MultiplyCoefficient = m_WheelPanelView.GetMultiplyCoefficient();
            m_WheelPanelView.SetArrowOnCurrentCoefficientPosition();
            m_WheelPanelView.HighlightCurrentCoefficient();
            void OnBeforeShown()
            {
                TimePauser.PauseTimeInUi();
            }
            void OnReward()
            {
                Multiply();
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
                m_MultiplyCoefficient = 1;
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
                () => SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.UnPauseLevel),
                false,
                3U));
        }

        private void Multiply()
        {
            var savedGameEntity = Managers.ScoreManager.GetSavedGameProgress(
                MazorCommonData.SavedGameFileName,
                true);
            void SetMoneyInBank()
            {
                bool castSuccess = savedGameEntity.Value.CastTo(out SavedGame savedGame);
                if (savedGameEntity.Result == EEntityResult.Fail || !castSuccess)
                {
                    Dbg.LogWarning("Failed to save new game data");
                    return;
                }
                long reward = MoneyCounter.CurrentLevelGroupMoney * m_MultiplyCoefficient;
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
                SetMoneyInBank));
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

        private void SendButtonPressedAnalytic(Object _Button)
        {
            string levelType = (string) Model.LevelStaging.Arguments.GetSafe(
                    CommonInputCommandArg.KeyCurrentLevelType, out _);
            bool isThisLevelBonus = levelType == CommonInputCommandArg.ParameterLevelTypeBonus;
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

        #endregion
    }
}