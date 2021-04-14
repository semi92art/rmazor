using System.Collections.Generic;
using System.Linq;
using Entities;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Models.ProceedInfos;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.MazeCommon;
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

        public ViewMazeMovingItemsGroupBase(
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

        public abstract void Init();

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
        }

        public void OnMazeItemMoveContinued(MazeItemMoveEventArgs _Args)
        {
            if (_Args.Item.Type == EMazeItemType.GravityBlock)
                MarkMazeItemBusyPositions(_Args.Item, 
                    (Data.ProceedInfos[_Args.Item.Type][_Args.Item] as MazeItemMovingProceedInfo).BusyPositions);
            if (!m_ItemsMoving.ContainsKey(_Args.Item))
                return;
            var item = m_ItemsMoving[_Args.Item];
            var mazeItem = MazeCommon.MazeItems.Single(_Item => _Item.Equal(_Args.Item));
            var pos = Vector2.Lerp(item.From, item.To, _Args.Progress);
            mazeItem.SetLocalPosition(pos);
        }
        
        public void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args)
        {
            if (_Args.Item.Type == EMazeItemType.GravityBlock)
                MarkMazeItemBusyPositions(_Args.Item, null);
            if (!m_ItemsMoving.ContainsKey(_Args.Item))
                return;
            var viewItem = MazeCommon.MazeItems.Single(_Item => _Item.Equal(_Args.Item));
            viewItem.SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(_Args.To));
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
                line.Color = _LinesAndJointsColor;
                line.SetPoints(points);
                line.Closed = false;
                line.SortingOrder = ViewUtils.GetPathLineSortingOrder();

                foreach (var point in points)
                {
                    var go1 = new GameObject("Joint");
                    go1.SetParent(ContainersGetter.MazeItemsContainer);
                    var disc = go1.AddComponent<Disc>();
                    go1.transform.SetLocalPosXY(point);
                    disc.Color = _LinesAndJointsColor;
                    disc.Radius = 0.5f;
                    disc.Type = DiscType.Disc;
                    disc.SortingOrder = ViewUtils.GetPathLineJointSortingOrder();
                }
            }
        }

        #endregion
    }
}