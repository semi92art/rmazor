using Common.Exceptions;
using Common.Helpers;
using Common.Managers.Advertising.AdBlocks;
using Common.Utils;
using UnityEngine.Events;

namespace Common.Managers.Advertising.AdsProviders
{
    public abstract class AdsProviderCommonBase : AdsProviderBase
    {
        #region inject

        private readonly   IAudioManager m_AudioManager;
        protected readonly IAdBase       m_InterstitialAd;
        protected readonly IAdBase       m_RewardedAd;
        
        protected AdsProviderCommonBase(
            GlobalGameSettings _GlobalGameSettings,
            IAdBase            _InterstitialAd,
            IAdBase            _RewardedAd) 
            : base(_GlobalGameSettings)
        {
            m_InterstitialAd = _InterstitialAd;
            m_RewardedAd     = _RewardedAd;
        }

        #endregion

        #region api
        
        public override bool RewardedAdReady     => m_RewardedAd != null && m_RewardedAd.Ready;
        public override bool InterstitialAdReady => m_InterstitialAd != null && m_InterstitialAd.Ready;
        
        public override void LoadAd(AdvertisingType _AdvertisingType)
        {
            switch (_AdvertisingType)
            {
                case AdvertisingType.Interstitial: m_InterstitialAd.LoadAd(); break;
                case AdvertisingType.Rewarded:     m_RewardedAd.LoadAd();     break;
                default:                           throw new SwitchCaseNotImplementedException(_AdvertisingType);
            }
        }

        #endregion

        #region nonpublic methods
        
        protected override void InitRewardedAd()
        {
            m_RewardedAd.Init(AppId, RewardedUnitId);
        }
    
        protected override void InitInterstitialAd()
        {
            m_InterstitialAd.Init(AppId, InterstitialUnitId);
        }
        
        protected override void ShowRewardedAdCore(UnityAction _OnShown, UnityAction _OnClicked)
        {
            if (RewardedAdReady)
            {
                MuteAudio(true);
                void OnShown()
                {
                    MuteAudio(false);
                    _OnShown?.Invoke();
                }
                m_RewardedAd.ShowAd(OnShown, _OnClicked);
            }
            else
            {
                Dbg.Log("Rewarded ad is not ready.");
                m_RewardedAd.LoadAd();
            }
        }
        
        protected override void ShowInterstitialAdCore(UnityAction _OnShown, UnityAction _OnClicked)
        {
            if (InterstitialAdReady)
            {
                MuteAudio(true);
                void OnShown()
                {
                    MuteAudio(false);
                    _OnShown?.Invoke();
                }
                m_InterstitialAd.ShowAd(OnShown, _OnClicked);
            }
            else
            {
                Dbg.Log("Interstitial ad is not ready.");
                m_InterstitialAd.LoadAd();
            }
        }

        #endregion
    }
}