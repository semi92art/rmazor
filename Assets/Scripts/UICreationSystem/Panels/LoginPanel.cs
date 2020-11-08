using Extensions;
using Network;
using Network.PacketArgs;
using Network.Packets;
using TMPro;
using UnityEngine;
using UICreationSystem.Factories;
using Utils;
using UnityEngine.UI;

namespace UICreationSystem.Panels
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
            TextMeshProUGUI continueAsGuestButtonText = lp.GetCompItem<TextMeshProUGUI>("continue_as_guest_button_text");
            TextMeshProUGUI registerButtonText = lp.GetCompItem<TextMeshProUGUI>("register_button_text");
            TextMeshProUGUI logoutButtonText = lp.GetCompItem<TextMeshProUGUI>("logout_button_text");
            
            m_LoginInputField = lp.GetCompItem<TMP_InputField>("login_input_field");
            m_PasswordInputField = lp.GetCompItem<TMP_InputField>("password_input_field");
            m_PasswordInputField.contentType = TMP_InputField.ContentType.Password;
            
            Button loginButton = lp.GetCompItem<Button>("login_button");
            Button loginAppleButton = lp.GetCompItem<Button>("login_apple_button");
            Button loginGoogleButton = lp.GetCompItem<Button>("login_google_button");
            Button continueAsGuestButton = lp.GetCompItem<Button>("continue_as_guest_button");
            Button registrationButton = lp.GetCompItem<Button>("register_button");
            Button logoutButton = lp.GetCompItem<Button>("logout_button");
            
            loginButton.SetOnClick(Login);
            loginAppleButton.SetOnClick(LoginWithApple);
            loginGoogleButton.SetOnClick(LoginWithGoogle);
            registrationButton.SetOnClick(Registration);
            continueAsGuestButton.SetOnClick(ContinueAsGuest);
            logoutButton.SetOnClick(Logout);
            
            //TODO when apple login function will be ready, delete line below
            lp.GetContentItem("login_apple_button_container").SetActive(false);

            bool isLogined = !string.IsNullOrEmpty(GameClient.Instance.Login);
            lp.GetContentItem("continue_as_guest_button_container").SetActive(!isLogined);
            lp.GetContentItem("logout_button_container").SetActive(isLogined);
            
            CleanErrorHandlers();
            return lp.RTransform();
        }
        
        #endregion
        
        #region event functions

        private void Login()
        {
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
            // TODO
            throw new System.NotImplementedException();    
        }

        private void LoginWithGoogle()
        {
            // TODO
            throw new System.NotImplementedException();    
        }

        private void Registration()
        {
            IDialogPanel regPanel = new RegistrationPanel(m_DialogViewer);
            regPanel.Show();
        }

        private void Logout()
        {
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

        private void ContinueAsGuest()
        {
            m_DialogViewer.Back();
        }
        
        #endregion
    }
}