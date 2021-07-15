﻿using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.MazeCommon;
using Games.RazorMaze.Views.MazeItems;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeTrapsReactItemGroup : IViewMazeTrapsReactItemsGroup
    {
        #region inject

        private IViewMazeCommon ViewMazeCommon { get; }

        public ViewMazeTrapsReactItemGroup(IViewMazeCommon _ViewMazeCommon)
        {
            ViewMazeCommon = _ViewMazeCommon;
        }
        
        #endregion
        
        #region api
        
        public void Init() { }

        public void OnMazeTrapReactStageChanged(MazeItemTrapReactEventArgs _Args)
        {
            ViewMazeCommon.GetItem<IViewMazeItemTrapReact>(_Args.Item).OnTrapReact(_Args);
        }
        
        #endregion
        
        

    }
}