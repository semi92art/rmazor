using System;
using System.Collections.Generic;
using DI.Extensions;
using DialogViewers;
using Entities;
using Games.RazorMaze.Views.Common;
using Ticker;
using TMPro;
using UI.Panels;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PanelItems.Setting_Panel_Items
{
    public class SettingItemInPanelSelector : SimpleUiDialogItemView
    {
        [SerializeField] private TextMeshProUGUI    title;
        [SerializeField] private Button             button;
        public                   TextMeshProUGUI    setting;

        public void Init(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            IBigDialogViewer _UiDialogViewer,
            IColorProvider _ColorProvider,
            string _TitleKey,
            Func<string> _Value,
            Action<string> _OnSelect,
            Func<List<string>> _ListOfItems = null,
            ISettingSelectorDialogPanel _SelectorPanel = null)
        {
            InitCore(_Managers, _UITicker, _ColorProvider);
            _Managers.LocalizationManager.AddTextObject(title, _TitleKey);
            setting.text = _Value?.Invoke();
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