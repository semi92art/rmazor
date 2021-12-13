using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Models.ProceedInfos;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.MazeItems;
using Shapes;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public interface IViewMazeMovingItemsGroup : IViewMazeItemGroup
    {
        void OnMazeItemMoveStarted(MazeItemMoveEventArgs _Args);
        void OnMazeItemMoveContinued(MazeItemMoveEventArgs _Args);
        void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args);
    }
    
    public class ViewMazeMovingItemsGroup : ViewMazeItemsGroupBase, IViewMazeMovingItemsGroup
    {
        #region types
        
        private class ViewMovingItemInfo
        {
            public Vector2 From { get; set; }
            public Vector2 To { get; set; }
            public Dictionary<V2Int, Disc> BusyPositions { get; set; }
        }
        
        #endregion
        
        #region nonpublic members
        
        private readonly Dictionary<IMazeItemProceedInfo, ViewMovingItemInfo> m_ItemsMoving =
            new Dictionary<IMazeItemProceedInfo, ViewMovingItemInfo>();
        
        #endregion
        
        #region inject
        
        private IMazeCoordinateConverter CoordinateConverter { get; }

        public ViewMazeMovingItemsGroup(
            IMazeCoordinateConverter _CoordinateConverter,
            IViewMazeCommon _Common)
            : base(_Common)
        {
            CoordinateConverter = _CoordinateConverter;
        }
        
        #endregion
        
        #region api

        public override EMazeItemType[] Types => new[] {EMazeItemType.TrapMoving};

        public void OnMazeItemMoveStarted(MazeItemMoveEventArgs _Args)
        {
            m_ItemsMoving.RemoveSafe(_Args.Info, out _);
            m_ItemsMoving.Add(_Args.Info, new ViewMovingItemInfo
            {
                From = CoordinateConverter.ToLocalMazeItemPosition(_Args.From),
                To = CoordinateConverter.ToLocalMazeItemPosition(_Args.To),
                BusyPositions = new Dictionary<V2Int, Disc>()
            });
            (Common.GetItem(_Args.Info) as IViewMazeItemMovingBlock)?.OnMoveStarted(_Args);
        }

        public void OnMazeItemMoveContinued(MazeItemMoveEventArgs _Args)
        {
            if (!m_ItemsMoving.ContainsKey(_Args.Info))
                return;
            (Common.GetItem(_Args.Info) as IViewMazeItemMovingBlock)?.OnMoving(_Args);
        }
        
        public void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args)
        {
            if (!m_ItemsMoving.ContainsKey(_Args.Info))
                return;
            (Common.GetItem(_Args.Info) as IViewMazeItemMovingBlock)?.OnMoveFinished(_Args);
            m_ItemsMoving.Remove(_Args.Info);
        }

        #endregion
    }
}