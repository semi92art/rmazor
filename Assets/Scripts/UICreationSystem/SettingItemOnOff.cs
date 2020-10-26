using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UICreationSystem
{
    public class SettingItemOnOff : MonoBehaviour
    {
        public TextMeshProUGUI title;
        public Toggle offToggle;
        public TextMeshProUGUI offText;
        public Toggle onToggle;
        public TextMeshProUGUI onText;

        public void Init(bool _IsOn, string _Name, UnityAction<bool> _Action)
        {
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
            
            onToggle.onValueChanged.AddListener(_Action);
        }
    }
}