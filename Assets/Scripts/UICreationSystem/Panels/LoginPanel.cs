using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UICreationSystem.Factories;
using Utils;
using UnityEngine.UI;

namespace UICreationSystem.Panels
{
    public class LoginPanel
    {
        public static void CreatePanel(Canvas _Canvas)
        {
            float indent = 75f;
            float smallIndent = 60f;
            float positionY = -26.3f;

            RectTransform loginPanel = UICreatorImage.Create(
                _Canvas.RTransform(),
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
                "textbox",
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
            positionY -= indent * 0.7f;
            UiTmpTextFactory.Create(
                loginPanel,
                "password",
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, positionY),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "textbox",
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
                    DestroyPanel(_Canvas);
                    RegistrationPanel.CreatePanel(_Canvas);
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

        public static void DestroyPanel(Canvas _Canvas)
        {
            GameObject loginPanel = _Canvas.transform.Find("login_panel").gameObject;
            Object.Destroy(loginPanel);
        }
    }
}