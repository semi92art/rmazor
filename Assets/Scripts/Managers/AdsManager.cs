using Entities;
using GoogleMobileAds.Api;
using Network;
using UI.Panels;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Managers
{
    public class AdsManager : MonoBehaviour, IGameObserver, ISingleton
    {
        #region singleton
    
        private static AdsManager _instance;

        public static AdsManager Instance
        {
            get
            {
                if (_instance is AdsManager ptm && !ptm.IsNull())
                    return _instance;
                var go = new GameObject("Ads Controller");
                _instance = go.AddComponent<AdsManager>();
                if (!GameClient.Instance.IsModuleTestsMode)
                    DontDestroyOnLoad(go);
                return _instance;
            }
        }
    
        #endregion
        
        private RewardedAd m_RewardedAd;
        private AdRequest m_AdRequest;
        private UnityAction m_OnPaid;

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
        }

        public void OnNotify(object _Sender, int _NotifyId, params object[] _Args)
        {
            switch (_Sender)
            {
                case WheelOfFortunePanel _:
                    if (_NotifyId == WheelOfFortunePanel.NotifyIdWatchAdButtonClick)
                        ShowRewardedAd(_Args[0] as UnityAction);
                    break;
            }
        }
        
        private void ShowRewardedAd(UnityAction _OnPaid)
        {
            m_OnPaid = _OnPaid;
            if (m_RewardedAd.IsLoaded())
                m_RewardedAd.Show();
            else
                m_RewardedAd.LoadAd(m_AdRequest);
        }
    }
}