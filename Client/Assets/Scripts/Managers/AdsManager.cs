using Entities;
using GoogleMobileAds.Api;
using Ticker;
using UnityEngine.Events;
using Utils;

namespace Managers
{
    public interface IAdsManager : IInit
    {
        bool ShowAds { get; set; }
        bool RewardedAdReady { get; }
        event UnityAction RewardedAdLoaded;
        void ShowRewardedAd(UnityAction _OnPaid);
    }
    
    public class AdsManager : IAdsManager, IUpdateTick
    {
        #region singleton
    
        private static AdsManager _instance;
        public static AdsManager Instance => _instance ?? (_instance = new AdsManager());
    
        #endregion

        #region nonpublic members

        private RewardedAd m_RewardedAd;
        private AdRequest m_AdRequest;
        private UnityAction m_OnPaid;
        private bool m_ShowAds;
        private bool m_Initialized;
        private bool m_RewardedAdLoadedCheck;

        #endregion

        #region api
        
        public bool ShowAds
        {
            get
            {
#if UNITY_EDITOR
                return !SaveUtils.GetValue<bool>(SaveKey.DisableAds);
#elif UNITY_ANDROID
                return GetShowAdsAndroid();
#elif UNITY_IPHONE
                return GetShowAdsIos();
#endif
            }
            set
            {
                
            }
        }

        public bool RewardedAdReady => m_RewardedAd.IsLoaded();
        public event UnityAction Initialized;
        public event UnityAction RewardedAdLoaded;
        
        public void Init()
        {
            new RequestConfiguration.Builder()
                .SetTestDeviceIds(ResLoader.GoogleTestDeviceIds)
                .build();
            MobileAds.Initialize(_InitStatus => { });
            
            m_AdRequest = new AdRequest.Builder().Build();
            m_RewardedAd = new RewardedAd(ResLoader.GoogleAdsRewardId);
            m_RewardedAd.LoadAd(m_AdRequest);
            m_RewardedAd.OnPaidEvent += (_, _Args) => m_OnPaid?.Invoke();
            m_RewardedAd.OnAdClosed += (_, _Args) => m_RewardedAd.LoadAd(m_AdRequest);
            
            Initialized?.Invoke();
            m_Initialized = true;
        }


        public void ShowRewardedAd(UnityAction _OnPaid)
        {
            m_OnPaid = _OnPaid;
            if (m_RewardedAd.IsLoaded())
                m_RewardedAd.Show();
            else
                m_RewardedAd.LoadAd(m_AdRequest);
        }


        public void UpdateTick()
        {
            if (!m_Initialized)
                return;
            if (!m_RewardedAdLoadedCheck && m_RewardedAd.IsLoaded())
                RewardedAdLoaded?.Invoke();
            m_RewardedAdLoadedCheck = m_RewardedAd.IsLoaded();
        }

        #endregion

        #region nonpublic methods

        private bool GetShowAdsAndroid()
        {
            //TODO
            return true;
        }

        private bool GetShowAdsIos()
        {
            //TODO
            return true;
        }
        
        #endregion
    }
}