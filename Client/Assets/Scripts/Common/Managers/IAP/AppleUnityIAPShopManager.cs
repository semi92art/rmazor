#if UNITY_IOS
using Common.Helpers;
using Common.Utils;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Common.Managers.IAP
{
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedType.Global
    public class AppleUnityIAPShopManager : UnityIapShopManagerBase
    {
        private IAppleExtensions   m_AppleExtensions;
        private GlobalGameSettings Settings { get; }
        
        public AppleUnityIAPShopManager(ILocalizationManager _LocalizationManager, GlobalGameSettings _Settings)
            : base(_LocalizationManager)
        {
            Settings = _Settings;
        }

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
            if (_JustSuggest)
                SA.iOS.StoreKit.ISN_SKStoreReviewController.RequestReview();
            else
                Application.OpenURL("itms-apps://itunes.apple.com/app/id1601083190");
            return true;
        }
    }
}
#endif
