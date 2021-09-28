using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeTrapsReactItemGroup : ViewMazeItemsGroupBase, IViewMazeTrapsReactItemsGroup
    {
        #region inject
        
        public ViewMazeTrapsReactItemGroup(IViewMazeCommon _Common) : base (_Common) { }
        
        #endregion
        
        #region api

        public override EMazeItemType[] Types => new[] {EMazeItemType.TrapReact};
        public event NoArgsHandler Initialized;
        public void Init() => Initialized?.Invoke();

        public void OnMazeTrapReactStageChanged(MazeItemTrapReactEventArgs _Args)
        {
            Common.GetItem<IViewMazeItemTrapReact>(_Args.Info).OnTrapReact(_Args);
        }
        
        #endregion


        
    }
}