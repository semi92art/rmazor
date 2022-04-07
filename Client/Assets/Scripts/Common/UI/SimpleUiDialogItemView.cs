using Common.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI
{
    public abstract class SimpleUiDialogItemView : SimpleUiItemBase
    {
        #region serialized fields

        [SerializeField] protected Image dialogBackground;  

        #endregion
        
        #region nonpublic members

        protected bool m_IsDialogBackgroundNotNull;

        #endregion

        #region nonpublic methods

        protected override void CheckIfSerializedItemsNotNull()
        {
            base.CheckIfSerializedItemsNotNull();
            m_IsDialogBackgroundNotNull = dialogBackground.IsNotNull();
        }

        protected override void SetColorsOnInit()
        {
            base.SetColorsOnInit();
            if (m_IsDialogBackgroundNotNull)
                dialogBackground.color = ColorProvider.GetColor(ColorIds.UiDialogBackground);
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId != ColorIds.UiDialogBackground) 
                return;
            if (m_IsDialogBackgroundNotNull)
                dialogBackground.color = _Color;
        }

        #endregion
    }
}