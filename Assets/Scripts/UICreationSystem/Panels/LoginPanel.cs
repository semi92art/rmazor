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
        public static void CreatePanel(RectTransform _Container)
        {
            float indent = 75f;
            float smallIndent = 60f;
            float positionY = -26.3f;

            RectTransform loginPanel = UICreatorImage.Create(
                _Container,
                "login_panel",
                UiAnchor.Create(Vector2.zero, Vector2.one),
                new Vector2(0, 10),
                Utility.HalfOne,
                new Vector2(-60, -300),
                "dark_panel");

            //Email Text
            UiTmpTextFactory.Create(
                loginPanel,
                "email",
                UiAnchor.Create(Vector2.up, Vector2.one),
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
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, positionY),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "InputFieldContainer");

            UiTmpInputFieldFactory.Create(
                email,
                "email_input",
                UiAnchor.Create(Vector2.up, Vector2.one),
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
                UiAnchor.Create(Vector2.up, Vector2.one),
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
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, positionY),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "InputFieldContainer");

            UiTmpInputFieldFactory.Create(
                password,
                "password_input",
                UiAnchor.Create(Vector2.up, Vector2.one),
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
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, positionY),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "buttonLoginContainer");

            UiTmpButtonFactory.Create(
                login,
                "button",
                "Login",
                UiAnchor.Create(Vector2.zero, Vector2.one),
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
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, positionY),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "buttonAppleAccountContainer");


            UiTmpButtonFactory.Create(
                appleAccount,
                "button",
                "Login with Apple",
                UiAnchor.Create(Vector2.zero, Vector2.one),
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
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, positionY),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "buttonGoogleAccountContainer");

            UiTmpButtonFactory.Create(
                googleAccount,
                "button",
                "Login with Google",
                UiAnchor.Create(Vector2.zero, Vector2.one),
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
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, positionY),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "buttonGuestContainer");

            UiTmpButtonFactory.Create(
                guestAccount,
                "button",
                "Login as guest",
                UiAnchor.Create(Vector2.zero, Vector2.one),
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
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(-140, -740f),
                Utility.HalfOne,
                new Vector2(buttonSizeDelta, 52.6f),
                "buttonRegisterContainer");

            UiTmpButtonFactory.Create(
                register,
                "button",
                "Register",
                UiAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero,
                "buttonRegister",
                () =>
                {
                    Debug.Log("buttonRegister Pushed");
                    //Button functionality
                    DestroyPanel(_Container);
                    RegistrationPanel.CreatePanel(_Container);
                }
            );

            //BackButton
            RectTransform back = UICreatorImage.Create(
                loginPanel,
                "buttonBack",
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(140, -740f),
                Utility.HalfOne,
                new Vector2(buttonSizeDelta, 52.6f),
                "buttonBackContainer");

            UiTmpButtonFactory.Create(
                back,
                "button",
                "Back",
                UiAnchor.Create(Vector2.zero, Vector2.one),
                Vector2.zero,
                Utility.HalfOne,
                Vector2.zero,
                "buttonBack",
                () =>
                {
                    Debug.Log("buttonBack Pushed");
                    DestroyPanel(_Container);
                }
            );
        }

        public static void DestroyPanel(RectTransform _Container)
        {
            GameObject loginPanel = _Container.Find("login_panel").gameObject;
            Object.Destroy(loginPanel);
        }
    }
}