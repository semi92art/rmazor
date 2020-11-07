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
        private const string TestUserPrefix = "test";
        
        public RegistrationPanel(IDialogViewer _DialogViewer) : base(_DialogViewer)
        { }
        
        #region private members

        private TMP_InputField m_RepeatPasswordInputField;
        private TextMeshProUGUI m_RepeatPasswordErrorHandler;
        private TextMeshProUGUI m_SelectCountryButtonText;
        private Image m_SelectCountryButtonIcon;
        private string m_CountryKey;

        #endregion
        
        #region private/protected methods
        
        protected override RectTransform Create()
        {
            GameObject rp = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu", "register_panel");

            m_SelectCountryButtonIcon = rp.GetCompItem<Image>("country_select_icon");
            m_SelectCountryButtonText = rp.GetCompItem<TextMeshProUGUI>("country_select_title");
            m_LoginErrorHandler = rp.GetCompItem<TextMeshProUGUI>("login_error_handler");
            m_PasswordErrorHandler = rp.GetCompItem<TextMeshProUGUI>("password_error_handler");
            m_RepeatPasswordErrorHandler = rp.GetCompItem<TextMeshProUGUI>("repeat_password_error_handler");
            
            TextMeshProUGUI registerButtonText = rp.GetCompItem<TextMeshProUGUI>("register_button_text");
            
            m_LoginInputField = rp.GetCompItem<TMP_InputField>("login_input_field");
            m_PasswordInputField = rp.GetCompItem<TMP_InputField>("password_input_field");
            m_PasswordInputField.contentType = TMP_InputField.ContentType.Password;
            m_RepeatPasswordInputField = rp.GetCompItem<TMP_InputField>("repeat_password_input_field");
            m_RepeatPasswordInputField.contentType = TMP_InputField.ContentType.Password;
            Button registerButton = rp.GetCompItem<Button>("register_button");
            Button selectCountryButton = rp.GetCompItem<Button>("country_select_button");
            
            registerButton.SetOnClick(Register);
            selectCountryButton.SetOnClick(SelectCountry);

            string ctrDefaultKey = Countries.Keys[0];
            m_SelectCountryButtonText.text = ctrDefaultKey;
            m_SelectCountryButtonIcon.sprite = Countries.GetFlag(ctrDefaultKey);
            m_CountryKey = ctrDefaultKey;
            
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
            if (!CheckForInputErrors())
                return;
            
            var packet = new RegisterUserPacket(new RegisterUserUserPacketRequestArgs
            {
                Name = m_LoginInputField.text,
                PasswordHash = Utility.GetMD5Hash(m_PasswordInputField.text),
                CountryKey = m_CountryKey
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

        private void SelectCountry()
        {
            var go = new GameObject("Countries Panel");
            var countriesPanel = go.AddComponent<CountriesPanel>();
            countriesPanel.Init(
                m_DialogViewer,
                m_CountryKey,
                _SelectedKey =>
                {
                    m_CountryKey = _SelectedKey;
                    m_SelectCountryButtonText.text = _SelectedKey;
                    m_SelectCountryButtonIcon.sprite = Countries.GetFlag(_SelectedKey);
                });
            countriesPanel.Show();
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
                SetPasswordError("passwords do not match");
            if (!IsNoError())
                return false;
            if (m_LoginInputField.text.StartsWith(TestUserPrefix))
                SetLoginError("name is unavailable");
            return true;
        }
        
        #endregion
    }
}