using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public abstract class ViewMazeItemMovingBase : ViewMazeItemBase
    {
        #region shapes
        
        protected override object[] Shapes => m_PathLines
            .Cast<object>()
            .Concat(m_PathJoints)
            .ToArray();

        protected readonly List<Polyline> m_PathLines = new List<Polyline>();
        protected readonly List<Disc> m_PathJoints = new List<Disc>();

        #endregion
        
        protected ViewMazeItemMovingBase(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker) :
            base(
                _ViewSettings, 
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker) { }

        protected virtual void InitWallBlockMovingPaths()
        {
            m_PathLines.Clear();
            m_PathJoints.Clear();
            
            var items = Model.Data.Info.MazeItems
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
                line.Color = DrawingUtils.ColorLines;
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
                    joint.Color = DrawingUtils.ColorLines;
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

        protected override void SetShape()
        {
            InitWallBlockMovingPaths();
        }

        protected override void Appear(bool _Appear)
        {
            base.Appear(_Appear);
            
            Coroutines.Run(Coroutines.WaitWhile(
                () => !Initialized,
                () =>
                {
                    var sets = new Dictionary<object[], Color>();
                    if (m_PathLines.Any())
                        sets.Add(m_PathLines.Cast<object>().ToArray(), DrawingUtils.ColorLines);
                    if (m_PathJoints.Any())
                        sets.Add(m_PathJoints.Cast<object>().ToArray(), DrawingUtils.ColorLines);
                    
                    RazorMazeUtils.DoAppearTransitionSimple(
                        _Appear,
                        GameTicker,
                        sets);
                }));
        }
    }
}