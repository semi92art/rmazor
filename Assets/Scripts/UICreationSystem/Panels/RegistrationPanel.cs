using Extentions;
using TMPro;
using UnityEngine;
using UICreationSystem.Factories;
using Utils;
using UnityEngine.UI;

namespace UICreationSystem.Panels
{
    public class RegistrationPanel : IDialogPanel
    {
        #region private members
        
        private readonly IDialogViewer m_DialogViewer;
        
        #endregion
        
        #region api

        public UiCategory Category => UiCategory.LoginOrRegistration;
        public RectTransform Panel { get; private set; }
        
        public RegistrationPanel(IDialogViewer _DialogViewer)
        {
            m_DialogViewer = _DialogViewer;
        }

        public void Show()
        {
            Panel = Create();
            m_DialogViewer.Show( this);
        }
        
        #endregion
        
        #region private methods
        
        private RectTransform Create()
        {
            GameObject rp = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu", "register_panel");
            
            TextMeshProUGUI loginLabel = rp.GetComponentItem<TextMeshProUGUI>("login_label");
            TMP_InputField loginInputField = rp.GetComponentItem<TMP_InputField>("login_input_field");
            TextMeshProUGUI passwordLabel = rp.GetComponentItem<TextMeshProUGUI>("password_label");
            TMP_InputField passwordInputField = rp.GetComponentItem<TMP_InputField>("password_input_field");
            TextMeshProUGUI repeatPasswordLabel = rp.GetComponentItem<TextMeshProUGUI>("repeat_password_label");
            TMP_InputField repeatPasswordInputField = rp.GetComponentItem<TMP_InputField>("repeat_password_input_field");
            Button registerButton = rp.GetComponentItem<Button>("register_button");
            TextMeshProUGUI registerButtonText = rp.GetComponentItem<TextMeshProUGUI>("register_button_text");
            
            return rp.RTransform();
        }
        
        #endregion
    }
}