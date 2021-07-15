using System.Collections;
using System.Collections.Generic;
using Constants;
using Entities;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.PanelItems
{
    public class SettingItemOnOff : MenuItemBase
    {
        public TextMeshProUGUI title;
        public Toggle offToggle;
        public TextMeshProUGUI offText;
        public Toggle onToggle;
        public TextMeshProUGUI onText;

        public void Init(
            bool _IsOn, 
            string _Name,
            UnityAction<bool> _Action,
            IEnumerable<GameObserver> _Observers)
        {
            base.Init(_Observers);
            name = $"{_Name} Setting";
            title.text = _Name;
            ToggleGroup tg = gameObject.AddComponent<ToggleGroup>();
            offToggle.group = tg;
            onToggle.group = tg;
            if (_IsOn)
                onToggle.isOn = true;
            else
                offToggle.isOn = true;

            offText.text = "Off";
            onText.text = "On";

            onToggle.onValueChanged.AddListener(_V =>
                Notifyer.RaiseNotify(this, CommonNotifyMessages.UiButtonClick));
            offToggle.onValueChanged.AddListener(_V =>
                Notifyer.RaiseNotify(this, CommonNotifyMessages.UiButtonClick));
            onToggle.onValueChanged.AddListener(_Action);
        }
    }
}