using UnityEngine.Purchasing;

namespace Managers.IAP
{
    public class AndroidUnityIAPShopManager : UnityIAPShopManager
    {
        public AndroidUnityIAPShopManager(ILocalizationManager _LocalizationManager) 
            : base(_LocalizationManager) { }

        protected override ConfigurationBuilder GetBuilder()
        {
            var builder = base.GetBuilder();
            builder.Configure<IGooglePlayConfiguration>().SetDeferredPurchaseListener(OnDeferredPurchase);
            return builder;
        }
    }
}