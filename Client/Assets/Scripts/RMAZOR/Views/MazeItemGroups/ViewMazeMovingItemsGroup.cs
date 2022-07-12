using System.Collections.Generic;
using Common.Entities;
using Common.Extensions;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.MazeItems;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItemGroups
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
        
        private ICoordinateConverter CoordinateConverter { get; }

        protected ViewMazeMovingItemsGroup(
            ICoordinateConverter _CoordinateConverter,
            IViewMazeCommon            _Common)
            : base(_Common)
        {
            CoordinateConverter = _CoordinateConverter;
        }
        
        #endregion
        
        #region api

        public override IEnumerable<EMazeItemType> Types => new[] {EMazeItemType.TrapMoving};

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