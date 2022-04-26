using System;
using Common;
using Common.Extensions;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Setting_Panel_Items
{
    public class SettingItemMiniButton : SimpleUiDialogItemView
    {
        [SerializeField] private Image              icon;
        [SerializeField] private Toggle             toggle;

        private bool m_IsIconNotNull;

        public void Init(
            IUITicker            _UITicker,
            IColorProvider       _ColorProvider,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            IPrefabSetManager    _PrefabSetManager,
            bool                 _IsOn,
            UnityAction<bool>    _Action,
            Sprite               _SpriteOn,
            Sprite               _SpriteOff)
        {
            base.Init(_UITicker, _ColorProvider, _AudioManager, _LocalizationManager, _PrefabSetManager);
            icon.sprite = _IsOn ? _SpriteOn : _SpriteOff;
            icon.color = _ColorProvider.GetColor(ColorIds.UI);
            toggle.isOn = _IsOn;
            toggle.onValueChanged.AddListener(_Action);
            toggle.onValueChanged.AddListener(_On => SoundOnClick());
            toggle.onValueChanged.AddListener(_On => icon.sprite = _On ? _SpriteOn : _SpriteOff);
        }

        public override void Init(
            IUITicker            _UITicker, 
            IColorProvider       _ColorProvider,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            IPrefabSetManager    _PrefabSetManager,
            bool                 _AutoFont = true)
        {
            throw new NotSupportedException();
        }

        protected override void CheckIfSerializedItemsNotNull()
        {
            base.CheckIfSerializedItemsNotNull();
            m_IsIconNotNull = icon.IsNotNull();
        }

        protected override void SetColorsOnInit()
        {
            base.SetColorsOnInit();
            icon.color = ColorProvider.GetColor(ColorIds.UI);
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId != ColorIds.UI) 
                return;
            if (m_IsIconNotNull)
                icon.color = _Color;
        }
    }
}