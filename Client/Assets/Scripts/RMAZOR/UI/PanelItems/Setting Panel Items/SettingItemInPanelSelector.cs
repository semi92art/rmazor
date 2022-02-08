using System;
using System.Collections.Generic;
using Common.Extensions;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using Managers;
using RMAZOR.UI.Panels;
using RMAZOR.Views.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.PanelItems.Setting_Panel_Items
{
    public class SettingItemInPanelSelector : SimpleUiDialogItemView
    {
        [SerializeField] private TextMeshProUGUI    title;
        [SerializeField] private Button             button;
        public                   TextMeshProUGUI    setting;

        public void Init(
            IManagersGetter             _Managers,
            IUITicker                   _UITicker,
            IBigDialogViewer            _UiDialogViewer,
            IColorProvider              _ColorProvider,
            string                      _TitleKey,
            Func<string>                _Value,
            UnityAction<string>         _OnSelect,
            Func<List<string>>          _ListOfItems   = null,
            ISettingSelectorDialogPanel _SelectorPanel = null)
        {
            Init(_Managers.AudioManager, _UITicker, _ColorProvider);
            _Managers.LocalizationManager.AddTextObject(title, _TitleKey);
            setting.text = _Value?.Invoke();
            button.SetOnClick(() =>
            {
                SoundOnClick();
                var items = _ListOfItems?.Invoke();
                if (items == null || _SelectorPanel == null)
                    return;
                _SelectorPanel.PreInit(_Value?.Invoke(), items, _OnSelect);
                _SelectorPanel.LoadPanel();
                _UiDialogViewer.Show(_SelectorPanel);
            });
        }
    }
}