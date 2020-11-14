using Constants;
using Extensions;
using Helpers;
using Managers;
using Network;
using Network.PacketArgs;
using Network.Packets;
using UI.Factories;
using UI.Managers;
using UI.Panels;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class MenuUi
    {
        #region private fields

        private Canvas m_Canvas;
        private ILoadingPanel m_LoadingPanel;
        private IDialogViewer m_DialogViewer;
        private ITransitionRenderer m_TransitionRenderer;
        private RectTransform m_Background;

        #endregion

        #region constructor

        private MenuUi(bool _OnStart)
        {
            CreateCanvas();
            CreateDialogContainer();
            CreateBackground();
            CreateTransitionRenderer();
            if (_OnStart)
                CreateLoadingPanel();
            else
                LoadMainMenu(false);
        }

        #endregion
    
        #region factory

        public static MenuUi Create(bool _OnStart)
        {
            return new MenuUi(_OnStart);
        }
    
        #endregion
    
        #region private methods

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
            var go = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Canvas.RTransform(),
                    RtrLites.FullFill),
                "main_menu",
                "background_panel");
            m_Background = go.RTransform();
        
            m_DialogViewer.AddNotDialogItem(m_Background, 
                UiCategory.Loading |
                UiCategory.Profile |
                UiCategory.Settings |
                UiCategory.Shop |
                UiCategory.DailyBonus |
                UiCategory.MainMenu |
                UiCategory.SelectGame |
                UiCategory.Login |
                UiCategory.Countries);
        }
    
        private void CreateDialogContainer()
        {
            var dialogPanelObj = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Canvas.RTransform(),
                    RtrLites.FullFill),
                "main_menu",
                "dialog_viewer");
            m_DialogViewer = dialogPanelObj.GetComponent<DefaultDialogViewer>();
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
            m_LoadingPanel = new LoadingPanel(m_DialogViewer);
            m_LoadingPanel.Show();

            Coroutines.Run(Coroutines.WaitEndOfFrame(() =>
            {
                float delayAnyway = 2f;
                bool isSuccess = false;
                float startTime = Time.time;
                Coroutines.Run(Coroutines.DoWhile(
                    () => delayAnyway = Mathf.Max(2f - (Time.time - startTime), 0),
                    null,
                    () => !isSuccess,
                    () => true));

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
                                LoadMainMenu();
                            }
                        );
                        loginPacket.OnFail(() =>
                        {
                            if (loginPacket.ErrorMessage.Id == RequestErrorCodes.AccontEntityNotFoundByDeviceId)
                            {
                                Debug.LogWarning(loginPacket.ErrorMessage);
                                var registerPacket = new RegisterUserPacket(
                                    new RegisterUserUserPacketRequestArgs
                                    {
                                        DeviceId = GameClient.Instance.DeviceId
                                    });
                                registerPacket.OnSuccess(() =>
                                    {
                                        Debug.Log("Register by DeviceId successfully");
                                        GameClient.Instance.AccountId = registerPacket.Response.Id;
                                        MoneyManager.Instance.GetBank(true);
                                        LoadMainMenu();
                                    })
                                    .OnFail(() => { Debug.LogError(loginPacket.ErrorMessage); });
                                GameClient.Instance.Send(registerPacket);
                            }
                            else if (loginPacket.ErrorMessage.Id == RequestErrorCodes.WrongLoginOrPassword)
                            {
                                Debug.LogError("Login failed: Wrong login or password");
                                LoadMainMenu();
                            }
                            else
                            {
                                Debug.LogError(loginPacket.ErrorMessage);
                                LoadMainMenu();
                            }
                        });
                        GameClient.Instance.Send(loginPacket);
                    },
                    delayAnyway));
            }));
        }

        private void LoadMainMenu(bool _OnStart = true)
        {
            if (_OnStart)
            {
                m_TransitionRenderer.TransitionAction = (_, _Args) =>
                {
                    m_LoadingPanel.DoLoading = false;
                    m_LoadingPanel.Hide();
                    LoadMainMenuCore();
                };
                m_TransitionRenderer.StartTransition();
            }
            else
                LoadMainMenuCore();
            
        }

        private void LoadMainMenuCore()
        {
            MainMenuUi.Create(
                m_Canvas.RTransform(),
                m_DialogViewer);
        }

        private GameObject CreateLoadingTransitionPanel()
        {
            return PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(m_Canvas.RTransform(), RtrLites.FullFill),
                "ui_panel_transition", "transition_panel");
        }
    
        #endregion
    }
}