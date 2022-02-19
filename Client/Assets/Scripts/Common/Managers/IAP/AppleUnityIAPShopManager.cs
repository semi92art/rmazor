#if UNITY_IOS || UNITY_IPHONE
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
        private CommonGameSettings Settings { get; }
        
        public AppleUnityIAPShopManager(ILocalizationManager _LocalizationManager, CommonGameSettings _Settings)
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
            {
                if (string.IsNullOrEmpty(Settings.iOsGameId) || Settings.iOsGameId == "[Empty]")
                {
                    Dbg.LogWarning("Message for testers: This function will be available after app appears in store.");
                    string title = LocalizationManager.GetTranslation("oops");
                    string text = LocalizationManager.GetTranslation("smth_went_wrong");
                    CommonUtils.ShowAlertDialog(title, text);
                }
                else
                {
                    Application.OpenURL("itms-apps://itunes.apple.com/app/id" + Settings.iOsGameId);
                }
            }
            return true;
        }
    }
}
#endif
