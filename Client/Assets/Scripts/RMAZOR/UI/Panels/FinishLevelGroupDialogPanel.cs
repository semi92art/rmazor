using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities;
using Common.Entities.UI;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using Common.UI.DialogViewers;
using Common.Utils;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.UI.Panels
{
    public interface IFinishLevelGroupDialogPanel : IDialogPanel { }
    
    public class FinishLevelGroupDialogPanelFake : IFinishLevelGroupDialogPanel
    {
        public EDialogViewerType DialogViewerType   => default;
        public EUiCategory       Category           => default;
        public EAppearingState   AppearingState     { get; set; }
        public RectTransform     PanelRectTransform => null;
        public Animator          Animator           => null;
        
        public void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose) { }
    }

    public class FinishLevelGroupDialogPanel : DialogPanelBase, IFinishLevelGroupDialogPanel
    {
        #region nonpublic members

        private Animator                    m_PanelAnimator;
        private MultiplyMoneyWheelPanelView m_WheelPanelView;
        private Image                       m_MoneyIcon;
        private Image                       m_IconWatchAds;
        private Image                       m_Background;
        private Button                      m_ButtonMultiplyMoney;
        private Button                      m_ButtonSkip;
        private Button                      m_ButtonContinue;
        private TextMeshProUGUI             m_TitleText;
        private TextMeshProUGUI             m_MoneyCountText;
        private TextMeshProUGUI             m_MultiplyButtonText;
        private TextMeshProUGUI             m_GetMoneyButtonText;
        private TextMeshProUGUI             m_ContinueButtonText;
        private Animator                    m_AnimLoadingAds;
        private AnimationTriggerer          m_Triggerer;
        private Sprite                      m_MultipliedMoneySprite;
        private long                        m_MultiplyCoefficient;
        private string                      m_MultiplyWord;

        #endregion

        #region inject

        private ViewSettings                ViewSettings      { get; }
        private IViewUIPrompt               Prompt            { get; }
        private IViewInputCommandsProceeder CommandsProceeder { get; }
        private IModelGame                  Model             { get; }
        private IMoneyCounter               MoneyCounter      { get; }

        private FinishLevelGroupDialogPanel(
            ViewSettings                _ViewSettings,
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewUIPrompt               _Prompt,
            IViewInputCommandsProceeder _CommandsProceeder,
            IModelGame                  _Model,
            IMoneyCounter               _MoneyCounter)
            : base(
                _Managers,
                _Ticker,
                _CameraProvider,
                _ColorProvider)
        {
            ViewSettings = _ViewSettings;
            Prompt            = _Prompt;
            CommandsProceeder = _CommandsProceeder;
            Model             = _Model;
            MoneyCounter      = _MoneyCounter;
        }

        #endregion

        #region api

        public override EDialogViewerType DialogViewerType => EDialogViewerType.Medium1;
        public override EUiCategory       Category         => EUiCategory.FinishGroup;
        public override Animator          Animator         => m_PanelAnimator;

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
            m_MoneyIcon.sprite = psm.GetObject<Sprite>(
                "icons", "icon_coin_ui");
            m_MultipliedMoneySprite = psm.GetObject<Sprite>(
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
                _ => $"{backgroundSpriteNameRaw}_1",
            };
            m_Background.sprite = psm.GetObject<Sprite>("views", backgroundSpriteName);
        }

        public override void OnDialogStartAppearing()
        {
            m_MultiplyWord = Managers.LocalizationManager
                .GetTranslation("multiply")
                .ToUpper(CultureInfo.CurrentUICulture);
            m_ButtonMultiplyMoney.gameObject.SetActive(true);
            m_ButtonSkip.gameObject.SetActive(true);
            m_ButtonContinue.gameObject.SetActive(false);
            m_MoneyCountText.text = Managers.LocalizationManager.GetTranslation("reward")
                                        .ToUpper(CultureInfo.CurrentUICulture)
                                    + ": " + MoneyCounter.CurrentLevelGroupMoney;
            m_ButtonSkip.interactable = false;
            m_ButtonMultiplyMoney.interactable = false;
            m_ButtonContinue.gameObject.SetActive(false);
            Cor.Run(Cor.WaitNextFrame(() =>
            {
                CommandsProceeder.LockCommands(GetCommandsToLock(), nameof(IFinishLevelGroupDialogPanel));
            }));
            Cor.Run(StartIndicatingAdLoadingCoroutine());
            base.OnDialogStartAppearing();
        }

        public override void OnDialogDisappearing()
        {
            m_ButtonSkip.interactable = false;
            m_ButtonMultiplyMoney.interactable = false;
            m_WheelPanelView.StopWheel();
            base.OnDialogDisappearing();
        }
        
        public override void OnDialogDisappeared()
        {
            Prompt.ShowPrompt(EPromptType.TapToNext);
            CommandsProceeder.UnlockCommands(GetCommandsToLock(), nameof(IFinishLevelGroupDialogPanel));
            base.OnDialogDisappeared();
        }

        #endregion

        #region nonpublic methods

        private void GetPrefabContentObjects(GameObject _GameObject)
        {
            var go = _GameObject;
            m_WheelPanelView          = go.GetCompItem<MultiplyMoneyWheelPanelView>("wheel_panel_view");
            m_PanelAnimator           = go.GetCompItem<Animator>("panel_animator");
            m_ButtonMultiplyMoney     = go.GetCompItem<Button>("multiply_money_button");
            m_ButtonSkip              = go.GetCompItem<Button>("skip_button");
            m_ButtonContinue          = go.GetCompItem<Button>("continue_button");
            m_TitleText               = go.GetCompItem<TextMeshProUGUI>("title_text");
            m_MoneyCountText          = go.GetCompItem<TextMeshProUGUI>("money_count_text");
            m_MultiplyButtonText      = go.GetCompItem<TextMeshProUGUI>("multiply_button_text");
            m_GetMoneyButtonText      = go.GetCompItem<TextMeshProUGUI>("skip_button_text");
            m_ContinueButtonText      = go.GetCompItem<TextMeshProUGUI>("continue_button_text");
            m_AnimLoadingAds          = go.GetCompItem<Animator>("loading_ads_anim");
            m_MoneyIcon               = go.GetCompItem<Image>("money_icon");
            m_IconWatchAds            = go.GetCompItem<Image>("watch_ads_icon");
            m_Background              = go.GetCompItem<Image>("background");
            m_Triggerer               = go.GetCompItem<AnimationTriggerer>("triggerer");
        }
        
        private void LocalizeTextObjectsOnLoad()
        {
            var locMan = Managers.LocalizationManager;
            m_WheelPanelView.Init(
                Ticker,
                Managers.AudioManager,
                locMan);
            // m_MoneyCountText.text = m_MultiplyWord + ": " + MoneyCounter.CurrentLevelGroupMoney;
            locMan.AddTextObject(new LocalizableTextObjectInfo(
                m_TitleText, ETextType.MenuUI, "stage_finished",
                _T => _T.ToUpper(CultureInfo.CurrentUICulture)));
            locMan.AddTextObject(new LocalizableTextObjectInfo(
                m_MoneyCountText, ETextType.MenuUI, "reward",
                _S => _S.ToUpper(CultureInfo.CurrentUICulture) 
                      + ": " + MoneyCounter.CurrentLevelGroupMoney));
            locMan.AddTextObject(new LocalizableTextObjectInfo(
                m_MultiplyButtonText, ETextType.MenuUI, "multiply",
                _S => _S.ToUpper(CultureInfo.CurrentUICulture)));
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
            m_ButtonMultiplyMoney.onClick.AddListener(     OnMultiplyButtonPressed);
            m_ButtonSkip.onClick.AddListener(              OnSkipButtonPressed);
            m_ButtonContinue.onClick.AddListener(          OnContinueButtonPressed);
            m_Triggerer.Trigger1                        += OnStartAppearingAnimationFinished;
            m_WheelPanelView.MultiplyCoefficientChanged += OnMultiplyCoefficientChanged;
        }
        
        private void IndicateAdsLoading(bool _Indicate)
        {
            if (AppearingState != EAppearingState.Appearing
                && AppearingState != EAppearingState.Appeared)
            {
                return;
            }
            m_AnimLoadingAds.SetGoActive(_Indicate);
            m_IconWatchAds.enabled             = !_Indicate;
            m_ButtonMultiplyMoney.interactable = !_Indicate;
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
            long reward = MoneyCounter.CurrentLevelGroupMoney * m_MultiplyCoefficient;
            string rewardWord = Managers.LocalizationManager.GetTranslation("reward").ToUpper();
            m_MoneyCountText.text = rewardWord + ": " + reward;
            m_MoneyIcon.sprite = m_MultipliedMoneySprite;
            Managers.AdsManager.ShowRewardedAd(_OnReward: () =>
            {
                Multiply();
                m_ButtonMultiplyMoney.gameObject.SetActive(false);
                m_ButtonSkip.gameObject.SetActive(false);
                m_ButtonContinue.gameObject.SetActive(true);
            }, _Skippable: false);
        }

        private void OnSkipButtonPressed()
        {
            m_WheelPanelView.StopWheel();
            m_MultiplyCoefficient = 1;
            Multiply();
            OnClose(null);
            CommandsProceeder.RaiseCommand(EInputCommand.ReadyToUnloadLevel, null);
        }

        private void OnContinueButtonPressed()
        {
            OnClose(null);
            CommandsProceeder.RaiseCommand(EInputCommand.ReadyToUnloadLevel, null);
        }

        private void Multiply()
        {
            var savedGameEntity = Managers.ScoreManager.GetSavedGameProgress(
                CommonData.SavedGameFileName,
                true);
            void SendAnalytic()
            {
                var eventData = new Dictionary<string, object> {{AnalyticIds.ParameterLevelIndex, Model.LevelStaging.LevelIndex}};
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
                long reward = MoneyCounter.CurrentLevelGroupMoney * m_MultiplyCoefficient;
                var newSavedGame = new SavedGame
                {
                    FileName = CommonData.SavedGameFileName,
                    Money = savedGame.Money + reward,
                    Level = Model.LevelStaging.LevelIndex
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

        private static IEnumerable<EInputCommand> GetCommandsToLock()
        {
            return new[]
                {
                    EInputCommand.ShopMenu,
                    EInputCommand.SettingsMenu
                }
                .Concat(RmazorUtils.MoveAndRotateCommands);
        }

        #endregion
    }
}