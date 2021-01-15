using DialogViewers;
using Constants;
using Controllers;
using Extensions;
using GameHelpers;
using Lean.Localization;
using Managers;
using Network;
using Network.Packets;
using TMPro;
using UI.Factories;
using UI.Managers;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Panels
{
    public class LoginPanel : LoginPanelBase
    {
        #region notify messages
        
        public const string NotifyMessageLoginButtonClick = nameof(NotifyMessageLoginButtonClick);
        public const string NotifyMessageLoginWithGoogleButtonClick = nameof(NotifyMessageLoginWithGoogleButtonClick);
        public const string NotifyMessageLoginWithAppleButtonClick = nameof(NotifyMessageLoginWithAppleButtonClick);
        public const string NotifyMessageRegistrationButtonClick = nameof(NotifyMessageRegistrationButtonClick);
        public const string NotifyMessageLogoutButtonClick = nameof(NotifyMessageLogoutButtonClick);
        
        #endregion
        
        #region api

        public override MenuUiCategory Category => MenuUiCategory.Login;
        public LoginPanel(IMenuDialogViewer _DialogViewer) : base(_DialogViewer) { }
        
        public override void Init()
        {
            GameObject lp = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    DialogViewer.Container,
                    RtrLites.FullFill),
                CommonStyleNames.MainMenuDialogPanels, "login_panel");

            LoginErrorHandler = lp.GetCompItem<TextMeshProUGUI>("login_error_handler");
            PasswordErrorHandler = lp.GetCompItem<TextMeshProUGUI>("password_error_handler");

            LoginInputField = lp.GetCompItem<TMP_InputField>("login_input_field");
            LeanLocalizedTextMeshProUGUI loginInputFieldLocalization = LoginInputField.transform.Find("Text Area")
                .gameObject.transform.Find("Placeholder").gameObject.AddComponent<LeanLocalizedTextMeshProUGUI>();
            loginInputFieldLocalization.TranslationName = "EnterLogin";
            
            PasswordInputField = lp.GetCompItem<TMP_InputField>("password_input_field");
            PasswordInputField.contentType = TMP_InputField.ContentType.Password;
            LeanLocalizedTextMeshProUGUI passwordInputFieldLocalization = PasswordInputField.transform.Find("Text Area")
                .gameObject.transform.Find("Placeholder").gameObject.AddComponent<LeanLocalizedTextMeshProUGUI>();
            passwordInputFieldLocalization.TranslationName = "EnterPassword";
            
            Button loginButton = lp.GetCompItem<Button>("login_button");
            Button loginAppleButton = lp.GetCompItem<Button>("login_apple_button");
            Button loginGoogleButton = lp.GetCompItem<Button>("login_google_button");
            Button registrationButton = lp.GetCompItem<Button>("register_button");
            Button logoutButton = lp.GetCompItem<Button>("logout_button");

            loginButton.SetOnClick(OnLoginButtonClick);
            loginAppleButton.SetOnClick(OnLoginWithAppleButtonClick);
            loginGoogleButton.SetOnClick(OnLoginWithGoogleButtonClick);
            registrationButton.SetOnClick(OnRegistrationButtonClick);
            logoutButton.SetOnClick(Logout);

            bool isLogined = !string.IsNullOrEmpty(GameClient.Instance.Login);
            logoutButton.SetGoActive(isLogined);
            loginButton.SetGoActive(!isLogined);

            CleanErrorHandlers();
            Panel = lp.RTransform();
        }
        
        #endregion
        
        #region nonpublic methods

        private void OnLoginButtonClick()
        {
            Notify(this, NotifyMessageLoginButtonClick);
            CleanErrorHandlers();
            if (string.IsNullOrEmpty(LoginInputField.text))
                SetLoginError(LeanLocalization.GetTranslationText("FieldIsEmpty"));
            if (string.IsNullOrEmpty(PasswordInputField.text))
                SetPasswordError(LeanLocalization.GetTranslationText("FieldIsEmpty"));
            if (!IsNoError())
                return;
            
            var packet = new LoginUserPacket(new LoginUserPacketRequestArgs
            {
                Name = LoginInputField.text,
                PasswordHash = CommonUtils.GetMd5Hash(PasswordInputField.text)
            });
            packet.OnSuccess(() =>
            {
                GameClient.Instance.Login = packet.Response.Name;
                GameClient.Instance.PasswordHash = packet.Response.PasswordHash;
                GameClient.Instance.AccountId = packet.Response.Id;
                BankManager.Instance.GetBank(true);
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
            Notify(this, NotifyMessageLoginWithGoogleButtonClick);
            var auth = new AuthController();
#if UNITY_ANDROID
            auth.AuthenticateWithGoogleOnAndroid();
#elif UNITY_IPHONE
            auth.AuthenticateWithGoogleOnIos();
#endif
        }
        
        private void OnLoginWithAppleButtonClick()
        {
            Notify(this, NotifyMessageLoginWithAppleButtonClick);
            var auth = new AuthController();
#if UNITY_ANDROID
            auth.AuthenticateWithAppleIdOnAndroid();
#elif UNITY_IPHONE
            auth.AuthenticateWithAppleOnIos();
#endif
        }
        
        private void OnRegistrationButtonClick()
        {
            Notify(this, NotifyMessageRegistrationButtonClick);
            var regPanel = new RegistrationPanel(DialogViewer);
            regPanel.AddObservers(GetObservers());
            regPanel.Init();
            DialogViewer.Show(regPanel);
        }

        private void Logout()
        {
            Notify(this, NotifyMessageLogoutButtonClick);
            var packet = new LoginUserPacket(new LoginUserPacketRequestArgs
            {
                DeviceId = GameClient.Instance.DeviceId
            });
            packet.OnSuccess(() =>
            {
                GameClient.Instance.Login = string.Empty;
                GameClient.Instance.PasswordHash = string.Empty;
                GameClient.Instance.AccountId = packet.Response.Id;
                BankManager.Instance.GetBank(true);
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