using Constants;
using Extensions;
using Utils;

namespace UI
{
    public class SimpleUiDialogItemView : SimpleUiItemBase, INormalPressedDisabled
    {
        protected override void OnEnable()
        {
            if (!background.IsNull())
                background.color = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UDialogItemNormal);
            if (!shadow.IsNull())
                shadow.color = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiDialogBackground);
            base.OnEnable();
        }
        
        public void SetNormal()
        {
            MakeTransition(ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UDialogItemNormal));
        }

        public void SetPressed()
        {
            MakeTransition(ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiDialogItemPressed));
        }

        public void SetDisabled()
        {
            MakeTransition(ColorUtils.GetColorFromCurrentPalette(
                CommonPaletteColors.UiDialogItemDisabled));
        }
    }
}