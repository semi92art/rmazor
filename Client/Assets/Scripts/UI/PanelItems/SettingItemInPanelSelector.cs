using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Constants.NotifyMessages;
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
            IMenuDialogViewer _MenuDialogViewer,
            Func<string> _Value,
            string _Name,
            Func<List<string>> _ListOfItems,
            Action<string> _Select,
            IGameObservable _GameObservable,
            IUITicker _UITicker)
        {
            base.Init(_GameObservable);
            setting.text = _Value?.Invoke();
            name = $"{_Name} Setting";
            title.text = _Name;
            button.SetOnClick(() =>
            {
                GameObservable.Notify(SoundNotifyMessages.PlayAudioClip, AudioClipNames.UIButtonClick);
                var items = _ListOfItems?.Invoke();
                if (items == null)
                    return;
                var selectorPanel = new SettingsSelectorPanel(
                    _MenuDialogViewer,
                    _Value?.Invoke(),
                    items, 
                    _Select,
                    _GameObservable,
                    _UITicker);
                selectorPanel.Init();
                _MenuDialogViewer.Show(selectorPanel);
            });
        }
    }
}