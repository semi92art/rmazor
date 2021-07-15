using System.Linq;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.MazeCommon;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazePortalsGroup : ViewMazePortalsGroupBase
    {
        #region inject
        
        private IViewMazeCommon ViewMazeCommon { get; }

        public ViewMazePortalsGroup(IViewMazeCommon _ViewMazeCommon)
        {
            ViewMazeCommon = _ViewMazeCommon;
        }
        
        #endregion
        
        
        public override void OnPortalEvent(PortalEventArgs _Args)
        {
            ViewMazeCommon.GetItem<IViewMazeItemPortal>(_Args.Item).DoTeleport(_Args);
            var pairPortal = ViewMazeCommon.MazeItems
                .Where(_Item => _Item is IViewMazeItemPortal)
                .SingleOrDefault(_Item => _Item.Props.Position == _Args.Item.Pair) as IViewMazeItemPortal;
            pairPortal?.DoTeleport(_Args);
        }
    }
}