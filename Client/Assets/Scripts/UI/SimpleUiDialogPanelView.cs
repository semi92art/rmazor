using Games.RazorMaze.Views.Common;
using Ticker;

namespace UI
{
    public class SimpleUiDialogPanelView : SimpleUiItemBase
    {
        public void Init(IUITicker _Ticker, IColorProvider _ColorProvider)
        {
            InitCore(_Ticker, _ColorProvider);
        }
    }
}