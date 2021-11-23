using DI.Extensions;
using Games.RazorMaze.Views.Common;
using Ticker;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SimpleUiButtonView : SimpleUiItemBase
    {
        [SerializeField] private Button button;
        
        public void Init(IUITicker _Ticker, IColorProvider _ColorProvider)
        {
            InitCore(_Ticker, _ColorProvider);
            // button.targetGraphic.color = _ColorProvider.GetColor(ColorIds.UiDialogItemNormal);
            if (button.IsNotNull())
                button.transition = Selectable.Transition.ColorTint;
            UpdateButtonColors();
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId == ColorIds.UiDialogItemNormal
                || _ColorId == ColorIds.UiDialogItemPressed
                || _ColorId == ColorIds.UiDialogItemSelected
                || _ColorId == ColorIds.UiDialogItemDisabled)
            {
                UpdateButtonColors();
            }
        }


        private void UpdateButtonColors()
        {
            if (button.IsNotNull())
                button.colors = GetColorBlock();
        }
    }
}