using Extensions;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using Shapes;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemPath : IViewMazeItem { }
    
    public class ViewMazeItemPath : ViewMazeItemBase, IViewMazeItemPath
    {

        #region nonpublic members

        private GameObject m_GameObject;
        private Rectangle m_Shape;
        private bool m_Active;
        
        #endregion
        
        #region inject

        public ViewMazeItemPath(
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter) : base(_CoordinateConverter, _ContainersGetter)
        { }
        
        #endregion

        #region api

        public GameObject Object => m_GameObject;

        public override bool Active
        {
            get => m_Active;
            set
            {
                m_Active = value;
                if (value) Fill();
                else Unfill();
            }
        }
        
        public void Init(ViewMazeItemProps _Props)
        {
            Props = _Props;
            SetShape();
        }
        
        #endregion
        
        #region nonpublic methods
        
        private void Fill() => m_Shape.Color = ViewUtils.ColorFill;
        private void Unfill() => m_Shape.Color = ViewUtils.ColorMain;

        private void SetShape()
        {
            var go = m_GameObject;
            if (go == null)
            {
                go = new GameObject("Path Item");
                go.SetParent(ContainersGetter.MazeItemsContainer);
            }
            go.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            go.DestroyChildrenSafe();
            var sh = go.GetOrAddComponent<Rectangle>();
            sh.Width = sh.Height = CoordinateConverter.GetScale() * 1.02f;
            sh.Type = Rectangle.RectangleType.HardSolid;
            sh.Color = ViewUtils.ColorMain;
            sh.SortingOrder = ViewUtils.GetPathSortingOrder();
            m_GameObject = go;
            m_Shape = sh;
        }
        
        #endregion


        public object Clone()
        {
            var res = new ViewMazeItemPath(CoordinateConverter, ContainersGetter);
            return res;
        }
    }
}