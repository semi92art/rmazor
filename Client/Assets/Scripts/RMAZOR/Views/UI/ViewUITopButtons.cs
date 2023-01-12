using System;
using System.Collections.Generic;
using Common;
using Common.Constants;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Utils;
using RMAZOR.Constants;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.UI.Panels;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.UI
{
    public interface IViewUITopButtons : IInitViewUIItem, IViewUIGetRenderers
    {
        void ShowControls(bool _Show, bool _Instantly);
    }
    
    public class ViewUITopButtons : IViewUITopButtons
    {
        #region nonpublic members
        
        private readonly List<Component> m_Renderers = new List<Component>();
        private ButtonOnRaycast 
            m_DisableAdsButton,
            m_OpenShopPanelButton, 
            m_OpenSettingsPanelButton, 
            m_OpenDailyGiftPanelButton,
            m_OpenLevelsPanelButton,
            m_RateGameButton,
            m_HomeButton;
        private float m_TopOffset;
        private bool  m_CanShowDailyGiftPanel;

        private bool IsNextLevelBonus =>
            (string) Model.LevelStaging.Arguments.GetSafe(
                CommonInputCommandArg.KeyNextLevelType, out _) ==
            CommonInputCommandArg.ParameterLevelTypeBonus;

        private bool CanShowDisableAdsButton
        {
            get
            {
                var saveKeyValue = SaveUtils.GetValue(SaveKeysMazor.DisableAds);
                return !saveKeyValue.HasValue || !saveKeyValue.Value;
            }
        }

        private bool CanShowShopButton
        {
            get
            {
                var saveKeyValue = SaveUtils.GetValue(SaveKeysMazor.DisableAds);
                return saveKeyValue.HasValue && saveKeyValue.Value;
            }
        }
        
        private bool CanShowDailyGiftButton => m_CanShowDailyGiftPanel 
                                               && (Model.LevelStaging.LevelIndex > 0 || IsNextLevelBonus);

        private bool CanShowLevelsButton => false;
        // private bool CanShowLevelsButton => Model.LevelStaging.LevelIndex > 0 || IsNextLevelBonus;
        
        private bool CanShowRateGameButton =>
            !m_CanShowDailyGiftPanel
            && ((Model.LevelStaging.LevelIndex > 8 && !IsNextLevelBonus)
                || (Model.LevelStaging.LevelIndex > 2 && IsNextLevelBonus));

        private bool CanShowHomeButton => true;

        #endregion

        #region inject

        private IModelGame                  Model                   { get; }
        private ICameraProvider             CameraProvider          { get; }
        private IManagersGetter             Managers                { get; }
        private IViewInputCommandsProceeder CommandsProceeder       { get; }
        private IViewInputTouchProceeder    ViewInputTouchProceeder { get; }
        private IDailyGiftPanel             DailyGiftPanel          { get; }

        private ViewUITopButtons(
            IModelGame                  _Model,
            ICameraProvider             _CameraProvider,
            IManagersGetter             _Managers,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewInputTouchProceeder    _ViewInputTouchProceeder,
            IDailyGiftPanel             _DailyGiftPanel)
        {
            Model                   = _Model;
            CameraProvider          = _CameraProvider;
            Managers                = _Managers;
            CommandsProceeder       = _CommandsProceeder;
            ViewInputTouchProceeder = _ViewInputTouchProceeder;
            DailyGiftPanel          = _DailyGiftPanel;
        }

        #endregion

        #region api
        
        public void Init(Vector4 _Offsets)
        {
            m_TopOffset = _Offsets.w;
            CheckIfDailyGiftPanelCanBeOpenedToday();
            InitDisableAdsButton();
            InitOpenShopPanelButton();
            InitOpenSettingsPanelButton();
            InitOpenDailyGiftPanelButton();
            InitOpenLevelsPanelButton();
            InitRateGameButton();
            InitHomeButton();
            CameraProvider.ActiveCameraChanged += OnActiveCameraChanged;
            Managers.ShopManager.AddPurchaseAction(
                PurchaseKeys.NoAds,
                () =>
                {
                    m_DisableAdsButton.SetGoActive(false);
                    m_OpenShopPanelButton.SetGoActive(true);
                });
        }
        
        public void ShowControls(bool _Show, bool _Instantly)
        {
            if (_Show || _Instantly)
            {
                m_OpenSettingsPanelButton .SetGoActive(_Show);
                m_DisableAdsButton        .SetGoActive(_Show && CanShowDisableAdsButton);
                m_OpenShopPanelButton     .SetGoActive(_Show && CanShowShopButton);
                m_OpenDailyGiftPanelButton.SetGoActive(_Show && CanShowDailyGiftButton);
                m_OpenLevelsPanelButton   .SetGoActive(_Show && CanShowLevelsButton);
                m_RateGameButton          .SetGoActive(_Show && CanShowRateGameButton);
                m_HomeButton              .SetGoActive(_Show && CanShowHomeButton);
            }
            m_OpenSettingsPanelButton.enabled  = _Show;
            m_DisableAdsButton.enabled         = _Show && CanShowDisableAdsButton;
            m_OpenShopPanelButton.enabled      = _Show && CanShowShopButton;
            m_OpenDailyGiftPanelButton.enabled = _Show && CanShowDailyGiftButton;
            m_OpenLevelsPanelButton.enabled    = _Show && CanShowLevelsButton;
            m_RateGameButton.enabled           = _Show && CanShowRateGameButton;
            m_HomeButton.enabled               = _Show && CanShowHomeButton;
        }

        public IEnumerable<Component> GetRenderers()
        {
            return m_Renderers;
        }

        #endregion

        #region nonpublic methods

        private void CheckIfDailyGiftPanelCanBeOpenedToday()
        {
            var today = DateTime.Now.Date;
            var dailyRewardGotDict = SaveUtils.GetValue(SaveKeysRmazor.DailyRewardGot)
                                     ?? new Dictionary<DateTime, bool>();
            bool dailyRewardGotToday = dailyRewardGotDict.GetSafe(today, out _);
            m_CanShowDailyGiftPanel = !dailyRewardGotToday;
        }
        
        private void OnActiveCameraChanged(Camera _Camera)
        {
            const float additionalVerticalOffset = 0f;
            const float horizontalOffset = 1f;
            var parent = _Camera.transform;
            var visibleBounds = GraphicUtils.GetVisibleBounds(_Camera);
            var scaleVec = Vector2.one * GraphicUtils.AspectRatio * 3f;
            float yPos = visibleBounds.max.y - m_TopOffset - additionalVerticalOffset;
            m_DisableAdsButton.transform
                .SetParentEx(parent)
                .SetLocalScaleXY(scaleVec)
                .SetLocalPosX(visibleBounds.min.x + horizontalOffset)
                .SetLocalPosY(yPos)
                .SetLocalPosZ(10f);
            m_OpenShopPanelButton.transform
                .SetParentEx(parent)
                .SetLocalScaleXY(scaleVec)
                .SetLocalPosX(visibleBounds.min.x + horizontalOffset)
                .SetLocalPosY(yPos)
                .SetLocalPosZ(10f);
            m_OpenSettingsPanelButton.transform
                .SetParentEx(parent)
                .SetLocalScaleXY(scaleVec)
                .SetLocalPosX(visibleBounds.max.x - horizontalOffset)
                .SetLocalPosY(yPos)
                .SetLocalPosZ(10f);
            m_OpenDailyGiftPanelButton.transform
                    .SetParentEx(parent)
                    .SetLocalScaleXY(scaleVec)
                    .SetLocalPosX(visibleBounds.min.x + horizontalOffset + 5f)
                    .SetLocalPosY(yPos)
                    .SetLocalPosZ(10f);
            m_RateGameButton.transform
                .SetParentEx(parent)
                .SetLocalScaleXY(scaleVec)
                .SetLocalPosX(visibleBounds.min.x + horizontalOffset + 5f)
                .SetLocalPosY(yPos)
                .SetLocalPosZ(10f);
            m_OpenLevelsPanelButton.transform
                    .SetParentEx(parent)
                    .SetLocalScaleXY(scaleVec)
                    .SetLocalPosX(visibleBounds.max.x - horizontalOffset - 5f)
                    .SetLocalPosY(yPos)
                    .SetLocalPosZ(10f);
            m_HomeButton.transform
                .SetParentEx(parent)
                .SetLocalScaleXY(scaleVec)
                .SetLocalPosX(visibleBounds.max.x - horizontalOffset - 5f)
                .SetLocalPosY(yPos)
                .SetLocalPosZ(10f);
        }

        private void InitDisableAdsButton()
        {
            var cont = CameraProvider.Camera.transform;
            var buttonGo = Managers.PrefabSetManager.InitPrefab(
                cont, CommonPrefabSetNames.UiGame, "disable_ads_button");
            var renderer = buttonGo.GetCompItem<SpriteRenderer>("button_sprite");
            renderer.sortingOrder = SortingOrders.GameUI;
            m_Renderers.Add(renderer);
            m_DisableAdsButton = buttonGo.GetCompItem<ButtonOnRaycast>("button");
            m_DisableAdsButton.Init(
                OnDisableAdsButtonPressed, 
                () => Model.LevelStaging.LevelStage, 
                CameraProvider,
                Managers.HapticsManager,
                ViewInputTouchProceeder);
            buttonGo.SetActive(false);
        }

        private void InitOpenShopPanelButton()
        {
            var cont = CameraProvider.Camera.transform;
            var buttonGo = Managers.PrefabSetManager.InitPrefab(
                cont, CommonPrefabSetNames.UiGame, "shop_button");
            var renderer = buttonGo.GetCompItem<SpriteRenderer>("button");
            renderer.sortingOrder = SortingOrders.GameUI;
            m_Renderers.Add(renderer);
            m_OpenShopPanelButton = buttonGo.GetCompItem<ButtonOnRaycast>("button");
            m_OpenShopPanelButton.Init(
                OnOpenShopPanelButtonPressed, 
                () => Model.LevelStaging.LevelStage, 
                CameraProvider,
                Managers.HapticsManager,
                ViewInputTouchProceeder);
            buttonGo.SetActive(false);
        }

        private void InitOpenSettingsPanelButton()
        {
            var cont = CameraProvider.Camera.transform;
            var buttonGo = Managers.PrefabSetManager.InitPrefab(
                cont, CommonPrefabSetNames.UiGame, "settings_button");
            var renderer = buttonGo.GetCompItem<SpriteRenderer>("button");
            renderer.sortingOrder = SortingOrders.GameUI;
            m_Renderers.Add(renderer);
            m_OpenSettingsPanelButton = buttonGo.GetCompItem<ButtonOnRaycast>("button");
            m_OpenSettingsPanelButton.Init(
                OnOpenSettingsPanelButtonPressed, 
                () => Model.LevelStaging.LevelStage,
                CameraProvider,
                Managers.HapticsManager,
                ViewInputTouchProceeder);
            buttonGo.SetActive(false);
        }

        private void InitOpenDailyGiftPanelButton()
        {
            DailyGiftPanel.OnClose = () =>
            {
                m_OpenDailyGiftPanelButton.SetGoActive(false);
                m_CanShowDailyGiftPanel = false;
                m_RateGameButton.SetGoActive(CanShowRateGameButton);
            };
            var cont = CameraProvider.Camera.transform;
            var buttonGo = Managers.PrefabSetManager.InitPrefab(
                cont, CommonPrefabSetNames.UiGame, "daily_gift_button");
            var renderer = buttonGo.GetCompItem<SpriteRenderer>("button_sprite");
            var renderer1 = buttonGo.GetCompItem<SpriteRenderer>("sprite_2");
            renderer.sortingOrder = SortingOrders.GameUI;
            renderer1.sortingOrder = SortingOrders.GameUI;
            m_Renderers.Add(renderer);
            m_Renderers.Add(renderer1);
            m_OpenDailyGiftPanelButton = buttonGo.GetCompItem<ButtonOnRaycast>("button");
            m_OpenDailyGiftPanelButton.Init(
                OnOpenDailyGiftPanelButtonPressed, 
                () => Model.LevelStaging.LevelStage,
                CameraProvider,
                Managers.HapticsManager,
                ViewInputTouchProceeder);
            buttonGo.SetActive(false);
        }

        private void InitOpenLevelsPanelButton()
        {
            var cont = CameraProvider.Camera.transform;
            var buttonGo = Managers.PrefabSetManager.InitPrefab(
                cont, CommonPrefabSetNames.UiGame, "levels_button");
            var renderer = buttonGo.GetCompItem<SpriteRenderer>("button_sprite");
            renderer.sortingOrder = SortingOrders.GameUI;
            m_Renderers.Add(renderer);
            m_OpenLevelsPanelButton = buttonGo.GetCompItem<ButtonOnRaycast>("button");
            m_OpenLevelsPanelButton.Init(
                OnOpenLevelsPanelButtonPressed, 
                () => Model.LevelStaging.LevelStage,
                CameraProvider,
                Managers.HapticsManager,
                ViewInputTouchProceeder);
            buttonGo.SetActive(false);
        }
        
        private void InitRateGameButton()
        {
            var cont = CameraProvider.Camera.transform;
            var buttonGo = Managers.PrefabSetManager.InitPrefab(
                cont, CommonPrefabSetNames.UiGame, "rate_game_button");
            var renderer = buttonGo.GetCompItem<SpriteRenderer>("button_sprite");
            renderer.sortingOrder = SortingOrders.GameUI;
            m_Renderers.Add(renderer);
            m_RateGameButton = buttonGo.GetCompItem<ButtonOnRaycast>("button");
            m_RateGameButton.Init(
                OnRateGameButtonPressed, 
                () => Model.LevelStaging.LevelStage,
                CameraProvider,
                Managers.HapticsManager,
                ViewInputTouchProceeder);
            buttonGo.SetActive(false);
        }

        private void InitHomeButton()
        {
            var cont = CameraProvider.Camera.transform;
            var buttonGo = Managers.PrefabSetManager.InitPrefab(
                cont, CommonPrefabSetNames.UiGame, "home_button");
            var renderer = buttonGo.GetCompItem<SpriteRenderer>("button_sprite");
            renderer.sortingOrder = SortingOrders.GameUI;
            m_Renderers.Add(renderer);
            m_HomeButton = buttonGo.GetCompItem<ButtonOnRaycast>("button");
            m_HomeButton.Init(
                OnHomeButtonPressed, 
                () => Model.LevelStaging.LevelStage,
                CameraProvider,
                Managers.HapticsManager,
                ViewInputTouchProceeder);
            buttonGo.SetActive(false);
        }
        
        private void OnDisableAdsButtonPressed()
        {   
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.DisableAdsMainButtonPressed);
            CallCommand(EInputCommand.DisableAdsPanel);
        }

        private void OnOpenShopPanelButtonPressed()
        {
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.ShopButtonPressed);
            CallCommand(EInputCommand.ShopPanel);
        }

        private void OnOpenSettingsPanelButtonPressed()
        {
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.SettingsButtonPressed);
            CallCommand(EInputCommand.SettingsPanel);
        }

        private void OnOpenDailyGiftPanelButtonPressed()
        {
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.DailyGiftButtonPressed);
            CallCommand(EInputCommand.DailyGiftPanel);
        }
        
        private void OnOpenLevelsPanelButtonPressed()
        {
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.LevelsButtonPressed);
            CallCommand(EInputCommand.LevelsPanel);
        }

        private void OnRateGameButtonPressed()
        {
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.RateGameMainButtonPressed);
            Managers.ShopManager.RateGame();
            SaveUtils.PutValue(SaveKeysCommon.GameWasRated, true);
        }

        private void OnHomeButtonPressed()
        {
            Managers.AnalyticsManager.SendAnalytic(AnalyticIdsRmazor.HomeButtonPressed);
            CallCommand(EInputCommand.MainMenuPanel);
        }

        private void CallCommand(EInputCommand _Command)
        {
            CommandsProceeder.RaiseCommand(_Command, null);
        }

        #endregion
    }
}