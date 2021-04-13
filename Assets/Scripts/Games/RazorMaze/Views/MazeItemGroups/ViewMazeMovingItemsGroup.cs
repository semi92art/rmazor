using System.Collections.Generic;
using System.Linq;
using Entities;
using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.MazeCommon;
using Games.RazorMaze.Views.Utils;
using Shapes;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeMovingItemsGroup : IViewMazeMovingItemsGroup
    {
        #region nonpublic members
        
        private readonly Dictionary<MazeItem, ViewMovingItemInfo> m_ItemsMoving = 
            new Dictionary<MazeItem, ViewMovingItemInfo>();
        
        #endregion

        #region inject
        
        private IModelMazeData Data { get; }
        private IMovingItemsProceeder MovingItemsProceeder { get; }
        private ICoordinateConverter CoordinateConverter { get; }
        private IContainersGetter ContainersGetter { get; }
        private IViewMazeCommon MazeCommon { get; }
        private ViewSettings ViewSettings { get; }

        public ViewMazeMovingItemsGroup(
            IModelMazeData _Data,
            IMovingItemsProceeder _MovingItemsProceeder,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IViewMazeCommon _MazeCommon,
            ViewSettings _ViewSettings)
        {
            Data = _Data;
            MovingItemsProceeder = _MovingItemsProceeder;
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
            MazeCommon = _MazeCommon;
            ViewSettings = _ViewSettings;
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
            m_ItemsMoving.Add(_Args.Item, new ViewMovingItemInfo
            {
                From = CoordinateConverter.ToLocalMazeItemPosition(_Args.From),
                To = CoordinateConverter.ToLocalMazeItemPosition(_Args.To),
                BusyPositions = new Dictionary<V2Int, Disc>()
            });
        }

        public void OnMazeItemMoveContinued(MazeItemMoveEventArgs _Args)
        {
            if (!m_ItemsMoving.ContainsKey(_Args.Item))
                return;
            var item = m_ItemsMoving[_Args.Item];
            var mazeItem = MazeCommon.MazeItems.Single(_Item => _Item.Equal(_Args.Item));
            var pos = Vector2.Lerp(item.From, item.To, _Args.Progress);
            mazeItem.SetLocalPosition(pos);
        }
        
        public void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args)
        {
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
                line.Thickness = ViewSettings.LineWidth * CoordinateConverter.GetScale();
                line.Color = ViewUtils.ColorLines;
                line.SetPoints(points);
                line.Closed = false;
                line.SortingOrder = ViewUtils.GetPathLineSortingOrder();

                foreach (var point in points)
                {
                    var go1 = new GameObject("Joint");
                    go1.SetParent(ContainersGetter.MazeContainer);
                    var disc = go1.AddComponent<Disc>();
                    go1.transform.SetLocalPosXY(point);
                    disc.Color = ViewUtils.ColorLines;
                    disc.Radius = ViewSettings.LineWidth * CoordinateConverter.GetScale() * 2f;
                    disc.Type = DiscType.Disc;
                    disc.SortingOrder = ViewUtils.GetPathLineJointSortingOrder();
                }
            }
        }
        
        #endregion
    }
}