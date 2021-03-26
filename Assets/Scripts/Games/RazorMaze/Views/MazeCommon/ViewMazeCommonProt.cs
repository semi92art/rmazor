using System.Collections.Generic;
using Games.RazorMaze.Models;
using Games.RazorMaze.Prot;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.MazeCommon
{
    public class ViewMazeCommonProt : IViewMazeCommon
    {
        #region inject

        private IMazeModel Model { get; }
        public IContainersGetter ContainersGetter { get; }

        public ViewMazeCommonProt(IMazeModel _Model, IContainersGetter _ContainersGetter)
        {
            Model = _Model;
            ContainersGetter = _ContainersGetter;
        }

        #endregion

        #region api
        
        public List<IViewMazeItem> MazeItems { get; private set; }
        
        public void Init()
        {
            MazeItems = RazorMazePrototypingUtils.CreateMazeItems(Model.Info, ContainersGetter.MazeItemsContainer);
        }
        
        #endregion
    }
}