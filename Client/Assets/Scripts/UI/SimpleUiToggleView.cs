using Games.RazorMaze.Views.Common;
using Ticker;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SimpleUiToggleView : SimpleUiItemBase
    {
        [SerializeField] private Toggle toggle;
        
        public void Init(IUITicker _Ticker, IColorProvider _ColorProvider)
        {
            InitCore(_Ticker, _ColorProvider);
            toggle.transition = Selectable.Transition.ColorTint;
            UpdateToggleColors();
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId == ColorIds.UiDialogItemNormal
                || _ColorId == ColorIds.UiDialogItemPressed
                || _ColorId == ColorIds.UiDialogItemSelected
                || _ColorId == ColorIds.UiDialogItemDisabled)
            {
                UpdateToggleColors();
            }
        }

        private void UpdateToggleColors()
        {
            toggle.colors = GetColorBlock();
        }
    }
}