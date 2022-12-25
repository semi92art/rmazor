using Common.Extensions;
using Common.Managers;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Managers;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Common.BackgroundIdleItems
{
    public interface IViewMazeBackgroundIdleItemSquare : IViewMazeBackgroundIdleItem { }
    
    public class ViewMazeBackgroundIdleItemSquare : 
        ViewMazeBackgroundIdleItemBase, 
        IViewMazeBackgroundIdleItemSquare
    {
        #region nonpublic members
        
        private Rectangle m_Rectangle;
        private Rectangle m_Border;

        #endregion

        #region inject
        
        private ViewMazeBackgroundIdleItemSquare(
            IPrefabSetManager    _PrefabSetManager,
            ICoordinateConverter _CoordinateConverter)
            : base(_PrefabSetManager, _CoordinateConverter) { }

        #endregion

        #region api
        
        public override object Clone()
        {
            return new ViewMazeBackgroundIdleItemSquare(PrefabSetManager, CoordinateConverter);
        }

        public override void Init(Transform _Parent, PhysicsMaterial2D _Material)
        {
            Obj = PrefabSetManager.InitPrefab(_Parent, "background", "idle_item_square");
            m_Rectangle = Obj.GetCompItem<Rectangle>("rectangle")
                .SetType(Rectangle.RectangleType.RoundedSolid)
                .SetSortingOrder(SortingOrders.BackgroundItem);
            m_Border = Obj.GetCompItem<Rectangle>("border")
                .SetType(Rectangle.RectangleType.RoundedBorder)
                .SetSortingOrder(SortingOrders.BackgroundItem);
            Shapes.Clear();
            Shapes.AddRange(new[] {m_Rectangle, m_Border});
            var rb = Obj.GetCompItem<Rigidbody2D>("rigidbody");
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.angularDrag = 0f;
            rb.sharedMaterial = _Material;
            Rigidbody = rb;
            Coll = Obj.GetCompItem<Collider2D>("collider");
            Coll.sharedMaterial = _Material;
        }

        public override void SetColor(Color _Color)
        {
            m_Rectangle.Color = _Color.SetA(0.4f);
            m_Border.Color = _Color.SetA(0.8f);
        }

        public override void SetScale(float _Scale)
        {
            Obj.transform.SetLocalScaleXY(Vector2.one * _Scale);
            Rigidbody.mass = _Scale * _Scale;
            Obj.transform.rotation = Quaternion.Euler(0f, 0f, Random.value * 360f);
            base.SetScale(_Scale);
        }

        #endregion
    }
}