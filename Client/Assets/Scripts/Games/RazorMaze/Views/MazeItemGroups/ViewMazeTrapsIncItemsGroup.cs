using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.MazeItems;
using UnityEngine.Events;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeTrapsIncItemsGroup :
        IInit,
        IViewMazeItemGroup
    {
        void OnMazeTrapIncreasingStageChanged(MazeItemTrapIncreasingEventArgs _Args);
    }
    
    public class ViewMazeTrapsIncItemsGroup : ViewMazeItemsGroupBase, IViewMazeTrapsIncItemsGroup
    {
        #region inject
        
        public ViewMazeTrapsIncItemsGroup(IViewMazeCommon _Common) : base(_Common) { }
        
        #endregion

        public override EMazeItemType[] Types       => new[] {EMazeItemType.TrapIncreasing};
        public          bool            Initialized { get; private set; }
        public event UnityAction        Initialize;
        
        public void Init()
        {
            Initialize?.Invoke();
            Initialized = true;
        }

        public void OnMazeTrapIncreasingStageChanged(MazeItemTrapIncreasingEventArgs _Args)
        {
            Common.GetItem<IViewMazeItemTrapIncreasing>(_Args.Item).OnIncreasing(_Args);
        }

    }
}