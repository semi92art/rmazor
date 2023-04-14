using System.Collections;
using Common.Constants;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.UI.PanelItems.Shop_Panel_Items;
using RMAZOR.Views.Common;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.Panels.ShopPanels
{
    public interface ISpecialOfferDialogPanel : IDialogPanel
    {
        UnityAction OnPanelClosedAction { get; set; }
    }

    public class SpecialOfferDialogPanelFake : DialogPanelFake, ISpecialOfferDialogPanel
    {
        public UnityAction OnPanelClosedAction { get; set; }
    }
    
    public class SpecialOfferDialogPanel : DialogPanelBase, ISpecialOfferDialogPanel
    {

        #region nonpublic members
        
        private Button               m_ButtonClose;
        private ShopItemSpecialOffer m_ItemSpecialOffer;
        
        protected override string PrefabName => "special_offer_panel";

        #endregion

        #region inject
        
        private ISpecialOfferTimerController SpecialOfferTimerController { get; }
        private IShopBuyActionsProvider      ShopBuyActionsProvider      { get; }

        private SpecialOfferDialogPanel(
            IManagersGetter              _Managers,
            IUITicker                    _Ticker,
            ICameraProvider              _CameraProvider,
            IColorProvider               _ColorProvider,
            IViewTimePauser              _TimePauser,
            IViewInputCommandsProceeder  _CommandsProceeder,
            ISpecialOfferTimerController _SpecialOfferTimerController,
            IShopBuyActionsProvider      _ShopBuyActionsProvider) 
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
        }

        #endregion

        #region api
        
        public UnityAction OnPanelClosedAction { get; set; }
        
        public override int DialogViewerId => DialogViewerIdsCommon.MediumCommon;
        
        public override void LoadPanel(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanel(_Container, _OnClose);
            Cor.Run(InitPanelItemsCoroutine());
        }

        #endregion

        #region nonpublic methods
        
        private void OnButtonCloseClick()
        {
            OnClose(() => OnPanelClosedAction?.Invoke());
            PlayButtonClickSound();
        }

        protected override void OnDialogStartAppearing()
        {
            SpecialOfferTimerController.ShownThisSession = true;
            m_ItemSpecialOffer.UpdateState();
            base.OnDialogStartAppearing();
        }
        
        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            m_ButtonClose      = _Go.GetCompItem<Button>("close_button");
            m_ItemSpecialOffer = _Go.GetCompItem<ShopItemSpecialOffer>("item_special_offer");
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
                true);
        }

        #endregion
    }
}