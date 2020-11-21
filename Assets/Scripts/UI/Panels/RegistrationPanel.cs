using DialogViewers;
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
    public class RegistrationPanel : LoginPanelBase
    {
        private const string TestUserPrefix = "test";
        
        public RegistrationPanel(IMenuDialogViewer _MenuDialogViewer) : base(_MenuDialogViewer)
        { }
        
        #region private members

        private TMP_InputField m_RepeatPasswordInputField;
        private TextMeshProUGUI m_RepeatPasswordErrorHandler;
        private TextMeshProUGUI m_RegisteredSuccessfully;

        #endregion
        
        #region nonpublic methods
        
        protected override RectTransform Create()
        {
            GameObject rp = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    MenuDialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu", "register_panel");
            
            m_LoginErrorHandler = rp.GetCompItem<TextMeshProUGUI>("login_error_handler");
            m_PasswordErrorHandler = rp.GetCompItem<TextMeshProUGUI>("password_error_handler");
            m_RepeatPasswordErrorHandler = rp.GetCompItem<TextMeshProUGUI>("repeat_password_error_handler");
            m_RegisteredSuccessfully = rp.GetCompItem<TextMeshProUGUI>("registered_successfully");
            m_RegisteredSuccessfully.enabled = false;
            
            TextMeshProUGUI registerButtonText = rp.GetCompItem<TextMeshProUGUI>("register_button_text");
            
            m_LoginInputField = rp.GetCompItem<TMP_InputField>("login_input_field");
            m_PasswordInputField = rp.GetCompItem<TMP_InputField>("password_input_field");
            m_PasswordInputField.contentType = TMP_InputField.ContentType.Password;
            m_RepeatPasswordInputField = rp.GetCompItem<TMP_InputField>("repeat_password_input_field");
            m_RepeatPasswordInputField.contentType = TMP_InputField.ContentType.Password;
            Button registerButton = rp.GetCompItem<Button>("register_button");

            registerButton.SetOnClick(Register);

            CleanErrorHandlers();
            
            return rp.RTransform();
        }

        private void SetRepeatPasswordError(string _Text)
        {
            m_RepeatPasswordErrorHandler.text = _Text;
        }

        protected override bool IsNoError()
        {
            return base.IsNoError() && string.IsNullOrEmpty(m_RepeatPasswordErrorHandler.text);
        }

        protected override void CleanErrorHandlers()
        {
            m_RepeatPasswordErrorHandler.text = string.Empty;
            base.CleanErrorHandlers();
        }

        #endregion
        
        #region event functions

        private void Register()
        {
            SoundManager.Instance.PlayMenuButtonClick();
            CleanErrorHandlers();
            if (!CheckForInputErrors())
                return;
            
            var packet = new RegisterUserPacket(new RegisterUserUserPacketRequestArgs
            {
                Name = m_LoginInputField.text,
                PasswordHash = Utility.GetMD5Hash(m_PasswordInputField.text)
            });
            packet.OnSuccess(() =>
            {
                GameClient.Instance.Login = packet.Response.Name;
                GameClient.Instance.PasswordHash = packet.Response.PasswordHash;
                GameClient.Instance.AccountId = packet.Response.Id;
                MoneyManager.Instance.GetBank(true);
                m_RegisteredSuccessfully.enabled = true;
                Coroutines.Run(Coroutines.Delay(() =>
                {
                    MenuDialogViewer.Back();
                    MenuDialogViewer.Back();
                }, 1f));
                
            }).OnFail(() =>
            {
                switch (packet.ErrorMessage.Id)
                {
                    case ServerErrorCodes.AccountWithThisNameAlreadyExist:
                        SetLoginError("user with this login already exists");
                        break;
                    default:
                        SetLoginError("login fail. try again later");
                        break;
                }
            });
            
            GameClient.Instance.Send(packet);
        }

        private bool CheckForInputErrors()
        {
            if (string.IsNullOrEmpty(m_LoginInputField.text))
                SetLoginError("field is empty");
            if (string.IsNullOrEmpty(m_PasswordInputField.text))
                SetPasswordError("field is empty");
            if (string.IsNullOrEmpty(m_RepeatPasswordInputField.text))
                SetRepeatPasswordError("field is empty");
            if (!IsNoError())
                return false;
            if (m_LoginInputField.text.Length > 20)
                SetLoginError("maximum name length: 20 symbols");
            if (m_PasswordInputField.text != m_RepeatPasswordInputField.text)
                SetRepeatPasswordError("passwords do not match");
            if (!IsNoError())
                return false;
            if (m_LoginInputField.text.StartsWith(TestUserPrefix))
                SetLoginError("name is unavailable");
            return true;
        }
        
        #endregion
    }
}