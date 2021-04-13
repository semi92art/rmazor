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

        private GameObject m_GameObject;
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
        
        public GameObject Object => m_GameObject;
        
        public void Init(ViewMazeItemProps _Props)
        {
            Props = _Props;
            SetShape();
            Item = m_GameObject.transform;
        }
        
        public object Clone() => new ViewMazeItemGravityBlock(CoordinateConverter, ContainersGetter, Data, ViewSettings);

        #endregion
        
        #region nonpublic methods

        private void SetShape()
        {
            var go = m_GameObject;
            if (go == null)
            {
                go = new GameObject("Gravity Block");
                go.SetParent(ContainersGetter.MazeItemsContainer);
            }
            go.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            go.DestroyChildrenSafe();
            var sh = go.AddComponent<Rectangle>();
            sh.Width = sh.Height = CoordinateConverter.GetScale() * 0.9f;
            sh.Type = Rectangle.RectangleType.RoundedHollow;
            sh.Thickness = ViewSettings.LineWidth * CoordinateConverter.GetScale();
            sh.CornerRadius = ViewSettings.CornerRadius * CoordinateConverter.GetScale();
            sh.Color = ViewUtils.ColorLines;
            sh.SortingOrder = ViewUtils.GetBlockSortingOrder(Props.Type);
            var goJoint = new GameObject("Joint");
            goJoint.SetParent(go);
            var joint = goJoint.AddComponent<Disc>();
            joint.transform.SetLocalPosXY(Vector2.zero);
            joint.Color = ViewUtils.ColorLines;
            joint.Radius = ViewSettings.LineWidth * CoordinateConverter.GetScale() * 2f;
            joint.SortingOrder = ViewUtils.GetBlockSortingOrder(Props.Type);

            m_GameObject = go;
            m_Shape = sh;
            m_Joint = joint;
        }

        
        #endregion
    }
}