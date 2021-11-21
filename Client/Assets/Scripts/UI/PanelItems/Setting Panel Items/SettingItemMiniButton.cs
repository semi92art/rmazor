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
        [SerializeField] private Image              icon;
        [SerializeField] private Toggle             toggle;


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
            icon.color = _ColorProvider.GetColor(ColorIds.UI);
            toggle.isOn = _IsOn;
            toggle.onValueChanged.AddListener(_Action);
            toggle.onValueChanged.AddListener(_On => SoundOnClick());
            toggle.onValueChanged.AddListener(_On => icon.sprite = _On ? _SpriteOn : _SpriteOff);
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId == ColorIds.UI)
            {
                icon.color = _Color;
            }
        }
    }
}