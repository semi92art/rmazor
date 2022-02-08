using System;
using System.Collections;
using Common;
using Common.Utils;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Managers.IAP
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
#if UNITY_ANDROID
            Cor.Run(RateGameAndroid(_JustSuggest));
#endif
            return true;
        }
        
#if UNITY_ANDROID

        private IEnumerator RateGameAndroid(bool _JustSuggest)
        {
            if (_JustSuggest)
            {
                var reviewManager = new Google.Play.Review.ReviewManager();
                var requestFlowOperation = reviewManager.RequestReviewFlow();
                yield return requestFlowOperation;
                if (requestFlowOperation.Error != Google.Play.Review.ReviewErrorCode.NoError)
                {
                    Dbg.LogWarning($"Failed to load rate game panel: {requestFlowOperation.Error}");
                    yield break;
                }
                var playReviewInfo = requestFlowOperation.GetResult();
                var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
                yield return launchFlowOperation;
                if (launchFlowOperation.Error != Google.Play.Review.ReviewErrorCode.NoError)
                {
                    Dbg.LogWarning($"Failed to launch rate game panel: {launchFlowOperation.Error}");
                    yield break;
                }
                SaveUtils.PutValue(SaveKeysCommon.GameWasRated, true);
                SaveUtils.PutValue(SaveKeysCommon.TimeSinceLastIapReviewDialogShown, DateTime.Now);
                
                yield break;
            }
            Application.OpenURL("market://details?id=" + Application.productName);
            SaveUtils.PutValue(SaveKeysCommon.GameWasRated, true);
        }
#endif
    }
}