using System.Linq;
using UnityEngine.Purchasing;

namespace Managers.IAP
{
    public class AppleUnityIAPShopManager : UnityIAPShopManager
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
    }
}