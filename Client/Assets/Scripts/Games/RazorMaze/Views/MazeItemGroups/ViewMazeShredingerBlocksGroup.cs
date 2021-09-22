using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeShredingerBlocksGroup : ViewMazeItemsGroupBase, IViewMazeShredingerBlocksGroup
    {
        public ViewMazeShredingerBlocksGroup(IViewMazeCommon _Common) : base(_Common) { }
        
        public override EMazeItemType[] Types => new[] {EMazeItemType.ShredingerBlock};
        
        public void OnShredingerBlockEvent(ShredingerBlockArgs _Args)
        {
            var item = Common.GetItem(_Args.Item);
            item.Proceeding = _Args.Stage == ShredingerBlocksProceeder.StageClosed;
        }
    }
}