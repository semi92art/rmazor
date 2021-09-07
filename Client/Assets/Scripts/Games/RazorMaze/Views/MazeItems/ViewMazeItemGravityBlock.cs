using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemMovingBlock : IViewMazeItem
    {
        void OnMoveStarted(MazeItemMoveEventArgs _Args);
        void OnMoving(MazeItemMoveEventArgs _Args);
        void OnMoveFinished(MazeItemMoveEventArgs _Args);
    }
    
    public interface IViewMazeItemGravityBlock : IViewMazeItemMovingBlock { }
    
    public class ViewMazeItemGravityBlock : ViewMazeItemBase, IViewMazeItemGravityBlock
    {

        #region nonpublic members

        private Rectangle m_Shape;
        private Disc m_Joint;
        
        #endregion
        
        #region inject
        
        private IModelMazeData Data { get; }
        private ViewSettings ViewSettings { get; }

        public ViewMazeItemGravityBlock(
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IModelMazeData _Data,
            ITicker _Ticker,            
            ViewSettings _ViewSettings) 
            : base(_CoordinateConverter, _ContainersGetter, _Ticker)
        {
            Data = _Data;
            ViewSettings = _ViewSettings;
        }
        
        #endregion
        
        #region api

        public void OnMoveStarted(MazeItemMoveEventArgs _Args)
        {
        }

        public void OnMoving(MazeItemMoveEventArgs _Args)
        {
            var pos = Vector2.Lerp(_Args.From.ToVector2(), _Args.To.ToVector2(), _Args.Progress);
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(pos));
        }

        public void OnMoveFinished(MazeItemMoveEventArgs _Args)
        {
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(_Args.To));
        }
        
        public object Clone() => new ViewMazeItemGravityBlock(
            CoordinateConverter, ContainersGetter, Data, Ticker, ViewSettings);

        #endregion
        
        #region nonpublic methods

        protected override void SetShape()
        {
            var go = Object;
            var sh = ContainersGetter.MazeItemsContainer.gameObject
                .GetOrAddComponentOnNewChild<Rectangle>("Gravity Block", ref go, 
                    CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            go.DestroyChildrenSafe();
            sh.Width = sh.Height = CoordinateConverter.GetScale() * 0.9f;
            sh.Type = Rectangle.RectangleType.RoundedHollow;
            sh.Thickness = ViewSettings.LineWidth * CoordinateConverter.GetScale();
            sh.CornerRadius = ViewSettings.CornerRadius * CoordinateConverter.GetScale();
            sh.Color = DrawingUtils.ColorLines;
            sh.SortingOrder = DrawingUtils.GetBlockSortingOrder(Props.Type);
            var joint = go.AddComponentOnNewChild<Disc>("Joint", out _);
            joint.transform.SetLocalPosXY(Vector2.zero);
            joint.Color = DrawingUtils.ColorLines;
            joint.Radius = ViewSettings.LineWidth * CoordinateConverter.GetScale() * 2f;
            joint.SortingOrder = DrawingUtils.GetBlockSortingOrder(Props.Type);

            Object = go;
            m_Shape = sh;
            m_Joint = joint;
        }
        
        #endregion
    }
}