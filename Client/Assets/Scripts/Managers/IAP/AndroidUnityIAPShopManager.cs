using System.Collections;
using Common;
using Common.Utils;
using RMAZOR;
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

        protected IEnumerator RateGameAndroid(bool _JustSuggest)
        {
            if (_JustSuggest)
            {
                string title = LocalizationManager.GetTranslation("rate_dialog_title");
                string text = LocalizationManager.GetTranslation("rate_dialog_text") + "\n" +
                    "\u2B50\u2B50\u2B50\u2B50\u2B50";
                string ok = LocalizationManager.GetTranslation("rate_yes");
                string notNow = LocalizationManager.GetTranslation("rate_not_now");
                string never = LocalizationManager.GetTranslation("rate_never");
                MTAssets.NativeAndroidToolkit.NativeAndroid.Dialogs.ShowNeutralDialog(title, text, ok, notNow, never);
                MTAssets.NativeAndroidToolkit.Events.DialogsEvents.onNeutralYes = () =>
                {
                    Cor.Run(RateGameAndroid(false));
                };
                MTAssets.NativeAndroidToolkit.Events.DialogsEvents.onNeutralNo = () =>
                {

                };
                MTAssets.NativeAndroidToolkit.Events.DialogsEvents.onNeutralNeutral = () =>
                {
                    SaveUtils.PutValue(SaveKeys.GameWasRated, true);
                };
                yield break;
            }
            
            static void OpenAppPageInStoreDirectly()
            {
                Application.OpenURL("market://details?id=" + Application.productName);
            }

            var reviewManager = new Google.Play.Review.ReviewManager();
            var requestFlowOperation = reviewManager.RequestReviewFlow();
            yield return requestFlowOperation;
            if (!requestFlowOperation.IsSuccessful)
            {
                Dbg.LogWarning($"requestFlowOperation.IsSuccessful: {requestFlowOperation.IsSuccessful}");
                OpenAppPageInStoreDirectly();
                yield break;
            }
            if (requestFlowOperation.Error != Google.Play.Review.ReviewErrorCode.NoError)
            {
                Dbg.LogWarning($"Failed to load rate game panel: {requestFlowOperation.Error}");
                OpenAppPageInStoreDirectly();
                yield break;
            }
            var playReviewInfo = requestFlowOperation.GetResult();
            var launchFlowOperation = reviewManager.LaunchReviewFlow(playReviewInfo);
            if (!launchFlowOperation.IsSuccessful)
            {
                Dbg.LogWarning($"launchFlowOperation.IsSuccessful: {launchFlowOperation.IsSuccessful}");
                OpenAppPageInStoreDirectly();
                yield break;
            }
            yield return launchFlowOperation;
            if (launchFlowOperation.Error != Google.Play.Review.ReviewErrorCode.NoError)
            {
                Dbg.LogWarning($"Failed to launch rate game panel: {launchFlowOperation.Error}");
                OpenAppPageInStoreDirectly();
                yield break;
            }
            SaveUtils.PutValue(SaveKeys.GameWasRated, true);
        }
        
#endif
    }
}