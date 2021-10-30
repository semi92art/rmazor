using Constants;
using DI.Extensions;
using Games.RazorMaze.Views.Common;
using Utils;

namespace UI
{
    public class SimpleUiDialogPanelView : SimpleUiItemBase
    {
        protected override void OnEnable()
        {
            if (!background.IsNull())
                background.color = m_ColorProvider.GetColor(ColorIds.UiDialogBackground);
            base.OnEnable();
        }
    }
}