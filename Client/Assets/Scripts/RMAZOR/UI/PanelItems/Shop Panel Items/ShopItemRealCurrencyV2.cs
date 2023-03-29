using System.Collections;
using System.Collections.Generic;
using Common;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Managers.IAP;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using UnityEngine;
using UnityEngine.Purchasing;

namespace RMAZOR.UI.PanelItems.Shop_Panel_Items
{
    public class ViewShopItemInfoRealCurrencyV2 : ViewShopItemInfoBaseV2
    {
        public IAP_ProductInfo IapProductInfo { get; set; }
    }
    
    public class ShopItemRealCurrencyV2 : ShopItemCurrencyBaseV2
    {
        #region serialized fields

        [SerializeField] private Animator loadingAnim;

        #endregion
        
        #region nonpublic members

        private ViewShopItemInfoRealCurrencyV2 m_ShopItemInfo;
        
        private IShopManager ShopManager { get; set; }
        
        #endregion
        
        #region api

        public virtual void Init(
            IUITicker                      _UITicker,
            IAudioManager                  _AudioManager,
            ILocalizationManager           _LocalizationManager,
            IAnalyticsManager              _AnalyticsManager,
            IShopManager                   _ShopManager,
            ViewShopItemInfoRealCurrencyV2 _ShopItemInfo)
        {
            ShopManager    = _ShopManager;
            m_ShopItemInfo = _ShopItemInfo;
            ShopManager.AddPurchaseAction(m_ShopItemInfo.IapProductInfo.PurchaseKey, m_ShopItemInfo.BuyAction);
            ShopManager.AddPurchaseAction(m_ShopItemInfo.IapProductInfo.PurchaseKey, SetItemAsPurchased);
            Cor.Run(IndicateLoadingCoroutine());
            base.Init(_UITicker, _AudioManager, _LocalizationManager, _AnalyticsManager, _ShopItemInfo);
        }
        
        #endregion

        #region nonpublic methods

        public override void UpdateState() { }

        protected override void OnBuyButtonClick()
        {
            PlayButtonClickSound();
            var args = new Dictionary<string, object>
            {{AnalyticIds.ParameterPurchaseProductId, m_ShopItemInfo.IapProductInfo.Id}};
            AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.BuyShopItemButtonClick, args);
            ShopManager.Purchase(m_ShopItemInfo.IapProductInfo.PurchaseKey);
        }
        
        private IEnumerator IndicateLoadingCoroutine()
        {
            SetLoadingIndication(!HasReceipt);
            if (HasReceipt)
            {
                SetItemAsPurchasedIfHasReceiptAndNonConsumable();
                yield break;
            }
            yield return Cor.WaitWhile(() => !AreShopItemArgsLoaded());
            SetLoadingIndication(false);
            IndicateLoadingCoroutineFinishAction();
        }

        private bool AreShopItemArgsLoaded()
        {
            return m_ShopItemInfo.IapProductInfo != null
                   && m_ShopItemInfo.IapProductInfo.Result() != EShopProductResult.Pending;
        }

        protected virtual void IndicateLoadingCoroutineFinishAction()
        {
            priceText.text = $"{m_ShopItemInfo.IapProductInfo.LocalizedPriceString}";
            SetItemAsPurchasedIfHasReceiptAndNonConsumable();
        }
        
        protected virtual void SetLoadingIndication(bool _Indicate)
        {
            priceText  .SetGoActive(!_Indicate);
            loadingAnim.SetGoActive(_Indicate);
            buyButton  .interactable = !_Indicate;
        }

        protected override bool HasReceipt
        {
            get
            {
                bool? hasReceiptAdditionalPredicate = m_ShopItemInfo.HasReceiptAdditionalPredicate?.Invoke();
                return IapProductInfoHasReceipt(m_ShopItemInfo.IapProductInfo)
                       || (hasReceiptAdditionalPredicate.HasValue && hasReceiptAdditionalPredicate.Value);
            }
        }

        private static bool IapProductInfoHasReceipt(IAP_ProductInfo _ProductInfo)
        {
            if (_ProductInfo == null)
                return false;
            var idsOfPurchasedItems = SaveUtils.GetValue(SaveKeysMazor.BoughtPurchaseIds) ?? new List<int>();
            if (idsOfPurchasedItems.Contains(_ProductInfo.PurchaseKey))
                return true;
            return _ProductInfo.Type == ProductType.NonConsumable
                   && _ProductInfo.HasReceipt;
        }

        protected override void SetItemAsPurchased()
        {
            var idsOfPurchasedItems = SaveUtils.GetValue(SaveKeysMazor.BoughtPurchaseIds);
            var iapProductInfo = m_ShopItemInfo.IapProductInfo;
            if (iapProductInfo != null && !idsOfPurchasedItems.Contains(iapProductInfo.PurchaseKey))
            {
                idsOfPurchasedItems.Add(iapProductInfo.PurchaseKey);
                SaveUtils.PutValue(SaveKeysMazor.BoughtPurchaseIds, idsOfPurchasedItems);
            }
            loadingAnim.SetGoActive(false);
            base.SetItemAsPurchased();
        }
        
        #endregion
    }
}