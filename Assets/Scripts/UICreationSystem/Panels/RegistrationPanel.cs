using Extentions;
using TMPro;
using UnityEngine;
using UICreationSystem.Factories;
using Utils;
using UnityEngine.UI;

namespace UICreationSystem.Panels
{
    public class RegistrationPanel
    {
        public RectTransform CreatePanel(IDialogViewer _DialogViewer)
        {
            GameObject rp = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _DialogViewer.DialogContainer,
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
    }
}