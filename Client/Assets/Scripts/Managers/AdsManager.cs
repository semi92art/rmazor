using System.Linq;
using Constants;
using Entities;
using GameHelpers;
using GoogleMobileAds.Api;
using Network;
using UI.Panels;
using UnityEngine.Events;
using Utils;

namespace Managers
{
    public class AdsManager : GameObserver, IInit
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

        #endregion

        #region api
        
        public bool ShowAds
        {
            get
            {
                var fromCache = SaveUtils.GetValue<bool?>(SaveKey.ShowAds);
                if (fromCache.HasValue)
                    return fromCache.Value;
                
#if UNITY_EDITOR
                return true;
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

        public event NoArgsHandler Initialized;
        
        public void Init()
        {
            new RequestConfiguration
                    .Builder()
                .SetTestDeviceIds(ResLoader.GoogleTestDeviceIds)
                .build();
            MobileAds.Initialize(_InitStatus => { });
            
            m_AdRequest = new AdRequest.Builder().Build();
            m_RewardedAd = new RewardedAd(ResLoader.GoogleAdsRewardId);
            m_RewardedAd.LoadAd(m_AdRequest);
            m_RewardedAd.OnPaidEvent += (_, _Args) => m_OnPaid?.Invoke();
            m_RewardedAd.OnAdClosed += (_, _Args) => m_RewardedAd.LoadAd(m_AdRequest);
            
            Initialized?.Invoke();
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
        
        private void ShowRewardedAd(UnityAction _OnPaid)
        {
            m_OnPaid = _OnPaid;
            if (m_RewardedAd.IsLoaded())
                m_RewardedAd.Show();
            else
                m_RewardedAd.LoadAd(m_AdRequest);
        }
        
        protected override void OnNotify(object _Sender, string _NotifyMessage, params object[] _Args)
        {
            if (_NotifyMessage == WheelOfFortunePanel.NotifyMessageWatchAdButtonClick)
                ShowRewardedAd(_Args[0] as UnityAction);
        }

        #endregion
        
    }
}