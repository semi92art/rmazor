#if UNITY_ANDROID
using System;
using System.Collections;
using Common.Utils;
using Google.Play.Review;
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

        public override bool RateGame(bool _JustSuggest = true)
        {
            if (!base.RateGame(_JustSuggest))
                return false;

            Cor.Run(RateGameAndroid(_JustSuggest));

            return true;
        }

        private static IEnumerator RateGameAndroid(bool _JustSuggest)
        {
            if (_JustSuggest)
            {
                var reviewManager = new ReviewManager();
                var requestFlowOperation = reviewManager.RequestReviewFlow();
                yield return requestFlowOperation;
                if (requestFlowOperation.Error != ReviewErrorCode.NoError)
                {
                    Dbg.LogWarning($"Failed to load rate game panel: {requestFlowOperation.Error}");
                    yield break;
                }
                var playReviewInfo = requestFlowOperation.GetResult();
                var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
                yield return launchFlowOperation;
                if (launchFlowOperation.Error != ReviewErrorCode.NoError)
                {
                    Dbg.LogWarning($"Failed to launch rate game panel: {launchFlowOperation.Error}");
                    yield break;
                }
                SaveUtils.PutValue(SaveKeysCommon.GameWasRated, true);
                SaveUtils.PutValue(SaveKeysCommon.TimeSinceLastIapReviewDialogShown, DateTime.Now);
                
                yield break;
            }
            Application.OpenURL("market://details?id=" + Application.identifier);
            SaveUtils.PutValue(SaveKeysCommon.GameWasRated, true);
        }
    }
}
#endif
