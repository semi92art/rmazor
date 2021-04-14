using Extensions;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using Shapes;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemGravityBlock : IViewMazeItem { }
    
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
            ViewSettings _ViewSettings) 
            : base(_CoordinateConverter, _ContainersGetter)
        {
            Data = _Data;
            ViewSettings = _ViewSettings;
        }
        
        #endregion
        
        #region api

        public object Clone() => new ViewMazeItemGravityBlock(CoordinateConverter, ContainersGetter, Data, ViewSettings);

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
            sh.Color = ViewUtils.ColorLines;
            sh.SortingOrder = ViewUtils.GetBlockSortingOrder(Props.Type);
            var joint = go.AddComponentOnNewChild<Disc>("Joint", out _, null);
            joint.transform.SetLocalPosXY(Vector2.zero);
            joint.Color = ViewUtils.ColorLines;
            joint.Radius = ViewSettings.LineWidth * CoordinateConverter.GetScale() * 2f;
            joint.SortingOrder = ViewUtils.GetBlockSortingOrder(Props.Type);

            Object = go;
            m_Shape = sh;
            m_Joint = joint;
        }

        
        #endregion
    }
}