using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Common;
using Common.Helpers;
using mazing.common.Runtime.CameraProviders;
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
using UnityEngine.UI;
using static RMAZOR.Models.ComInComArg;

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

        private Animator           
            m_PanelAnimator,
            m_AnimLoadingAds;
        private AnimationTriggerer m_Triggerer;
        private Image
            m_MoneyInBankIcon,
            m_MoneyIconInPayButton,
            m_IconWatchAds,
            m_Countdown,
            m_CountdownBackground;
        private TextMeshProUGUI    
            m_TextYouHaveMoney,
            m_MoneyInBankText,
            m_TextContinue,
            m_TextPayMoneyCount,
            m_TextRevive;
        private Button
            m_ButtonWatchAds,
            m_ButtonPayMoney;
        private bool
            m_AdsWatched,
            m_MoneyPayed,
            m_PanelShowing,
            m_WentToShopPanel;
        private long   m_MoneyCount;
        private float  m_CountdownValue;

        protected override string PrefabName => "character_died_panel";

        #endregion

        #region inject

        private GlobalGameSettings        GlobalGameSettings   { get; }
        private IModelGame                Model                { get; }
        private IViewBetweenLevelAdShower BetweenLevelAdShower { get; }
        private IViewLevelStageSwitcher   LevelStageSwitcher   { get; }

        private CharacterDiedDialogPanel(
            GlobalGameSettings          _GlobalGameSettings,
            IModelGame                  _Model,
            IManagersGetter             _Managers,
            IUITicker                   _UITicker,
            ICameraProvider             _CameraProvider,
            IViewTimePauser             _TimePauser,
            IColorProvider              _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewBetweenLevelAdShower   _BetweenLevelAdShower,
            IViewLevelStageSwitcher     _LevelStageSwitcher)
            : base(
                _Managers,
                _UITicker, 
                _CameraProvider,
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder)
        {
            GlobalGameSettings   = _GlobalGameSettings;
            Model                = _Model;
            BetweenLevelAdShower = _BetweenLevelAdShower;
            LevelStageSwitcher   = _LevelStageSwitcher;
        }
        
        #endregion
        
        #region api
        
        public override    int      DialogViewerId => DialogViewerIdsCommon.MediumCommon;
        public override    Animator Animator       => m_PanelAnimator;

        public void ReturnFromShopPanel()
        {
            var savedGame = Managers.ScoreManager.GetSavedGame(MazorCommonData.SavedGameFileName);
            object bankMoneyCountArg = savedGame.Arguments.GetSafe(KeyMoneyCount, out _);
            m_MoneyCount = Convert.ToInt64(bankMoneyCountArg);
            m_MoneyInBankText.text = m_MoneyCount.ToString();
            m_AdsWatched      = false;
            m_MoneyPayed      = false;
            m_WentToShopPanel = false;
            m_PanelShowing    = true;
            Cor.Run(StartCountdown());
        }

        #endregion
        
        #region nonpublic methods
        
        protected override void LoadPanelCore(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanelCore(_Container, _OnClose);
            var moneyIconSprite = Managers.PrefabSetManager.GetObject<Sprite>(
                "icons", "icon_coin_ui");
            m_MoneyInBankIcon.sprite      = moneyIconSprite;
            m_MoneyIconInPayButton.sprite = moneyIconSprite;
            m_Countdown.color             = ColorProvider.GetColor(ColorIds.UiBorder);
            m_CountdownBackground.color   = Color.black;
        }
        
        protected override void OnDialogStartAppearing()
        {
            m_TextPayMoneyCount.text = GlobalGameSettings.payToContinueMoneyCount.ToString();
            TimePauser.PauseTimeInGame();
            m_CountdownValue = 1f;
            m_AdsWatched       = false;
            m_MoneyPayed       = false;
            m_WentToShopPanel  = false;
            m_PanelShowing     = true;
            m_Countdown          .fillAmount = 1f;
            m_CountdownBackground.fillAmount = 1f;
            IndicateAdsLoading(true);
            Cor.Run(Cor.WaitWhile(
                () => !Managers.AdsManager.RewardedAdReady,
                () => IndicateAdsLoading(false),
                () => !m_PanelShowing));
            var savedGame = Managers.ScoreManager.GetSavedGame(MazorCommonData.SavedGameFileName);
            var bankMoneyCountArg = savedGame.Arguments.GetSafe(KeyMoneyCount, out _);
            m_MoneyCount = Convert.ToInt64(bankMoneyCountArg);
            m_MoneyInBankText.text = m_MoneyCount.ToString();
            SetBankIsLoaded();
            base.OnDialogStartAppearing();
        }

        protected override void OnDialogDisappeared()
        {
            m_PanelShowing = false;
            base.OnDialogDisappeared();
            CommandsProceeder.UnlockCommands(RmazorUtils.MoveAndRotateCommands, "all");
        }

        protected override void SubscribeButtonEvents()
        {
            m_Triggerer.Trigger1 = () => Cor.Run(StartCountdown());
            m_ButtonWatchAds.onClick.AddListener(OnWatchAdsButtonClick);
            m_ButtonPayMoney.onClick.AddListener(OnPayMoneyButtonClick);
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId == ColorIds.UiBorder)
                m_Countdown.color = _Color;
        }
        
        private void OnWatchAdsButtonClick()
        {
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.WatchAdInCharacterDiedPanelClick);
            void OnAdReward()
            {
                m_AdsWatched = true;
            }
            void OnBeforeAdShown()
            {
                TimePauser.PauseTimeInUi();
            }
            void OnAdClosed()
            {
                TimePauser.UnpauseTimeInGame();
                TimePauser.UnpauseTimeInUi();
            }
            Managers.AdsManager.ShowRewardedAd(
                _OnReward:       OnAdReward, 
                _OnBeforeShown:  OnBeforeAdShown,
                _OnClosed:       OnAdClosed,
                _OnFailedToShow: OnAdClosed);
        }

        private void OnPayMoneyButtonClick()
        {
            bool isMoneyEnough = m_MoneyCount >= GlobalGameSettings.payToContinueMoneyCount;
            if (!isMoneyEnough)
            {
                var args = new Dictionary<string, object> {{KeyLoadShopPanelFromCharacterDiedPanel, true}};
                CommandsProceeder.RaiseCommand(EInputCommand.ShopPanel, args, true);
                m_WentToShopPanel = true;
            }
            else
            {
                var savedGame = Managers.ScoreManager.GetSavedGame(MazorCommonData.SavedGameFileName);
                var bankMoneyCountArg = savedGame.Arguments.GetSafe(KeyMoneyCount, out _);
                long money = Convert.ToInt64(bankMoneyCountArg);
                money -= GlobalGameSettings.payToContinueMoneyCount;
                savedGame.Arguments.SetSafe(KeyMoneyCount, money);
                Managers.ScoreManager.SaveGame(savedGame);
                m_MoneyPayed = true;
            }
        }
        
        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            m_PanelAnimator        = _Go.GetCompItem<Animator>("animator");
            m_Triggerer            = _Go.GetCompItem<AnimationTriggerer>("triggerer");
            m_ButtonWatchAds       = _Go.GetCompItem<Button>("watch_ads_button");
            m_ButtonPayMoney       = _Go.GetCompItem<Button>("pay_money_button");
            m_TextYouHaveMoney     = _Go.GetCompItem<TextMeshProUGUI>("you_have_text");
            m_MoneyInBankText      = _Go.GetCompItem<TextMeshProUGUI>("money_count_text");
            m_TextPayMoneyCount    = _Go.GetCompItem<TextMeshProUGUI>("pay_money_count_text");
            m_TextContinue         = _Go.GetCompItem<TextMeshProUGUI>("continue_text");
            m_TextRevive           = _Go.GetCompItem<TextMeshProUGUI>("revive_text");
            m_AnimLoadingAds       = _Go.GetCompItem<Animator>("loading_ads_anim");
            m_MoneyInBankIcon      = _Go.GetCompItem<Image>("money_icon_1");
            m_MoneyIconInPayButton = _Go.GetCompItem<Image>("money_icon_2");
            m_IconWatchAds         = _Go.GetCompItem<Image>("watch_ads_icon");
            m_Countdown            = _Go.GetCompItem<Image>("round_filled_border");
            m_CountdownBackground  = _Go.GetCompItem<Image>("round_filled_border_back");
        }
        
        protected override void LocalizeTextObjectsOnLoad()
        {
            static string TextFormula(string _Text) => _Text.ToUpper(CultureInfo.CurrentUICulture);
            var locTextInfos = new[]
            {
                new LocTextInfo(m_MoneyInBankText, ETextType.MenuUI_H1, "empty_key",
                    _TextLocalizationType: ETextLocalizationType.OnlyFont), 
                new LocTextInfo(m_TextPayMoneyCount, ETextType.MenuUI_H1, "empty_key",
                    _TextLocalizationType: ETextLocalizationType.OnlyFont), 
                new LocTextInfo(m_TextYouHaveMoney, ETextType.MenuUI_H1, "you_have", TextFormula),
                new LocTextInfo(m_TextContinue,     ETextType.MenuUI_H1, "continue", TextFormula),
                new LocTextInfo(m_TextRevive,       ETextType.MenuUI_H1, "revive?",  TextFormula)
            };
            foreach (var locTextInfo in locTextInfos)
                Managers.LocalizationManager.AddLocalization(locTextInfo);
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
                    if (_Broken && m_WentToShopPanel)
                        return;
                    OnClose(() =>
                    {
                        if (_Broken) RaiseContinueCommand();
                        else         RaiseLoadFirstLevelInGroupCommand();
                    });
                });
        }
        
        private void RaiseLoadFirstLevelInGroupCommand()
        {
            TimePauser.UnpauseTimeInGame();
            TimePauser.UnpauseTimeInUi();
            BetweenLevelAdShower.ShowAdEnabled = false;
            var arguments = new Dictionary<string, object>
            {
                {KeySource, ParameterSourceCharacterDiedPanel}
            };
            LevelStageSwitcher.SwitchLevelStage(EInputCommand.StartUnloadingLevel, arguments);
        }

        private void RaiseContinueCommand()
        {
            TimePauser.UnpauseTimeInGame();
            TimePauser.UnpauseTimeInUi();
            CommandsProceeder.RaiseCommand(
                EInputCommand.ReadyToStartLevel,
                Model.LevelStaging.Arguments, 
                true);
        }

        #endregion
    }
}