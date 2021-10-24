using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.MazeItems;
using UnityEngine.Events;

namespace Games.RazorMaze.Views.MazeItemGroups
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

        public override EMazeItemType[] Types => new[] {EMazeItemType.TrapReact};
        public event UnityAction Initialized;
        public void Init() => Initialized?.Invoke();

        public void OnMazeTrapReactStageChanged(MazeItemTrapReactEventArgs _Args)
        {
            Common.GetItem<IViewMazeItemTrapReact>(_Args.Info).OnTrapReact(_Args);
        }
        
        #endregion
    }
}