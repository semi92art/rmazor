using System;
using System.Collections.Generic;
using System.Linq;
using Controllers;
using DialogViewers;
using Entities;
using Exceptions;
using Extensions;
using GameHelpers;
using Lean.Localization;
using Managers;
using UI.Factories;
using UI.Managers;
using UI.Panels;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class MenuUi : GameObservable
    {
        #region nonpublic members

        private readonly bool m_OnStart;
        private MainMenuUi m_MainMenuUi;
        private Canvas m_Canvas;
        private LoadingPanel m_LoadingPanel;
        private IMenuDialogViewer m_MenuDialogViewer;
        private INotificationViewer m_NotificationViewer;
        private ITransitionRenderer m_TransitionRenderer;
        private MainBackgroundRenderer m_MainBackgroundRenderer;

        #endregion

        #region api

        public MenuUi(bool _OnStart)
        {
            m_OnStart = _OnStart;
        }

        public void Init()
        {
            CreateCanvas();
            CreateDialogViewers();
            CreateBackground();
            CreateTransitionRenderer();
            
            PreloadMainMenu();
            if (m_OnStart)
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
                MenuUiCategory.Loading |
                MenuUiCategory.Profile |
                MenuUiCategory.Settings |
                MenuUiCategory.Shop |
                MenuUiCategory.DailyBonus |
                MenuUiCategory.MainMenu |
                MenuUiCategory.SelectGame |
                MenuUiCategory.Login |
                MenuUiCategory.PlusMoney);
        }
    
        private void CreateDialogViewers()
        {
            m_MenuDialogViewer = MainMenuDialogViewer.Create(
                m_Canvas.RTransform(), GetObservers());
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
            m_LoadingPanel = new LoadingPanel(m_MenuDialogViewer);
            m_LoadingPanel.Init();
            m_MenuDialogViewer.Show(m_LoadingPanel);

            Coroutines.Run(Coroutines.WaitEndOfFrame(() =>
            {
                bool loadingStopped = false;
                float percents = 0;
                var loadingTexts = new Dictionary<int, string>
                {
                    {1, "Loading resources"},
                    {2, "Connecting to server"},
                    {3, "Done"}
                };
                Func<int, string> getLoadingText = _Idx =>
                    Mathf.Abs(percents - 100) < float.Epsilon ? loadingTexts[3] : loadingTexts[_Idx];
                
                m_LoadingPanel.SetProgress(percents, loadingTexts[1]);

                Coroutines.Run(Coroutines.WaitWhile(
                    () => !AssetBundleManager.Instance.Initialized,
                    () =>
                    {
                        if (AssetBundleManager.Instance.Errors.Any())
                        {
                            m_LoadingPanel.Break("Failed to load game resources. Need internet for first connection");
                            loadingStopped = true;
                        }
                        else
                        {
                            percents += 50;
                            m_LoadingPanel.SetProgress(percents, getLoadingText.Invoke(2));
                        }
                    }, () => loadingStopped));
                
                var authCtrl = new AuthController();
                authCtrl.Authenticate(_AuthResult =>
                {
                    UnityAction setBankStart = () =>
                    {
                        BankManager.Instance.SetBank(new Dictionary<BankItemType, long>
                        {
                            {BankItemType.FirstCurrency, 10},
                            {BankItemType.SecondCurrency, 10}
                        });
                    };
                    
                    switch (_AuthResult)
                    {
                        case AuthController.AuthResult.LoginSuccess:
                            BankManager.Instance.GetBank(true);
                            break;
                        case AuthController.AuthResult.RegisterSuccess:
                            setBankStart.Invoke();
                            BankManager.Instance.GetBank(true);
                            break;
                        case AuthController.AuthResult.LoginFailed:
                        case AuthController.AuthResult.RegisterFailed:
                            var bank = BankManager.Instance.GetBank();
                            Coroutines.Run(Coroutines.WaitWhile(() => !bank.Loaded,
                                () =>
                                {
                                    if (bank.BankItems.Any())
                                        return;
                                    setBankStart.Invoke();
                                }));
                            break;
                        default:
                            throw new SwitchCaseNotImplementedException(_AuthResult);
                    }

                    if (loadingStopped)
                        return;
                    
                    percents += 50;
                    m_LoadingPanel.SetProgress(percents, getLoadingText.Invoke(1));
                    ShowMainMenu(true);
                });
            }));
        }

        private void ShowMainMenu(bool _OnStart)
        {
            if (_OnStart)
            {
                m_TransitionRenderer.TransitionAction = (_, _Args) =>
                {
                    m_LoadingPanel.Break(null);
                    m_MenuDialogViewer.Back();
                    m_MainMenuUi.Show();
                };
                m_TransitionRenderer.StartTransition();
            }
            else
                m_MainMenuUi.Show();
            
        }

        private void PreloadMainMenu()
        {
            m_MainMenuUi = new MainMenuUi(
                m_Canvas.RTransform(),
                m_MenuDialogViewer,
                m_NotificationViewer,
                m_MainBackgroundRenderer);
            m_MainMenuUi.AddObservers(GetObservers());
            m_MainMenuUi.Init();
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