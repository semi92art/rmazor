using Entities;
using Games.RazorMaze.Views.Common;
using Ticker;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.PanelItems.Setting_Panel_Items
{
    public class SettingItemMiniButton : SimpleUiDialogItemView
    {
        public Image icon;
        public Toggle toggle;
        
        public void Init(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            IColorProvider _ColorProvider,
            bool _IsOn, 
            UnityAction<bool> _Action,
            Sprite _SpriteOn,
            Sprite _SpriteOff)
        {
            InitCore(_Managers, _UITicker, _ColorProvider);
            icon.sprite = _IsOn ? _SpriteOn : _SpriteOff;
            toggle.isOn = _IsOn;
            toggle.onValueChanged.AddListener(_Action);
            toggle.onValueChanged.AddListener(_On => SoundOnClick());
            toggle.onValueChanged.AddListener(_On => icon.sprite = _On ? _SpriteOn : _SpriteOff);
        }
    }
}