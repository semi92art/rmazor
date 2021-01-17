using Constants;
using DialogViewers;
using Entities;
using Extensions;
using GameHelpers;
using Managers;
using Network;
using Network.Packets;
using UI.Factories;
using UI.Managers;
using UI.Panels;
using UnityEngine;
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
                MenuUiCategory.PlusLifes |
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
                float delayAnyway = 1f;

                bool isSuccess = false;
                float startTime = UiTimeProvider.Instance.Time;
                Coroutines.Run(Coroutines.DoWhile(
                    () => delayAnyway = Mathf.Max(2f - (UiTimeProvider.Instance.Time - startTime), 0),
                    null,
                    () => !isSuccess || !AssetBundleManager.Instance.Initialized));

                Coroutines.Run(Coroutines.Delay(
                    () =>
                    {
                        var loginPacket = new LoginUserPacket(new LoginUserPacketRequestArgs
                        {
                            Name = GameClient.Instance.Login,
                            PasswordHash = GameClient.Instance.PasswordHash,
                            DeviceId = GameClient.Instance.DeviceId
                        });
                        loginPacket.OnSuccess(() =>
                            {
                                isSuccess = true;
                                Debug.Log("Login successfully");
                                GameClient.Instance.AccountId = loginPacket.Response.Id;
                                ShowMainMenu(true);
                            }
                        );
                        loginPacket.OnFail(() =>
                        {
                            if (loginPacket.ErrorMessage.Id == ServerErrorCodes.AccountNotFoundByDeviceId)
                            {
                                Debug.LogWarning(loginPacket.ErrorMessage);
                                var registerPacket = new RegisterUserPacket(
                                    new RegisterUserPacketRequestArgs
                                    {
                                        DeviceId = GameClient.Instance.DeviceId,
                                        GameId = GameClient.Instance.DefaultGameId
                                    });
                                registerPacket.OnSuccess(() =>
                                    {
                                        Debug.Log("Register by DeviceId successfully");
                                        GameClient.Instance.AccountId = registerPacket.Response.Id;
                                        var bank = BankManager.Instance.GetBank(true);
                                        
                                        Coroutines.Run(Coroutines.WaitWhile(
                                            () => !bank.Loaded,
                                            () => ShowMainMenu(true)));
                                    })
                                    .OnFail(() => { Debug.LogError(registerPacket.ErrorMessage); });
                                GameClient.Instance.Send(registerPacket);
                            }
                            else if (loginPacket.ErrorMessage.Id == ServerErrorCodes.WrongLoginOrPassword)
                            {
                                Debug.LogError("Login failed: Wrong login or password");
                                ShowMainMenu(true);
                            }
                            else
                            {
                                Debug.LogError(loginPacket.ErrorMessage);
                                ShowMainMenu(true);
                            }
                        });
                        GameClient.Instance.Send(loginPacket);
                    },
                    delayAnyway));
            }));
        }

        private void ShowMainMenu(bool _OnStart)
        {
            if (_OnStart)
            {
                m_TransitionRenderer.TransitionAction = (_, _Args) =>
                {
                    m_LoadingPanel.DoLoading = false;
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
            return PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(m_Canvas.RTransform(), RtrLites.FullFill),
                "ui_panel_transition", "transition_panel");
        }

        private GameObject CreateMainMenuBackgroundPanel()
        {
            return PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(m_Canvas.RTransform(), RtrLites.FullFill),
                "main_menu", "main_menu_background_panel");
        }
    
        #endregion
    }
}