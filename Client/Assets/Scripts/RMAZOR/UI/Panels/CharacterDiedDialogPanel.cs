using System.Collections;
using Common;
using Common.CameraProviders;
using Common.CameraProviders.Camera_Effects_Props;
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
        private AnimationTriggerer m_Triggerer;
        private Image              m_MoneyInBankIcon;
        private Image              m_MoneyIconInPayButton;
        private Image              m_IconWatchAds;
        private TextMeshProUGUI    m_TextYouHaveMoney;
        private TextMeshProUGUI    m_MoneyInBankText;
        private TextMeshProUGUI    m_TextContinue;
        private TextMeshProUGUI    m_TextPayMoneyCount;
        private TextMeshProUGUI    m_TextNotEnoughMoney;
        private Button             m_ButtonWatchAds;
        private Button             m_ButtonPayMoney;
        private Animator           m_AnimLoadingAds;
        private Image              m_RoundFilledBorder;
        private bool               m_AdsWatched;
        private bool               m_MoneyPayed;
        private long               m_MoneyCount;
        private bool               m_PanelShowing;
        private bool               m_WentToShopPanel;
        private float              m_BorderFillAmount;

        #endregion

        #region inject

        private GlobalGameSettings          GlobalGameSettings      { get; }
        private IModelGame                  Model                   { get; }
        private IViewInputCommandsProceeder CommandsProceeder       { get; }
        private IViewBetweenLevelAdLoader   BetweenLevelAdLoader    { get; }

        protected CharacterDiedDialogPanel(
            GlobalGameSettings          _GlobalGameSettings,
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
            m_PanelAnimator           = go.GetCompItem<Animator>(          "animator");
            m_Triggerer               = go.GetCompItem<AnimationTriggerer>("triggerer");
            m_ButtonWatchAds          = go.GetCompItem<Button>(            "watch_ads_button");
            m_ButtonPayMoney          = go.GetCompItem<Button>(            "pay_money_button");
            m_TextYouHaveMoney        = go.GetCompItem<TextMeshProUGUI>(   "you_have_text");
            m_MoneyInBankText         = go.GetCompItem<TextMeshProUGUI>(   "money_count_text");
            m_TextPayMoneyCount       = go.GetCompItem<TextMeshProUGUI>(   "pay_money_count_text");
            m_TextContinue            = go.GetCompItem<TextMeshProUGUI>(   "continue_text");
            m_TextNotEnoughMoney      = go.GetCompItem<TextMeshProUGUI>(   "not_enough_money_text");
            m_AnimLoadingAds          = go.GetCompItem<Animator>(          "loading_ads_anim");
            m_MoneyInBankIcon         = go.GetCompItem<Image>(             "money_icon_1");
            m_MoneyIconInPayButton    = go.GetCompItem<Image>(             "money_icon_2");
            m_IconWatchAds            = go.GetCompItem<Image>(             "watch_ads_icon");
            m_RoundFilledBorder       = go.GetCompItem<Image>(             "round_filled_border");
            m_Triggerer.Trigger1      = () => Cor.Run(StartCountdown());
            m_TextPayMoneyCount.text  = GlobalGameSettings.payToContinueMoneyCount.ToString();
            Managers.LocalizationManager.AddTextObject(
                new LocalizableTextObjectInfo(m_TextYouHaveMoney, ETextType.MenuUI, "you_have",
                    _T => _T.ToUpper()));
            Managers.LocalizationManager.AddTextObject(
                new LocalizableTextObjectInfo(m_TextContinue, ETextType.MenuUI, "continue",
                    _T => _T.ToUpper()));
            Managers.LocalizationManager.AddTextObject(
                new LocalizableTextObjectInfo(m_TextNotEnoughMoney, ETextType.MenuUI, "not_enough_money",
                    _T => _T.ToUpper()));
            var moneyIconSprite = Managers.PrefabSetManager.GetObject<Sprite>(
                "icons", "icon_coin_ui");
            m_MoneyInBankIcon.sprite = moneyIconSprite;
            m_MoneyIconInPayButton.sprite = moneyIconSprite;
            m_ButtonWatchAds.onClick.AddListener(OnWatchAdsButtonClick);
            m_ButtonPayMoney.onClick.AddListener(OnPayMoneyButtonClick);
            m_RoundFilledBorder.color = ColorProvider.GetColor(ColorIds.UiBorder);
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
            m_BorderFillAmount = 1f;
            m_AdsWatched = false;
            m_MoneyPayed = false;
            m_WentToShopPanel = false;
            m_PanelShowing = true;
            m_RoundFilledBorder.fillAmount = 1f;
            CameraProvider.EnableEffect(ECameraEffect.Glitch, true);
            var props = new FastGlitchProps
            {
                ChromaticGlitch = 0.03f,
                FrameGlitch = 0.03f,
                PixelGlitch = 0.3f
            };
            CameraProvider.SetEffectProps(ECameraEffect.Glitch, props);
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
            Cor.Run(Cor.WaitNextFrame(() =>
            {
                CommandsProceeder.LockCommands(
                    new [] { EInputCommand.ShopMenu, EInputCommand.SettingsMenu},
                    nameof(ICharacterDiedDialogPanel));
            }));
            base.OnDialogStartAppearing();
        }

        public override void OnDialogDisappeared()
        {
            CameraProvider.EnableEffect(ECameraEffect.Glitch, false);
            m_PanelShowing = false;
            CommandsProceeder.UnlockCommands(
                new [] { EInputCommand.ShopMenu, EInputCommand.SettingsMenu},
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
                m_RoundFilledBorder.color = _Color;
        }
        
        private void OnWatchAdsButtonClick()
        {
            Managers.AnalyticsManager.SendAnalytic(AnalyticIds.WatchAdInCharacterDiedPanelPressed);
            Managers.AdsManager.ShowRewardedAd(_OnReward: () => m_AdsWatched = true, _Skippable: false);
        }

        private void OnPayMoneyButtonClick()
        {
            bool isMoneyEnough = m_MoneyCount >= GlobalGameSettings.payToContinueMoneyCount;
            if (!isMoneyEnough)
            {
                RegisterPurchaseActionsFinishCurrentLevel();
                CommandsProceeder.RaiseCommand(EInputCommand.ShopMenu,
                    new object[] {CommonInputCommandArgs.LoadShopPanelFromCharacterDiedPanel}, true);
                m_WentToShopPanel = true;
                CameraProvider.EnableEffect(ECameraEffect.Glitch, false);
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
            m_TextNotEnoughMoney.enabled   = false;
        }

        private IEnumerator StartCountdown()
        {
            yield return Cor.Lerp(
                Ticker,
                CountdownTime * m_BorderFillAmount,
                m_BorderFillAmount,
                0f,
                _P => m_RoundFilledBorder.fillAmount = m_BorderFillAmount = _P,
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
        
        private void RegisterPurchaseActionsFinishCurrentLevel()
        {
            // TODO по идее обновлять инфу нужно сразу после покупки, но хуй с ним
            // var set = Managers.PrefabSetManager.GetObject<ShopPanelMoneyItemsScriptableObject>(
            //     "shop_items", "shop_money_items_set").set;
            // foreach (var item in set)
            // {
            //     Managers.ShopManager.AddPurchaseAction(item.purchaseKey, 2, () =>
            //     {
            //         var commandsHistoryReversed = CommandsProceeder.CommandsHistory.ToArray().Reverse();
            //         System.Tuple<EInputCommand, object[]> lastShopCommand = commandsHistoryReversed.FirstOrDefault(
            //             _Command => _Command.Item1 == EInputCommand.ShopMenu);
            //
            //         var savedGameEntity = Managers.ScoreManager.GetSavedGameProgress(
            //             CommonData.SavedGameFileName, 
            //             true);
            //         
            //         if (lastShopCommand == null ||
            //             !lastShopCommand.Item2.Contains(CommonInputCommandArgs.LoadShopPanelFromCharacterDiedPanel))
            //         {
            //             return;
            //         }
            //         var savedGame = new SavedGame
            //         {
            //             FileName = CommonData.SavedGameFileName,
            //             Money = m_MoneyCount - GlobalGameSettings.payToContinueMoneyCount,
            //             Level = Model.LevelStaging.LevelIndex
            //         };
            //         Managers.ScoreManager.SaveGameProgress(savedGame, false);
            //         m_MoneyPayed = true;
            //         var dv1 = DialogViewersController.GetViewer(ShopDialogPanel.DialogViewerType);
            //         dv1.Back();
            //         dv1 = DialogViewersController.GetViewer(DialogViewerType);
            //         dv1.Back();
            //         RaiseContinueCommand();
            //     });
            // }
        }
        
        private void RaiseContinueCommand()
        {
            CommandsProceeder.RaiseCommand(
                EInputCommand.ReadyToStartLevel,
                null, 
                true);
        }
        
        #endregion
    }
}