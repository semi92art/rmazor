using System.Runtime.CompilerServices;
using Common.Helpers;
using Common.Managers.Advertising;
using mazing.common.Runtime;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI.DialogViewers;
using RMAZOR.Helpers;
using RMAZOR.Models;
using RMAZOR.UI.Panels.ShopPanels;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using UnityEngine.Events;

namespace RMAZOR.Views.Common
{
    public interface IViewBetweenLevelAdShower: IInit
    {
        bool ShowAdEnabled { get; set; }
        
        void TryShowAd(UnityAction _OnAdClosed, EAdvertisingType? _AdvertisingType = null);
    }
    
    public class ViewBetweenLevelAdShower 
        : InitBase,
          IViewBetweenLevelAdShower,
          IUpdateTick
    {
        #region nonpublic members

        private float m_TimeWithoutAdsInSeconds;

        #endregion
        
        #region inject

        private IAdsManager                  AdsManager                  { get; }
        private GlobalGameSettings           GlobalGameSettings          { get; }
        private IViewTimePauser              TimePauser                  { get; }
        private IViewLevelStageSwitcher      LevelStageSwitcher          { get; }
        private ICommonTicker                CommonTicker                { get; }
        private ISpecialOfferTimerController SpecialOfferTimerController { get; }
        private ISpecialOfferDialogPanel     SpecialOfferDialogPanel     { get; }
        private IDialogViewersController     DialogViewersController     { get; }

        public ViewBetweenLevelAdShower(
            IAdsManager                  _AdsManager,
            GlobalGameSettings           _GlobalGameSettings,
            IViewTimePauser              _TimePauser,
            IViewLevelStageSwitcher      _LevelStageSwitcher,
            ICommonTicker                _CommonTicker,
            ISpecialOfferTimerController _SpecialOfferTimerController,
            ISpecialOfferDialogPanel     _SpecialOfferDialogPanel,
            IDialogViewersController     _DialogViewersController)
        {
            AdsManager                  = _AdsManager;
            GlobalGameSettings          = _GlobalGameSettings;
            TimePauser                  = _TimePauser;
            LevelStageSwitcher          = _LevelStageSwitcher;
            CommonTicker                = _CommonTicker;
            SpecialOfferTimerController = _SpecialOfferTimerController;
            SpecialOfferDialogPanel     = _SpecialOfferDialogPanel;
            DialogViewersController     = _DialogViewersController;
        }

        #endregion

        #region api

        public bool ShowAdEnabled { get; set; }

        public override void Init()
        {
            ShowAdEnabled = true;
            m_TimeWithoutAdsInSeconds = GlobalGameSettings.betweenLevelAdShowIntervalInSeconds;
            CommonTicker.Register(this);
            base.Init();
        }

        public void TryShowAd(UnityAction _OnAdClosed, EAdvertisingType? _AdvertisingType = null)
        {
            bool IsPossibleToShowAd()
            {
                if (m_TimeWithoutAdsInSeconds < GlobalGameSettings.betweenLevelAdShowIntervalInSeconds)
                    return false;
                if (!ShowAdEnabled)
                    return false;
                if (!_AdvertisingType.HasValue)
                    return AdsManager.RewardedAdReady || AdsManager.InterstitialAdReady;
                return _AdvertisingType.Value switch
                {
                    EAdvertisingType.Interstitial => AdsManager.InterstitialAdReady,
                    EAdvertisingType.Rewarded     => AdsManager.RewardedAdReady,
                    _                             => throw new SwitchExpressionException(_AdvertisingType.Value)
                };
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
                    if (!SpecialOfferTimerController.ShownThisSession && !SpecialOfferTimerController.IsTimeGone)
                        ShowSpecialOffer(() => _OnAdClosed?.Invoke());
                    else
                        _OnAdClosed?.Invoke();
                }
                ShowAd(OnBeforeAdShown, OnAdClosedOrFailedToShow, OnAdClosedOrFailedToShow, _AdvertisingType);
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

        private void ShowSpecialOffer(UnityAction _OnPanelClosed)
        {
            SpecialOfferDialogPanel.OnPanelClosedAction = _OnPanelClosed;
            var dv = DialogViewersController.GetViewer(SpecialOfferDialogPanel.DialogViewerId);
            dv.Show(SpecialOfferDialogPanel);
        }

        private void ShowAd(
            UnityAction       _OnBeforeAdShown,
            UnityAction       _OnAdClosed,
            UnityAction       _OnAdFailedToShow,
            EAdvertisingType? _AdvertisingType)
        {
            bool? showRewarded = null;
            if (_AdvertisingType.HasValue)
            {
                showRewarded = _AdvertisingType.Value switch
                {
                    EAdvertisingType.Interstitial => false,
                    EAdvertisingType.Rewarded     => true,
                    _                             => throw new SwitchExpressionException(_AdvertisingType.Value)
                };
            }
            else
            {
                if (AdsManager.RewardedAdReady)          showRewarded = true;
                else if (AdsManager.InterstitialAdReady) showRewarded = false;
            }
            if (!showRewarded.HasValue)
                return;
            ShowAd(showRewarded.Value, _OnBeforeAdShown, _OnAdClosed, _OnAdFailedToShow);
        }

        private void ShowAd(
            bool        _Rewarded,
            UnityAction _OnBeforeAdShown,
            UnityAction _OnAdClosed,
            UnityAction _OnAdFailedToShow)
        {
            if (_Rewarded)
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