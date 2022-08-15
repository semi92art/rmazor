using Common.Managers.Advertising.AdBlocks;
using UnityEngine.Events;

namespace Common.Managers.Advertising.AdsProviders
{
    public abstract class AdsProviderCommonBase : AdsProviderBase
    {
        #region inject

        private readonly   IAudioManager       m_AudioManager;
        protected readonly IInterstitialAdBase m_InterstitialAd;
        protected readonly IRewardedAdBase     m_RewardedAd;
        
        protected AdsProviderCommonBase(
            IInterstitialAdBase _InterstitialAd,
            IRewardedAdBase     _RewardedAd)
        {
            m_InterstitialAd         = _InterstitialAd;
            m_RewardedAd             = _RewardedAd;
        }

        #endregion

        #region api
        
        public override bool RewardedAdReady             => 
            m_RewardedAd != null && m_RewardedAd.Ready;
        public override bool RewardedNonSkippableAdReady => RewardedAdReady;
        public override bool InterstitialAdReady         => 
            m_InterstitialAd != null && m_InterstitialAd.Ready;

        public override void LoadRewardedAd()
        {
            m_RewardedAd.LoadAd();
        }
        
        public override void LoadInterstitialAd()
        {
            m_InterstitialAd.LoadAd();
        }

        #endregion

        #region nonpublic methods
        
        protected override void InitRewardedAd()
        {
            m_RewardedAd.Skippable = true;
            m_RewardedAd.Init(AppId, RewardedUnitId);
        }

        protected override void InitInterstitialAd()
        {
            m_InterstitialAd.Skippable = true;
            m_InterstitialAd.Init(AppId, InterstitialUnitId);
        }
        
        protected override void ShowRewardedAdCore(
            UnityAction _OnShown,
            UnityAction _OnClicked,
            UnityAction _OnReward)
        {
            if (!RewardedAdReady) 
                return;
            MuteAudio(true);
            void OnShown()
            {
                MuteAudio(false);
                _OnShown?.Invoke();
            }
            m_RewardedAd.ShowAd(OnShown, _OnClicked, _OnReward);
        }
        
        protected override void ShowInterstitialAdCore(UnityAction _OnShown, UnityAction _OnClicked)
        {
            if (!InterstitialAdReady) 
                return;
            MuteAudio(true);
            void OnShown()
            {
                MuteAudio(false);
                _OnShown?.Invoke();
            }
            m_InterstitialAd.ShowAd(OnShown, _OnClicked);
        }

        #endregion
    }
}