using System;
using System.Collections.Generic;
using System.Linq;
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
            IMenuDialogViewer _MenuDialogViewer,
            Func<string> _Value,
            string _Name,
            Func<List<string>> _ListOfItems,
            Action<string> _Select,
            IEnumerable<GameObserver> _Observers, 
            IUITicker _Ticker)
        {
            var observers = _Observers.ToArray();
            base.Init(observers, _Ticker);
            setting.text = _Value?.Invoke();
            name = $"{_Name} Setting";
            title.text = _Name;
            button.SetOnClick(() =>
            {
                Notifyer.RaiseNotify(this, CommonNotifyMessages.UiButtonClick);
                var items = _ListOfItems?.Invoke();
                if (items == null)
                    return;
                var selectorPanel = new SettingsSelectorPanel(
                    _MenuDialogViewer,
                    _Ticker,
                    _Value?.Invoke(),
                    items, 
                    _Select);
                selectorPanel.AddObservers(observers);
                selectorPanel.Init();
                _MenuDialogViewer.Show(selectorPanel);
            });
        }
    }
}