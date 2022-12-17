using Common;
using Common.Helpers;
using Common.Managers.Advertising;
using Common.Ticker;
using RMAZOR.Models;
using UnityEngine.Events;

namespace RMAZOR.Views.Common
{
    public interface IViewBetweenLevelAdShower: IInit
    {
        bool ShowAd { get; set; }
        
        void TryShowAd(
            long        _LevelIndex,
            bool        _IsBonus,
            UnityAction _OnAdClosed);
    }
    
    public class ViewBetweenLevelAdShower : InitBase, IViewBetweenLevelAdShower, IUpdateTick
    {
        #region nonpublic members

        private float m_TimeWithoutAdsInSeconds = 2f * 60f;

        #endregion
        
        #region inject

        private IAdsManager                         AdsManager                     { get; }
        private GlobalGameSettings                  GameSettings                   { get; }
        private IViewTimePauser                     TimePauser                     { get; }
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }
        private ICommonTicker                       CommonTicker                   { get; }

        public ViewBetweenLevelAdShower(
            IAdsManager                         _AdsManager,
            GlobalGameSettings                  _GameSettings,
            IViewTimePauser                     _TimePauser,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker,
            ICommonTicker                       _CommonTicker)
        {
            AdsManager                     = _AdsManager;
            GameSettings                   = _GameSettings;
            TimePauser                     = _TimePauser;
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
            CommonTicker = _CommonTicker;
        }

        #endregion

        #region api

        public bool ShowAd { get; set; }

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
            bool DoTryShowAd()
            {
                if (m_TimeWithoutAdsInSeconds < 3f * 60f)
                    return false;
                return _LevelIndex >= GameSettings.firstLevelToShowAds
                       && !_IsBonus
                       && ShowAd
                       && AdsManager.InterstitialAdReady;
            }
            if (DoTryShowAd())
            {
                void OnBeforeAdShown()
                {
                    TimePauser.PauseTimeInGame();
                    SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.PauseLevel);
                }
                void OnAdClosedOrFailedToShow()
                {
                    TimePauser.UnpauseTimeInGame();
                    m_TimeWithoutAdsInSeconds = 0f;
                    _OnAdClosed?.Invoke();
                }
                AdsManager.ShowInterstitialAd(
                    OnBeforeAdShown, 
                    _OnClosed: OnAdClosedOrFailedToShow,
                    _OnFailedToShow: OnAdClosedOrFailedToShow);
            }
            else
            {
                _OnAdClosed?.Invoke();
            }
            ShowAd = true;
        }
        
        public void UpdateTick()
        {
            m_TimeWithoutAdsInSeconds += CommonTicker.DeltaTime;
        }

        #endregion
    }
}