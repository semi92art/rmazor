using DI.Extensions;
using Games.RazorMaze.Views.Common;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SimpleUiToggleView : SimpleUiItemBase
    {
        [SerializeField] private Toggle toggle;

        private bool m_IsToggleNotNull;
        
        protected override void CheckIfSerializedItemsNotNull()
        {
            base.CheckIfSerializedItemsNotNull();
            m_IsToggleNotNull = toggle.IsNotNull();
        }

        protected override void SetColorsOnInit()
        {
            base.SetColorsOnInit();
            if (m_IsToggleNotNull)
                toggle.targetGraphic.color = ColorProvider.GetColor(ColorIds.UI);
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId == ColorIds.UI)
            {
                if (m_IsToggleNotNull)
                    toggle.targetGraphic.color = _Color;
            }
        }
    }
}