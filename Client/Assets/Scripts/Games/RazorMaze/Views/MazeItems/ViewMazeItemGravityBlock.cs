using DI.Extensions;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemGravityBlock : IViewMazeItemMovingBlock { }
    
    public class ViewMazeItemGravityBlock : ViewMazeItemGravityBlockFree, IViewMazeItemGravityBlock
    {
        #region shapes

        protected override string ObjectName => "Gravity Block";
        protected override object[] DefaultColorShapes => new object[] {m_Shape, m_Joint};
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
        

        #endregion
        
        #region nonpublic methods

        protected override void InitShape()
        {
            base.InitShape();
            var joint = Object.AddComponentOnNewChild<Disc>("Joint", out _);
            joint.transform.SetLocalPosXY(Vector2.zero);
            joint.Color = DrawingUtils.ColorLines;
            joint.Radius = ViewSettings.LineWidth * CoordinateConverter.Scale * 2f;
            joint.SortingOrder = DrawingUtils.GetBlockSortingOrder(Props.Type);
            m_Joint = joint;
        }

        protected override void UpdateShape()
        {
            base.UpdateShape();
            m_Joint.Radius = ViewSettings.LineWidth * CoordinateConverter.Scale * 2f;
        }

        protected override void InitWallBlockMovingPaths()
        {
            InitWallBlockMovingPathsCore();
        }

        #endregion
    }
}