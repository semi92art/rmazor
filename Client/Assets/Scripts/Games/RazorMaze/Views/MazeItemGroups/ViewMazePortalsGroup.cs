using System.Linq;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazePortalsGroup : IViewMazeItemGroup
    {
        void OnPortalEvent(PortalEventArgs _Args);
    }
    
    public class ViewMazePortalsGroup : ViewMazeItemsGroupBase, IViewMazePortalsGroup
    {
        #region inject
        
        public ViewMazePortalsGroup(IViewMazeCommon _Common) : base(_Common) { }
        
        #endregion
        
        #region api
        
        public override EMazeItemType[] Types => new[] {EMazeItemType.Portal};

        public void OnPortalEvent(PortalEventArgs _Args)
        {
            Common.GetItem<IViewMazeItemPortal>(_Args.Info).DoTeleport(_Args);
            var pairPortal = Common.MazeItems
                .Where(_Item => _Item is IViewMazeItemPortal)
                .SingleOrDefault(_Item => _Item.Props.Position == _Args.Info.Pair) as IViewMazeItemPortal;
            pairPortal?.DoTeleport(_Args);
        }
        
        #endregion
    }
}