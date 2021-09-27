using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemMovingTrap : IViewMazeItemMovingBlock { }
    
    public class ViewMazeItemMovingTrap : ViewMazeItemBase, IViewMazeItemMovingTrap, IUpdateTick
    {
        #region nonpublic members
        
        private Vector2 m_Position;
        private bool m_Rotating;
        
        #endregion
        
        #region shapes

        protected override object[] Shapes => new object[] {m_Saw}
            .Concat(m_PathLines)
            .Concat(m_PathJoints)
            .ToArray();
        private SpriteRenderer m_Saw;
        private readonly List<Polyline> m_PathLines = new List<Polyline>();
        private readonly List<Disc> m_PathJoints = new List<Disc>();
        
        #endregion
        
        #region inject
        
        public ViewMazeItemMovingTrap(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker) 
            : base(
                _ViewSettings,
                _Model, 
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker) { }
        
        #endregion
        
        #region api
        
        public override object Clone() => new ViewMazeItemMovingTrap(
            ViewSettings,
            Model, 
            CoordinateConverter, 
            ContainersGetter,
            GameTicker);

        public override EProceedingStage ProceedingStage
        {
            get => base.ProceedingStage;
            set
            {
                base.ProceedingStage = value;
                if (value == EProceedingStage.Inactive)
                    StopRotation();
                else
                    StartRotation();
            }
        }

        public void OnMoveStarted(MazeItemMoveEventArgs _Args) { }

        public void OnMoving(MazeItemMoveEventArgs _Args)
        {
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            var precisePosition = Vector2.Lerp(
                _Args.From.ToVector2(), _Args.To.ToVector2(), _Args.Progress);
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(precisePosition));
        }

        public void OnMoveFinished(MazeItemMoveEventArgs _Args)
        {
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(_Args.To));
        }

        public void UpdateTick()
        {
            if (!Initialized || !Activated)
                return;
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            DoRotation();
        }
        
        #endregion

        #region nonpublic methods

        protected override void SetShape()
        {
            var go = Object;
            var saw = ContainersGetter.MazeItemsContainer.gameObject
                .GetOrAddComponentOnNewChild<SpriteRenderer>(
                    "Moving Trap", 
                    ref go,
                    CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            go.DestroyChildrenSafe();
            saw.sprite = PrefabUtilsEx.GetObject<Sprite>("views", "moving_trap");
            saw.color = DrawingUtils.ColorTrap;
            saw.sortingOrder = DrawingUtils.GetBlockSortingOrder(Props.Type);
            saw.enabled = false;
            var coll = go.AddComponent<CircleCollider2D>();
            coll.radius = 0.5f;

            go.transform.localScale = Vector3.one * CoordinateConverter.GetScale();
            
            Object = go;
            m_Saw = saw;

            InitWallBlockMovingPaths();
        }
        
        private void InitWallBlockMovingPaths()
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

        private void DoRotation()
        {
            if (!m_Rotating)
                return;
            float rotSpeed = ViewSettings.MovingTrapRotationSpeed * Time.deltaTime; 
            Object.transform.Rotate(Vector3.forward * rotSpeed);
        }

        private void StartRotation()
        {
            m_Rotating = true;
        }

        private void StopRotation()
        {
            m_Rotating = false;
        }

        protected override void Appear(bool _Appear)
        {
            base.Appear(_Appear);
            
            Coroutines.Run(Coroutines.WaitWhile(
                () => !Initialized,
                () =>
                {
                    var sets = new Dictionary<object[], Color>
                    {
                        {new object[] {m_Saw}, DrawingUtils.ColorLines}
                    };
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

        #endregion
    }
}