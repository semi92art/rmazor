using System.Collections.Generic;
using Common.Constants;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities.UI;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Managers.IAP;
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
using UnityEngine.UI;

namespace RMAZOR.UI.Panels
{
    public interface IDisableAdsDialogPanel : IDialogPanel { }

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
            m_StrikeoutLine;
        private Animator     m_Animator;
        private Button       m_BuyButton;
        private Button       m_CloseButton;
        private bool         m_LastItemPriceRequestIsFail;
        private string       m_OldPriceTextString;
        private string       m_NewPriceTextString;
        private ShopItemArgs m_DisableAdsShopItemArgs;

        #endregion

        #region inject

        private IModelGame                          Model                          { get; }
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }
        private IFontProvider                       FontProvider                   { get; }

        private DisableAdsDialogPanel(
            IModelGame                          _Model,
            IManagersGetter                     _Managers,
            IUITicker                           _Ticker,
            ICameraProvider                     _CameraProvider,
            IColorProvider                      _ColorProvider,
            IViewTimePauser                     _TimePauser,
            IViewInputCommandsProceeder         _CommandsProceeder,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker,
            IFontProvider                       _FontProvider) 
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
            FontProvider                   = _FontProvider;
        }

        #endregion

        #region api

        public override EDialogViewerType DialogViewerType => EDialogViewerType.Medium1;
        public override Animator          Animator         => m_Animator;

        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            Managers.LocalizationManager.LanguageChanged += OnLanguageChanged;
            var go  = Managers.PrefabSetManager.InitUiPrefab(
                UIUtils.UiRectTransform(_Container, RectTransformLite.FullFill),
                CommonPrefabSetNames.DialogPanels, "disable_ads_panel");
            PanelRectTransform = go.RTransform();
            go.SetActive(false);
            GetPrefabContentObjects(go);
            SubscribeButtons();
            m_DisableAdsShopItemArgs = GetDisableAdsShopItemArgs();
        }

        private void OnLanguageChanged(ELanguage _Language)
        {
            SetBuyButtonText();
        }

        public override void OnDialogStartAppearing()
        {
            var font = FontProvider.GetFont(ETextType.MenuUI, Managers.LocalizationManager.GetCurrentLanguage());
            m_BuyButtonTextDefault.font  = font;
            m_BuyButtonTextPriceOld.font = font;
            m_BuyButtonTextPriceNew.font = font;
            if (m_DisableAdsShopItemArgs.Result() != EShopProductResult.Success)
                m_DisableAdsShopItemArgs = GetDisableAdsShopItemArgs();
            SetBuyButtonText();
            base.OnDialogStartAppearing();
        }

        #endregion

        #region nonpublic methods

        private void GetPrefabContentObjects(GameObject _Go)
        {
            m_Animator              = _Go.GetCompItem<Animator>("animator");
            m_CloseButton           = _Go.GetCompItem<Button>("close_button");
            m_BuyButton             = _Go.GetCompItem<Button>("buy_button");
            m_BuyButtonTextPriceOld = _Go.GetCompItem<TextMeshProUGUI>("buy_button_text_price_old");
            m_BuyButtonTextPriceNew = _Go.GetCompItem<TextMeshProUGUI>("buy_button_text_price_new");
            m_BuyButtonTextDefault  = _Go.GetCompItem<TextMeshProUGUI>("buy_button_text_default");
            m_SalesIcon             = _Go.GetCompItem<Image>("sales_icon");
            m_StrikeoutLine         = _Go.GetCompItem<Image>("strikeout_line");
        }

        private void SubscribeButtons()
        {
            m_CloseButton.onClick.AddListener(OnButtonCloseClick);
            m_BuyButton.onClick.AddListener(OnButtonBuyClick);
        }

        private void OnButtonCloseClick()
        {
            base.OnClose(() =>
            {
                SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.UnPauseLevel);
            });
            Managers.AudioManager.PlayClip(CommonAudioClipArgs.UiButtonClick);
        }

        private void OnButtonBuyClick()
        {
            SendButtonPressedAnalytic();
            Managers.ShopManager.Purchase(PurchaseKeys.NoAds);
        }

        private void SetBuyButtonText()
        {
            var locMan = Managers.LocalizationManager;
            string buyText = locMan.GetTranslation("buy");
            m_BuyButtonTextDefault.text = buyText;
            if (m_DisableAdsShopItemArgs == null
                || m_BuyButtonTextDefault.IsNull()
                || m_BuyButtonTextPriceOld.IsNull()
                || m_BuyButtonTextPriceNew.IsNull())
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
                (float)m_DisableAdsShopItemArgs.LocalizedPrice / SaleCoefficient);
            m_NewPriceTextString = m_DisableAdsShopItemArgs.LocalizedPriceString;
            m_BuyButtonTextPriceOld.text = m_OldPriceTextString;
            m_BuyButtonTextPriceNew.text = m_NewPriceTextString;
            var font = FontProvider.GetFont(ETextType.MenuUI, locMan.GetCurrentLanguage());
            m_BuyButtonTextPriceOld.font = font;
            if (m_NewPriceTextString != buyText)
                font = FontProvider.GetFont(ETextType.MenuUI, ELanguage.English);
            m_BuyButtonTextPriceNew.font = font;
        }

        private ShopItemArgs GetDisableAdsShopItemArgs()
        {
            return Managers.ShopManager.GetItemInfo(PurchaseKeys.NoAds);
        }
        
        private void SendButtonPressedAnalytic()
        {
            string levelType = (string) Model.LevelStaging.Arguments.GetSafe(
                CommonInputCommandArg.KeyCurrentLevelType, out _);
            bool isThisLevelBonus = levelType == CommonInputCommandArg.ParameterLevelTypeBonus;
            const string analyticId = "button_buy_no_ads_in_disable_ads_panel_pressed";
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