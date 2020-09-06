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

        UIFactory.UIImage(
            UIFactory.UIRectTransform(
                m_Canvas.GetComponent<RectTransform>(),
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
        m_Canvas = UIFactory.UICanvas(
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
                m_Canvas.GetComponent<RectTransform>(),
                "login_panel",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                new Vector2(0, 10),
                Utility.HalfOne,
                new Vector2(-90, -300),
                "dark_panel");

        UICreatorText.Create(
                loginPanel,
                "email",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "email");

        //UICreatorInputField.Create(
        //        loginPanel,
        //        "email_input",
        //        UIAnchor.Create(Vector2.up, Vector2.one),
        //        new Vector2(0, -indent - 26.3f),
        //        Utility.HalfOne,
        //        new Vector2(-100, 52.6f),
        //        "textbox");

        RectTransform email = UICreatorImage.Create(
               loginPanel,
               "inputEmail",
               UIAnchor.Create(Vector2.up, Vector2.one),
               new Vector2(0, -indent - 26.3f),
               Utility.HalfOne,
               new Vector2(-100, 52.6f),
               "InputFieldContainer");

        UICreatorText.Create(
                email,
                "emailText",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "InputField");

        UICreatorText.Create(
                email,
                "emailPlaceholder",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "InputField");

        UICreatorInputField.Create(
                email,
                "email_input",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "InputField",
                email.GetComponent<Image>(),
                email.Find("emailText").GetComponent<Text>(),
                email.Find("emailPlaceholder").GetComponent<Text>());

        UICreatorText.Create(
                loginPanel,
                "password",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 2 - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "password");

        //UICreatorInputField.Create(
        //        loginPanel,
        //        "password_input",
        //        UIAnchor.Create(Vector2.up, Vector2.one),
        //        new Vector2(0, -indent * 3 - 26.3f),
        //        Utility.HalfOne,
        //        new Vector2(-100, 52.6f),
        //        "textbox");

        //LoginButton
        //TODO явно напрашивается некая конструкция на всю кнопку (кнопка+имэджи)
        RectTransform login = UICreatorImage.Create(
                loginPanel,
                "buttonLogin",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 4 - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "buttonLoginContainer");

        UICreatorImage.Create(
                login,
                "frame",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0f, -26f),
                Utility.HalfOne,
                new Vector2(6f, 60f),
                "buttonWhiteFrame");

        UICreatorImage.Create(
                login,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f),
                "buttonLogin");

        UICreatorText.Create(
                login,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f),
                "buttonLogin");

        UICreatorButton.Create(
                login,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero,
                "buttonLogin",
                () =>
                {
                    Debug.Log("LoginButton Pushed");
                    //Button functionality
                },
                login.GetComponent<Image>());

        //AppleButton
        RectTransform appleAccount = UICreatorImage.Create(
                loginPanel,
                "buttonAppleAccount",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 5 - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "buttonAppleAccountContainer");

        UICreatorImage.Create(
                appleAccount,
                "frame",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0f, -26f),
                Utility.HalfOne,
                new Vector2(6f, 60f),
                "buttonWhiteFrame");

        UICreatorImage.Create(
                appleAccount,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f),
                "buttonAppleAccount");

        UICreatorText.Create(
                appleAccount,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f),
                "buttonAppleAccount");

        UICreatorButton.Create(
                appleAccount,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero,
                "buttonAppleAccount",
                () =>
                {
                    Debug.Log("AppleAccount Pushed");
                    //Button functionality
                },
                appleAccount.GetComponent<Image>());

        //GoogleButton
        RectTransform googleAccount = UICreatorImage.Create(
               loginPanel,
               "buttonGoogleAccount",
               UIAnchor.Create(Vector2.up, Vector2.one),
               new Vector2(0, -indent * 6 - 26.3f),
               Utility.HalfOne,
               new Vector2(-100, 52.6f),
               "buttonGoogleAccountContainer");

        UICreatorImage.Create(
                googleAccount,
                "frame",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0f, -26f),
                Utility.HalfOne,
                new Vector2(6f, 60f),
                "buttonWhiteFrame");

        UICreatorImage.Create(
                googleAccount,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f),
                "buttonGoogleAccount");

        UICreatorText.Create(
                googleAccount,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f),
                "buttonGoogleAccount");

        UICreatorButton.Create(
                googleAccount,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero,
                "buttonGoogleAccount",
                () =>
                {
                    Debug.Log("buttonGoogleAccount Pushed");
                    //Button functionality
                },
                googleAccount.GetComponent<Image>());

        //GuestButton
        RectTransform guestAccount = UICreatorImage.Create(
            loginPanel,
              "buttonGuest",
              UIAnchor.Create(Vector2.up, Vector2.one),
              new Vector2(0, -indent * 7 - 26.3f),
              Utility.HalfOne,
              new Vector2(-100, 52.6f),
              "buttonGuestContainer");

        UICreatorImage.Create(
                guestAccount,
                "frame",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0f, -26f),
                Utility.HalfOne,
                new Vector2(6f, 60f),
                "buttonWhiteFrame");

        UICreatorImage.Create(
                guestAccount,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f),
                "buttonGuest");

        UICreatorText.Create(
                guestAccount,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f),
                "buttonGuest");

        UICreatorButton.Create(
                guestAccount,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero,
                "buttonGuest",
                () =>
                {
                    Debug.Log("guestAccount Pushed");
                    //Button functionality
                },
                guestAccount.GetComponent<Image>());

        //RegisterButton
        RectTransform register = UICreatorImage.Create(
            loginPanel,
             "buttonRegister",
             UIAnchor.Create(Vector2.up, Vector2.one),
             new Vector2(-140, -740f),
             Utility.HalfOne,
             new Vector2(-320, 52.6f),
             "buttonRegisterContainer");

        UICreatorImage.Create(
                register,
                "frame",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0f, -26f),
                Utility.HalfOne,
                new Vector2(6f, 60f),
                "buttonWhiteFrame");

        UICreatorImage.Create(
                register,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f),
                "buttonRegister");

        UICreatorText.Create(
                register,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f),
                "buttonRegister");

        UICreatorButton.Create(
                register,
                "button",
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
                },
                register.GetComponent<Image>());

        //BackButton

        RectTransform back = UICreatorImage.Create(
            loginPanel,
            "buttonBack",
            UIAnchor.Create(Vector2.up, Vector2.one),
            new Vector2(140, -740f),
            Utility.HalfOne,
            new Vector2(-320, 52.6f),
            "buttonBackContainer");

        UICreatorImage.Create(
                back,
                "frame",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0f, -26f),
                Utility.HalfOne,
                new Vector2(6f, 60f),
                "buttonWhiteFrame");

        UICreatorImage.Create(
                back,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f),
                "buttonBack");

        UICreatorText.Create(
                back,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f),
                "buttonBack");

        UICreatorButton.Create(
                back,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero,
                "buttonBack",
                () =>
                {
                    Debug.Log("buttonBack Pushed");
                    //Button functionality
                },
                back.GetComponent<Image>());
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
                m_Canvas.GetComponent<RectTransform>(),
                "register_panel",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                new Vector2(0, 10),
                Utility.HalfOne,
                new Vector2(-90, -300),
                "dark_panel");

        UICreatorText.Create(
                registerPanel,
                "email",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "email");

        //RectTransform email = UICreatorImage.Create(
        //        registerPanel,
        //        "inputEmail",
        //        UIAnchor.Create(Vector2.up, Vector2.one),
        //        new Vector2(140, -indent - 26.3f),
        //        Utility.HalfOne,
        //        new Vector2(-320, 52.6f),
        //        "InputFieldContainer");

        //UICreatorInputField.Create(
        //        email,
        //        "email_input",
        //        UIAnchor.Create(Vector2.up, Vector2.one),
        //        new Vector2(0, - 26.3f),
        //        Utility.HalfOne,
        //        new Vector2(-100, 52.6f),
        //        "textbox",
        //        email.GetComponent<Image>());

        UICreatorText.Create(
                registerPanel,
                "password",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 2 - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "password");

        //UICreatorInputField.Create(
        //        registerPanel,
        //        "password_input",
        //        UIAnchor.Create(Vector2.up, Vector2.one),
        //        new Vector2(0, -indent * 3 - 26.3f),
        //        Utility.HalfOne,
        //        new Vector2(-100, 52.6f),
        //        "textbox");

        UICreatorText.Create(
               registerPanel,
               "repeat password",
               UIAnchor.Create(Vector2.up, Vector2.one),
               new Vector2(0, -indent * 4 - 26.3f),
               Utility.HalfOne,
               new Vector2(-100, 52.6f),
               "repeat_password");


        //UICreatorInputField.Create(
        //        registerPanel,
        //        "password_input",
        //        UIAnchor.Create(Vector2.up, Vector2.one),
        //        new Vector2(0, -indent * 5 - 26.3f),
        //        Utility.HalfOne,
        //        new Vector2(-100, 52.6f),
        //        "textbox");

        //RegisterButton
        RectTransform register = UICreatorImage.Create(
                registerPanel,
                "buttonRegister",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(140, -indent * 6 - 26.3f),
                Utility.HalfOne,
                new Vector2(-320, 52.6f),
                "buttonRegisterContainer");

        UICreatorImage.Create(
                register,
                "frame",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0f, -26f),
                Utility.HalfOne,
                new Vector2(6f, 60f),
                "buttonWhiteFrame");

        UICreatorImage.Create(
                register,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f),
                "buttonRegister");

        UICreatorText.Create(
                register,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f),
                "buttonRegister");

        UICreatorButton.Create(
                register,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero,
                "buttonRegister",
                () =>
                {
                Debug.Log("buttonRegister Pushed");
                //Button functionality
                },
                register.GetComponent<Image>());

        //BackButton
        RectTransform back = UICreatorImage.Create(
                registerPanel,
                "buttonBack",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(140, -740f),
                Utility.HalfOne,
                new Vector2(-320, 52.6f),
                "buttonBackContainer");

        UICreatorImage.Create(
                back,
                "frame",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0f, -26f),
                Utility.HalfOne,
                new Vector2(6f, 60f),
                "buttonWhiteFrame");

        UICreatorImage.Create(
                back,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f),
                "buttonBack");

        UICreatorText.Create(
                back,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f),
                "buttonBack");

        UICreatorButton.Create(
                back,
                "button",
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
                },
                back.GetComponent<Image>());
    }

    public void DestroyRegisterPanel()
    {
        GameObject registerPanel = m_Canvas.transform.Find("register_panel").gameObject;
        Destroy(registerPanel);
    }
}