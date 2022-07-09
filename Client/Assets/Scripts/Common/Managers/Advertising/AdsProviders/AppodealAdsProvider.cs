#if APPODEAL_3
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppodealStack.ConsentManagement.Api;
using AppodealStack.ConsentManagement.Common;
using AppodealStack.Monetization.Api;
using AppodealStack.Monetization.Common;
using Common.Managers.Advertising.AdBlocks;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Managers.Advertising.AdsProviders
{
    public interface IAppodealAdsProvider : IAdsProvider, IConsentInfoUpdateListener, IAppodealInitializationListener { }
    
    public class AppodealAdsProvider : AdsProviderCommonBase, IAppodealAdsProvider
    {
        #region nonpublic members

        protected override string AppId => Application.isEditor ? string.Empty : base.AppId;

        private IConsent     m_Consent;
        private UnityAction m_OnSuccess;

        #endregion
        
        #region inject
        
        private AppodealAdsProvider(
            IAppodealInterstitialAd _InterstitialAd,
            IAppodealRewardedAd     _RewardedAd) 
            : base(_InterstitialAd, _RewardedAd) { }

        #endregion

        #region api

        public override string Source              => AdvertisingNetworks.Appodeal;
        public override bool   RewardedAdReady     => RewardedAd.Ready;
        public override bool   InterstitialAdReady => InterstitialAd.Ready;
        
        // User's consent status successfully updated.
        public void OnConsentInfoUpdated(IConsent _Consent)
        {
            m_Consent = _Consent;
            Dbg.Log("Attempt to initialize Appodeal inside of OnConsentInfoUpdated");
            InitAppodeal();
        }

        // User's consent status failed to update.
        public void OnFailedToUpdateConsentInfo(IConsentManagerException _Error)
        {
            Dbg.Log($"OnFailedToUpdateConsentInfo Reason: {_Error.GetReason()}");
        }
        
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
            // TestMode = false;
            // Appodeal.SetLogLevel(AppodealLogLevel.Verbose);
            Appodeal.SetTesting(TestMode);
            Appodeal.SetLogLevel(TestMode ? AppodealLogLevel.Verbose : AppodealLogLevel.None);
            Appodeal.SetAutoCache(AppodealAdType.Interstitial | AppodealAdType.RewardedVideo, false);
            Appodeal.SetChildDirectedTreatment(true);
            var consentManager = ConsentManager.GetInstance();
            consentManager.RequestConsentInfoUpdate(AppId, this);
        }

        private void InitAppodeal()
        {
            Appodeal.Initialize(
                AppId, 
                AppodealAdType.Interstitial | AppodealAdType.RewardedVideo, 
                this);
        }

        protected override void InitRewardedAd()
        {
            RewardedAd.Init(AppId, RewardedUnitId);
        }

        protected override void InitInterstitialAd()
        {
            InterstitialAd.Init(AppId, InterstitialUnitId);
        }
        
        #endregion
    }
}
#endif