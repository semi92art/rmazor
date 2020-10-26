using Network;
using Network.PacketArgs;
using Network.Packets;
using UICreationSystem;
using UICreationSystem.Factories;
using UICreationSystem.Panels;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class MenuUI
{
    #region private fields

    private Canvas m_Canvas;
    private ILoadingPanel m_LoadingPanel;
    private IDialogViewer m_DialogViewer;
    private ITransitionRenderer m_TransitionRenderer;
    private MainMenuUi m_MainMenuUi;

    #endregion

    #region constructor

    public MenuUI()
    {
        CreateCanvas();
        CreateBackground();
        CreateDialogContainer();
        CreateLoadingPanel();
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
        m_DialogViewer = dialogPanelObj.GetComponent<TransparentTransitionDialogViewer>();
    }

    private void CreateLoadingPanel()
    {
        m_LoadingPanel = LoadingPanel.Create(m_DialogViewer);
        m_LoadingPanel.Show();
        GameObject transitionPanel = CreateLoadingTransitionPanel();
        Coroutines.StartCoroutine(Coroutines.WaitEndOfFrame(() =>
        {
            m_TransitionRenderer = CircleTransparentTransitionRenderer.Create();
            m_TransitionRenderer.TransitionPanel = transitionPanel.RTransform();
            
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
                            IPacket loginPacket = new LoginUserPacket(new LoginUserPacketRequestArgs
                            {
                                Name = SaveUtils.GetValue<string>(SaveKey.Login),
                                PasswordHash = SaveUtils.GetValue<string>(SaveKey.PasswordHash),
                                DeviceId = SystemInfo.deviceUniqueIdentifier
                            }).OnSuccess(() =>
                                {
                                    Debug.Log("Login successfully");
                                    /*here we must load main menu*/
                                    m_TransitionRenderer.TransitionAction = (_, _Args) =>
                                    {
                                        transitionPanel.RTransform().Set(RtrLites.FullFill);
                                        m_LoadingPanel.DoLoading = false;
                                        m_LoadingPanel.Hide();
                                        
                                        m_MainMenuUi = new MainMenuUi(
                                            m_Canvas.RTransform(),
                                            m_DialogViewer);
                                    };
                                    m_TransitionRenderer.StartTransition();
                                }
                                );
                            loginPacket.OnFail(() =>
                            {
                                if (loginPacket.ErrorMessage.Id == RequestErrorCodes.AccontEntityNotFoundByDeviceId)
                                {
                                    IPacket registerPacket = new RegisterUserPacket(
                                        new RegisterUserUserPacketRequestArgs
                                        {
                                            DeviceId = SystemInfo.deviceUniqueIdentifier
                                        }).OnSuccess(() =>
                                        {
                                            Debug.Log("Register by DeviceId successfully");
                                            /*here we must load main menu*/
                                        })
                                        .OnFail(() => { Debug.LogError($"Register by DeviceId failed: {loginPacket.ErrorMessage.Message}"); });
                                    GameClient.Instance.Send(registerPacket);
                                }
                                else if (loginPacket.ErrorMessage.Id == RequestErrorCodes.WrongLoginOrPassword)
                                {
                                    m_TransitionRenderer.TransitionAction = (_, _Args) =>
                                    {
                                        m_LoadingPanel.DoLoading = false;
                                    };
                                    m_TransitionRenderer.StartTransition();
                                }
                                else
                                    Debug.LogError($"Login failed: {loginPacket.ErrorMessage.Message}");
                            });
                            GameClient.Instance.Send(loginPacket);
                        },
                        delayAnyway));
                    
                    
                })
                .OnFail(() => Debug.LogError("Failed test connection"));
            GameClient.Instance.Send(testPacket);
        }));
    }

    private GameObject CreateLoadingTransitionPanel()
    {
        return PrefabInitializer.InitUiPrefab(
            UiFactory.UiRectTransform(m_Canvas.RTransform(), RtrLites.FullFill),
            "ui_panel_transition", "transition_panel");
    }
    
    
    #endregion
}