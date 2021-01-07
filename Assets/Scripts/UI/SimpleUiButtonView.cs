using Constants;
using Extensions;
using Utils;

namespace UI
{
    public class SimpleUiButtonView : SimpleUiItemBase, INormalPressedDisabled
    {
        protected override void OnEnable()
        {
            if (!background.IsNull())
                background.color = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiButtonNormal);
            if (!shadow.IsNull())
                shadow.color = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiDialogBackground);
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

        public void SetDisabled()
        {
            MakeTransition(ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiButtonDisabled));
        }
    }
}