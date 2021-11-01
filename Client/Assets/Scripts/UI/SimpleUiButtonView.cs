using DI.Extensions;
using Games.RazorMaze.Views.Common;

namespace UI
{
    public class SimpleUiButtonView : SimpleUiItemBase, INormalPressedDisabled
    {
        private IColorProvider ColorProvider { get; set; }
        
        public void Init(IColorProvider _ColorProvider)
        {
            ColorProvider = _ColorProvider;
            if (!background.IsNull())
                background.color = ColorProvider.GetColor(ColorIds.UiDialogItemNormal);
        }

        public void SetNormal()
        {
            MakeTransition(ColorProvider.GetColor(ColorIds.UiDialogItemNormal));
        }

        public void SetPressed()
        {
            MakeTransition(ColorProvider.GetColor(ColorIds.UiDialogItemPressed));
        }

        public void SetSelected()
        {
            MakeTransition(ColorProvider.GetColor(ColorIds.UiDialogItemPressed));
        }

        public void SetDisabled()
        {
            MakeTransition(ColorProvider.GetColor(ColorIds.UiDialogItemDisabled));
        }
    }
}