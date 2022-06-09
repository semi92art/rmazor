using System.Collections.Generic;
using Common;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.MazeItemGroups
{
    public interface IViewMazeTrapsReactItemsGroup : 
        IInit,
        IViewMazeItemGroup
    {
        void OnMazeTrapReactStageChanged(MazeItemTrapReactEventArgs _Args);
    }
    
    public class ViewMazeTrapsReactSpikesItemGroup : ViewMazeItemsGroupBase, IViewMazeTrapsReactItemsGroup
    {
        #region inject
        
        private ViewMazeTrapsReactSpikesItemGroup(IViewMazeCommon _Common) : base (_Common) { }
        
        #endregion
        
        #region api

        public override IEnumerable<EMazeItemType> Types => new[] {EMazeItemType.TrapReact};
        
        public void OnMazeTrapReactStageChanged(MazeItemTrapReactEventArgs _Args)
        {
            Common.GetItem<IViewMazeItemTrapReact>(_Args.Info)?.OnTrapReact(_Args);
        }
        
        #endregion
    }
}