using System.Linq;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.MazeCommon;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public abstract class ViewMazeShredingerBlocksGroupBase : IViewMazeShredingerBlocksGroup
    {
        #region inject
        
        protected IViewMazeCommon MazeCommon { get; }

        protected ViewMazeShredingerBlocksGroupBase(IViewMazeCommon _MazeCommon)
        {
            MazeCommon = _MazeCommon;
        }
        
        #endregion
        
        #region api
        
        public virtual void OnShredingerBlockEvent(ShredingerBlockArgs _Args)
        {
            var item = MazeCommon.MazeItems.First(_Item => _Item.Equal(_Args.Item));
            if (_Args.Stage != 1)
                item.Active = !_Args.Opened;
        }
        
        #endregion
    }
}