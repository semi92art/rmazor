using System.Linq;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.MazeItemGroups
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
            var pair = Common.GetItems<IViewMazeItemPortal>()
                .SingleOrDefault(_Item => _Item.Props.Position == _Args.Info.Pair);
            pair?.DoTeleport(_Args);
        }
        
        #endregion
    }
}