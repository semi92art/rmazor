using Network;
using Network.PacketArgs;
using Network.Packets;
using UICreationSystem;
using UICreationSystem.Factories;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class MenuUI
{
    #region private fields

    private Canvas m_Canvas;
    private RectTransform m_DialogContainer;
    private LoadingPanel m_LoadingPanel;
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
        UiFactory.UiImage(
            UiFactory.UiRectTransform(
                m_Canvas.RTransform(),
                "background",
                UiAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Vector2.one * 0.5f,
                Vector2.zero
            ),
            "menu_background");
    }
    
    private void CreateDialogContainer()
    {
        m_DialogContainer = UiFactory.UiRectTransform(
            m_Canvas.RTransform(),
            "Dialog Container",
            UiAnchor.Create(Vector2.zero, Vector2.one), 
            new Vector2(0, 10),
            Vector2.one * 0.5f,
            new Vector2(-90, -300));
    }

    private void CreateLoadingPanel()
    {
        m_LoadingPanel = LoadingPanel.Create(
            m_DialogContainer,
            "Loading Panel",
            UiAnchor.Create(Vector2.zero, Vector2.one),
            Vector2.zero, 
            Vector2.one * 0.5f, 
            Vector2.zero);
        GameObject transitionPanel = CreateLoadingTransitionPanel();
        Coroutines.StartCoroutine(Coroutines.WaitEndOfFrame(() =>
        {
            m_TransitionRenderer = CircleTransparentTransitionRenderer.Create();
            
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
                                    m_TransitionRenderer.OnTransitionMoment = (_, _Args) =>
                                    {
                                        var rTr = transitionPanel.RTransform();
                                        rTr.anchoredPosition = Vector2.zero;
                                        rTr.sizeDelta = Vector2.zero;
                                        
                                        m_LoadingPanel.DoLoading = false;
                                        m_LoadingPanel.gameObject.SetActive(false);
                                        
                                        m_MainMenuUi = new MainMenuUi(m_Canvas.RTransform(), null);
                                    };
                                    // m_TransitionRenderer.OnTransitionMoment = (_, _Args) =>
                                    // {
                                    //     m_LoadingPanel.DoLoading = false;
                                    //     CreateLoginPanel();
                                    // };
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
                                    m_TransitionRenderer.OnTransitionMoment = (_, _Args) =>
                                    {
                                        m_LoadingPanel.DoLoading = false;
                                        CreateLoginPanel();
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
        UiAnchor anchor = UiAnchor.Create(Vector2.zero, Vector2.one);
        Vector2 anchoredPosition = new Vector2(0, 10);
        Vector2 pivot = Vector2.one * 0.5f;
        Vector2 sizeDelta = new Vector2(-90, -300);
        
        RectTransform rTr = UiFactory.UiRectTransform(
            m_Canvas.RTransform(), "TransitionPanel", anchor, anchoredPosition, pivot, sizeDelta);

        return PrefabInitializer.InitUiPrefab(rTr, "ui_panel_transition", "transition_panel");
    }
    
    private void CreateLoginPanel()
    {
        float indent = 75f;

        RectTransform loginPanel = UICreatorImage.Create(
            m_DialogContainer,
            "login_panel",
            UiAnchor.Create(Vector2.zero, Vector2.one),
            Vector2.zero,
            Vector2.one * 0.5f,
            Vector2.zero,
            "dark_panel");

        //Email Text
        UiTmpTextFactory.Create(
            loginPanel,
            "email",
            UiAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, -26.3f),
            Vector2.one * 0.5f,
            new Vector2(-100, 52.6f),
            "email");

        //Email Input
        RectTransform email = UICreatorImage.Create(
            loginPanel,
            "inputEmail",
            UiAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, -indent - 26.3f),
            Vector2.one * 0.5f,
            new Vector2(-100, 52.6f),
            "InputFieldContainer");

        UiTmpInputFieldFactory.Create(
                email,
                "email_input",
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Vector2.one * 0.5f,
                new Vector2(-100, 52.6f),
                "InputField",
                email.GetComponent<Image>()
        );

        //Password Text
        UiTmpTextFactory.Create(
                loginPanel,
                "password",
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 2 - 26.3f),
                Vector2.one * 0.5f,
                new Vector2(-100, 52.6f),
                "password");

        //Email Input
        RectTransform password = UICreatorImage.Create(
            loginPanel,
            "inputPassword",
            UiAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, -indent * 3 - 26.3f),
            Vector2.one * 0.5f,
            new Vector2(-100, 52.6f),
            "InputFieldContainer");

        UiTmpInputFieldFactory.Create(
            password,
            "password_input",
            UiAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, -26.3f),
            Vector2.one * 0.5f,
            new Vector2(-100, 52.6f),
            "InputField",
            password.GetComponent<Image>()
        );

        //LoginButton
        RectTransform login = UICreatorImage.Create(
                loginPanel,
                "buttonLogin",
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 4 - 26.3f),
                Vector2.one * 0.5f,
                new Vector2(-100, 52.6f),
                "buttonLoginContainer");

        UiTmpButtonFactory.Create(
            login,
            "button",
            "Login",
            UiAnchor.Create(Vector2.zero, Vector2.one),
            Vector2.zero,
            Vector2.one * 0.5f,
            Vector2.zero,
            "buttonLogin",
            () =>
            {
                Debug.Log("LoginButton Pushed");
                //Button functionality
            }
        );

        //AppleButton
        RectTransform appleAccount = UICreatorImage.Create(
                loginPanel,
                "buttonAppleAccount",
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 5 - 26.3f),
                Vector2.one * 0.5f,
                new Vector2(-100, 52.6f),
                "buttonAppleAccountContainer");


        UiTmpButtonFactory.Create(
                appleAccount,
                "button",
                "Login with Apple",
                UiAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Vector2.one * 0.5f,
                Vector2.zero,
                "buttonAppleAccount",
                () =>
                {
                    Debug.Log("AppleAccount Pushed");
                    //Button functionality
                }
                );

        //GoogleButton
        RectTransform googleAccount = UICreatorImage.Create(
               loginPanel,
               "buttonGoogleAccount",
               UiAnchor.Create(Vector2.up, Vector2.one),
               new Vector2(0, -indent * 6 - 26.3f),
               Vector2.one * 0.5f,
               new Vector2(-100, 52.6f),
               "buttonGoogleAccountContainer");

        UiTmpButtonFactory.Create(
                googleAccount,
                "button",
                "Login with Google",
                UiAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Vector2.one * 0.5f,
                Vector2.zero,
                "buttonGoogleAccount",
                () =>
                {
                    Debug.Log("buttonGoogleAccount Pushed");
                    //Button functionality
                }
                );

        //GuestButton
        RectTransform guestAccount = UICreatorImage.Create(
            loginPanel,
              "buttonGuest",
              UiAnchor.Create(Vector2.up, Vector2.one),
              new Vector2(0, -indent * 7 - 26.3f),
            Vector2.one * 0.5f,
              new Vector2(-100, 52.6f),
              "buttonGuestContainer");

        UiTmpButtonFactory.Create(
                guestAccount,
                "button",
                "Login as guest",
                UiAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Vector2.one * 0.5f,
                Vector2.zero,
                "buttonGuest",
                () =>
                {
                    Debug.Log("guestAccount Pushed");
                    //Button functionality
                }
                );

        //RegisterButton
        RectTransform register = UICreatorImage.Create(
            loginPanel,
             "buttonRegister",
             UiAnchor.Create(Vector2.up, Vector2.one),
             new Vector2(-140, -740f),
            Vector2.one * 0.5f,
             new Vector2(-320, 52.6f),
             "buttonRegisterContainer");

        UiTmpButtonFactory.Create(
                register,
                "button",
                "Register",
                UiAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Vector2.one * 0.5f,
                Vector2.zero,
                "buttonRegister",
                () =>
                {
                    Debug.Log("buttonRegister Pushed");
                    //Button functionality
                    this.DestroyLoginPanel();
                    this.CreateRegisterPanel();
                }
                );

        //BackButton
        RectTransform back = UICreatorImage.Create(
            loginPanel,
            "buttonBack",
            UiAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(140, -740f),
            Vector2.one * 0.5f,
            new Vector2(-320, 52.6f),
            "buttonBackContainer");

        UiTmpButtonFactory.Create(
                back,
                "button",
                "Back",
                UiAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Vector2.one * 0.5f,
                Vector2.zero,
                "buttonBack",
                () =>
                {
                    Debug.Log("buttonBack Pushed");
                    //Button functionality
                }
                );
    }

    private void DestroyLoginPanel()
    {
        GameObject loginPanel = m_Canvas.transform.Find("login_panel").gameObject;
        Object.Destroy(loginPanel);
    }

    //RegisterPanel

    private void CreateRegisterPanel()
    {
        float indent = 75f;

        RectTransform registerPanel = UICreatorImage.Create(
                m_Canvas.RTransform(),
                "register_panel",
                UiAnchor.Create(Vector2.zero, Vector2.one),
                new Vector2(0, 10),
                Vector2.one * 0.5f,
                new Vector2(-90, -300),
                "dark_panel");

        //Email Text
        UiTmpTextFactory.Create(
                registerPanel,
                "email",
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Vector2.one * 0.5f,
                new Vector2(-100, 52.6f),
                "email");

        //Email Input
        RectTransform email = UICreatorImage.Create(
            registerPanel,
            "inputEmail",
            UiAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, -indent - 26.3f),
            Vector2.one * 0.5f,
            new Vector2(-100, 52.6f),
            "InputFieldContainer");

        UiTmpInputFieldFactory.Create(
            email,
            "email_input",
            UiAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, -26.3f),
            Vector2.one * 0.5f,
            new Vector2(-100, 52.6f),
            "InputField",
            email.GetComponent<Image>()
        );

        //Password Text
        UiTmpTextFactory.Create(
                registerPanel,
                "password",
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 2 - 26.3f),
                Vector2.one * 0.5f,
                new Vector2(-100, 52.6f),
                "password");

        //Password Input
        RectTransform password = UICreatorImage.Create(
            registerPanel,
            "inputPassword",
            UiAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, -indent * 3 - 26.3f),
            Vector2.one * 0.5f,
            new Vector2(-100, 52.6f),
            "InputFieldContainer");

        UiTmpInputFieldFactory.Create(
            password,
            "password_input",
            UiAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0,  - 26.3f),
            Vector2.one * 0.5f,
            new Vector2(-100, 52.6f),
            "InputField",
            password.GetComponent<Image>()
        );

        //Repeat Password Text
        UiTmpTextFactory.Create(
               registerPanel,
               "repeat_password",
               UiAnchor.Create(Vector2.up, Vector2.one),
               new Vector2(0, -indent * 4 - 26.3f),
               Vector2.one * 0.5f,
               new Vector2(-100, 52.6f),
               "repeat_password");

        //Repeat Password Input
        RectTransform repeatPassword = UICreatorImage.Create(
            registerPanel,
            "inputRepeatPassword",
            UiAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, -indent * 5 - 26.3f),
            Vector2.one * 0.5f,
            new Vector2(-100, 52.6f),
            "InputFieldContainer");

        UiTmpInputFieldFactory.Create(
            repeatPassword,
            "repeatPassword_input",
            UiAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, - 26.3f),
            Vector2.one * 0.5f,
            new Vector2(-100, 52.6f),
            "InputField",
            repeatPassword.GetComponent<Image>()
        );

        //RegisterButton
        RectTransform register = UICreatorImage.Create(
                registerPanel,
                "buttonRegister",
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(140, -indent * 6 - 26.3f),
                Vector2.one * 0.5f,
                new Vector2(-320, 52.6f),
                "buttonRegisterContainer");

        UiTmpButtonFactory.Create(
                register,
                "button",
                "Register",
                UiAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Vector2.one * 0.5f,
                Vector2.zero,
                "buttonRegister",
                () =>
                {
                Debug.Log("buttonRegister Pushed");
                //Button functionality
                }
                );

        //BackButton
        RectTransform back = UICreatorImage.Create(
                registerPanel,
                "buttonBack",
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(140, -740f),
                Vector2.one * 0.5f,
                new Vector2(-320, 52.6f),
                "buttonBackContainer");

        UiTmpButtonFactory.Create(
                back,
                "button",
                "Back",
                UiAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Vector2.one * 0.5f,
                Vector2.zero,
                "buttonBack",
                () =>
                {
                    Debug.Log("buttonBack Pushed");
                    //Button functionality
                    DestroyRegisterPanel();
                    CreateLoginPanel();
                }
                );
    }

    private void DestroyRegisterPanel()
    {
        GameObject registerPanel = m_Canvas.transform.Find("register_panel").gameObject;
        Object.Destroy(registerPanel);
    }
    
    #endregion
}