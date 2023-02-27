using Common.Helpers;
using Common.Managers.Advertising;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Ticker;
using RMAZOR.Models;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using UnityEngine.Events;

namespace RMAZOR.Views.Common
{
    public interface IViewBetweenLevelAdShower: IInit
    {
        bool ShowAdEnabled { get; set; }
        
        void TryShowAd(
            long        _LevelIndex,
            bool        _IsBonus,
            UnityAction _OnAdClosed);
    }
    
    public class ViewBetweenLevelAdShower : InitBase, IViewBetweenLevelAdShower, IUpdateTick
    {
        #region nonpublic members

        private float m_TimeWithoutAdsInSeconds = 1f * 60f;

        private bool AdIsReady => GlobalGameSettings.showOnlyRewardedAds
            ? AdsManager.RewardedAdReady
            : AdsManager.InterstitialAdReady;

        #endregion
        
        #region inject

        private IAdsManager                         AdsManager                     { get; }
        private GlobalGameSettings                  GlobalGameSettings             { get; }
        private IViewTimePauser                     TimePauser                     { get; }
        private IViewLevelStageSwitcher LevelStageSwitcher { get; }
        private ICommonTicker                       CommonTicker                   { get; }

        public ViewBetweenLevelAdShower(
            IAdsManager                         _AdsManager,
            GlobalGameSettings                  _GlobalGameSettings,
            IViewTimePauser                     _TimePauser,
            IViewLevelStageSwitcher _LevelStageSwitcher,
            ICommonTicker                       _CommonTicker)
        {
            AdsManager                     = _AdsManager;
            GlobalGameSettings             = _GlobalGameSettings;
            TimePauser                     = _TimePauser;
            LevelStageSwitcher = _LevelStageSwitcher;
            CommonTicker = _CommonTicker;
        }

        #endregion

        #region api

        public bool ShowAdEnabled { get; set; }

        public override void Init()
        {
            CommonTicker.Register(this);
            base.Init();
        }

        public void TryShowAd(
            long        _LevelIndex,
            bool        _IsBonus,
            UnityAction _OnAdClosed)
        {
            bool IsPossibleToShowAd()
            {
                if (m_TimeWithoutAdsInSeconds < GlobalGameSettings.betweenLevelAdShowIntervalInSeconds)
                    return false;
                return _LevelIndex >= GlobalGameSettings.firstLevelToShowAds
                       && !_IsBonus
                       && ShowAdEnabled
                       && AdIsReady;
            }
            if (IsPossibleToShowAd())
            {
                void OnBeforeAdShown()
                {
                    TimePauser.PauseTimeInGame();
                    LevelStageSwitcher.SwitchLevelStage(EInputCommand.PauseLevel);
                }
                void OnAdClosedOrFailedToShow()
                {
                    TimePauser.UnpauseTimeInGame();
                    m_TimeWithoutAdsInSeconds = 0f;
                    _OnAdClosed?.Invoke();
                }
                ShowAd(OnBeforeAdShown, OnAdClosedOrFailedToShow, OnAdClosedOrFailedToShow);
            }
            else
            {
                _OnAdClosed?.Invoke();
            }
            ShowAdEnabled = true;
        }
        
        public void UpdateTick()
        {
            m_TimeWithoutAdsInSeconds += CommonTicker.DeltaTime;
        }

        #endregion

        #region nonpublic methods

        private void ShowAd(
            UnityAction _OnBeforeAdShown,
            UnityAction _OnAdClosed,
            UnityAction _OnAdFailedToShow)
        {
            if (GlobalGameSettings.showOnlyRewardedAds)
            {
                AdsManager.ShowRewardedAd(
                    _OnBeforeAdShown, 
                    _OnClosed: _OnAdClosed,
                    _OnFailedToShow: _OnAdFailedToShow);
            }
            else
            {
                AdsManager.ShowInterstitialAd(
                    _OnBeforeAdShown, 
                    _OnClosed: _OnAdClosed,
                    _OnFailedToShow: _OnAdFailedToShow);
            }
        }

        #endregion
    }
}