using Extentions;
using Network;
using Network.PacketArgs;
using Network.Packets;
using UICreationSystem;
using UICreationSystem.Factories;
using UICreationSystem.Panels;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class MenuUi
{
    #region private fields

    private Canvas m_Canvas;
    private ILoadingPanel m_LoadingPanel;
    private IDialogViewer m_DialogViewer;
    private ITransitionRenderer m_TransitionRenderer;

    #endregion

    #region constructor

    public MenuUi()
    {
        CreateCanvas();
        CreateBackground();
        CreateDialogContainer();
        CreateLoadingPanel();
    }

    #endregion
    
    #region factory

    public static void Create()
    {
        new MenuUi();
    }
    
    #endregion
    
    #region private methods

    private void CreateCanvas()
    {
        m_Canvas = UiFactory.UiCanvas(
            "MenuCanvas",
            RenderMode.ScreenSpaceOverlay,
            true,
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
        PrefabInitializer.InitUiPrefab(
            UiFactory.UiRectTransform(
                m_Canvas.RTransform(),
                RtrLites.FullFill),
            "main_menu",
            "background_panel");
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

    private void CreateLoadingPanel()
    {
        m_LoadingPanel = new LoadingPanel(m_DialogViewer);
        m_LoadingPanel.Show();
        
        Debug.Log($"Device Id: {GameClient.Instance.DeviceId}");
        Debug.Log($"Account Id: {GameClient.Instance.AccountId}");
        
        Coroutines.StartCoroutine(Coroutines.WaitEndOfFrame(() =>
        {
            var transitionPanelObj = CreateLoadingTransitionPanel();
            m_TransitionRenderer = CircleTransparentTransitionRenderer.Create();
            RawImage rImage = transitionPanelObj.GetComponentItem<RawImage>("raw_image");
            rImage.texture = m_TransitionRenderer.Texture;
            //wait 2 seconds anyway
            float delayAnyway = 2f;
            bool isSuccess = false;
            float startTime = Time.time;
            Coroutines.StartCoroutine(Coroutines.DoWhile(
                () => delayAnyway = Mathf.Max(2f - (Time.time - startTime), 0),
                null,
                () => !isSuccess,
                () => true));

            IPacket testPacket = new TestConnectionPacket(null)
                .OnSuccess(() =>
                {
                    Debug.Log("Test connection successfully");
                    isSuccess = true;
                    Coroutines.StartCoroutine(Coroutines.Delay(
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
                                    IPacket registerPacket = new RegisterUserPacket(
                                        new RegisterUserUserPacketRequestArgs
                                        {
                                            DeviceId = GameClient.Instance.DeviceId
                                        }).OnSuccess(() =>
                                        {
                                            Debug.Log("Register by DeviceId successfully");
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
                    
                    
                })
                .OnFail(() => Debug.LogError("Failed test connection"));
            GameClient.Instance.Send(testPacket);
        }));
    }

    private void LoadMainMenu()
    {
        m_TransitionRenderer.TransitionAction = (_, _Args) =>
        {
            m_LoadingPanel.DoLoading = false;
            m_LoadingPanel.Hide();
                                        
            MainMenuUi.Create(
                m_Canvas.RTransform(),
                m_DialogViewer);
        };
        m_TransitionRenderer.StartTransition();
    }

    private GameObject CreateLoadingTransitionPanel()
    {
        return PrefabInitializer.InitUiPrefab(
            UiFactory.UiRectTransform(m_Canvas.RTransform(), RtrLites.FullFill),
            "ui_panel_transition", "transition_panel");
    }
    
    #endregion
}