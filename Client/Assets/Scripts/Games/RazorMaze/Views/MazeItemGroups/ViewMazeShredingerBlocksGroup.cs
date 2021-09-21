using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeShredingerBlocksGroup : ViewMazeItemsGroupBase, IViewMazeShredingerBlocksGroup
    {
        public ViewMazeShredingerBlocksGroup(IViewMazeCommon _MazeCommon) : base(_MazeCommon) { }
        
        public override EMazeItemType[] Types => new[] {EMazeItemType.ShredingerBlock};
        
        public void OnShredingerBlockEvent(ShredingerBlockArgs _Args)
        {
            var item = Common.GetItem(_Args.Item);
            if (_Args.Stage != ShredingerBlocksProceeder.StageOpened && item != null)
                item.Proceeding = !_Args.Opened;
        }
    }
}