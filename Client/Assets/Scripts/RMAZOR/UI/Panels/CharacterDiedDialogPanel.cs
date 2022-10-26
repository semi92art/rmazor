using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.Constants;
using Common.Entities;
using Common.Entities.UI;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using Common.UI.DialogViewers;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RMAZOR.UI.Panels
{
    public interface ICharacterDiedDialogPanel : IDialogPanel
    {
        void ReturnFromShopPanel();
    }
    
    public class CharacterDiedDialogPanel : DialogPanelBase, ICharacterDiedDialogPanel
    {
        #region constants

        private const float CountdownTime = 5f;

        #endregion
        
        #region nonpublic members

        private Animator           m_PanelAnimator;
        private Animator           m_AnimLoadingAds;
        private AnimationTriggerer m_Triggerer;
        private Image              m_Background;
        private Image              m_MoneyInBankIcon;
        private Image              m_MoneyIconInPayButton;
        private Image              m_IconWatchAds;
        private Image              m_Countdown;
        private Image              m_CountdownBackground;
        private TextMeshProUGUI    m_TextYouHaveMoney;
        private TextMeshProUGUI    m_MoneyInBankText;
        private TextMeshProUGUI    m_TextContinue;
        private TextMeshProUGUI    m_TextPayMoneyCount;
        private TextMeshProUGUI    m_TextRevive;
        private Button             m_ButtonWatchAds;
        private Button             m_ButtonPayMoney;
        private bool               m_AdsWatched;
        private bool               m_MoneyPayed;
        private long               m_MoneyCount;
        private bool               m_PanelShowing;
        private bool               m_WentToShopPanel;
        private float              m_CountdownValue;

        #endregion

        #region inject

        private GlobalGameSettings          GlobalGameSettings   { get; }
        private ViewSettings                ViewSettings         { get; }
        private IModelGame                  Model                { get; }
        private IViewInputCommandsProceeder CommandsProceeder    { get; }
        private IViewBetweenLevelAdLoader   BetweenLevelAdLoader { get; }

        protected CharacterDiedDialogPanel(
            GlobalGameSettings          _GlobalGameSettings,
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            IManagersGetter             _Managers,
            IUITicker                   _UITicker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewBetweenLevelAdLoader   _BetweenLevelAdLoader)
            : base(
                _Managers,
                _UITicker, 
                _CameraProvider,
                _ColorProvider)
        {
            GlobalGameSettings      = _GlobalGameSettings;
            ViewSettings            = _ViewSettings;
            Model                   = _Model;
            CommandsProceeder       = _CommandsProceeder;
            BetweenLevelAdLoader    = _BetweenLevelAdLoader;
        }
        
        #endregion
        
        #region api

        public override EDialogViewerType DialogViewerType => EDialogViewerType.Medium1;
        public override EUiCategory       Category         => EUiCategory.CharacterDied;
        public override Animator          Animator         => m_PanelAnimator;

        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            var go = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(
                    _Container,
                    RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels, "character_died_panel");
            PanelRectTransform = go.RTransform();
            PanelRectTransform.SetGoActive(false);
            GetPrefabContentObjects(go);
            LocalizeTextObjectsOnLoad();
            m_Triggerer.Trigger1 = () => Cor.Run(StartCountdown());
            m_ButtonWatchAds.onClick.AddListener(OnWatchAdsButtonClick);
            m_ButtonPayMoney.onClick.AddListener(OnPayMoneyButtonClick);
            var moneyIconSprite = Managers.PrefabSetManager.GetObject<Sprite>(
                "icons", "icon_coin_ui");
            m_MoneyInBankIcon.sprite = moneyIconSprite;
            m_MoneyIconInPayButton.sprite = moneyIconSprite;
            m_Countdown.color = ColorProvider.GetColor(ColorIds.UiBorder);
            m_CountdownBackground.color = Color.black;
            const string backgroundSpriteNameRaw = "character_died_panel_background";
            string backgroundSpriteName = ViewSettings.characterDiedPanelBackgroundVariant switch
            {
                1 => $"{backgroundSpriteNameRaw}_1",
                2 => $"{backgroundSpriteNameRaw}_2",
                3 => $"{backgroundSpriteNameRaw}_3",
                4 => $"{backgroundSpriteNameRaw}_4",
                5 => $"{backgroundSpriteNameRaw}_5",
                _ => $"{backgroundSpriteNameRaw}_1",
            };
            m_Background.sprite = Managers.PrefabSetManager.GetObject<Sprite>(
                "views", backgroundSpriteName);
        }

        public void ReturnFromShopPanel()
        {
            var savedGameEntity = Managers.ScoreManager.GetSavedGameProgress(
                CommonData.SavedGameFileName, 
                true);
            Cor.Run(Cor.WaitWhile(() => savedGameEntity.Result == EEntityResult.Pending,
                () =>
                {
                    bool castSuccess = savedGameEntity.Value.CastTo(out SavedGame savedGame);
                    if (savedGameEntity.Result == EEntityResult.Fail || !castSuccess)
                    {
                        Dbg.LogError("Failed to load money count entity when " +
                                     "return from shop panel to character died panel");
                        return;
                    }
                    m_MoneyCount = savedGame.Money;
                    m_MoneyInBankText.text = m_MoneyCount.ToString();
                }));
            m_AdsWatched      = false;
            m_MoneyPayed      = false;
            m_WentToShopPanel = false;
            m_PanelShowing    = true;
            Cor.Run(StartCountdown());
        }

        public override void OnDialogStartAppearing()
        {
            m_CountdownValue = 1f;
            m_AdsWatched       = false;
            m_MoneyPayed       = false;
            m_WentToShopPanel  = false;
            m_PanelShowing     = true;
            m_Countdown    .fillAmount = 1f;
            m_CountdownBackground.fillAmount = 1f;
            IndicateAdsLoading(true);
            Cor.Run(Cor.WaitWhile(
                () => !Managers.AdsManager.RewardedAdReady,
                () => IndicateAdsLoading(false),
                () => !m_PanelShowing));
            var savedGameEntity = Managers.ScoreManager.GetSavedGameProgress(
                CommonData.SavedGameFileName, 
                true);
            Cor.Run(Cor.WaitWhile(
                () => savedGameEntity.Result == EEntityResult.Pending,
                () =>
                {
                    bool castSuccess = savedGameEntity.Value.CastTo(out SavedGame savedGame);
                    if (savedGameEntity.Result == EEntityResult.Fail || !castSuccess)
                    {
                        Dbg.LogError("Failed to load money count entity");
                        return;
                    }
                    m_MoneyCount = savedGame.Money;
                    m_MoneyInBankText.text = m_MoneyCount.ToString();
                    SetBankIsLoaded();
                }));
            CommandsProceeder.LockCommands(
                GetCommandsToLock(),
                nameof(ICharacterDiedDialogPanel));
            base.OnDialogStartAppearing();
        }

        public override void OnDialogDisappeared()
        {
            m_PanelShowing = false;
            CommandsProceeder.UnlockCommands(
                GetCommandsToLock(),
                nameof(ICharacterDiedDialogPanel));
            CommandsProceeder.UnlockCommands(RmazorUtils.MoveAndRotateCommands, "all");
            base.OnDialogDisappeared();
        }
        
        #endregion
        
        #region nonpublic methods

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId == ColorIds.UiBorder)
                m_Countdown.color = _Color;
        }
        
        private void OnWatchAdsButtonClick()
        {
            Managers.AnalyticsManager.SendAnalytic(AnalyticIds.WatchAdInCharacterDiedPanelPressed);
            Managers.AdsManager.ShowRewardedAd(
                _OnReward:      () => m_AdsWatched = true, 
                _OnBeforeShown: () => TickerUtils.PauseTickers(true, Ticker),
                _OnClosed:      () => TickerUtils.PauseTickers(false, Ticker),
                _Skippable:     false);
        }

        private void OnPayMoneyButtonClick()
        {
            bool isMoneyEnough = m_MoneyCount >= GlobalGameSettings.payToContinueMoneyCount;
            if (!isMoneyEnough)
            {
                CommandsProceeder.RaiseCommand(EInputCommand.ShopMenu,
                    new object[] {CommonInputCommandArgs.LoadShopPanelFromCharacterDiedPanel}, true);
                m_WentToShopPanel = true;
            }
            else
            {
                var savedGame = new SavedGame
                {
                    FileName = CommonData.SavedGameFileName,
                    Money = m_MoneyCount - GlobalGameSettings.payToContinueMoneyCount,
                    Level = Model.LevelStaging.LevelIndex
                };
                Managers.ScoreManager.SaveGameProgress(savedGame, false);
                m_MoneyPayed = true;
            }
        }
        
        private void GetPrefabContentObjects(GameObject _GameObject)
        {
            var go = _GameObject;
            m_PanelAnimator        = go.GetCompItem<Animator>("animator");
            m_Triggerer            = go.GetCompItem<AnimationTriggerer>("triggerer");
            m_ButtonWatchAds       = go.GetCompItem<Button>("watch_ads_button");
            m_ButtonPayMoney       = go.GetCompItem<Button>("pay_money_button");
            m_TextYouHaveMoney     = go.GetCompItem<TextMeshProUGUI>("you_have_text");
            m_MoneyInBankText      = go.GetCompItem<TextMeshProUGUI>("money_count_text");
            m_TextPayMoneyCount    = go.GetCompItem<TextMeshProUGUI>("pay_money_count_text");
            m_TextContinue         = go.GetCompItem<TextMeshProUGUI>("continue_text");
            m_TextRevive           = go.GetCompItem<TextMeshProUGUI>("revive_text");
            m_AnimLoadingAds       = go.GetCompItem<Animator>("loading_ads_anim");
            m_Background           = go.GetCompItem<Image>("background");
            m_MoneyInBankIcon      = go.GetCompItem<Image>("money_icon_1");
            m_MoneyIconInPayButton = go.GetCompItem<Image>("money_icon_2");
            m_IconWatchAds         = go.GetCompItem<Image>("watch_ads_icon");
            m_Countdown            = go.GetCompItem<Image>("round_filled_border");
            m_CountdownBackground  = go.GetCompItem<Image>("round_filled_border_back");
        }
        
        private void LocalizeTextObjectsOnLoad()
        {
            var locMan = Managers.LocalizationManager;
            m_TextPayMoneyCount.text  = GlobalGameSettings.payToContinueMoneyCount.ToString();
            locMan.AddTextObject(
                new LocalizableTextObjectInfo(m_TextYouHaveMoney, ETextType.MenuUI, "you_have",
                    _T => _T.ToUpper()));
            locMan.AddTextObject(
                new LocalizableTextObjectInfo(m_TextContinue, ETextType.MenuUI, "continue",
                    _T => _T.ToUpper()));
            locMan.AddTextObject(
                new LocalizableTextObjectInfo(m_TextRevive, ETextType.MenuUI, "revive?",
                    _T => _T.ToUpper()));
        }
        
        private void IndicateAdsLoading(bool _Indicate)
        {
            if (!m_PanelShowing)
                return;
            m_AnimLoadingAds.SetGoActive(_Indicate);
            m_IconWatchAds.enabled        = !_Indicate;
            m_ButtonWatchAds.interactable = !_Indicate;
        }

        private void SetBankIsLoaded()
        {
            m_MoneyInBankIcon.enabled      = true;
            m_MoneyInBankText.enabled      = true;
            m_MoneyIconInPayButton.enabled = true;
            m_TextPayMoneyCount.enabled    = true;
            m_ButtonPayMoney.interactable  = true;
        }

        private IEnumerator StartCountdown()
        {
            yield return Cor.Lerp(
                Ticker,
                CountdownTime * m_CountdownValue,
                m_CountdownValue,
                0f,
                _P =>
                {
                    (m_CountdownValue, m_Countdown.fillAmount, m_CountdownBackground.fillAmount) = (_P, _P, _P);
                },
                _BreakPredicate: () => m_AdsWatched || m_MoneyPayed || m_WentToShopPanel,
                _OnFinishEx: (_Broken, _) =>
                {
                    void RaiseLoadFirstLevelInGroupCommand()
                    {
                        BetweenLevelAdLoader.ShowAd = false;
                        CommandsProceeder.RaiseCommand(
                            EInputCommand.ReadyToUnloadLevel,
                            new object[] { CommonInputCommandArgs.LoadFirstLevelFromGroupArg }, 
                            true);
                    }
                    if (_Broken && m_WentToShopPanel)
                        return;
                    OnClose(() =>
                    {
                        if (_Broken)
                            RaiseContinueCommand();
                        else 
                            RaiseLoadFirstLevelInGroupCommand();
                    });
                });
        }

        private void RaiseContinueCommand()
        {
            CommandsProceeder.RaiseCommand(
                EInputCommand.ReadyToStartLevel,
                null, 
                true);
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