using System.Collections.Generic;
using TMPro;
using UICreationSystem.Factories;
using UICreationSystem.Panels;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UICreationSystem
{
    public class SettingItemInPanelSelector : MonoBehaviour
    {
        public TextMeshProUGUI title;
        public Button button;
        public TextMeshProUGUI setting;

        public void Init(
            RectTransform _SettingsPanel,
            IDialogViewer _DialogViewer,
            System.Func<string> _Value,
            string _Name,
            System.Func<List<string>> _ListOfItems,
            UnityAction<string> _Select)
        {
            setting.text = _Value?.Invoke();
            name = $"{_Name} Setting";
            title.text = _Name;
            button.SetOnClick(() =>
            {
                var items = _ListOfItems?.Invoke();
                if (items == null)
                    return;
                
                var selectorPanel = new SettingsSelectorPanel();
                RectTransform selPanRtr = selectorPanel.CreatePanel(_Value?.Invoke(), _DialogViewer, items, _Select);
                _DialogViewer.Show(_SettingsPanel, selPanRtr);
            });
        }
    }
}