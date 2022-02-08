using Common.Providers;
using Common.Ticker;
using Common.UI;
using RMAZOR.Managers;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Setting_Panel_Items
{
    public class SettingItemOnOff : SimpleUiDialogItemView
    {
        public TextMeshProUGUI title;
        public Toggle offToggle;
        public TextMeshProUGUI offText;
        public Toggle onToggle;
        public TextMeshProUGUI onText;

        public void Init(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            IColorProvider _ColorProvider,
            bool _IsOn, 
            string _TitleKey,
            UnityAction<bool> _Action)
        {
            Init(_Managers.AudioManager, _UITicker, _ColorProvider);
            name = "Setting";
            _Managers.LocalizationManager.AddTextObject(title, _TitleKey);
            var tg = gameObject.AddComponent<ToggleGroup>();
            offToggle.group = tg;
            onToggle.group = tg;
            if (_IsOn)
                onToggle.isOn = true;
            else
                offToggle.isOn = true;

            offText.text = "Off";
            onText.text = "On";

            onToggle.onValueChanged.AddListener(_Value => SoundOnClick());
            offToggle.onValueChanged.AddListener(_Value => SoundOnClick());
            onToggle.onValueChanged.AddListener(_Action);

            _Managers.LocalizationManager.AddTextObject(onText, "on");
            _Managers.LocalizationManager.AddTextObject(offText, "off");
        }
    }
}