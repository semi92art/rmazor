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
using TimeProviders;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItemGroups
{
    public class ViewMazeMovingItemsGroup : ViewMazeItemsGroupBase, IViewMazeMovingItemsGroup
    {
        #region nonpublic members
        
        private readonly Dictionary<MazeItem, ViewMovingItemInfo> m_ItemsMoving = new Dictionary<MazeItem, ViewMovingItemInfo>();
        private readonly List<Polyline> m_PathLines = new List<Polyline>();
        private readonly List<Disc> m_PathJoints = new List<Disc>();
        private bool m_Initialized;
        
        #endregion
        
        #region inject
        
        private IModelMazeData Data { get; }
        private IMovingItemsProceeder MovingItemsProceeder { get; }
        private ICoordinateConverter CoordinateConverter { get; }
        private IContainersGetter ContainersGetter { get; }
        private IGameTimeProvider GameTimeProvider { get; }

        public ViewMazeMovingItemsGroup(
            IModelMazeData _Data,
            IMovingItemsProceeder _MovingItemsProceeder,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IViewMazeCommon _Common,
            IGameTimeProvider _GameTimeProvider) : base(_Common)
        {
            Data = _Data;
            MovingItemsProceeder = _MovingItemsProceeder;
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
            GameTimeProvider = _GameTimeProvider;
        }
        
        #endregion
        
        #region api

        public override EMazeItemType[] Types => new[]
            {EMazeItemType.GravityBlock, EMazeItemType.GravityTrap, EMazeItemType.TrapMoving};
        public event NoArgsHandler Initialized;

        public void Init()
        {
            DrawWallBlockMovingPaths(DrawingUtils.ColorLines);
            Initialized?.Invoke();
            m_Initialized = true;
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
            (Common.GetItem(_Args.Item) as IViewMazeItemMovingBlock)?.OnMoveStarted(_Args);
        }

        public void OnMazeItemMoveContinued(MazeItemMoveEventArgs _Args)
        {
            if (_Args.Item.Type == EMazeItemType.GravityBlock || _Args.Item.Type == EMazeItemType.GravityTrap)
                MarkMazeItemBusyPositions(_Args.Item, _Args.BusyPositions);
            if (!m_ItemsMoving.ContainsKey(_Args.Item))
                return;
            (Common.GetItem(_Args.Item) as IViewMazeItemMovingBlock)?.OnMoving(_Args);
        }
        
        public void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args)
        {
            if (_Args.Item.Type == EMazeItemType.GravityBlock || _Args.Item.Type == EMazeItemType.GravityTrap)
                MarkMazeItemBusyPositions(_Args.Item, null);
            if (!m_ItemsMoving.ContainsKey(_Args.Item))
                return;
            
            (Common.GetItem(_Args.Item) as IViewMazeItemMovingBlock)?.OnMoveFinished(_Args);
            m_ItemsMoving.Remove(_Args.Item);
        }
        
        #endregion
        
        #region nonpublic methods

        private void DrawWallBlockMovingPaths(Color _LinesAndJointsColor)
        {
            DrawWallBlockMovingPathsCore(_LinesAndJointsColor);
        }

        private void DrawWallBlockMovingPathsCore(Color _LinesAndJointsColor)
        {
            m_PathLines.Clear();
            m_PathJoints.Clear();
            
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
                line.SortingOrder = DrawingUtils.GetPathLineSortingOrder();
                m_PathLines.Add(line);
                
                foreach (var point in points)
                {
                    var go1 = new GameObject("Joint");
                    go1.SetParent(ContainersGetter.MazeItemsContainer);
                    var joint = go1.AddComponent<Disc>();
                    go1.transform.SetLocalPosXY(point);
                    joint.Color = _LinesAndJointsColor;
                    joint.Radius = 0.5f;
                    joint.Type = DiscType.Disc;
                    joint.SortingOrder = DrawingUtils.GetPathLineJointSortingOrder();
                    m_PathJoints.Add(joint);
                }
            }

            foreach (var line in m_PathLines)
                line.enabled = false;
            foreach (var joint in m_PathJoints)
                joint.enabled = false;
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);

            bool? appear = null;
            if (_Args.Stage == ELevelStage.Loaded)
                appear = true;
            else if (_Args.Stage == ELevelStage.Unloaded)
                appear = false;

            if (!appear.HasValue)
                return;
            
            Coroutines.Run(Coroutines.WaitWhile(
                () => !m_Initialized,
                () =>
                {
                    RazorMazeUtils.DoAppearTransitionSimple(
                        appear.Value,
                        GameTimeProvider,
                        new Dictionary< IEnumerable<ShapeRenderer>, Color>
                        {
                            {m_PathLines, DrawingUtils.ColorLines},
                            {m_PathJoints, DrawingUtils.ColorLines}
                        });
                }));
        }
        
        private void MarkMazeItemBusyPositions(MazeItem _Item, IEnumerable<V2Int> _Positions) { }
        
        #endregion
    }
}