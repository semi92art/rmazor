using DI.Extensions;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItems
{

    
    public interface IViewMazeItemGravityBlock : IViewMazeItemMovingBlock { }
    
    public class ViewMazeItemGravityBlock : ViewMazeItemMovingBase, IViewMazeItemGravityBlock
    {
        #region shapes

        protected override string ObjectName => "Gravity Block";
        protected override object[] DefaultColorShapes => new object[] {m_Shape, m_Joint};
        private Rectangle m_Shape;
        private Disc m_Joint;
        
        #endregion
        
        #region inject
        
        public ViewMazeItemGravityBlock(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            IMazeCoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker,
            IViewAppearTransitioner _Transitioner,
            IManagersGetter _Managers)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers) { }
        
        #endregion
        
        #region api
        
        public override object Clone() => new ViewMazeItemGravityBlock(
            ViewSettings,
            Model,
            CoordinateConverter, 
            ContainersGetter, 
            GameTicker,
            Transitioner,
            Managers);
        
        public override void OnMoving(MazeItemMoveEventArgs _Args)
        {
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            var pos = Vector2.Lerp(_Args.From.ToVector2(), _Args.To.ToVector2(), _Args.Progress);
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(pos));
        }

        public override void OnMoveFinished(MazeItemMoveEventArgs _Args)
        {
            base.OnMoveFinished(_Args);
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(_Args.To));
            Managers.Notify();
        }

        #endregion
        
        #region nonpublic methods

        protected override void InitShape()
        {
            var sh = Object.AddComponentOnNewChild<Rectangle>("Block", out _);
            sh.Type = Rectangle.RectangleType.RoundedHollow;
            sh.Color = DrawingUtils.ColorLines;
            sh.SortingOrder = DrawingUtils.GetBlockSortingOrder(Props.Type);
            var joint = Object.AddComponentOnNewChild<Disc>("Joint", out _);
            joint.transform.SetLocalPosXY(Vector2.zero);
            joint.Color = DrawingUtils.ColorLines;
            joint.Radius = ViewSettings.LineWidth * CoordinateConverter.Scale * 2f;
            joint.SortingOrder = DrawingUtils.GetBlockSortingOrder(Props.Type);
            m_Shape = sh;
            m_Joint = joint;
        }

        protected override void UpdateShape()
        {
            base.UpdateShape();
            m_Shape.Width = m_Shape.Height = CoordinateConverter.Scale * 0.9f;
            m_Shape.Thickness = ViewSettings.LineWidth * CoordinateConverter.Scale;
            m_Shape.CornerRadius = ViewSettings.CornerRadius * CoordinateConverter.Scale;
            m_Joint.Radius = ViewSettings.LineWidth * CoordinateConverter.Scale * 2f;
        }

        #endregion
    }
}