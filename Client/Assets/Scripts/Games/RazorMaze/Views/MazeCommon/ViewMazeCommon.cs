using System.Collections.Generic;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.MazeCommon
{
    public class ViewMazeCommon : ViewMazeCommonBase
    {
        #region nonpublic members

        private const int PathItemsCount = 300;
        private const int MazeItemsCount = 100;
        //private Dictionary<EMazeItemType, SpawnPool<IViewMazeItem>> m_Pools;
        
        #endregion
        
        
        #region inject
        
        public ViewMazeCommon(
            IMazeItemsCreator _MazeItemsCreator, 
            IModelMazeData _Model,
            IContainersGetter _ContainersGetter, 
            ICoordinateConverter _CoordinateConverter) 
            : base(_MazeItemsCreator, _Model, _ContainersGetter, _CoordinateConverter)
        { }
        
        #endregion
        
        #region api

        public override void Init()
        {
            throw new System.NotImplementedException();
        }

        public override IViewMazeItem GetItem(MazeItem _Item)
        {
            throw new System.NotImplementedException();
        }

        public override T GetItem<T>(MazeItem _Item)
        {
            throw new System.NotImplementedException();
        }
        
        #endregion
        
        #region nonpublic methods

        private void InitPools()
        {
            
        }
        
        #endregion
    }
}