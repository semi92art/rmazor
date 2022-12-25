using System.Collections.Generic;
using Common.Constants;
using Common.Extensions;
using Common.Managers.IAP;
using Common.UI;
using Common.Utils;
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

        #region nonpublic members

        private Animator        m_Animator;
        private TextMeshProUGUI m_BuyButtonTextTitle, m_BuyButtonTextPrice;
        private Button          m_BuyButton;
        private Button          m_CloseButton;
        private bool            m_LastItemPriceRequestIsFail;
        private string          m_NoAdsText, m_PriceText;
        private ShopItemArgs    m_DisableAdsShopItemArgs;

        #endregion

        #region inject
        
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }
        private IFontProvider                       FontProvider                   { get; }

        private DisableAdsDialogPanel(
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
            m_BuyButtonTextTitle.font = font;
            if (m_DisableAdsShopItemArgs.Result() != EShopProductResult.Success)
                m_DisableAdsShopItemArgs = GetDisableAdsShopItemArgs();
            SetBuyButtonText();
            base.OnDialogStartAppearing();
        }

        #endregion

        #region nonpublic methods

        private void GetPrefabContentObjects(GameObject _Go)
        {
            m_Animator           = _Go.GetCompItem<Animator>("animator");
            m_CloseButton        = _Go.GetCompItem<Button>("close_button");
            m_BuyButton          = _Go.GetCompItem<Button>("buy_button");
            m_BuyButtonTextTitle = _Go.GetCompItem<TextMeshProUGUI>("buy_button_text_title");
            m_BuyButtonTextPrice = _Go.GetCompItem<TextMeshProUGUI>("buy_button_text_price");
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
            BuyHideAdsItem();
            OnButtonCloseClick();
        }
        
        private void BuyHideAdsItem()
        {
            Managers.AnalyticsManager.SendAnalytic(
                AnalyticIds.Purchase,
                new Dictionary<string, object> { {AnalyticIds.ParameterPurchaseProductId, "no_ads"}});
            Managers.AdsManager.ShowAds = false;
            string dialogTitle = Managers.LocalizationManager.GetTranslation("purchase") + ":";
            string dialogText = Managers.LocalizationManager.GetTranslation("mandatory_ads_disabled");
            MazorCommonUtils.ShowAlertDialog(dialogTitle, dialogText);
        }

        private void SetBuyButtonText()
        {
            var locMan = Managers.LocalizationManager;
            m_NoAdsText = locMan.GetTranslation("no_ads");
            string buyText = locMan.GetTranslation("buy"); 
            m_PriceText = m_DisableAdsShopItemArgs.Result() != EShopProductResult.Success ? 
                locMan.GetTranslation("buy") : m_DisableAdsShopItemArgs.Price;
            m_BuyButtonTextTitle.text = m_NoAdsText;
            m_BuyButtonTextPrice.text = m_PriceText;
            var font = FontProvider.GetFont(ETextType.MenuUI, locMan.GetCurrentLanguage());
            m_BuyButtonTextTitle.font = font;
            if (m_PriceText != buyText)
                font = FontProvider.GetFont(ETextType.MenuUI, ELanguage.English);
            m_BuyButtonTextPrice.font = font;
        }

        private ShopItemArgs GetDisableAdsShopItemArgs()
        {
            return Managers.ShopManager.GetItemInfo(PurchaseKeys.NoAds);
        }

        #endregion
    }
}