using System;
using System.Collections.Generic;
using Common.Extensions;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using RMAZOR.UI.Panels;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Setting_Panel_Items
{
    public class SettingItemInPanelSelector : SimpleUiDialogItemView
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Button          button;
        public                   TextMeshProUGUI setting;

        private Func<string> m_Font;

        public void Init(
            IUITicker                   _UITicker,
            IColorProvider              _ColorProvider,
            IAudioManager               _AudioManager,
            ILocalizationManager        _LocalizationManager,
            IPrefabSetManager           _PrefabSetManager,
            IBigDialogViewer            _UiDialogViewer,
            string                      _TitleLocalizationKey,
            Func<string>                _Value,
            UnityAction<object>         _OnSelect,
            Func<List<object>>          _ListOfItems   = null,
            ISettingSelectorDialogPanel _SelectorPanel = null,
            Func<TMP_FontAsset>         _Font          = null)
        {
            base.Init(_UITicker, _ColorProvider, _AudioManager, _LocalizationManager, _PrefabSetManager);
            var textInfo = new LocalizableTextObjectInfo(title, ETextType.MenuUI, _TitleLocalizationKey);
            _LocalizationManager.AddTextObject(textInfo);
            setting.text = _Value?.Invoke();
            if (_Font != null)
                setting.font = _Font();
            void OnClick()
            {
                SoundOnClick();
                var items = _ListOfItems?.Invoke();
                if (items == null || _SelectorPanel == null)
                    return;
                _SelectorPanel.PreInit(_Value?.Invoke(), items, _OnSelect);
                _SelectorPanel.LoadPanel();
                _UiDialogViewer.Show(_SelectorPanel);
            }
            button.SetOnClick(OnClick);
        }
    }
}