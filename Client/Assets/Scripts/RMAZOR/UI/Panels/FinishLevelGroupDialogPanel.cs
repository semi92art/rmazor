using System.Collections;
using System.Collections.Generic;
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
        public EUiCategory     Category       => default;
        public bool            AllowMultiple  => false;
        public EAppearingState AppearingState { get; set; }
        public RectTransform   PanelObject    => null;
        public Animator        Animator       => null;
        public void LoadPanel() { }
    }

    public class FinishLevelGroupDialogPanel : DialogPanelBase, IFinishLevelGroupDialogPanel
    {
        #region nonpublic members

        private Animator                    m_PanelAnimator;
        private SimpleUiDialogPanelView     m_PanelView;
        private MultiplyMoneyWheelPanelView m_WheelPanelView;
        private Image                       m_MoneyIcon;
        private Image                       m_IconWatchAds;
        private Button                      m_ButtonMultiplyMoney;
        private Button                      m_ButtonSkip;
        private Button                      m_ButtonContinue;
        private SimpleUiButtonView          m_ButtonMultiplyMoneyView;
        private SimpleUiButtonView          m_ButtonSkipView;
        private SimpleUiButtonView          m_ButtonContinueView;
        private TextMeshProUGUI             m_TextYouHaveMoney;
        private TextMeshProUGUI             m_TextMoneyCount;
        private TextMeshProUGUI             m_MultiplyButtonText;
        private TextMeshProUGUI             m_SkipButtonText;
        private TextMeshProUGUI             m_ContinueButtonText;
        private Animator                    m_AnimLoadingAds;
        private AnimationTriggerer          m_Triggerer;
        private string                      m_MultiplyText;
        private long                        m_MultiplyCoefficient;

        #endregion

        #region inject
        
        private IViewUIPrompt               Prompt            { get; }
        private IViewInputCommandsProceeder CommandsProceeder { get; }
        private IModelGame                  Model             { get; }
        private IMoneyCounter               MoneyCounter      { get; }

        private FinishLevelGroupDialogPanel(
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            IDialogViewersController    _DialogViewersController,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewUIPrompt               _Prompt,
            IViewInputCommandsProceeder _CommandsProceeder,
            IModelGame                  _Model,
            IMoneyCounter               _MoneyCounter)
            : base(
                _Managers,
                _Ticker,
                _DialogViewersController,
                _CameraProvider,
                _ColorProvider)
        {
            Prompt            = _Prompt;
            CommandsProceeder = _CommandsProceeder;
            Model             = _Model;
            MoneyCounter      = _MoneyCounter;
        }

        #endregion

        #region api

        public override EUiCategory Category      => EUiCategory.FinishGroup;
        public override bool        AllowMultiple => false;
        public override Animator    Animator      => m_PanelAnimator;

        public override void LoadPanel()
        {
            base.LoadPanel();
            var dv = DialogViewersController.GetViewer(EDialogViewerType.Proposal);
            var go = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    dv.Container,
                    RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels, "finish_level_group_panel");
            PanelObject = go.RTransform();
            go.SetActive(false);
            m_PanelView               = go.GetCompItem<SimpleUiDialogPanelView>("panel_view");
            m_WheelPanelView          = go.GetCompItem<MultiplyMoneyWheelPanelView>("wheel_panel_view");
            m_PanelAnimator           = go.GetCompItem<Animator>("panel_animator");
            m_ButtonMultiplyMoney     = go.GetCompItem<Button>("multiply_money_button");
            m_ButtonSkip              = go.GetCompItem<Button>("skip_button");
            m_ButtonContinue          = go.GetCompItem<Button>("continue_button");
            m_ButtonMultiplyMoneyView = go.GetCompItem<SimpleUiButtonView>("multiply_money_button");
            m_ButtonSkipView          = go.GetCompItem<SimpleUiButtonView>("skip_button");
            m_ButtonContinueView      = go.GetCompItem<SimpleUiButtonView>("continue_button");
            m_TextYouHaveMoney        = go.GetCompItem<TextMeshProUGUI>("reward_text");
            m_TextMoneyCount          = go.GetCompItem<TextMeshProUGUI>("money_count_text");
            m_MultiplyButtonText      = go.GetCompItem<TextMeshProUGUI>("multiply_button_text");
            m_SkipButtonText          = go.GetCompItem<TextMeshProUGUI>("skip_button_text");
            m_ContinueButtonText      = go.GetCompItem<TextMeshProUGUI>("continue_button_text");
            m_AnimLoadingAds          = go.GetCompItem<Animator>("loading_ads_anim");
            m_MoneyIcon               = go.GetCompItem<Image>("money_icon");
            m_IconWatchAds            = go.GetCompItem<Image>("watch_ads_icon");
            m_Triggerer               = go.GetCompItem<AnimationTriggerer>("triggerer");
            m_PanelView.Init(
                Ticker,
                ColorProvider,
                Managers.AudioManager,
                Managers.LocalizationManager,
                Managers.PrefabSetManager);
            m_WheelPanelView.Init(
                Ticker,
                ColorProvider,
                Managers.AudioManager,
                Managers.LocalizationManager,
                Managers.PrefabSetManager);
            m_ButtonMultiplyMoneyView.Init(
                Ticker,
                ColorProvider,
                Managers.AudioManager,
                Managers.LocalizationManager,
                Managers.PrefabSetManager);
            m_ButtonSkipView.Init(
                Ticker,
                ColorProvider,
                Managers.AudioManager,
                Managers.LocalizationManager,
                Managers.PrefabSetManager);
            m_MoneyIcon.sprite = Managers.PrefabSetManager.GetObject<Sprite>(
                "icons", "icon_coin_ui");
            m_ButtonMultiplyMoney.onClick.AddListener(OnMultiplyButtonPressed);
            m_ButtonSkip.onClick.AddListener(OnSkipButtonPressed);
            m_ButtonContinue.onClick.AddListener(OnContinueButtonPressed);
            Managers.LocalizationManager.AddTextObject(
                new LocalizableTextObjectInfo(m_TextYouHaveMoney, ETextType.MenuUI, "reward"));
            Managers.LocalizationManager.AddTextObject(
                new LocalizableTextObjectInfo(m_MultiplyButtonText, ETextType.MenuUI, "multiply"));
            Managers.LocalizationManager.AddTextObject(
                new LocalizableTextObjectInfo(m_SkipButtonText, ETextType.MenuUI, "skip"));
            Managers.LocalizationManager.AddTextObject(
                new LocalizableTextObjectInfo(m_ContinueButtonText, ETextType.MenuUI, "continue"));
            m_MultiplyText = Managers.LocalizationManager.GetTranslation("multiply");
            m_TextMoneyCount.text = MoneyCounter.CurrentLevelGroupMoney.ToString();
            m_Triggerer.Trigger1 += OnStartAppearingAnimationFinished;
            m_WheelPanelView.MultiplyCoefficientChanged += OnMultiplyCoefficientChanged;
        }

        public override void OnDialogStartAppearing()
        {
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
            m_MultiplyButtonText.text =
                Managers.LocalizationManager.GetTranslation("multiply") + " x" + _Coefficient;
        }
        
        private void OnMultiplyButtonPressed()
        {
            if (AppearingState != EAppearingState.Appeared)
                return;
            m_WheelPanelView.StopWheel();
            m_MultiplyCoefficient = m_WheelPanelView.GetMultiplyCoefficient();
            m_WheelPanelView.SetArrowOnCurrentCoefficientPosition();
            long reward = MoneyCounter.CurrentLevelGroupMoney * m_MultiplyCoefficient;
            m_TextMoneyCount.text = reward.ToString();
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
            var dv = DialogViewersController.GetViewer(EDialogViewerType.Proposal);
            dv.Back();
            CommandsProceeder.RaiseCommand(EInputCommand.ReadyToUnloadLevel, null);
        }

        private void OnContinueButtonPressed()
        {
            var dv = DialogViewersController.GetViewer(EDialogViewerType.Proposal);
            dv.Back();
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