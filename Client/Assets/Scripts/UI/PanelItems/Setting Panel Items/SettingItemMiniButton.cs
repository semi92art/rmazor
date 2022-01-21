using System;
using Common.Extensions;
using Common.Ticker;
using Managers;
using RMAZOR.Views.Common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.PanelItems.Setting_Panel_Items
{
    public class SettingItemMiniButton : SimpleUiDialogItemView
    {
        [SerializeField] private Image              icon;
        [SerializeField] private Toggle             toggle;

        private bool m_IsIconNotNull;

        public void Init(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            IColorProvider _ColorProvider,
            bool _IsOn,
            UnityAction<bool> _Action,
            Sprite _SpriteOn,
            Sprite _SpriteOff)
        {
            base.Init(_Managers, _UITicker, _ColorProvider);
            icon.sprite = _IsOn ? _SpriteOn : _SpriteOff;
            icon.color = _ColorProvider.GetColor(ColorIds.UI);
            toggle.isOn = _IsOn;
            toggle.onValueChanged.AddListener(_Action);
            toggle.onValueChanged.AddListener(_On => SoundOnClick());
            toggle.onValueChanged.AddListener(_On => icon.sprite = _On ? _SpriteOn : _SpriteOff);
        }

        public override void Init(IManagersGetter _Managers, IUITicker _UITicker, IColorProvider _ColorProvider)
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
            if (_ColorId == ColorIds.UI)
            {
                if (m_IsIconNotNull)
                    icon.color = _Color;
            }
        }
    }
}