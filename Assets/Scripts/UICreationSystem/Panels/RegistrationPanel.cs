using Extentions;
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
    public class RegistrationPanel : LoginPanelBase
    {
        public RegistrationPanel(IDialogViewer _DialogViewer) : base(_DialogViewer)
        { }
        
        #region private members

        private TMP_InputField m_RepeatPasswordInputField;
        private TextMeshProUGUI m_RepeatPasswordErrorHandler;

        #endregion

        
        #region private/protected methods
        
        protected override RectTransform Create()
        {
            GameObject rp = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu", "register_panel");
            
            m_LoginErrorHandler = rp.GetComponentItem<TextMeshProUGUI>("login_error_handler");
            m_PasswordErrorHandler = rp.GetComponentItem<TextMeshProUGUI>("password_error_handler");
            m_RepeatPasswordErrorHandler = rp.GetComponentItem<TextMeshProUGUI>("repeat_password_error_handler");
            
            TextMeshProUGUI registerButtonText = rp.GetComponentItem<TextMeshProUGUI>("register_button_text");
            
            m_LoginInputField = rp.GetComponentItem<TMP_InputField>("login_input_field");
            m_PasswordInputField = rp.GetComponentItem<TMP_InputField>("password_input_field");
            m_RepeatPasswordInputField = rp.GetComponentItem<TMP_InputField>("repeat_password_input_field");
            Button registerButton = rp.GetComponentItem<Button>("register_button");
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
            CleanErrorHandlers();
            
            if (string.IsNullOrEmpty(m_LoginInputField.text))
                SetLoginError("field is empty");
            if (string.IsNullOrEmpty(m_PasswordInputField.text))
                SetPasswordError("field is empty");
            if (string.IsNullOrEmpty(m_RepeatPasswordInputField.text))
                SetRepeatPasswordError("field is empty");
            if (!IsNoError())
                return;
            if (m_PasswordInputField.text != m_RepeatPasswordInputField.text)
                SetPasswordError("passwords do not match");
            if (!IsNoError())
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
                MoneyManager.Instance.GetMoney(true);
                m_DialogViewer.Show(null, true);
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
        
        #endregion
    }
}