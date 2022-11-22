using System.Collections.Generic;
using Common.CameraProviders;
using Common.Constants;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.UI.DialogViewers;
using Common.Utils;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
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
        private          ButtonOnRaycast m_ShopButton;
        private          ButtonOnRaycast m_SettingsButton;
        private          float           m_TopOffset;

        #endregion

        #region inject

        private IModelGame                  Model                   { get; }
        private ICameraProvider             CameraProvider          { get; }
        private IManagersGetter             Managers                { get; }
        private IDialogViewerFullscreen     DialogViewerFullscreen  { get; }
        private IViewInputCommandsProceeder CommandsProceeder       { get; }
        private IViewInputTouchProceeder    ViewInputTouchProceeder { get; }

        private ViewUITopButtons(
            IModelGame                  _Model,
            ICameraProvider             _CameraProvider,
            IManagersGetter             _Managers,
            IDialogViewerFullscreen     _DialogViewerFullscreen,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewInputTouchProceeder    _ViewInputTouchProceeder)
        {
            Model                   = _Model;
            CameraProvider          = _CameraProvider;
            Managers                = _Managers;
            DialogViewerFullscreen  = _DialogViewerFullscreen;
            CommandsProceeder       = _CommandsProceeder;
            ViewInputTouchProceeder = _ViewInputTouchProceeder;
        }

        #endregion

        #region api
        
        public void Init(Vector4 _Offsets)
        {
            m_TopOffset = _Offsets.w;
            InitShopButton();
            InitSettingsButton();
            CameraProvider.ActiveCameraChanged += OnActiveCameraChanged;
        }
        
        public void ShowControls(bool _Show, bool _Instantly)
        {
            if (_Show || _Instantly)
            {
                m_ShopButton.SetGoActive(_Show);
                m_SettingsButton.SetGoActive(_Show);
            }
            m_ShopButton.enabled = _Show;
            m_SettingsButton.enabled = _Show;
        }

        public IEnumerable<Component> GetRenderers()
        {
            return m_Renderers;
        }

        #endregion

        #region nonpublic methods
        
        private void OnActiveCameraChanged(Camera _Camera)
        {
            const float additionalVerticalOffset = 0f;
            const float horizontalOffset = 1f;
            var parent = _Camera.transform;
            var screenBounds = GraphicUtils.GetVisibleBounds(_Camera);
            var scaleVec = Vector2.one * GraphicUtils.AspectRatio * 3f;
            float yPos = screenBounds.max.y - m_TopOffset - additionalVerticalOffset;
            m_ShopButton.transform
                .SetParentEx(parent)
                .SetLocalScaleXY(scaleVec)
                .SetLocalPosX(screenBounds.min.x + horizontalOffset)
                .SetLocalPosY(yPos)
                .SetLocalPosZ(10f);
            m_SettingsButton.transform
                .SetParentEx(parent)
                .SetLocalScaleXY(scaleVec)
                .SetLocalPosX(screenBounds.max.x - horizontalOffset)
                .SetLocalPosY(yPos)
                .SetLocalPosZ(10f);
        }

        private void InitShopButton()
        {
            var cont = CameraProvider.Camera.transform;
            var goShopButton = Managers.PrefabSetManager.InitPrefab(
                cont, CommonPrefabSetNames.UiGame, "shop_button");
            var renderer = goShopButton.GetCompItem<SpriteRenderer>("button");
            renderer.sortingOrder = SortingOrders.GameUI;
            m_Renderers.Add( renderer);
            m_ShopButton = goShopButton.GetCompItem<ButtonOnRaycast>("button");
            m_ShopButton.Init(
                CommandShop, 
                () => Model.LevelStaging.LevelStage, 
                CameraProvider,
                Managers.HapticsManager,
                ViewInputTouchProceeder);
            goShopButton.SetActive(false);
        }

        private void InitSettingsButton()
        {
            var cont = CameraProvider.Camera.transform;
            var goSettingsButton = Managers.PrefabSetManager.InitPrefab(
                cont, CommonPrefabSetNames.UiGame, "settings_button");
            var renderer = goSettingsButton.GetCompItem<SpriteRenderer>("button");
            renderer.sortingOrder = SortingOrders.GameUI;
            m_Renderers.Add( renderer);
            m_SettingsButton = goSettingsButton.GetCompItem<ButtonOnRaycast>("button");
            m_SettingsButton.Init(
                CommandSettings, 
                () => Model.LevelStaging.LevelStage,
                CameraProvider,
                Managers.HapticsManager,
                ViewInputTouchProceeder);
            goSettingsButton.SetActive(false);
        }

        private void CommandShop()
        {
            if (DialogViewerFullscreen.CurrentPanel != null
                && DialogViewerFullscreen.CurrentPanel.AppearingState != EAppearingState.Dissapeared)
                return;
            CommandsProceeder.RaiseCommand(EInputCommand.ShopPanel, null);
        }

        private void CommandSettings()
        {
            if (DialogViewerFullscreen.CurrentPanel != null
                && DialogViewerFullscreen.CurrentPanel.AppearingState != EAppearingState.Dissapeared)
                return;
            CommandsProceeder.RaiseCommand(EInputCommand.SettingsPanel, null);
        }

        #endregion
    }
}