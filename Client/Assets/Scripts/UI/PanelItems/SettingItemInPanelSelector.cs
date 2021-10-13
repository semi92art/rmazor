using System;
using System.Collections.Generic;
using Constants;
using DI.Extensions;
using DialogViewers;
using Entities;
using Ticker;
using TMPro;
using UI.Panels;
using UnityEngine.UI;

namespace UI.PanelItems
{
    public class SettingItemInPanelSelector : MenuItemBase
    {
        public TextMeshProUGUI title;
        public Button button;
        public TextMeshProUGUI setting;

        public void Init(
            IDialogViewer _UiDialogViewer,
            Func<string> _Value,
            string _Name,
            Func<List<string>> _ListOfItems,
            Action<string> _OnSelect,
            IManagersGetter _ManagersGetter,
            ISettingSelectorDialogPanel _SelectorPanel)
        {
            base.Init(_ManagersGetter);
            setting.text = _Value?.Invoke();
            name = $"{_Name} Setting";
            title.text = _Name;
            button.SetOnClick(() =>
            {
                Managers.Notify(_SM => _SM.PlayClip(AudioClipNames.UIButtonClick));
                var items = _ListOfItems?.Invoke();
                if (items == null)
                    return;
                _SelectorPanel.PreInit(_Value?.Invoke(), items, _OnSelect);
                _SelectorPanel.Init();
                _UiDialogViewer.Show(_SelectorPanel);
            });
        }
    }
}