using System.Linq;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.MazeCommon;
using Utils;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeShredingerBlocksGroupProt : IViewMazeShredingerBlocksGroup
    {
        #region inject
        private IViewMazeCommon MazeCommon { get; }

        public ViewMazeShredingerBlocksGroupProt(IViewMazeCommon _MazeCommon)
        {
            MazeCommon = _MazeCommon;
        }
        
        #endregion
        
        #region api
        
        public void OnShredingerBlockEvent(ShredingerBlockArgs _Args)
        {
            var item = MazeCommon.MazeItems.First(_Item => _Item.Equal(_Args.Item));
            item.Active = !_Args.Opened;
        }
        
        #endregion
    }
}