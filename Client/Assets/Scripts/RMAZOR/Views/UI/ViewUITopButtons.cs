using System;
using System.Collections.Generic;
using Common.CameraProviders;
using Common.Constants;
using Common.Enums;
using Common.Extensions;
using Common.UI.DialogViewers;
using Common.Utils;
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
        #region constants

        #endregion

        #region nonpublic members
        
        private readonly List<Component> m_Renderers = new List<Component>();
        private ButtonOnRaycast 
            m_OpenShopPanelButton, 
            m_OpenSettingsPanelButton, 
            m_OpenDailyGiftPanelButton,
            m_OpenLevelsPanelButton;
        private float m_TopOffset;
        private bool  m_CanShowDailyGiftPanel;

        private bool CanShowDailyGiftPanel => m_CanShowDailyGiftPanel && Model.LevelStaging.LevelIndex > 0;
        private bool CanShowLevelsPanel    => !m_CanShowDailyGiftPanel;

        #endregion

        #region inject

        private IModelGame                  Model                   { get; }
        private ICameraProvider             CameraProvider          { get; }
        private IManagersGetter             Managers                { get; }
        private IDialogViewerFullscreen     DialogViewerFullscreen  { get; }
        private IViewInputCommandsProceeder CommandsProceeder       { get; }
        private IViewInputTouchProceeder    ViewInputTouchProceeder { get; }
        private IDailyGiftPanel             DailyGiftPanel          { get; }

        private ViewUITopButtons(
            IModelGame                  _Model,
            ICameraProvider             _CameraProvider,
            IManagersGetter             _Managers,
            IDialogViewerFullscreen     _DialogViewerFullscreen,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewInputTouchProceeder    _ViewInputTouchProceeder,
            IDailyGiftPanel             _DailyGiftPanel)
        {
            Model                   = _Model;
            CameraProvider          = _CameraProvider;
            Managers                = _Managers;
            DialogViewerFullscreen  = _DialogViewerFullscreen;
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
            InitOpenShopPanelButton();
            InitOpenSettingsPanelButton();
            InitOpenDailyGiftPanelButton();
            InitOpenLevelsPanelButton();
            CameraProvider.ActiveCameraChanged += OnActiveCameraChanged;
        }
        
        public void ShowControls(bool _Show, bool _Instantly)
        {
            if (_Show || _Instantly)
            {
                m_OpenShopPanelButton .SetGoActive(_Show);
                m_OpenSettingsPanelButton .SetGoActive(_Show);
                if (CanShowDailyGiftPanel)
                    m_OpenDailyGiftPanelButton.SetGoActive(_Show);
                if (CanShowLevelsPanel)
                    m_OpenLevelsPanelButton.SetGoActive(_Show);
            }
            m_OpenShopPanelButton.enabled = _Show;
            m_OpenSettingsPanelButton.enabled = _Show;
            if (CanShowDailyGiftPanel)
                m_OpenDailyGiftPanelButton.enabled = _Show;
            if (CanShowLevelsPanel)
                m_OpenLevelsPanelButton.enabled = _Show;
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
            var dict = SaveUtils.GetValue(SaveKeysRmazor.SessionCountByDays) 
                       ?? new Dictionary<DateTime, int>();
            int sessionsCount = dict.GetSafe(today, out _);
            m_CanShowDailyGiftPanel = sessionsCount == 1;
        }
        
        private void OnActiveCameraChanged(Camera _Camera)
        {
            const float additionalVerticalOffset = 0f;
            const float horizontalOffset = 1f;
            var parent = _Camera.transform;
            var screenBounds = GraphicUtils.GetVisibleBounds(_Camera);
            var scaleVec = Vector2.one * GraphicUtils.AspectRatio * 3f;
            float yPos = screenBounds.max.y - m_TopOffset - additionalVerticalOffset;
            m_OpenShopPanelButton.transform
                .SetParentEx(parent)
                .SetLocalScaleXY(scaleVec)
                .SetLocalPosX(screenBounds.min.x + horizontalOffset)
                .SetLocalPosY(yPos)
                .SetLocalPosZ(10f);
            m_OpenSettingsPanelButton.transform
                .SetParentEx(parent)
                .SetLocalScaleXY(scaleVec)
                .SetLocalPosX(screenBounds.max.x - horizontalOffset)
                .SetLocalPosY(yPos)
                .SetLocalPosZ(10f);
            m_OpenDailyGiftPanelButton.transform
                    .SetParentEx(parent)
                    .SetLocalScaleXY(scaleVec)
                    .SetLocalPosX(screenBounds.max.x - horizontalOffset - 8f)
                    .SetLocalPosY(yPos)
                    .SetLocalPosZ(10f);
            m_OpenLevelsPanelButton.transform
                    .SetParentEx(parent)
                    .SetLocalScaleXY(scaleVec)
                    .SetLocalPosX(screenBounds.max.x - horizontalOffset - 5f)
                    .SetLocalPosY(yPos)
                    .SetLocalPosZ(10f);
        }

        private void InitOpenShopPanelButton()
        {
            var cont = CameraProvider.Camera.transform;
            var goShopButton = Managers.PrefabSetManager.InitPrefab(
                cont, CommonPrefabSetNames.UiGame, "shop_button");
            var renderer = goShopButton.GetCompItem<SpriteRenderer>("button");
            renderer.sortingOrder = SortingOrders.GameUI;
            m_Renderers.Add( renderer);
            m_OpenShopPanelButton = goShopButton.GetCompItem<ButtonOnRaycast>("button");
            m_OpenShopPanelButton.Init(
                OnOpenShopPanelButtonPressed, 
                () => Model.LevelStaging.LevelStage, 
                CameraProvider,
                Managers.HapticsManager,
                ViewInputTouchProceeder);
            goShopButton.SetActive(false);
        }

        private void InitOpenSettingsPanelButton()
        {
            var cont = CameraProvider.Camera.transform;
            var goSettingsButton = Managers.PrefabSetManager.InitPrefab(
                cont, CommonPrefabSetNames.UiGame, "settings_button");
            var renderer = goSettingsButton.GetCompItem<SpriteRenderer>("button");
            renderer.sortingOrder = SortingOrders.GameUI;
            m_Renderers.Add( renderer);
            m_OpenSettingsPanelButton = goSettingsButton.GetCompItem<ButtonOnRaycast>("button");
            m_OpenSettingsPanelButton.Init(
                OnOpenSettingsPanelButtonPressed, 
                () => Model.LevelStaging.LevelStage,
                CameraProvider,
                Managers.HapticsManager,
                ViewInputTouchProceeder);
            goSettingsButton.SetActive(false);
        }

        private void InitOpenDailyGiftPanelButton()
        {
            DailyGiftPanel.OnClose = () =>
            {
                m_OpenDailyGiftPanelButton.SetGoActive(false);
                m_OpenLevelsPanelButton.SetGoActive(true);
                m_CanShowDailyGiftPanel = false;
            };
            var cont = CameraProvider.Camera.transform;
            var goDailyGiftButton = Managers.PrefabSetManager.InitPrefab(
                cont, CommonPrefabSetNames.UiGame, "daily_gift_button");
            var renderer = goDailyGiftButton.GetCompItem<SpriteRenderer>("button_sprite");
            renderer.sortingOrder = SortingOrders.GameUI;
            m_Renderers.Add(renderer);
            m_OpenDailyGiftPanelButton = goDailyGiftButton.GetCompItem<ButtonOnRaycast>("button");
            m_OpenDailyGiftPanelButton.Init(
                OnOpenDailyGiftPanelButtonPressed, 
                () => Model.LevelStaging.LevelStage,
                CameraProvider,
                Managers.HapticsManager,
                ViewInputTouchProceeder);
            goDailyGiftButton.SetActive(false);
        }

        private void InitOpenLevelsPanelButton()
        {
            var cont = CameraProvider.Camera.transform;
            var goLevelsButton = Managers.PrefabSetManager.InitPrefab(
                cont, CommonPrefabSetNames.UiGame, "levels_button");
            var renderer = goLevelsButton.GetCompItem<SpriteRenderer>("button_sprite");
            renderer.sortingOrder = SortingOrders.GameUI;
            m_Renderers.Add(renderer);
            m_OpenLevelsPanelButton = goLevelsButton.GetCompItem<ButtonOnRaycast>("button");
            m_OpenLevelsPanelButton.Init(
                OnOpenLevelsPanelButtonPressed, 
                () => Model.LevelStaging.LevelStage,
                CameraProvider,
                Managers.HapticsManager,
                ViewInputTouchProceeder);
            goLevelsButton.SetActive(false);
        }

        private void OnOpenShopPanelButtonPressed()
        {
            CallCommand(EInputCommand.ShopPanel);
        }

        private void OnOpenSettingsPanelButtonPressed()
        {
            CallCommand(EInputCommand.SettingsPanel);
        }

        private void OnOpenDailyGiftPanelButtonPressed()
        {
            CallCommand(EInputCommand.DailyGiftPanel);
        }
        
        private void OnOpenLevelsPanelButtonPressed()
        {
            CallCommand(EInputCommand.LevelsPanel);
        }

        private void CallCommand(EInputCommand _Command)
        {
            if (DialogViewerFullscreen.CurrentPanel != null
                && DialogViewerFullscreen.CurrentPanel.AppearingState != EAppearingState.Dissapeared)
                return;
            CommandsProceeder.RaiseCommand(_Command, null);
        }

        #endregion
    }
}