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

        CreateLogin();
    }

    #endregion

    public void CreateCanvas()
    {
        m_Canvas = UIFactory.UICanvas(
                   "Canvas",
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

    public void CreateLogin()
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
            "login_email");

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                loginPanel,
                "email_input",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f)),
            "login_textbox");

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                loginPanel,
                "password",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 2 - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f)),
            "login_password");

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                loginPanel,
                "password_input",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -indent * 3 - 26.3f),
                Utility.HalfOne,
                new Vector2(-100, 52.6f)),
            "login_textbox");

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
            "login_buttonLoginContainer").rectTransform;

        UIFactory.UIImage(
            UIFactory.UIRectTransform(
                login,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "login_buttonLogin");

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                login,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "login_buttonLogin");        

        UIFactory.UIButton(
            UIFactory.UIRectTransform(
                login,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero),
            "login_buttonLogin",
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
            "login_buttonAppleAccountContainer").rectTransform;

        UIFactory.UIImage(
            UIFactory.UIRectTransform(
                appleAccount,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "login_buttonAppleAccount");

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                appleAccount,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "login_buttonAppleAccount");

        UIFactory.UIButton(
            UIFactory.UIRectTransform(
                appleAccount,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero),
            "login_buttonAppleAccount",
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
            "login_buttonGoogleAccountContainer").rectTransform;

        UIFactory.UIImage(
            UIFactory.UIRectTransform(
                googleAccount,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "login_buttonGoogleAccount");

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                googleAccount,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "login_buttonGoogleAccount");

        UIFactory.UIButton(
            UIFactory.UIRectTransform(
                googleAccount,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero),
            "login_buttonGoogleAccount",
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
            "login_buttonGuestContainer").rectTransform;

        UIFactory.UIImage(
            UIFactory.UIRectTransform(
                guestAccount,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "login_buttonGuest");

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                guestAccount,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "login_buttonGuest");

        UIFactory.UIButton(
            UIFactory.UIRectTransform(
                guestAccount,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero),
            "login_buttonGuest",
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
            "login_buttonRegisterContainer").rectTransform;

        UIFactory.UIImage(
            UIFactory.UIRectTransform(
                register,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "login_buttonRegister");

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                register,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "login_buttonRegister");

        UIFactory.UIButton(
            UIFactory.UIRectTransform(
                register,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero),
            "login_buttonRegister",
            () =>
            {
                Debug.Log("buttonRegister Pushed");
                //Button functionality
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
            "login_buttonBackContainer").rectTransform;

        UIFactory.UIImage(
            UIFactory.UIRectTransform(
                back,
                "icon",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "login_buttonBack");

        UIFactory.UIText(
            UIFactory.UIRectTransform(
                back,
                "text",
                UIAnchor.Create(Vector2.zero, Vector2.right),
                new Vector2(0, 26.3f),
                Utility.HalfOne,
                new Vector2(0, 52.6f)),
            "login_buttonBack");

        UIFactory.UIButton(
            UIFactory.UIRectTransform(
                back,
                "button",
                UIAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero),
            "login_buttonBack",
            () =>
            {
                Debug.Log("buttonBack Pushed");
                //Button functionality
            },
            back.GetComponent<Image>());

    }
}