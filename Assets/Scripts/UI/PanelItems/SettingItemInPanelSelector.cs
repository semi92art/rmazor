using System.Collections.Generic;
using DialogViewers;
using Extensions;
using Managers;
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
            IMenuDialogViewer _MenuDialogViewer,
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
                SoundManager.Instance.PlayMenuButtonClick();
                var items = _ListOfItems?.Invoke();
                if (items == null)
                    return;
                
                IMenuDialogPanel selectorPanel = new SettingsSelectorPanel(
                    _MenuDialogViewer,
                    _Value?.Invoke(),
                    items, 
                    _Select);
                selectorPanel.Show();
                
            });
        }
    }
}