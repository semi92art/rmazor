using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.Helpers.MazeItemsCreators;
using Games.RazorMaze.Views.MazeItems;
using Games.RazorMaze.Views.Utils;
using Ticker;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeCommon
{
    public class ViewMazeCommonProt : ViewMazeCommonBase
    {
        #region nonpublic members
        
        private List<IViewMazeItem> m_MazeItems;
        
        #endregion
        
        #region inject
        
        public ViewMazeCommonProt(
            ITicker _Ticker,
            IMazeItemsCreator _MazeItemsCreator,
            IModelMazeData _Model,
            IContainersGetter _ContainersGetter, 
            ICoordinateConverter _CoordinateConverter) : base(_Ticker, _MazeItemsCreator, _Model, _ContainersGetter, _CoordinateConverter)
        {
            Ticker.Register(this);
        }

        #endregion

        #region api

        public override List<IViewMazeItem> MazeItems => m_MazeItems;

        public override IViewMazeItem GetItem(MazeItem _Item)
        {
            return m_MazeItems.SingleOrDefault(_Itm => _Itm.Equal(_Item));
        }
        
        public override void Init()
        {
            Camera.main.backgroundColor = DrawingUtils.ColorBack;
            m_MazeItems = MazeItemsCreator.CreateMazeItems(Model.Info);
            base.Init();
        }

        #endregion
    }
}