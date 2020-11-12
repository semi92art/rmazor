using Extensions;
using Helpers;
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
        public LoginPanel(IDialogViewer _DialogViewer) : base(_DialogViewer) { }
        
        #region protected methods

        protected override RectTransform Create()
        {
            GameObject lp = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
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
            m_PasswordInputField = lp.GetCompItem<TMP_InputField>("password_input_field");
            m_PasswordInputField.contentType = TMP_InputField.ContentType.Password;
            
            Button loginButton = lp.GetCompItem<Button>("login_button");
            Button loginAppleButton = lp.GetCompItem<Button>("login_apple_button");
            Button loginGoogleButton = lp.GetCompItem<Button>("login_google_button");
            Button registrationButton = lp.GetCompItem<Button>("register_button");
            Button logoutButton = lp.GetCompItem<Button>("logout_button");
            
            loginButton.SetOnClick(Login);
            loginAppleButton.SetOnClick(LoginWithApple);
            loginGoogleButton.SetOnClick(LoginWithGoogle);
            registrationButton.SetOnClick(Registration);
            logoutButton.SetOnClick(Logout);
            
            //TODO when apple login function will be ready, delete line below
            loginAppleButton.gameObject.SetActive(false);

            bool isLogined = !string.IsNullOrEmpty(GameClient.Instance.Login);
            logoutButton.gameObject.SetActive(isLogined);
            
            CleanErrorHandlers();
            return lp.RTransform();
        }
        
        #endregion
        
        #region event functions

        private void Login()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            CleanErrorHandlers();
            if (string.IsNullOrEmpty(m_LoginInputField.text))
                SetLoginError("field is empty");
            if (string.IsNullOrEmpty(m_PasswordInputField.text))
                SetPasswordError("field is empty");
            if (!IsNoError())
                return;
            
            var packet = new LoginUserPacket(new LoginUserPacketRequestArgs
            {
                Name = m_LoginInputField.text,
                PasswordHash = Utility.GetMD5Hash(m_PasswordInputField.text)
            });
            packet.OnSuccess(() =>
            {
                GameClient.Instance.Login = packet.Response.Name;
                GameClient.Instance.PasswordHash = packet.Response.PasswordHash;
                GameClient.Instance.AccountId = packet.Response.Id;
                MoneyManager.Instance.GetMoney(true);
                m_DialogViewer.Back();       
            });
            packet.OnFail(() =>
            {
                switch (packet.ErrorMessage.Id)
                {
                    case ServerErrorCodes.AccountWithThisNameAlreadyExist:
                        SetLoginError("user with this login already exists");
                        break;
                    case ServerErrorCodes.WrongLoginOrPassword:
                        SetLoginError("wrong login or password");
                        break;
                    default:
                        SetLoginError("login fail. try again later");
                        break;
                }
            });
            
            GameClient.Instance.Send(packet);
        }

        private void LoginWithApple()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            // TODO
        }

        private void LoginWithGoogle()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            // TODO
        }

        private void Registration()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            IDialogPanel regPanel = new RegistrationPanel(m_DialogViewer);
            regPanel.Show();
        }

        private void Logout()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            var packet = new LoginUserPacket(new LoginUserPacketRequestArgs
            {
                DeviceId = GameClient.Instance.DeviceId
            });
            packet.OnSuccess(() =>
            {
                GameClient.Instance.Login = string.Empty;
                GameClient.Instance.PasswordHash = string.Empty;
                GameClient.Instance.AccountId = packet.Response.Id;
                MoneyManager.Instance.GetMoney(true);
                m_DialogViewer.Back();       
            });
            packet.OnFail(() =>
            {
                
            });

            GameClient.Instance.Send(packet);
        }

        #endregion
    }
}