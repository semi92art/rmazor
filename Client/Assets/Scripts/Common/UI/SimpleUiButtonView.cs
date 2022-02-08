using Common;
using Common.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SimpleUiButtonView : SimpleUiItemBase
    {
        [SerializeField] private Button button;

        private bool m_IsButtonNotNull;
        
        protected override void CheckIfSerializedItemsNotNull()
        {
            base.CheckIfSerializedItemsNotNull();
            m_IsButtonNotNull = button.IsNotNull();
        }

        protected override void SetColorsOnInit()
        {
            base.SetColorsOnInit();
            if (button.IsNotNull())
                button.targetGraphic.color = ColorProvider.GetColor(ColorIdsCommon.UiDialogItemNormal);
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId != ColorIdsCommon.UiDialogItemNormal)
                return;
            if (m_IsButtonNotNull)
                button.targetGraphic.color = _Color;
        }
    }
}