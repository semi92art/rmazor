using System.Collections.Generic;
using Controllers;
using DI.Extensions;
using DialogViewers;
using Entities;
using Exceptions;
using GameHelpers;
using Managers;
using Ticker;
using UI.Factories;
using UI.Managers;
using UI.Panels;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public interface IMainMenuUiLoader
    {
        void Init(bool _OnStart);
    }
    
    public class MainMenuUiLoader : GameObservable, IMainMenuUiLoader
    {
        #region nonpublic members

        private readonly bool m_OnStart;
        private Canvas m_Canvas;
        private IMenuDialogViewer m_MenuDialogViewer;
        private INotificationViewer m_NotificationViewer;
        private ITransitionRenderer m_TransitionRenderer;
        private MainBackgroundRenderer m_MainBackgroundRenderer;
        private LoadingController m_StartLoadingController;

        #endregion

        #region inject
        
        private IMainMenuUI MainMenuUI { get; set; }

        // [Inject]
        // public void Inject(IMainMenuUI _MainMenuUI, IUITicker _Ticker)
        // {
        //     MainMenuUI = _MainMenuUI;
        //     Ticker = _Ticker;
        // }
        
        public MainMenuUiLoader(IMainMenuUI _MainMenuUI, IUITicker _UITicker) : base(_UITicker)
        {
            MainMenuUI = _MainMenuUI;
        }
        
        #endregion
        
        
        #region api


        
        public void Init(bool _OnStart)
        {
            DataFieldsMigrator.InitDefaultDataFieldValues();
            CreateCanvas();
            CreateDialogViewers();
            CreateBackground();
            CreateTransitionRenderer();

            PreloadMainMenu();
            if (_OnStart)
                CreateLoadingPanel();
            else
                ShowMainMenu(false);
        }

        #endregion

        #region nonpublic methods

        private void CreateCanvas()
        {
            m_Canvas = UiFactory.UiCanvas(
                "MenuCanvas",
                RenderMode.ScreenSpaceOverlay,
                false,
                0,
                AdditionalCanvasShaderChannels.None,
                CanvasScaler.ScaleMode.ScaleWithScreenSize,
                new Vector2Int(1920, 1080),
                CanvasScaler.ScreenMatchMode.Shrink,
                0f,
                100,
                true,
                GraphicRaycaster.BlockingObjects.None);
        }

        private void CreateBackground()
        {
            var backgroundPanel = CreateMainMenuBackgroundPanel();
            m_MainBackgroundRenderer = MainBackgroundRenderer.Create();
            RawImage rImage = backgroundPanel.GetCompItem<RawImage>("raw_image");
            rImage.texture = m_MainBackgroundRenderer.Texture;

            m_MenuDialogViewer.AddNotDialogItem(backgroundPanel.RTransform(),
                MenuUiCategory.Loading 
                | MenuUiCategory.Settings 
                | MenuUiCategory.Shop 
                | MenuUiCategory.DailyBonus 
                | MenuUiCategory.MainMenu 
                | MenuUiCategory.SelectGame 
                | MenuUiCategory.PlusMoney);
        }

        private void CreateDialogViewers()
        {
            m_MenuDialogViewer = MainMenuDialogViewer.Create(
                m_Canvas.RTransform(), GetObservers(), (IUITicker)Ticker);
            m_NotificationViewer = MainMenuNotificationViewer.Create(
                m_Canvas.RTransform());
        }

        private void CreateTransitionRenderer()
        {
            var transitionPanelObj = CreateLoadingTransitionPanel();
            m_TransitionRenderer = CircleTransparentTransitionRenderer.Create();
            RawImage rImage = transitionPanelObj.GetCompItem<RawImage>("raw_image");
            rImage.texture = m_TransitionRenderer.Texture;
        }

        private void CreateLoadingPanel()
        {
            bool authFinished = false;
            var loadingPanel = new LoadingPanel(m_MenuDialogViewer, (IUITicker)Ticker);
            loadingPanel.Init();
            m_MenuDialogViewer.Show(loadingPanel);
            m_StartLoadingController = new LoadingController(loadingPanel, _LoadingResult =>
            {
                switch (_LoadingResult)
                {
                    case LoadingResult.Success:
                        ShowMainMenu(true);
                        break;
                    case LoadingResult.Fail:
                        //TODO if failed to load, do something with it
                        break;
                    default:
                        throw new SwitchCaseNotImplementedException(_LoadingResult);
                }
            },
            new List<LoadingStage>
            {
                new LoadingStage(1, "Loading resources", 0.5f, () => {}),
                new LoadingStage(2, "Connecting to server", 0.5f, () =>
                {
                    var authCtrl = new AuthController();
                    authCtrl.Authenticate(_AuthResult =>
                    {
                        bool authorizedAtLeastOnce = SaveUtils.GetValue<bool>(SaveKey.AuthorizedAtLeastOnce);

                        switch (_AuthResult)
                        {
                            case AuthController.AuthResult.LoginSuccess:
                                if (!authorizedAtLeastOnce)
                                {
                                    DataFieldsMigrator.MigrateFromDatabase();
                                    SaveUtils.PutValue(SaveKey.AuthorizedAtLeastOnce, true);
                                }
                                break;
                            case AuthController.AuthResult.RegisterSuccess:
                                DataFieldsMigrator.MigrateFromPrevious();
                                SaveUtils.PutValue(SaveKey.AuthorizedAtLeastOnce, true);
                                break;
                            case AuthController.AuthResult.LoginFailed:
                            case AuthController.AuthResult.RegisterFailed:
                            case AuthController.AuthResult.FailedNoInternet:
                                break;
                            default:
                                throw new SwitchCaseNotImplementedException(_AuthResult);
                        }

                        authFinished = true;
                    });
                })
            });

            m_StartLoadingController.StartStage(1, 
                () => AssetBundleManager.Instance.BundlesLoaded, 
                () => AssetBundleManager.Instance.Errors);
            
            m_StartLoadingController.StartStage(2,
                () => authFinished, () => null);
        }

        private void ShowMainMenu(bool _OnStart)
        {
            if (!_OnStart)
            {
                MainMenuUI.Show();
                return;
            }
            
            m_TransitionRenderer.TransitionAction = (_, _Args) =>
            {
                m_MenuDialogViewer.Back();
                MainMenuUI.Show();
            };
            m_TransitionRenderer.StartTransition();
        }

        private void PreloadMainMenu()
        {
            (MainMenuUI as MainMenuUi)?.InitDirty(
            m_Canvas.RTransform(),
                m_MenuDialogViewer,
                m_NotificationViewer,
                m_MainBackgroundRenderer);
            (MainMenuUI as MainMenuUi)?.AddObservers(GetObservers());
            MainMenuUI.Init();
        }

        private GameObject CreateLoadingTransitionPanel()
        {
            return PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(m_Canvas.RTransform(), RtrLites.FullFill),
                "ui_panel_transition", "transition_panel");
        }

        private GameObject CreateMainMenuBackgroundPanel()
        {
            return PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(m_Canvas.RTransform(), RtrLites.FullFill),
                "main_menu", "main_menu_background_panel");
        }
        
        #endregion
    }
}