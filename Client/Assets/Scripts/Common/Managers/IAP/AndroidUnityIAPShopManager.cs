#if UNITY_ANDROID
using System.Collections;
using Common.Utils;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Common.Managers.IAP
{
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedType.Global
    public class AndroidUnityIAPShopManager : UnityIapShopManagerBase
    {
        public AndroidUnityIAPShopManager(ILocalizationManager _LocalizationManager) 
            : base(_LocalizationManager) { }

        protected override ConfigurationBuilder GetBuilder()
        {
            var builder = base.GetBuilder();
            builder.Configure<IGooglePlayConfiguration>().SetDeferredPurchaseListener(OnDeferredPurchase);
            return builder;
        }

        public override bool RateGame()
        {
            if (!base.RateGame())
                return false;
            Cor.Run(RateGameAndroid());
            return true;
        }

        private static IEnumerator RateGameAndroid()
        {
            Application.OpenURL("market://details?id=" + Application.identifier);
            SaveUtils.PutValue(SaveKeysCommon.GameWasRated, true);
            yield return null;
        }
    }
}
#endif
