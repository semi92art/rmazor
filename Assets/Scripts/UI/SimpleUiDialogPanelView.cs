using Constants;
using Utils;

namespace UI
{
    public class SimpleUiDialogPanelView : SimpleUiItemBase
    {
        protected override void OnEnable()
        {
            if (!background.IsNull())
                background.color = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiDialogBackground);
            if (!shadow.IsNull())
                shadow.color = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiMainBackground);
            base.OnEnable();
        }
    }
}