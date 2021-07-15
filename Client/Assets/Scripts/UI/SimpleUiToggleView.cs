using Constants;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class SimpleUiToggleView : SimpleUiItemBase, INormalPressedDisabled
    {
        [SerializeField] private Image checkMark;

        protected override void OnEnable()
        {
            checkMark.color = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiToggleCheckMark);
            base.OnEnable();
        }

        public void SetNormal()
        {
            MakeTransition(ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiToggleNormal));
        }

        public void SetPressed()
        {
            MakeTransition(ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiTogglePressed));
        }

        public void SetDisabled()
        {
            MakeTransition(ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiToggleDisabled));
        }
    }
}