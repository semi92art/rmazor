using TMPro;
using UnityEngine;
using UICreationSystem.Factories;
using Utils;
using UnityEngine.UI;

namespace UICreationSystem.Panels
{
    public class LoginPanel
    {
        private RectTransform m_RegistrationPanel;
        
        public RectTransform CreatePanel(IDialogViewer _DialogViewer)
        {
            GameObject lp = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu", "login_panel");

            TextMeshProUGUI loginLabel = lp.GetComponentItem<TextMeshProUGUI>("login_label");
            TMP_InputField loginInputField = lp.GetComponentItem<TMP_InputField>("login_input_field");
            TextMeshProUGUI passwordLabel = lp.GetComponentItem<TextMeshProUGUI>("password_label");
            TMP_InputField passwordInputField = lp.GetComponentItem<TMP_InputField>("password_input_field");
            Button loginButton = lp.GetComponentItem<Button>("login_button");
            TextMeshProUGUI loginButtonText = lp.GetComponentItem<TextMeshProUGUI>("login_button_text");
            Button loginAppleButton = lp.GetComponentItem<Button>("login_apple_button");
            TextMeshProUGUI loginAppleButtonText = lp.GetComponentItem<TextMeshProUGUI>("login_apple_button_text");
            Button loginGoogleButton = lp.GetComponentItem<Button>("login_google_button");
            TextMeshProUGUI loginGoogleButtonText = lp.GetComponentItem<TextMeshProUGUI>("login_google_button_text");
            Button continueAsGuestButton = lp.GetComponentItem<Button>("continue_as_guest_button");
            TextMeshProUGUI continueAsGuestButtonText = lp.GetComponentItem<TextMeshProUGUI>("continue_as_guest_button_text");
            Button registerButton = lp.GetComponentItem<Button>("register_button");
            TextMeshProUGUI registerButtonText = lp.GetComponentItem<TextMeshProUGUI>("register_button_text");
            
            registerButton.SetOnClick(() =>
            {
                var regPanel = new RegistrationPanel();
                m_RegistrationPanel = regPanel.CreatePanel(_DialogViewer);
                _DialogViewer.Show(lp.RTransform(), m_RegistrationPanel);
            });
            
            return lp.RTransform();
        }
    }
}