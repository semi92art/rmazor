using System;
using System.Collections.Generic;
using DI.Extensions;
using DialogViewers;
using Entities;
using Ticker;
using TMPro;
using UI.Panels;
using UnityEngine.UI;

namespace UI.PanelItems.Setting_Panel_Items
{
    public class SettingItemInPanelSelector : SimpleUiDialogItemView
    {
        public TextMeshProUGUI title;
        public Button button;
        public TextMeshProUGUI setting;

        public void Init(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            IDialogViewer _UiDialogViewer,
            Func<string> _Value,
            Action<string> _OnSelect,
            Func<List<string>> _ListOfItems = null,
            ISettingSelectorDialogPanel _SelectorPanel = null)
        {
            InitCore(_Managers, _UITicker);
            setting.text = _Value?.Invoke();
            title.text = "Setting";
            button.SetOnClick(() =>
            {
                SoundOnClick();
                var items = _ListOfItems?.Invoke();
                if (items == null || _SelectorPanel == null)
                    return;
                _SelectorPanel.PreInit(_Value?.Invoke(), items, _OnSelect);
                _SelectorPanel.Init();
                _UiDialogViewer.Show(_SelectorPanel);
            });
        }
    }
}