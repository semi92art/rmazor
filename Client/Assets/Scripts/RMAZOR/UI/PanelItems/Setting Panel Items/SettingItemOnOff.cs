using Common.Enums;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.UI;
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
            IUITicker            _UITicker,
            IColorProvider       _ColorProvider,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            IPrefabSetManager    _PrefabSetManager,
            bool                 _IsOn,
            string               _TitleLocalizatoinKey,
            UnityAction<bool>    _Action)
        {
            Init(_UITicker, _ColorProvider, _AudioManager, _LocalizationManager, _PrefabSetManager);
            name = "Setting";
            _LocalizationManager.AddTextObject(new LocalizableTextObjectInfo(title, ETextType.MenuUI, _TitleLocalizatoinKey));
            var tg = gameObject.AddComponent<ToggleGroup>();
            offToggle.group = tg;
            onToggle.group  = tg;
            if (_IsOn) onToggle.isOn = true;
            else       offToggle.isOn = true;
            onToggle.onValueChanged.AddListener(_Value => SoundOnClick());
            offToggle.onValueChanged.AddListener(_Value => SoundOnClick());
            onToggle.onValueChanged.AddListener(_Action);
            _LocalizationManager.AddTextObject(new LocalizableTextObjectInfo(onText, ETextType.MenuUI, "on"));
            _LocalizationManager.AddTextObject(new LocalizableTextObjectInfo(offText, ETextType.MenuUI, "off"));
        }
    }
}