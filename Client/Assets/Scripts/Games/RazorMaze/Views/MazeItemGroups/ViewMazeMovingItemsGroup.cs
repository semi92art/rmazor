using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Models.ProceedInfos;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.MazeItems;
using Shapes;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeMovingItemsGroup : ViewMazeItemsGroupBase, IViewMazeMovingItemsGroup, IOnBackgroundColorChanged
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
        
        private ICoordinateConverter CoordinateConverter { get; }
        
        public ViewMazeMovingItemsGroup(
            ICoordinateConverter _CoordinateConverter,
            IViewMazeCommon _Common)
            : base(_Common)
        {
            CoordinateConverter = _CoordinateConverter;
        }
        
        #endregion
        
        #region api

        public override EMazeItemType[] Types => new[]
        {
            EMazeItemType.GravityBlock, 
            EMazeItemType.GravityTrap,
            EMazeItemType.TrapMoving
        };

        public void OnMazeItemMoveStarted(MazeItemMoveEventArgs _Args)
        {
            if (m_ItemsMoving.ContainsKey(_Args.Info))
                m_ItemsMoving.Remove(_Args.Info);
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
        
        public void OnBackgroundColorChanged(Color _Color)
        {
            var items = GetItems()
                .Select(_Item => _Item as IOnBackgroundColorChanged)
                .Where(_Item => _Item != null);
            foreach (var item in items)
                item.OnBackgroundColorChanged(_Color);
        }
        
        #endregion
    }
}