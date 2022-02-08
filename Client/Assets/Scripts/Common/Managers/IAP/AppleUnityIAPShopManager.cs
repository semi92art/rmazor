using UnityEngine.Purchasing;

namespace Managers.IAP
{
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedType.Global
    public class AppleUnityIAPShopManager : UnityIapShopManagerBase
    {
        private IAppleExtensions m_AppleExtensions;
        
        public AppleUnityIAPShopManager(ILocalizationManager _LocalizationManager)
            : base(_LocalizationManager) { }

        public override void OnInitialized(IStoreController _Controller, IExtensionProvider _Extensions)
        {
            base.OnInitialized(_Controller, _Extensions);
            m_AppleExtensions = _Extensions.GetExtension<IAppleExtensions>();
#if DEVELOPMENT_BUILD
            m_AppleExtensions.simulateAskToBuy = true;
#endif
            m_AppleExtensions.RegisterPurchaseDeferredListener(OnDeferredPurchase);
        }

        public override bool RateGame(bool _JustSuggest = true)
        {
            if (!base.RateGame(_JustSuggest))
                return false;
#if UNITY_IOS || UNITY_IPHONE
            SA.iOS.StoreKit.ISN_SKStoreReviewController.RequestReview();
#endif
            return true;
        }
    }
}