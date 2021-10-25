using Constants;
using DI.Extensions;
using Utils;

namespace UI
{
    public class SimpleUiButtonView : SimpleUiItemBase, INormalPressedDisabled
    {
        protected override void OnEnable()
        {
            if (!background.IsNull())
                background.color = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiButtonNormal);
            base.OnEnable();
        }

        public void SetNormal()
        {
            MakeTransition(ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiButtonNormal));
        }

        public void SetPressed()
        {
            MakeTransition(ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiButtonPressed));
        }

        public void SetSelected()
        {
            MakeTransition(ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiButtonPressed));
        }

        public void SetDisabled()
        {
            MakeTransition(ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiButtonDisabled));
        }
    }
}