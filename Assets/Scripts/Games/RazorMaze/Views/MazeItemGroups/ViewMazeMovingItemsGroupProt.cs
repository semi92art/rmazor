﻿using System.Collections.Generic;
using System.Linq;
using Entities;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Models.ProceedInfos;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeCommon;
using Shapes;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeMovingItemsGroupProt : IViewMazeMovingItemsGroup
    {
        #region types

        private class MovingItemInfo
        {
            public Vector2 From { get; set; }
            public Vector2 To { get; set; }
            public Dictionary<V2Int, Disc> BusyPositions { get; set; }
        }
        
        #endregion
        
        #region nonpublic members
        
        private readonly Dictionary<MazeItem, MovingItemInfo> m_ItemsMoving = 
            new Dictionary<MazeItem,MovingItemInfo>();
        
        #endregion
        
        #region inject
        
        private IModelMazeData Data { get; }
        private IMovingItemsProceeder MovingItemsProceeder { get; }
        private ICoordinateConverter CoordinateConverter { get; }
        private IContainersGetter ContainersGetter { get; }
        private IViewMazeCommon MazeCommon { get; }

        public ViewMazeMovingItemsGroupProt(
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
        
        public void Init()
        {
            DrawWallBlockMovingPaths();
        }

        public void OnMazeItemMoveStarted(MazeItemMoveEventArgs _Args)
        {
            if (m_ItemsMoving.ContainsKey(_Args.Item))
                m_ItemsMoving.Remove(_Args.Item);
            m_ItemsMoving.Add(_Args.Item, new MovingItemInfo
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

        private void DrawWallBlockMovingPaths()
        {
            var items = Data.Info.MazeItems
                .Where(_O => _O.Type == EMazeItemType.GravityBlock
                || _O.Type == EMazeItemType.GravityTrap
                || _O.Type == EMazeItemType.TrapMoving);
            foreach (var obs in items)
            {
                var points = obs.Path.Select(_P => CoordinateConverter.ToLocalCharacterPosition(_P)).ToList();

                var go = new GameObject("Line");
                go.SetParent(ContainersGetter.MazeContainer);
                go.transform.SetLocalPosXY(Vector2.zero);
                var line = go.AddComponent<Polyline>();
                line.Thickness = 0.3f;
                line.Color = Color.black;
                line.SetPoints(points);
                line.Closed = false;
                line.SortingOrder = 25;

                foreach (var point in points)
                {
                    var go1 = new GameObject("Joint");
                    go1.SetParent(ContainersGetter.MazeContainer);
                    var disc = go1.AddComponent<Disc>();
                    go1.transform.SetLocalPosXY(point);
                    disc.Color = Color.black;
                    disc.Radius = 0.5f;
                    disc.Type = DiscType.Disc;
                    disc.SortingOrder = 25;
                }
            }
        }

        private void MarkMazeItemBusyPositions(MazeItem _Item, IEnumerable<V2Int> _Positions)
        {
            if (!m_ItemsMoving.ContainsKey(_Item))
                m_ItemsMoving.Add(_Item, new MovingItemInfo
                {
                    BusyPositions = new Dictionary<V2Int, Disc>()
                });

            var busyPoss = m_ItemsMoving[_Item].BusyPositions;

            foreach (var kvp in busyPoss.Where(_Kvp => !_Kvp.Value.IsNull()))
                kvp.Value.DestroySafe();
            busyPoss.Clear();

            if (_Positions == null)
                return;
            
            foreach (var pos in _Positions)
            {
                var go = new GameObject("Busy Pos");
                go.SetParent(ContainersGetter.MazeContainer);
                go.transform.localPosition = CoordinateConverter.ToLocalCharacterPosition(pos);
                var disc = go.AddComponent<Disc>();
                disc.Color = Color.red;
                disc.Radius = 0.3f;
                disc.SortingOrder = 20;
                busyPoss.Add(pos, disc);
            }
        }
        
        #endregion
    }
}