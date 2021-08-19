using System.Collections.Generic;
using System.Linq;
using Constants;
using DialogViewers;
using Entities;
using Extensions;
using Managers;
using TMPro;
using UI.Panels;
using UnityEngine;
using UnityEngine.UI;
using UnityGameLoopDI;

namespace UI.PanelItems
{
    public class SettingItemInPanelSelector : MenuItemBase
    {
        public TextMeshProUGUI title;
        public Button button;
        public TextMeshProUGUI setting;

        public void Init(
            IMenuDialogViewer _MenuDialogViewer,
            System.Func<string> _Value,
            string _Name,
            System.Func<List<string>> _ListOfItems,
            System.Action<string> _Select,
            IEnumerable<GameObserver> _Observers, 
            ITicker _Ticker)
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