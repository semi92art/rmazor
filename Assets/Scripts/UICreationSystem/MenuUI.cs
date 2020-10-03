using TMPro;
using UICreationSystem;
using UICreationSystem.Factories;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class MenuUI : MonoBehaviour
{
    #region attributes

    private Canvas m_canvas;
    private Resolution m_resolution;

    #endregion

    #region engine methods

    private void Start()
    {
        m_resolution = Screen.currentResolution;
        CreateCanvas();

        UiFactory.UiImage(
            UiFactory.UiRectTransform(
                m_canvas.RTransform(),
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
        m_canvas = UiFactory.UiCanvas(
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
        float smallIndent = 60f;
        float positionY = -26.3f;

        RectTransform loginPanel = UICreatorImage.Create(
            m_canvas.RTransform(),
            "login_panel",
            UIAnchor.Create(Vector2.zero, Vector2.one),
            new Vector2(0, 10),
            Utility.HalfOne,
            new Vector2(-60, -300),
            "dark_panel");

        //Email Text
        UiTmpTextFactory.Create(
            loginPanel,
            "email",
            UIAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, positionY),
            Utility.HalfOne,
            new Vector2(-100, 52.6f),
            "email");

        //Email Input
        positionY -= smallIndent;
        RectTransform email = UICreatorImage.Create(
            loginPanel,
            "inputEmail",
            UIAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, positionY),
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
        positionY -= indent*0.7f;
        UiTmpTextFactory.Create(
                loginPanel,
                "password",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, positionY),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "password");

        //Email Input
        positionY -= smallIndent;
        RectTransform password = UICreatorImage.Create(
            loginPanel,
            "inputPassword",
            UIAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, positionY),
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
        positionY -= indent;
        RectTransform login = UICreatorImage.Create(
                loginPanel,
                "buttonLogin",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, positionY),
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
        positionY -= indent;
        RectTransform appleAccount = UICreatorImage.Create(
                loginPanel,
                "buttonAppleAccount",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, positionY),
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
        positionY -= indent;
        RectTransform googleAccount = UICreatorImage.Create(
               loginPanel,
               "buttonGoogleAccount",
               UIAnchor.Create(Vector2.up, Vector2.one),
               new Vector2(0, positionY),
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
        positionY -= indent;
        RectTransform guestAccount = UICreatorImage.Create(
            loginPanel,
              "buttonGuest",
              UIAnchor.Create(Vector2.up, Vector2.one),
              new Vector2(0, positionY),
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
        float x = Screen.height / Screen.width;
        float buttonSizeDelta = 39.4706f * x * x * (-1) + 257.7418f * x - 697.7933f;
        RectTransform register = UICreatorImage.Create(
            loginPanel,
             "buttonRegister",
             UIAnchor.Create(Vector2.up, Vector2.one),
             new Vector2(-140, -740f),
             Utility.HalfOne,
             new Vector2(buttonSizeDelta, 52.6f),
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
            new Vector2(buttonSizeDelta, 52.6f),
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
        GameObject loginPanel = m_canvas.transform.Find("login_panel").gameObject;
        Destroy(loginPanel);
    }

    //RegisterPanel

    public void CreateRegisterPanel()
    {
        float indent = 75f;
        float smallIndent = 60f;
        float positionY = -26.3f;

        RectTransform registerPanel = UICreatorImage.Create(
                m_canvas.RTransform(),
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
                new Vector2(0, positionY),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "email");

        //Email Input
        positionY -= smallIndent;
        RectTransform email = UICreatorImage.Create(
            registerPanel,
            "inputEmail",
            UIAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, positionY),
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
        positionY -= indent * 0.7f;
        UiTmpTextFactory.Create(
                registerPanel,
                "password",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, positionY),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "password");

        //Password Input
        positionY -= smallIndent;
        RectTransform password = UICreatorImage.Create(
            registerPanel,
            "inputPassword",
            UIAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, positionY),
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
        positionY -= indent*0.7f;
        UiTmpTextFactory.Create(
               registerPanel,
               "repeat_password",
               UIAnchor.Create(Vector2.up, Vector2.one),
               new Vector2(0, positionY),
               Utility.HalfOne,
               new Vector2(-100, 52.6f),
               "repeat_password");

        //Repeat Password Input
        positionY -= smallIndent;
        RectTransform repeatPassword = UICreatorImage.Create(
            registerPanel,
            "inputRepeatPassword",
            UIAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(0, positionY),
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
        positionY -= indent;
        RectTransform register = UICreatorImage.Create(
                registerPanel,
                "buttonRegister",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(140, positionY),
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
        GameObject registerPanel = m_canvas.transform.Find("register_panel").gameObject;
        Destroy(registerPanel);
    }
}