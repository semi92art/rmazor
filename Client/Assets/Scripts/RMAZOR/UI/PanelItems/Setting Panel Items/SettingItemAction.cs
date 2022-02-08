using System;
using Common;
using Common.Extensions;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.UI;
using RMAZOR.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RMAZOR.UI.PanelItems.Setting_Panel_Items
{
    public class SettingItemAction : SimpleUiDialogItemView
    {
        [SerializeField] private Button button;
        public TextMeshProUGUI title;

        private bool m_IsTitleNotNull;

        public void Init(
            IManagersGetter _Managers,
            IUITicker _UITicker,
            IColorProvider _ColorProvider,
            UnityAction _Select)
        {
            base.Init(_Managers.AudioManager, _UITicker, _ColorProvider);
            name = "Setting";
            button.onClick.AddListener(SoundOnClick);
            button.onClick.AddListener(_Select);
        }

        public override void Init(IAudioManager _AudioManager, IUITicker _UITicker, IColorProvider _ColorProvider)
        {
            throw new NotSupportedException();
        }

        protected override void CheckIfSerializedItemsNotNull()
        {
            base.CheckIfSerializedItemsNotNull();
            m_IsTitleNotNull = title.IsNotNull();
        }

        protected override void SetColorsOnInit()
        {
            base.SetColorsOnInit();
            title.color = ColorProvider.GetColor(ColorIdsCommon.UiText);
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId != ColorIdsCommon.UiText) 
                return;
            if (m_IsTitleNotNull)
                title.color = _Color;
        }
    }
}