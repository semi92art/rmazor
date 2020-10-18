using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UICreationSystem;
using UICreationSystem.Factories;
using Utils;
using UnityEngine.UI;

namespace UICreationSystem.Panels
{
    public class RegistrationPanel
    {
        // Start is called before the first frame update
        public static void CreatePanel(RectTransform _Container)
        {
            float indent = 75f;
            float smallIndent = 60f;
            float positionY = -26.3f;

            RectTransform registerPanel = UICreatorImage.Create(
                _Container,
                    "register_panel",
                    UiAnchor.Create(Vector2.zero, Vector2.one),
                    new Vector2(0, 10),
                    Utility.HalfOne,
                    new Vector2(-90, -300),
                    "dark_panel");

            //Email Text
            UiTmpTextFactory.Create(
                    registerPanel,
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
                registerPanel,
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
                    registerPanel,
                    "password",
                    UiAnchor.Create(Vector2.up, Vector2.one),
                    new Vector2(0, positionY),
                    Utility.HalfOne,
                    new Vector2(-100, 52.6f),
                    "textbox",
                    "password");

            //Password Input
            positionY -= smallIndent;
            RectTransform password = UICreatorImage.Create(
                registerPanel,
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

            //Repeat Password Text
            positionY -= indent * 0.7f;
            UiTmpTextFactory.Create(
                   registerPanel,
                   "repeat_password",
                   UiAnchor.Create(Vector2.up, Vector2.one),
                   new Vector2(0, positionY),
                   Utility.HalfOne,
                   new Vector2(-100, 52.6f),
                   "textbox",
                   "repeat password");

            //Repeat Password Input
            positionY -= smallIndent;
            RectTransform repeatPassword = UICreatorImage.Create(
                registerPanel,
                "inputRepeatPassword",
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, positionY),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "InputFieldContainer");

            UiTmpInputFieldFactory.Create(
                repeatPassword,
                "repeatPassword_input",
                UiAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, -26.3f),
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
                    UiAnchor.Create(Vector2.up, Vector2.one),
                    new Vector2(140, positionY),
                    Utility.HalfOne,
                    new Vector2(-320, 52.6f),
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
                    }
                    );

            //BackButton
            RectTransform back = UICreatorImage.Create(
                    registerPanel,
                    "buttonBack",
                    UiAnchor.Create(Vector2.up, Vector2.one),
                    new Vector2(140, -740f),
                    Utility.HalfOne,
                    new Vector2(-320, 52.6f),
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
                        //Button functionality
                        DestroyPanel(_Container);
                        LoginPanel.CreatePanel(_Container);
                    }
                    );
        }

        // Update is called once per frame
        public static void DestroyPanel(RectTransform _Container)
        {
            GameObject registerPanel = _Container.Find("register_panel").gameObject;
            Object.Destroy(registerPanel);
        }
    }
}