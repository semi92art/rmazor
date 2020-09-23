using TMPro;
using UICreationSystem;
using UICreationSystem.Factories;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    #region attributes

    private Canvas m_Canvas;

    #endregion

    #region engine methods

    private void Start()
    {
        CreateCanvas();

        UiFactory.UiImage(
            UiFactory.UiRectTransform(
                m_Canvas.RTransform(),
                "background",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero
            ),
            "menu_background");

        CreateLoginPanel();
    }

    #endregion

    public void CreateCanvas()
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

    public void CreateLoginPanel()
    {
        float indent = 75f;

        RectTransform loginPanel = UICreatorImage.Create(
            m_Canvas.RTransform(),
            "login_panel",
            UIAnchor.Create(Vector2.zero, Vector2.one),
            new Vector2(0, 10),
            Utility.HalfOne,
            new Vector2(-90, -300),
            "dark_panel");

        //Email Text
        UiTmpTextFactory.Create(
            loginPanel,
            "email",
            UIAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, -26.3f),
            Utility.HalfOne,
            new Vector2(-100, 52.6f),
            "email");

        //Email Input
        RectTransform email = UICreatorImage.Create(
            loginPanel,
            "inputEmail",
            UIAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, -indent - 26.3f),
            Utility.HalfOne,
            new Vector2(-100, 52.6f),
            "InputFieldContainer");

        UiTmpInputFieldFactory.Create(
                email,
                "email_input",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "InputField",
                email.GetComponent<Image>()
        );

        //Password Text
        UiTmpTextFactory.Create(
                loginPanel,
                "password",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 2 - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "password");

        //Email Input
        RectTransform password = UICreatorImage.Create(
            loginPanel,
            "inputPassword",
            UIAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, -indent * 3 - 26.3f),
            Utility.HalfOne,
            new Vector2(-100, 52.6f),
            "InputFieldContainer");

        UiTmpInputFieldFactory.Create(
            password,
            "password_input",
            UIAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, -26.3f),
            Utility.HalfOne,
            new Vector2(-100, 52.6f),
            "InputField",
            password.GetComponent<Image>()
        );

        //LoginButton
        RectTransform login = UICreatorImage.Create(
                loginPanel,
                "buttonLogin",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 4 - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "buttonLoginContainer");

        UiTmpButtonFactory.Create(
            login,
            "button",
            "Login",
            UIAnchor.Create(Vector2.zero, Vector2.one),
            Vector2.zero,
            Utility.HalfOne,
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
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 5 - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "buttonAppleAccountContainer");


        UiTmpButtonFactory.Create(
                appleAccount,
                "button",
                "Login with Apple",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
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
               UIAnchor.Create(Vector2.up, Vector2.one),
               new Vector2(0, -indent * 6 - 26.3f),
               Utility.HalfOne,
               new Vector2(-100, 52.6f),
               "buttonGoogleAccountContainer");

        UiTmpButtonFactory.Create(
                googleAccount,
                "button",
                "Login with Google",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
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
              UIAnchor.Create(Vector2.up, Vector2.one),
              new Vector2(0, -indent * 7 - 26.3f),
              Utility.HalfOne,
              new Vector2(-100, 52.6f),
              "buttonGuestContainer");

        UiTmpButtonFactory.Create(
                guestAccount,
                "button",
                "Login as guest",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
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
             UIAnchor.Create(Vector2.up, Vector2.one),
             new Vector2(-140, -740f),
             Utility.HalfOne,
             new Vector2(-320, 52.6f),
             "buttonRegisterContainer");

        UiTmpButtonFactory.Create(
                register,
                "button",
                "Register",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
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
            UIAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(140, -740f),
            Utility.HalfOne,
            new Vector2(-320, 52.6f),
            "buttonBackContainer");

        UiTmpButtonFactory.Create(
                back,
                "button",
                "Back",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero,
                "buttonBack",
                () =>
                {
                    Debug.Log("buttonBack Pushed");
                    //Button functionality
                }
                );
    }

    public void DestroyLoginPanel()
    {
        GameObject loginPanel = m_Canvas.transform.Find("login_panel").gameObject;
        Destroy(loginPanel);
    }

    //RegisterPanel

    public void CreateRegisterPanel()
    {
        float indent = 75f;

        RectTransform registerPanel = UICreatorImage.Create(
                m_Canvas.RTransform(),
                "register_panel",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                new Vector2(0, 10),
                Utility.HalfOne,
                new Vector2(-90, -300),
                "dark_panel");

        //Email Text
        UiTmpTextFactory.Create(
                registerPanel,
                "email",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "email");

        //Email Input
        RectTransform email = UICreatorImage.Create(
            registerPanel,
            "inputEmail",
            UIAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, -indent - 26.3f),
            Utility.HalfOne,
            new Vector2(-100, 52.6f),
            "InputFieldContainer");

        UiTmpInputFieldFactory.Create(
            email,
            "email_input",
            UIAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, -26.3f),
            Utility.HalfOne,
            new Vector2(-100, 52.6f),
            "InputField",
            email.GetComponent<Image>()
        );

        //Password Text
        UiTmpTextFactory.Create(
                registerPanel,
                "password",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 2 - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "password");

        //Password Input
        RectTransform password = UICreatorImage.Create(
            registerPanel,
            "inputPassword",
            UIAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, -indent * 3 - 26.3f),
            Utility.HalfOne,
            new Vector2(-100, 52.6f),
            "InputFieldContainer");

        UiTmpInputFieldFactory.Create(
            password,
            "password_input",
            UIAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0,  - 26.3f),
            Utility.HalfOne,
            new Vector2(-100, 52.6f),
            "InputField",
            password.GetComponent<Image>()
        );

        //Repeat Password Text
        UiTmpTextFactory.Create(
               registerPanel,
               "repeat_password",
               UIAnchor.Create(Vector2.up, Vector2.one),
               new Vector2(0, -indent * 4 - 26.3f),
               Utility.HalfOne,
               new Vector2(-100, 52.6f),
               "repeat_password");

        //Repeat Password Input
        RectTransform repeatPassword = UICreatorImage.Create(
            registerPanel,
            "inputRepeatPassword",
            UIAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, -indent * 5 - 26.3f),
            Utility.HalfOne,
            new Vector2(-100, 52.6f),
            "InputFieldContainer");

        UiTmpInputFieldFactory.Create(
            repeatPassword,
            "repeatPassword_input",
            UIAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, - 26.3f),
            Utility.HalfOne,
            new Vector2(-100, 52.6f),
            "InputField",
            repeatPassword.GetComponent<Image>()
        );

        //RegisterButton
        RectTransform register = UICreatorImage.Create(
                registerPanel,
                "buttonRegister",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(140, -indent * 6 - 26.3f),
                Utility.HalfOne,
                new Vector2(-320, 52.6f),
                "buttonRegisterContainer");

        UiTmpButtonFactory.Create(
                register,
                "button",
                "Register",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
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
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(140, -740f),
                Utility.HalfOne,
                new Vector2(-320, 52.6f),
                "buttonBackContainer");

        UiTmpButtonFactory.Create(
                back,
                "button",
                "Back",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero,
                "buttonBack",
                () =>
                {
                    Debug.Log("buttonBack Pushed");
                    //Button functionality
                    this.DestroyRegisterPanel();
                    this.CreateLoginPanel();
                }
                );
    }

    public void DestroyRegisterPanel()
    {
        GameObject registerPanel = m_Canvas.transform.Find("register_panel").gameObject;
        Destroy(registerPanel);
    }
}