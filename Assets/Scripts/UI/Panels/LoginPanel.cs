using System.Collections.Generic;
using DialogViewers;
using Entities;
using Extensions;
using Helpers;
using Lean.Localization;
using Managers;
using Network;
using Network.PacketArgs;
using Network.Packets;
using TMPro;
using UI.Factories;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Panels
{
    public class LoginPanel : LoginPanelBase
    {
        #region notify message ids
        
        public const int NotifyIdLoginButtonClick = 0;
        public const int NotifyIdLoginWithGoogleButtonClick = 1;
        public const int NotifyIdLoginWithAppleButtonClick = 2;
        public const int NotifyIdRegistrationButtonClick = 3;
        public const int NotifyIdLogoutButtonClick = 4;
        
        #endregion
        
        public LoginPanel(IMenuDialogViewer _DialogViewer,
            IEnumerable<IGameObserver> _Observers) : base(_DialogViewer, _Observers) { }
        
        #region protected methods

        protected override RectTransform Create()
        {
            GameObject lp = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu", "login_panel");

            m_LoginErrorHandler = lp.GetCompItem<TextMeshProUGUI>("login_error_handler");
            m_PasswordErrorHandler = lp.GetCompItem<TextMeshProUGUI>("password_error_handler");

            TextMeshProUGUI loginButtonText = lp.GetCompItem<TextMeshProUGUI>("login_button_text");
            TextMeshProUGUI loginAppleButtonText = lp.GetCompItem<TextMeshProUGUI>("login_apple_button_text");
            TextMeshProUGUI loginGoogleButtonText = lp.GetCompItem<TextMeshProUGUI>("login_google_button_text");
            TextMeshProUGUI registerButtonText = lp.GetCompItem<TextMeshProUGUI>("register_button_text");
            TextMeshProUGUI logoutButtonText = lp.GetCompItem<TextMeshProUGUI>("logout_button_text");
            
            m_LoginInputField = lp.GetCompItem<TMP_InputField>("login_input_field");
            LeanLocalizedTextMeshProUGUI loginInputFieldLocalization = m_LoginInputField.transform.Find("Text Area")
                .gameObject.transform.Find("Placeholder").gameObject.AddComponent<LeanLocalizedTextMeshProUGUI>();
            loginInputFieldLocalization.TranslationName = "EnterLogin";
            
            m_PasswordInputField = lp.GetCompItem<TMP_InputField>("password_input_field");
            m_PasswordInputField.contentType = TMP_InputField.ContentType.Password;
            LeanLocalizedTextMeshProUGUI passwordInputFieldLocalization = m_PasswordInputField.transform.Find("Text Area")
                .gameObject.transform.Find("Placeholder").gameObject.AddComponent<LeanLocalizedTextMeshProUGUI>();
            passwordInputFieldLocalization.TranslationName = "EnterPassword";
            
            Button loginButton = lp.GetCompItem<Button>("login_button");
            LeanLocalizedTextMeshProUGUI loginButtonLocalization = loginButton.transform.Find("Background")
                .gameObject.transform.Find("Text").gameObject.AddComponent<LeanLocalizedTextMeshProUGUI>();
            loginButtonLocalization.TranslationName = "Login";
            
            Button loginAppleButton = lp.GetCompItem<Button>("login_apple_button");
            LeanLocalizedTextMeshProUGUI loginAppleButtonLocalization = loginAppleButton.transform.Find("Background")
                .gameObject.transform.Find("Text").gameObject.AddComponent<LeanLocalizedTextMeshProUGUI>();
            loginAppleButtonLocalization.TranslationName = "LoginWithApple";
            
            Button loginGoogleButton = lp.GetCompItem<Button>("login_google_button");
            LeanLocalizedTextMeshProUGUI loginGoogleButtonLocalization = loginGoogleButton.transform.Find("Background")
                .gameObject.transform.Find("Text").gameObject.AddComponent<LeanLocalizedTextMeshProUGUI>();
            loginGoogleButtonLocalization.TranslationName = "LoginWithGoogle";
            
            Button registrationButton = lp.GetCompItem<Button>("register_button");
            LeanLocalizedTextMeshProUGUI registrationButtonLocalization = registrationButton.transform.Find("Background")
                .gameObject.transform.Find("Text").gameObject.AddComponent<LeanLocalizedTextMeshProUGUI>();
            registrationButtonLocalization.TranslationName = "Registration";
            
            Button logoutButton = lp.GetCompItem<Button>("logout_button");
            LeanLocalizedTextMeshProUGUI logoutButtonLocalization = logoutButton.transform.Find("Background")
                .gameObject.transform.Find("Text").gameObject.AddComponent<LeanLocalizedTextMeshProUGUI>();
            logoutButtonLocalization.TranslationName = "Logout";
            
            loginButton.SetOnClick(OnLoginButtonClick);
            loginAppleButton.SetOnClick(OnLoginWithAppleButtonClick);
            loginGoogleButton.SetOnClick(OnLoginWithGoogleButtonClick);
            registrationButton.SetOnClick(OnRegistrationButtonClick);
            logoutButton.SetOnClick(Logout);
            
            //TODO when apple login function will be ready, delete line below
            loginAppleButton.gameObject.SetActive(false);

            bool isLogined = !string.IsNullOrEmpty(GameClient.Instance.Login);
            logoutButton.SetGoActive(isLogined);
            loginButton.SetGoActive(!isLogined);

            CleanErrorHandlers();
            return lp.RTransform();
        }
        
        #endregion
        
        #region event functions

        private void OnLoginButtonClick()
        {
            Notify(this, NotifyIdLoginButtonClick);
            CleanErrorHandlers();
            if (string.IsNullOrEmpty(m_LoginInputField.text))
                //TODO get translation name from localization
            
                SetLoginError(LeanLocalization.GetTranslationText("FieldIsEmpty"));
            if (string.IsNullOrEmpty(m_PasswordInputField.text))
                SetPasswordError(LeanLocalization.GetTranslationText("FieldIsEmpty"));
            if (!IsNoError())
                return;
            
            var packet = new LoginUserPacket(new LoginUserPacketRequestArgs
            {
                Name = m_LoginInputField.text,
                PasswordHash = CommonUtils.GetMD5Hash(m_PasswordInputField.text)
            });
            packet.OnSuccess(() =>
            {
                GameClient.Instance.Login = packet.Response.Name;
                GameClient.Instance.PasswordHash = packet.Response.PasswordHash;
                GameClient.Instance.AccountId = packet.Response.Id;
                MoneyManager.Instance.GetBank(true);
                DialogViewer.Back();       
            });
            packet.OnFail(() =>
            {
                switch (packet.ErrorMessage.Id)
                {
                    case ServerErrorCodes.AccountWithThisNameAlreadyExist:
                        SetLoginError(LeanLocalization.GetTranslationText("UserExists"));
                        break;
                    case ServerErrorCodes.WrongLoginOrPassword:
                        SetLoginError(LeanLocalization.GetTranslationText("WrongLoginOrPassword"));
                        break;
                    default:
                        SetLoginError(LeanLocalization.GetTranslationText("LoginFail"));
                        break;
                }
            });
            
            GameClient.Instance.Send(packet);
        }

        private void OnLoginWithGoogleButtonClick()
        {
            Notify(this, NotifyIdLoginWithGoogleButtonClick);
            // TODO
        }
        
        private void OnLoginWithAppleButtonClick()
        {
            Notify(this, NotifyIdLoginWithAppleButtonClick);
            // TODO
        }
        
        private void OnRegistrationButtonClick()
        {
            Notify(this, NotifyIdRegistrationButtonClick);
            IMenuDialogPanel regPanel = new RegistrationPanel(DialogViewer, GetObservers());
            regPanel.Show();
        }

        private void Logout()
        {
            Notify(this, NotifyIdLogoutButtonClick);
            var packet = new LoginUserPacket(new LoginUserPacketRequestArgs
            {
                DeviceId = GameClient.Instance.DeviceId
            });
            packet.OnSuccess(() =>
            {
                GameClient.Instance.Login = string.Empty;
                GameClient.Instance.PasswordHash = string.Empty;
                GameClient.Instance.AccountId = packet.Response.Id;
                MoneyManager.Instance.GetBank(true);
                DialogViewer.Back();       
            });
            packet.OnFail(() =>
            {
                
            });

            GameClient.Instance.Send(packet);
        }

        #endregion
    }
}