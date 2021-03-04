using Constants;
using Extensions;
using Utils;

namespace UI
{
    public class SimpleUiDialogPanelView : SimpleUiItemBase
    {
        protected override void OnEnable()
        {
            if (!background.IsNull())
                background.color = ColorUtils.GetColorFromCurrentPalette(CommonPaletteColors.UiDialogBackground);
            base.OnEnable();
        }
    }
}