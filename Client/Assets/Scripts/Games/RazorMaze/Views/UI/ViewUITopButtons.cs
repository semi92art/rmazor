using System.Collections.Generic;
using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.InputConfigurators;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.UI
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

        private IModelGame                  Model               { get; }
        private IContainersGetter           ContainersGetter    { get; }
        private ICameraProvider             CameraProvider      { get; }
        private IManagersGetter             Managers            { get; }
        private IBigDialogViewer            BigDialogViewer     { get; }
        private IViewInputCommandsProceeder CommandsProceeder   { get; }

        public ViewUITopButtons(
            IModelGame _Model,
            IContainersGetter _ContainersGetter,
            ICameraProvider _CameraProvider,
            IManagersGetter _Managers,
            IBigDialogViewer _BigDialogViewer,
            IViewInputCommandsProceeder _CommandsProceeder)
        {
            Model = _Model;
            ContainersGetter = _ContainersGetter;
            CameraProvider = _CameraProvider;
            Managers = _Managers;
            BigDialogViewer = _BigDialogViewer;
            CommandsProceeder = _CommandsProceeder;
        }

        #endregion

        #region api
        
        public void Init(Vector4 _Offsets)
        {
            m_TopOffset = _Offsets.w;
            InitTopButtons();
        }
        
        public void ShowControls(bool _Show, bool _Instantly)
        {
            if (_Instantly && !_Show || _Show)
            {
                m_ShopButton.SetGoActive(_Show);
                m_SettingsButton.SetGoActive(_Show);
            }
            
            m_ShopButton.enabled = _Show;
            m_SettingsButton.enabled = _Show;
        }

        public List<Component> GetRenderers()
        {
            return m_Renderers;
        }

        #endregion

        #region nonpublic methods
        
        private void InitTopButtons()
        {
            const float topOffset = 0f;
            const float horOffset = 1f;
            float scale = GraphicUtils.AspectRatio * 3f;
            var screenBounds = GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera);
            float yPos = screenBounds.max.y - m_TopOffset;
            var cont = ContainersGetter.GetContainer(ContainerNames.GameUI);
            var goShopButton = PrefabUtilsEx.InitPrefab(
                cont, "ui_game", "shop_button");
            var goSettingsButton = PrefabUtilsEx.InitPrefab(
                cont, "ui_game", "settings_button");
            m_Renderers.AddRange( new Component[]
            {
                goShopButton.GetCompItem<SpriteRenderer>("button"),
                goSettingsButton.GetCompItem<SpriteRenderer>("button")
            });
            goShopButton.transform.localScale = scale * Vector3.one;
            goShopButton.transform.SetPosXY(
                new Vector2(screenBounds.min.x, yPos)
                + Vector2.right * horOffset + Vector2.down * topOffset);
            goSettingsButton.transform.localScale = scale * Vector3.one;
            goSettingsButton.transform.SetPosXY(
                new Vector2(screenBounds.max.x, yPos)
                + Vector2.left * horOffset + Vector2.down * topOffset);
            m_ShopButton = goShopButton.GetCompItem<ButtonOnRaycast>("button");
            m_SettingsButton = goSettingsButton.GetCompItem<ButtonOnRaycast>("button");
            m_ShopButton.Init(
                CommandShop, 
                () => Model.LevelStaging.LevelStage, 
                CameraProvider,
                Managers.HapticsManager);
            m_SettingsButton.Init(
                CommandSettings, 
                () => Model.LevelStaging.LevelStage,
                CameraProvider,
                Managers.HapticsManager);
            goShopButton.SetActive(false);
            goSettingsButton.SetActive(false);
        }
        
        private void CommandShop()
        {
            if (BigDialogViewer.IsShowing || BigDialogViewer.IsInTransition)
                return;
            Managers.AnalyticsManager.SendAnalytic(AnalyticIds.ShopButtonPressed);
            CommandsProceeder.RaiseCommand(EInputCommand.ShopMenu, null);
        }

        private void CommandSettings()
        {
            if (BigDialogViewer.IsShowing || BigDialogViewer.IsInTransition)
                return;
            Managers.AnalyticsManager.SendAnalytic(AnalyticIds.SettingsButtonPressed);
            CommandsProceeder.RaiseCommand(EInputCommand.SettingsMenu, null);
        }

        #endregion
    }
}