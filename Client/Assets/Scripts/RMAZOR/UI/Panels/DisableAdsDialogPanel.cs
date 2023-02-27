using System.Collections.Generic;
using Common.Constants;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers.IAP;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
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
    public interface IDisableAdsDialogPanel : IDialogPanel
    {
        UnityAction OnClosePanelAction { get; set; }
    }

    public class DisableAdsDialogPanel : DialogPanelBase, IDisableAdsDialogPanel
    {
        #region constants

        private const float SaleCoefficient = 0.5f;

        #endregion
        
        #region nonpublic members

        private TextMeshProUGUI
            m_BuyButtonTextPriceOld, 
            m_BuyButtonTextPriceNew,
            m_BuyButtonTextDefault;
        private Image       
            m_SalesIcon,
            m_StrikeoutLine,
            m_BackGlowDark;
        private Animator     m_Animator;
        private Button       m_BuyButton;
        private Button       m_CloseButton;
        private bool         m_LastItemPriceRequestIsFail;
        private string       m_OldPriceTextString;
        private string       m_NewPriceTextString;
        private ShopItemArgs m_DisableAdsShopItemArgs;
        
        protected override string PrefabName => "disable_ads_panel";
        
        #endregion

        #region inject

        private IModelGame              Model              { get; }
        private IViewLevelStageSwitcher LevelStageSwitcher { get; }

        private DisableAdsDialogPanel(
            IModelGame                  _Model,
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewTimePauser             _TimePauser,
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
        
        
        public UnityAction OnClosePanelAction { get; set; }

        public override int DialogViewerId => DialogViewerIdsCommon.FullscreenCommon;

        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            Managers.LocalizationManager.LanguageChanged += OnLanguageChanged;
            m_DisableAdsShopItemArgs = GetDisableAdsShopItemArgs();
            Managers.ShopManager.AddPurchaseAction(PurchaseKeys.NoAds, OnButtonCloseClick);
        }
        
        #endregion

        #region nonpublic methods
        
        protected override void OnDialogStartAppearing()
        {
            m_BackGlowDark.enabled = Model.LevelStaging.LevelStage == ELevelStage.None;
            var font = Managers.LocalizationManager.GetFont(ETextType.MenuUI);
            m_BuyButtonTextDefault.font  = font;
            m_BuyButtonTextPriceOld.font = font;
            m_BuyButtonTextPriceNew.font = font;
            if (m_DisableAdsShopItemArgs.Result() != EShopProductResult.Success)
                m_DisableAdsShopItemArgs = GetDisableAdsShopItemArgs();
            SetBuyButtonText();
            base.OnDialogStartAppearing();
        }
        
        private void OnLanguageChanged(ELanguage _Language)
        {
            SetBuyButtonText();
        }

        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            m_Animator              = _Go.GetCompItem<Animator>("animator");
            m_CloseButton           = _Go.GetCompItem<Button>("close_button");
            m_BuyButton             = _Go.GetCompItem<Button>("buy_button");
            m_BuyButtonTextPriceOld = _Go.GetCompItem<TextMeshProUGUI>("buy_button_text_price_old");
            m_BuyButtonTextPriceNew = _Go.GetCompItem<TextMeshProUGUI>("buy_button_text_price_new");
            m_BuyButtonTextDefault  = _Go.GetCompItem<TextMeshProUGUI>("buy_button_text_default");
            m_SalesIcon             = _Go.GetCompItem<Image>("sales_icon");
            m_StrikeoutLine         = _Go.GetCompItem<Image>("strikeout_line");
            m_BackGlowDark          = _Go.GetCompItem<Image>("back_glow_dark");
        }

        protected override void LocalizeTextObjectsOnLoad() { }

        protected override void SubscribeButtonEvents()
        {
            m_CloseButton.onClick.AddListener(OnButtonCloseClick);
            m_BuyButton.onClick.AddListener(OnButtonBuyClick);
        }

        private void OnButtonCloseClick()
        {
            OnClose(() =>
            {
                OnClosePanelAction?.Invoke();
                LevelStageSwitcher.SwitchLevelStage(EInputCommand.UnPauseLevel);
            });
            PlayButtonClickSound();
        }

        private void OnButtonBuyClick()
        {
            SendButtonPressedAnalytic();
            Managers.ShopManager.Purchase(PurchaseKeys.NoAds);
        }

        private void SetBuyButtonText()
        {
            if (!AllObjectsAreNotNull())
                return;
            var locMan = Managers.LocalizationManager;
            string buyText = locMan.GetTranslation("buy");
            m_BuyButtonTextDefault.text = buyText;
            if (m_DisableAdsShopItemArgs == null)
            {
                m_BuyButtonTextDefault.enabled  = true;
                m_BuyButtonTextPriceOld.enabled = false;
                m_StrikeoutLine.enabled         = false;
                m_BuyButtonTextPriceNew.enabled = false;
                m_SalesIcon.enabled             = false;
                return;
            }
            var argsResult = m_DisableAdsShopItemArgs.Result();
            bool argsResultSuccess = argsResult == EShopProductResult.Success;
            m_BuyButtonTextDefault.enabled  = !argsResultSuccess;
            m_BuyButtonTextPriceOld.enabled = argsResultSuccess;
            m_BuyButtonTextPriceNew.enabled = argsResultSuccess;
            m_StrikeoutLine.enabled         = argsResultSuccess;
            m_SalesIcon.enabled             = argsResultSuccess;
            int k = 0;
            string currencyString = string.Empty;
            while (char.IsLetter(m_DisableAdsShopItemArgs.LocalizedPriceString[k]))
                currencyString += m_DisableAdsShopItemArgs.LocalizedPriceString[k++];
            m_OldPriceTextString = currencyString + " " + Mathf.CeilToInt(
                (float)m_DisableAdsShopItemArgs.LocalizedPrice / SaleCoefficient) + ".00";
            m_NewPriceTextString = m_DisableAdsShopItemArgs.LocalizedPriceString;
            m_BuyButtonTextPriceOld.text = m_OldPriceTextString;
            m_BuyButtonTextPriceNew.text = m_NewPriceTextString;
            var font = Managers.LocalizationManager.GetFont(ETextType.MenuUI);
            m_BuyButtonTextPriceOld.font = font;
            if (m_NewPriceTextString != buyText)
                font = Managers.LocalizationManager.GetFont(ETextType.MenuUI, ELanguage.English);
            m_BuyButtonTextPriceNew.font = font;
        }

        private ShopItemArgs GetDisableAdsShopItemArgs()
        {
            return Managers.ShopManager.GetItemInfo(PurchaseKeys.NoAds);
        }
        
        private void SendButtonPressedAnalytic()
        {
            string levelType = (string) Model.LevelStaging.Arguments.GetSafe(
                ComInComArg.KeyCurrentLevelType, out _);
            bool isThisLevelBonus = levelType == ComInComArg.ParameterLevelTypeBonus;
            const string analyticId = "button_buy_no_ads_in_disable_ads_panel_pressed";
            var eventData = new Dictionary<string, object>
            {
                {AnalyticIds.ParameterLevelIndex, Model.LevelStaging.LevelIndex},
                {AnalyticIds.ParameterLevelType, isThisLevelBonus ? 2 : 1},
            };
            Managers.AnalyticsManager.SendAnalytic(analyticId, eventData);
        }

        private bool AllObjectsAreNotNull()
        {
            return m_BuyButtonTextDefault.IsNotNull()
                   && m_BuyButtonTextPriceNew.IsNotNull()
                   && m_BuyButtonTextPriceOld.IsNotNull()
                   && m_Animator.IsNotNull()
                   && m_BuyButton.IsNotNull()
                   && m_CloseButton.IsNotNull()
                   && m_SalesIcon.IsNotNull()
                   && m_StrikeoutLine.IsNotNull();
        }

        #endregion
    }
}