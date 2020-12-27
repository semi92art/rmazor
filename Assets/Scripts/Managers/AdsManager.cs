﻿using Entities;
using GoogleMobileAds.Api;
using Network;
using UI.Panels;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Managers
{
    public class AdsManager : GameObserver, ISingleton
    {
        #region singleton
    
        private static AdsManager _instance;
        public static AdsManager Instance => _instance ?? (_instance = new AdsManager());
    
        #endregion

        #region nonpublic members

        private RewardedAd m_RewardedAd;
        private AdRequest m_AdRequest;
        private UnityAction m_OnPaid;

        #endregion

        #region api

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

        protected override void OnNotify(object _Sender, string _NotifyMessage, params object[] _Args)
        {
            switch (_Sender)
            {
                case WheelOfFortunePanel _:
                    if (_NotifyMessage == WheelOfFortunePanel.NotifyMessageWatchAdButtonClick)
                        ShowRewardedAd(_Args[0] as UnityAction);
                    break;
            }
        }

        #endregion

        #region nonpublic methods

        private void ShowRewardedAd(UnityAction _OnPaid)
        {
            m_OnPaid = _OnPaid;
            if (m_RewardedAd.IsLoaded())
                m_RewardedAd.Show();
            else
                m_RewardedAd.LoadAd(m_AdRequest);
        }

        #endregion
        
    }
}