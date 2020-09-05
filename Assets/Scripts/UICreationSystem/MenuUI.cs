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

        RectTransform loginPanel = UIFactory.UIImage(
            UIFactory.UIRectTransform(
                m_Canvas.GetComponent<RectTransform>(),
                "login_panel",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                new Vector2(0,10),
                Utility.HalfOne,
                new Vector2(-90, -300)
            ),
            "dark_panel").rectTransform;

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                loginPanel,
                "email",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f)),
            "email");

        UIFactory.UIInputField(
            UIFactory.UIRectTransform(
                loginPanel,
                "email_input",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f)),
            "textbox");

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                loginPanel,
                "password",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 2 - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f)),
            "password");

        UIFactory.UIInputField(
            UIFactory.UIRectTransform(
                loginPanel,
                "password_input",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 3 - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f)),
            "textbox");

        //LoginButton
        RectTransform login = UIFactory.UIImage(
            UIFactory.UIRectTransform(
                loginPanel,
                "buttonLogin",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 4 - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f)
            ),
            "buttonLoginContainer").rectTransform;

        UIFactory.UIImage(
            UIFactory.UIRectTransform(
                login,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "buttonLogin");

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                login,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "buttonLogin");        

        UIFactory.UIButton(
            UIFactory.UIRectTransform(
                login,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero),
            "buttonLogin",
            () =>
            {
                Debug.Log("LoginButton Pushed");
                //Button functionality
            },
            login.GetComponent<Image>());

        //AppleButton
        RectTransform appleAccount = UIFactory.UIImage(
            UIFactory.UIRectTransform(
                loginPanel,
                "buttonAppleAccountLogin",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 5 - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f)
            ),
            "buttonAppleAccountContainer").rectTransform;

        UIFactory.UIImage(
            UIFactory.UIRectTransform(
                appleAccount,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "buttonAppleAccount");

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                appleAccount,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "buttonAppleAccount");

        UIFactory.UIButton(
            UIFactory.UIRectTransform(
                appleAccount,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero),
            "buttonAppleAccount",
            () =>
            {
                Debug.Log("AppleAccount Pushed");
                //Button functionality
            },
            appleAccount.GetComponent<Image>());

        //GoogleButton
        RectTransform googleAccount = UIFactory.UIImage(
            UIFactory.UIRectTransform(
                loginPanel,
                "buttonGoogleAccountLogin",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 6 - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f)
            ),
            "buttonGoogleAccountContainer").rectTransform;

        UIFactory.UIImage(
            UIFactory.UIRectTransform(
                googleAccount,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "buttonGoogleAccount");

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                googleAccount,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "buttonGoogleAccount");

        UIFactory.UIButton(
            UIFactory.UIRectTransform(
                googleAccount,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero),
            "buttonGoogleAccount",
            () =>
            {
                Debug.Log("GoogleAccount Pushed");
                //Button functionality
            },
            googleAccount.GetComponent<Image>());

        //GuestButton
        RectTransform guestAccount = UIFactory.UIImage(
            UIFactory.UIRectTransform(
                loginPanel,
                "buttonGuestLogin",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 7 - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f)
            ),
            "buttonGuestContainer").rectTransform;

        UIFactory.UIImage(
            UIFactory.UIRectTransform(
                guestAccount,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "buttonGuest");

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                guestAccount,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "buttonGuest");

        UIFactory.UIButton(
            UIFactory.UIRectTransform(
                guestAccount,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero),
            "buttonGuest",
            () =>
            {
                Debug.Log("GuestAccount Pushed");
                //Button functionality
            },
            guestAccount.GetComponent<Image>());

        //RegisterButton
        RectTransform register = UIFactory.UIImage(
            UIFactory.UIRectTransform(
                loginPanel,
                "buttonRegister",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(-140, - 740f),
                Utility.HalfOne,
                new Vector2(-320, 52.6f)
            ),
            "buttonRegisterContainer").rectTransform;

        UIFactory.UIImage(
            UIFactory.UIRectTransform(
                register,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "buttonRegister");

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                register,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "buttonRegister");

        UIFactory.UIButton(
            UIFactory.UIRectTransform(
                register,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero),
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
        RectTransform back = UIFactory.UIImage(
            UIFactory.UIRectTransform(
                loginPanel,
                "buttonBack",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(140, -740f),
                Utility.HalfOne,
                new Vector2(-320, 52.6f)
            ),
            "buttonBackContainer").rectTransform;

        UIFactory.UIImage(
            UIFactory.UIRectTransform(
                back,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "buttonBack");

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                back,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "buttonBack");

        UIFactory.UIButton(
            UIFactory.UIRectTransform(
                back,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero),
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

        RectTransform registerPanel = UIFactory.UIImage(
            UIFactory.UIRectTransform(
                m_Canvas.GetComponent<RectTransform>(),
                "register_panel",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                new Vector2(0, 10),
                Utility.HalfOne,
                new Vector2(-90, -300)
            ),
            "dark_panel").rectTransform;

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                registerPanel,
                "email",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f)),
            "email");

        UIFactory.UIInputField(
            UIFactory.UIRectTransform(
                registerPanel,
                "email_input",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f)),
            "textbox");

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                registerPanel,
                "password",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 2 - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f)),
            "password");

        UIFactory.UIInputField(
            UIFactory.UIRectTransform(
                registerPanel,
                "password_input",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 3 - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f)),
            "textbox");

        UIFactory.UIText(
           UIFactory.UIRectTransform(
               registerPanel,
               "repeat password",
               UIAnchor.Create(Vector2.up, Vector2.one),
               new Vector2(0, -indent * 4 - 26.3f),
               Utility.HalfOne,
               new Vector2(-100, 52.6f)),
           "repeat_password");

        UIFactory.UIInputField(
            UIFactory.UIRectTransform(
                registerPanel,
                "password_input",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 5 - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f)),
            "textbox");

        //RegisterButton
        RectTransform register = UIFactory.UIImage(
            UIFactory.UIRectTransform(
                registerPanel,
                "buttonRegister",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(140, -indent * 6 - 26.3f),
                Utility.HalfOne,
                new Vector2(-320, 52.6f)
            ),
            "buttonRegisterContainer").rectTransform;

        UIFactory.UIImage(
            UIFactory.UIRectTransform(
                register,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "buttonRegister");

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                register,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "buttonRegister");

        UIFactory.UIButton(
            UIFactory.UIRectTransform(
                register,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero),
            "buttonRegister",
            () =>
            {
                Debug.Log("buttonRegister Pushed");
                //Button functionality
                
            },
            register.GetComponent<Image>());

        //BackButton
        RectTransform back = UIFactory.UIImage(
            UIFactory.UIRectTransform(
                registerPanel,
                "buttonBack",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(140, -740f),
                Utility.HalfOne,
                new Vector2(-320, 52.6f)
            ),
            "buttonBackContainer").rectTransform;

        UIFactory.UIImage(
            UIFactory.UIRectTransform(
                back,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "buttonBack");

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                back,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "buttonBack");

        UIFactory.UIButton(
            UIFactory.UIRectTransform(
                back,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero),
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