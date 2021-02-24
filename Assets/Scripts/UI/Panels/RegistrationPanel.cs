using DialogViewers;
using Constants;
using Extensions;
using GameHelpers;
using Managers;
using Network;
using Network.Packets;
using TMPro;
using UI.Factories;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Lean.Localization;
using UI.Managers;

namespace UI.Panels
{
    public class RegistrationPanel : LoginPanelBase
    {
        #region notify messages

        public const string NotifyMessageRegisterButtonClick = nameof(NotifyMessageRegisterButtonClick);
        
        #endregion

        #region nonpublic members

        private const string TestUserPrefix = "test";
        private TMP_InputField m_RepeatPasswordInputField;
        private TextMeshProUGUI m_RepeatPasswordErrorHandler;
        private TextMeshProUGUI m_RegisteredSuccessfully;

        #endregion
        
        #region api
        
        public RegistrationPanel(IMenuDialogViewer _DialogViewer) : base(_DialogViewer)
        { }
        
        public override void Init()
        {
            GameObject rp = PrefabUtilsEx.InitUiPrefab(
                UiFactory.UiRectTransform(
                    DialogViewer.Container,
                    RtrLites.FullFill),
                CommonPrefabSetNames.MainMenuDialogPanels, "register_panel");
            
            LoginErrorHandler = rp.GetCompItem<TextMeshProUGUI>("login_error_handler");
            PasswordErrorHandler = rp.GetCompItem<TextMeshProUGUI>("password_error_handler");
            m_RepeatPasswordErrorHandler = rp.GetCompItem<TextMeshProUGUI>("repeat_password_error_handler");
            m_RegisteredSuccessfully = rp.GetCompItem<TextMeshProUGUI>("registered_successfully");
            m_RegisteredSuccessfully.enabled = false;

            LoginInputField = rp.GetCompItem<TMP_InputField>("login_input_field");
            LeanLocalizedTextMeshProUGUI loginInputFieldLocalization = LoginInputField.transform.Find("Text Area")
                .gameObject.transform.Find("Placeholder").gameObject.AddComponent<LeanLocalizedTextMeshProUGUI>();
            loginInputFieldLocalization.TranslationName = "EnterLogin";
            
            PasswordInputField = rp.GetCompItem<TMP_InputField>("password_input_field");
            PasswordInputField.contentType = TMP_InputField.ContentType.Password;
            LeanLocalizedTextMeshProUGUI passwordInputFieldLocalization = PasswordInputField.transform.Find("Text Area")
                .gameObject.transform.Find("Placeholder").gameObject.AddComponent<LeanLocalizedTextMeshProUGUI>();
            passwordInputFieldLocalization.TranslationName = "EnterPassword";
            
            m_RepeatPasswordInputField = rp.GetCompItem<TMP_InputField>("repeat_password_input_field");
            m_RepeatPasswordInputField.contentType = TMP_InputField.ContentType.Password;
            LeanLocalizedTextMeshProUGUI repeatPasswordInputFieldLocalization = m_RepeatPasswordInputField.transform.Find("Text Area")
                .gameObject.transform.Find("Placeholder").gameObject.AddComponent<LeanLocalizedTextMeshProUGUI>();
            repeatPasswordInputFieldLocalization.TranslationName = "RepeatPassword";
            
            rp.GetCompItem<Button>("register_button").SetOnClick(OnRegisterButtonClick);
            CleanErrorHandlers();
            Panel = rp.RTransform();
        }
        
        #endregion

        #region nonpublic methods

        private void SetRepeatPasswordError(string _Text)
        {
            m_RepeatPasswordErrorHandler.text = _Text;
        }

        public override MenuUiCategory Category => MenuUiCategory.Login;

        protected override bool IsNoError()
        {
            return base.IsNoError() && string.IsNullOrEmpty(m_RepeatPasswordErrorHandler.text);
        }

        protected override void CleanErrorHandlers()
        {
            m_RepeatPasswordErrorHandler.text = string.Empty;
            base.CleanErrorHandlers();
        }

        private void OnRegisterButtonClick()
        {
            Notify(this, NotifyMessageRegisterButtonClick);
            CleanErrorHandlers();
            if (!CheckForInputErrors())
                return;
            
            var packet = new RegisterUserPacket(new RegisterUserPacketRequestArgs
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
                m_RegisteredSuccessfully.enabled = true;
                Coroutines.Run(Coroutines.Delay(() =>
                {
                    DialogViewer.Back();
                    DialogViewer.Back();
                }, 1f));
                
            }).OnFail(() =>
            {
                switch (packet.ErrorMessage.Id)
                {
                    case ServerErrorCodes.AccountWithThisNameAlreadyExist:
                        SetLoginError(LeanLocalization.GetTranslationText("UserExists"));
                        break;
                    default:
                        SetLoginError(LeanLocalization.GetTranslationText("LoginFail"));
                        break;
                }
            });
            
            GameClient.Instance.Send(packet);
        }

        private bool CheckForInputErrors()
        {
            if (string.IsNullOrEmpty(LoginInputField.text))
                SetLoginError(LeanLocalization.GetTranslationText("FieldIsEmpty"));
            if (string.IsNullOrEmpty(PasswordInputField.text))
                SetPasswordError(LeanLocalization.GetTranslationText("FieldIsEmpty"));
            if (string.IsNullOrEmpty(m_RepeatPasswordInputField.text))
                SetRepeatPasswordError(LeanLocalization.GetTranslationText("FieldIsEmpty"));
            if (!IsNoError())
                return false;
            if (LoginInputField.text.Length > 20)
                SetLoginError(LeanLocalization.GetTranslationText("MaxNameLength"));
            if (PasswordInputField.text != m_RepeatPasswordInputField.text)
                SetRepeatPasswordError(LeanLocalization.GetTranslationText("PasswordsDoNotMatch"));
            if (!IsNoError())
                return false;
            if (LoginInputField.text.StartsWith(TestUserPrefix))
                SetLoginError(LeanLocalization.GetTranslationText("NameIsUnavailable"));
            return true;
        }
        
        #endregion
    }
}