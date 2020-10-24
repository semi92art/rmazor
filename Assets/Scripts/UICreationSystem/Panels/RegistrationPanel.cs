using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UICreationSystem.Factories;
using Utils;
using UnityEngine.UI;

namespace UICreationSystem.Panels
{
    public class RegistrationPanel
    {
        // Start is called before the first frame update
        public static void CreatePanel(Canvas _Canvas)
        {
            float indent = 75f;
            float smallIndent = 60f;
            float positionY = -26.3f;

            RectTransform registerPanel = UICreatorImage.Create(
                    _Canvas.RTransform(),
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
                    "textbox",
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
                    "textbox",
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
                   UIAnchor.Create(Vector2.up, Vector2.one),
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
                UIAnchor.Create(Vector2.up, Vector2.one),
                new Vector2(0, positionY),
                Utility.HalfOne,
                new Vector2(-100, 52.6f),
                "InputFieldContainer");

            UiTmpInputFieldFactory.Create(
                repeatPassword,
                "repeatPassword_input",
                UIAnchor.Create(Vector2.up, Vector2.one),
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
                        DestroyPanel(_Canvas);
                        LoginPanel.CreatePanel(_Canvas);
                    }
                    );
        }

        // Update is called once per frame
        public static void DestroyPanel(Canvas _Canvas)
        {
            GameObject registerPanel = _Canvas.transform.Find("register_panel").gameObject;
            Object.Destroy(registerPanel);
        }
    }
}