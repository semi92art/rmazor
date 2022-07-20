#if APPODEAL_3
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppodealStack.ConsentManagement.Common;
using AppodealStack.Monetization.Api;
using AppodealStack.Monetization.Common;
using Common.Helpers;
using Common.Managers.Advertising.AdBlocks;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Managers.Advertising.AdsProviders
{
    public interface IAppodealAdsProvider : IAdsProvider, IAppodealInitializationListener { }
    
    public class AppodealAdsProvider : AdsProviderCommonBase, IAppodealAdsProvider
    {

        #region nonpublic members

        protected override string AppId => Application.isEditor ? string.Empty : base.AppId;

        private IConsent     m_Consent;
        private UnityAction m_OnSuccess;

        #endregion
        
        #region inject
        
        
        private AppodealAdsProvider(
            GlobalGameSettings      _GlobalGameSettings,
            IAppodealInterstitialAd _InterstitialAd,
            IAppodealRewardedAd     _RewardedAd) 
            : base(_GlobalGameSettings, _InterstitialAd, _RewardedAd) { }

        #endregion

        #region api

        public override string Source              => AdvertisingNetworks.Appodeal;
        public override bool   RewardedAdReady     => RewardedAd.Ready;
        public override bool   InterstitialAdReady => InterstitialAd.Ready;

        public void OnInitializationFinished(List<string> _Errors)
        {
            if (_Errors == null || !_Errors.Any())
            {
                Dbg.Log("Appodeal successfully initialized");
                m_OnSuccess?.Invoke();
                return;
            }
            var sb = new StringBuilder();
            sb.AppendLine("Failed to initialize Appodeal!");
            foreach (string error in _Errors)
                sb.AppendLine(error);
            Dbg.LogError(sb.ToString());
        }

        #endregion

        #region nonpublic methods

        protected override void InitConfigs(UnityAction _OnSuccess)
        {
            m_OnSuccess = _OnSuccess;
            Appodeal.SetTesting(TestMode && !GlobalGameSettings.apkForAppodeal);
            var logLevel = TestMode || GlobalGameSettings.apkForAppodeal ? AppodealLogLevel.Verbose : AppodealLogLevel.None;
            Appodeal.SetLogLevel(logLevel);
            Appodeal.SetChildDirectedTreatment(true);
            const int adTypes = AppodealAdType.Interstitial | AppodealAdType.RewardedVideo;
            Appodeal.SetAutoCache(adTypes, false);
            Appodeal.Initialize(AppId, adTypes, this);
        }
        
        protected override void InitRewardedAd()
        {
            RewardedAd.Init(AppId, null);
        }

        protected override void InitInterstitialAd()
        {
            InterstitialAd.Init(AppId, null);
        }
        
        #endregion
    }
}
#endif