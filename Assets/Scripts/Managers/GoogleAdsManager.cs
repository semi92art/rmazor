using GoogleMobileAds.Api;
using UnityEngine.Events;
using Utils;

namespace Managers
{
    public class GoogleAdsManager : ISingleton
    {
        private static GoogleAdsManager _instance;
        public static GoogleAdsManager Instance => _instance ?? (_instance = new GoogleAdsManager());
        
        private RewardedAd m_RewardedAd;
        private AdRequest m_AdRequest;
        private UnityAction m_OnRewardedAdPaid;

        public void Init()
        {
            new RequestConfiguration
                .Builder()
                .SetTestDeviceIds(ResLoader.GoogleTestDeviceIds)
                .build();
            MobileAds.Initialize(_InitStatus => { });
            
            m_AdRequest = new AdRequest.Builder().Build();
            m_RewardedAd = new RewardedAd(ResLoader.GoogleAdsRewardId);
            //m_RewardedAd.LoadAd(m_AdRequest);
            //m_RewardedAd.OnPaidEvent += (_, _Args) => m_OnRewardedAdPaid?.Invoke();
            //m_RewardedAd.OnAdClosed += (_, _Args) => m_RewardedAd.LoadAd(m_AdRequest);
        }

        public bool ShowRewardedAd(UnityAction _OnPaid)
        {
            m_OnRewardedAdPaid = _OnPaid;
            bool isLoaded = m_RewardedAd.IsLoaded();
            if (isLoaded)
                m_RewardedAd.Show();
            else
                m_RewardedAd.LoadAd(m_AdRequest);
            
#if UNITY_EDITOR
            _OnPaid?.Invoke();
#endif
            return isLoaded;
        }
    }
}