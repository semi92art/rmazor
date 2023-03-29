using System.Collections;
using System.Collections.Generic;
using Common;
using Common.Constants;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.UI.DialogViewers;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.UI.PanelItems.Customoze_Character_Panel_Items;
using RMAZOR.UI.PanelItems.Shop_Panel_Items;
using RMAZOR.UI.Utils;
using RMAZOR.Views.Common;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;
using UnityEngine.UI;

namespace RMAZOR.UI.Panels.ShopPanels
{
    public interface IShopDialogPanel : IDialogPanel, ISetOnCloseFinishAction
    {
        event UnityAction<int> BadgesNumberChanged;
        int                    GetBadgesNumber();
    }
    
    public class ShopDialogPanel : DialogPanelBase, IShopDialogPanel
    {
        #region nonpublic members

        private Button               m_ButtonClose;
        private ShopItemSpecialOffer m_ItemSpecialOffer;
        
        private ShopItemRealCurrencyV2
            m_ItemCoins1,  
            m_ItemCoins2,
            m_ItemCoins3,
            m_ItemNoAds,
            m_ItemX2NewCoins;
        
        private ShopItemGameCurrencyV2 
            m_ItemRetroMode,
            m_ItemFullCustomization;
        
        private UnityAction m_OnCloseFinishAction;
        
        protected override string PrefabName => "shop_panel";

        #endregion

        #region inject

        private ISpecialOfferTimerController SpecialOfferTimerController { get; }
        private IShopBuyActionsProvider      ShopBuyActionsProvider      { get; }
        private IShopMoneyDialogPanel        ShopMoneyDialogPanel        { get; }
        private IDialogViewersController     DialogViewersController     { get; }

        private ShopDialogPanel(
            IManagersGetter              _Managers,
            IUITicker                    _Ticker,
            ICameraProvider              _CameraProvider,
            IColorProvider               _ColorProvider,
            IViewTimePauser              _TimePauser,
            IViewInputCommandsProceeder  _CommandsProceeder,
            ISpecialOfferTimerController _SpecialOfferTimerController,
            IShopBuyActionsProvider      _ShopBuyActionsProvider,
            IShopMoneyDialogPanel        _ShopMoneyDialogPanel,
            IDialogViewersController     _DialogViewersController) 
            : base(
                _Managers,
                _Ticker,
                _CameraProvider, 
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder)
        {
            SpecialOfferTimerController = _SpecialOfferTimerController;
            ShopBuyActionsProvider      = _ShopBuyActionsProvider;
            ShopMoneyDialogPanel        = _ShopMoneyDialogPanel;
            DialogViewersController     = _DialogViewersController;
        }

        #endregion

        #region api

        public override int           DialogViewerId => DialogViewerIdsCommon.FullscreenCommon;
        public event UnityAction<int> BadgesNumberChanged;

        public int GetBadgesNumber()
        {
            var tabBadgesDict = SaveUtils.GetValue(SaveKeysRmazor.TabBadgesDict);
            return tabBadgesDict.ContainsKey("shop") ? 0 : 1;
        }
        
        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            SpecialOfferTimerController.Init();
            Cor.Run(InitPanelItemsCoroutine());
        }
        
        public void SetOnCloseFinishAction(UnityAction _Action)
        {
            m_OnCloseFinishAction = _Action;
        }

        #endregion

        #region nonpublic methods
        
        private void OnButtonCloseClick()
        {
            OnClose(m_OnCloseFinishAction);
            PlayButtonClickSound();
        }

        protected override void OnDialogStartAppearing()
        {
            SpecialOfferTimerController.ShownThisSession = true;
            m_ItemSpecialOffer     .UpdateState();
            m_ItemCoins1           .UpdateState();
            m_ItemCoins2           .UpdateState();
            m_ItemCoins3           .UpdateState();
            m_ItemNoAds            .UpdateState();
            m_ItemRetroMode        .UpdateState();
            m_ItemFullCustomization.UpdateState();
            base.OnDialogStartAppearing();
        }

        protected override void OnDialogAppeared()
        {
            base.OnDialogAppeared();
            var tabBadgesDict = SaveUtils.GetValue(SaveKeysRmazor.TabBadgesDict);
            tabBadgesDict.SetSafe("shop", new List<Badge> {new Badge{Number = 0}});
            BadgesNumberChanged?.Invoke(0);
        }

        private void UpdateGameCurrencyShopItemsState()
        {
            m_ItemRetroMode        .UpdateState();
            m_ItemFullCustomization.UpdateState();
        }

        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            m_ButtonClose           = _Go.GetCompItem<Button>("close_button");
            m_ItemSpecialOffer      = _Go.GetCompItem<ShopItemSpecialOffer>("item_special_offer");
            m_ItemCoins1            = _Go.GetCompItem<ShopItemRealCurrencyV2>("item_coins_1");
            m_ItemCoins2            = _Go.GetCompItem<ShopItemRealCurrencyV2>("item_coins_2");
            m_ItemCoins3            = _Go.GetCompItem<ShopItemRealCurrencyV2>("item_coins_3");
            m_ItemRetroMode         = _Go.GetCompItem<ShopItemGameCurrencyV2>("item_retro_mode");
            m_ItemFullCustomization = _Go.GetCompItem<ShopItemGameCurrencyV2>("item_full_customization");
            m_ItemNoAds             = _Go.GetCompItem<ShopItemRealCurrencyV2>("item_no_ads");
            m_ItemX2NewCoins        = _Go.GetCompItem<ShopItemRealCurrencyV2>("item_x2_all_new_coins");
        }

        protected override void LocalizeTextObjectsOnLoad() { }
        
        protected override void SubscribeButtonEvents()
        {
            m_ButtonClose.SetOnClick(OnButtonCloseClick);
        }
        
        private IEnumerator InitPanelItemsCoroutine()
        {
            yield return Cor.WaitWhile(() => !Managers.ShopManager.Initialized);
            InitPanelItems();
        }
        
        private void InitPanelItems()
        {
            InitSpecialOfferItem();
            InitCoinItems();
            InitRetroModeItem();
            InitFullCustomizationItem();
            InitNoAdsItem();
            InitX2CoinsItem();
        }

        private void InitSpecialOfferItem()
        {
            var itemInfo = new ViewShopItemInfoSpecialOffer
            {
                BuyAction               = ShopBuyActionsProvider.GetAction("special_offer"),
                IapProductInfo          = Managers.ShopManager.GetItemInfo(PurchaseKeys.SpecialOffer),
                NameLocalizationKey     = "special_offer",
                ContentLocalizationKeys = new[]
                {
                    "so_disable_ads",
                    "so_special_coin_pack",
                    "so_x3_new_coins",
                    "so_full_customization",
                    "so_retro_mode"
                }
            };
            m_ItemSpecialOffer.Init(
                Ticker,
                Managers.AudioManager,
                Managers.LocalizationManager,
                Managers.AnalyticsManager,
                Managers.ShopManager,
                SpecialOfferTimerController,
                itemInfo,
                false);
        }

        private void InitCoinItems()
        {
            var (t, am, lm, anm, sm) = 
                (Ticker, Managers.AudioManager, Managers.LocalizationManager, Managers.AnalyticsManager, Managers.ShopManager);
            var info1 = new ViewShopItemInfoRealCurrencyV2
            {
                BuyAction           = ShopBuyActionsProvider.GetAction("coins_1"),
                IapProductInfo      = Managers.ShopManager.GetItemInfo(PurchaseKeys.Money1),
                NameLocalizationKey = "coins_pack_small",
                HasReceiptAdditionalPredicate = () => null
            };
            m_ItemCoins1.Init(t, am, lm, anm, sm, info1);
            var info2 = new ViewShopItemInfoRealCurrencyV2
            {
                BuyAction           = ShopBuyActionsProvider.GetAction("coins_2"),
                IapProductInfo      = Managers.ShopManager.GetItemInfo(PurchaseKeys.Money2),
                NameLocalizationKey = "coins_pack_medium",
                HasReceiptAdditionalPredicate = () => null
            };
            m_ItemCoins2.Init(t, am, lm, anm, sm, info2);
            var info3 = new ViewShopItemInfoRealCurrencyV2
            {
                BuyAction           = ShopBuyActionsProvider.GetAction("coins_3"),
                IapProductInfo      = Managers.ShopManager.GetItemInfo(PurchaseKeys.Money3),
                NameLocalizationKey = "coins_pack_large",
                HasReceiptAdditionalPredicate = () => null
            };
            m_ItemCoins3.Init(t, am, lm, anm, sm, info3);
        }

        private void InitRetroModeItem()
        {
            var info = new ViewShopItemInfoGameCurrencyV2
            {
                BuyAction                      = ShopBuyActionsProvider.GetAction("retro_mode"),
                Consumable                     = false,
                Id                             = "so_retro_mode",
                NameLocalizationKey            = "so_retro_mode",
                Price                          = 500,
                HasReceiptAdditionalPredicate  = SpecialOfferHasReceipt,
                BuyButtonClickIfNotEnoughMoney = OnAddMoneyButtonClick
            };
            m_ItemRetroMode.Purchased += UpdateGameCurrencyShopItemsState;
            m_ItemRetroMode.Init(
                Ticker, 
                Managers.AudioManager,
                Managers.LocalizationManager, 
                Managers.AnalyticsManager, 
                Managers.ScoreManager,
                info);
        }

        private void InitFullCustomizationItem()
        {
            var info = new ViewShopItemInfoGameCurrencyV2
            {
                BuyAction                      = ShopBuyActionsProvider.GetAction("full_customization"),
                Consumable                     = false,
                Id                             = "so_full_customization",
                NameLocalizationKey            = "so_full_customization",
                Price                          = 500,
                HasReceiptAdditionalPredicate  = SpecialOfferHasReceipt,
                BuyButtonClickIfNotEnoughMoney = OnAddMoneyButtonClick
            };
            m_ItemFullCustomization.Purchased += UpdateGameCurrencyShopItemsState;
            m_ItemFullCustomization.Init(
                Ticker, 
                Managers.AudioManager,
                Managers.LocalizationManager, 
                Managers.AnalyticsManager, 
                Managers.ScoreManager,
                info);
        }

        private void InitNoAdsItem()
        {
            var itemInfo = new ViewShopItemInfoRealCurrencyV2
            {
                BuyAction                     = ShopBuyActionsProvider.GetAction("no_ads"),
                IapProductInfo                = Managers.ShopManager.GetItemInfo(PurchaseKeys.NoAds),
                NameLocalizationKey           = "no_ads_2",
                HasReceiptAdditionalPredicate = SpecialOfferHasReceipt
            };
            m_ItemNoAds.Init(
                Ticker,
                Managers.AudioManager,
                Managers.LocalizationManager,
                Managers.AnalyticsManager,
                Managers.ShopManager,
                itemInfo);
        }

        private void InitX2CoinsItem()
        {
            var itemInfo = new ViewShopItemInfoRealCurrencyV2
            {
                BuyAction                     = ShopBuyActionsProvider.GetAction("x2_new_coins"),
                IapProductInfo                = Managers.ShopManager.GetItemInfo(PurchaseKeys.X2NewCoins),
                NameLocalizationKey           = "x2_new_coins",
                HasReceiptAdditionalPredicate = SpecialOfferHasReceipt
            };
            m_ItemX2NewCoins.Init(
                Ticker,
                Managers.AudioManager,
                Managers.LocalizationManager,
                Managers.AnalyticsManager,
                Managers.ShopManager,
                itemInfo);
        }
        
        private bool? SpecialOfferHasReceipt()
        {
            var productInfo = Managers.ShopManager.GetItemInfo(PurchaseKeys.SpecialOffer);
            if (productInfo == null)
                return null;
            var idsOfPurchasedItems = SaveUtils.GetValue(SaveKeysMazor.BoughtPurchaseIds) ?? new List<int>();
            if (idsOfPurchasedItems.Contains(productInfo.PurchaseKey))
                return true;
            return productInfo.Type == ProductType.NonConsumable
                   && productInfo.HasReceipt;
        }
        
        private void OnAddMoneyButtonClick()
        {
            PlayButtonClickSound();
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.MainMenuAddMoneyButtonClick);
            var dv = DialogViewersController.GetViewer(ShopMoneyDialogPanel.DialogViewerId);
            dv.Show(ShopMoneyDialogPanel, 100f, _AdditionalCameraEffectsAction: AdditionalCameraEffectsActionDefaultCoroutine);
        }

        private void AdditionalCameraEffectsActionDefaultCoroutine(bool _Appear, float _Time)
        {
            Cor.Run(MainMenuUtils.SubPanelsAdditionalCameraEffectsActionCoroutine(_Appear, _Time,
                CameraProvider, Ticker));
        }
        
        #endregion
    }
}