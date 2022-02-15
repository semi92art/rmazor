using Common;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.MazeItems;
using UnityEngine.Events;

namespace RMAZOR.Views.MazeItemGroups
{
    public interface IViewMazeTrapsReactItemsGroup : 
        IInit,
        IViewMazeItemGroup
    {
        void OnMazeTrapReactStageChanged(MazeItemTrapReactEventArgs _Args);
    }
    
    public class ViewMazeTrapsReactItemGroup : ViewMazeItemsGroupBase, IViewMazeTrapsReactItemsGroup
    {
        #region inject
        
        public ViewMazeTrapsReactItemGroup(IViewMazeCommon _Common) : base (_Common) { }
        
        #endregion
        
        #region api

        public override EMazeItemType[] Types       => new[] {EMazeItemType.TrapReact};

        public void OnMazeTrapReactStageChanged(MazeItemTrapReactEventArgs _Args)
        {
            Common.GetItem<IViewMazeItemTrapReact>(_Args.Info).OnTrapReact(_Args);
        }
        
        #endregion
    }
}