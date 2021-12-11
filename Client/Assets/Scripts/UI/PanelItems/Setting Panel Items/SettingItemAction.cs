using Entities;
using Games.RazorMaze.Views.Common;
using StansAssets.Foundation.Extensions;
using Ticker;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI.PanelItems.Setting_Panel_Items
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
            base.Init(_Managers, _UITicker, _ColorProvider);
            name = "Setting";
            button.onClick.AddListener(SoundOnClick);
            button.onClick.AddListener(_Select);
        }

        public override void Init(IManagersGetter _Managers, IUITicker _UITicker, IColorProvider _ColorProvider)
        {
            throw new System.NotSupportedException();
        }

        protected override void CheckIfSerializedItemsNotNull()
        {
            base.CheckIfSerializedItemsNotNull();
            m_IsTitleNotNull = title.IsNotNull();
        }

        protected override void SetColorsOnInit()
        {
            base.SetColorsOnInit();
            title.color = ColorProvider.GetColor(ColorIds.UiText);
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId == ColorIds.UiText)
            {
                if (m_IsTitleNotNull)
                    title.color = _Color;
            }
        }
    }
}