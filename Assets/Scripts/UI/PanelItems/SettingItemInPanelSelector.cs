using System.Collections.Generic;
using Extensions;
using TMPro;
using UI.Panels;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PanelItems
{
    public class SettingItemInPanelSelector : MonoBehaviour
    {
        public TextMeshProUGUI title;
        public Button button;
        public TextMeshProUGUI setting;

        public void Init(
            IDialogViewer _DialogViewer,
            System.Func<string> _Value,
            string _Name,
            System.Func<List<string>> _ListOfItems,
            System.Action<string> _Select)
        {
            setting.text = _Value?.Invoke();
            name = $"{_Name} Setting";
            title.text = _Name;
            button.SetOnClick(() =>
            {
                var items = _ListOfItems?.Invoke();
                if (items == null)
                    return;
                
                IDialogPanel selectorPanel = new SettingsSelectorPanel(
                    _DialogViewer,
                    _Value?.Invoke(),
                    items, 
                    _Select);
                selectorPanel.Show();
            });
        }
    }
}