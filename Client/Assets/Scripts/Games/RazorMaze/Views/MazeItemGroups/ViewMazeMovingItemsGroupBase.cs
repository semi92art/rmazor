using System.Collections.Generic;
using System.Linq;
using Entities;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.MazeItems;
using Games.RazorMaze.Views.Utils;
using Shapes;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public abstract class ViewMazeMovingItemsGroupBase : IViewMazeMovingItemsGroup
    {
        #region nonpublic members
        
        protected readonly Dictionary<MazeItem, ViewMovingItemInfo> m_ItemsMoving = 
            new Dictionary<MazeItem, ViewMovingItemInfo>();
        
        #endregion
        
        #region inject
        
        protected IModelMazeData Data { get; }
        protected IMovingItemsProceeder MovingItemsProceeder { get; }
        protected ICoordinateConverter CoordinateConverter { get; }
        protected IContainersGetter ContainersGetter { get; }
        protected IViewMazeCommon MazeCommon { get; }

        protected ViewMazeMovingItemsGroupBase(
            IModelMazeData _Data,
            IMovingItemsProceeder _MovingItemsProceeder,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IViewMazeCommon _MazeCommon)
        {
            Data = _Data;
            MovingItemsProceeder = _MovingItemsProceeder;
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
            MazeCommon = _MazeCommon;
        }
        
        #endregion
        
        #region api

        public event NoArgsHandler Initialized;

        public virtual void Init()
        {
            Initialized?.Invoke();
        }

        public void OnMazeItemMoveStarted(MazeItemMoveEventArgs _Args)
        {
            if (m_ItemsMoving.ContainsKey(_Args.Item))
                m_ItemsMoving.Remove(_Args.Item);
            m_ItemsMoving.Add(_Args.Item, new ViewMovingItemInfo
            {
                From = CoordinateConverter.ToLocalMazeItemPosition(_Args.From),
                To = CoordinateConverter.ToLocalMazeItemPosition(_Args.To),
                BusyPositions = new Dictionary<V2Int, Disc>()
            });
            (MazeCommon.GetItem(_Args.Item) as IViewMazeItemMovingBlock)?.OnMoveStarted(_Args);
        }

        public void OnMazeItemMoveContinued(MazeItemMoveEventArgs _Args)
        {
            if (_Args.Item.Type == EMazeItemType.GravityBlock || _Args.Item.Type == EMazeItemType.GravityTrap)
                MarkMazeItemBusyPositions(_Args.Item, _Args.BusyPositions);
            if (!m_ItemsMoving.ContainsKey(_Args.Item))
                return;
            (MazeCommon.GetItem(_Args.Item) as IViewMazeItemMovingBlock)?.OnMoving(_Args);
        }
        
        public void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args)
        {
            if (_Args.Item.Type == EMazeItemType.GravityBlock || _Args.Item.Type == EMazeItemType.GravityTrap)
                MarkMazeItemBusyPositions(_Args.Item, null);
            if (!m_ItemsMoving.ContainsKey(_Args.Item))
                return;
            
            (MazeCommon.GetItem(_Args.Item) as IViewMazeItemMovingBlock)?.OnMoveFinished(_Args);
            m_ItemsMoving.Remove(_Args.Item);
        }
        
        #endregion
        
        #region nonpublic methods

        protected virtual void DrawWallBlockMovingPaths(Color _LinesAndJointsColor)
        {
            DrawWallBlockMovingPathsCore(_LinesAndJointsColor);
        }
        
        protected abstract void MarkMazeItemBusyPositions(MazeItem _Item, IEnumerable<V2Int> _Positions);

        private void DrawWallBlockMovingPathsCore(Color _LinesAndJointsColor)
        {
            var items = Data.Info.MazeItems
                .Where(_O => _O.Type == EMazeItemType.GravityBlock
                             || _O.Type == EMazeItemType.GravityTrap
                             || _O.Type == EMazeItemType.TrapMoving);
            foreach (var obs in items)
            {
                var points = obs.Path
                    .Select(_P => CoordinateConverter.ToLocalMazeItemPosition(_P))
                    .ToList();

                var go = new GameObject("Line");
                go.SetParent(ContainersGetter.MazeItemsContainer);
                go.transform.SetLocalPosXY(Vector2.zero);
                var line = go.AddComponent<Polyline>();
                line.Thickness = 0.3f;
                // line.Color = ColorUtils.MixColors(_LinesAndJointsColor, DrawingUtils.ColorMain).SetA(1);
                line.Color = _LinesAndJointsColor;
                line.SetPoints(points);
                line.Closed = false;
                line.SortingOrder = DrawingUtils.GetPathLineSortingOrder();

                foreach (var point in points)
                {
                    var go1 = new GameObject("Joint");
                    go1.SetParent(ContainersGetter.MazeItemsContainer);
                    var disc = go1.AddComponent<Disc>();
                    go1.transform.SetLocalPosXY(point);
                    // disc.Color = ColorUtils.MixColors(_LinesAndJointsColor, DrawingUtils.ColorMain).SetA(1);
                    disc.Color = _LinesAndJointsColor;
                    disc.Radius = 0.5f;
                    disc.Type = DiscType.Disc;
                    disc.SortingOrder = DrawingUtils.GetPathLineJointSortingOrder();
                }
            }
        }

        #endregion
    }
}