using Common.Extensions;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Common.BackgroundIdleItems
{
    public interface IViewMazeBackgroundIdleItemSquare : IViewMazeBackgroundIdleItem
    {
        void SetParams(float _Width, float _Height, float _Thickness);
    }
    
    public class ViewMazeBackgroundIdleItemSquare : ViewMazeBackgroundIdleItemBase, IViewMazeBackgroundIdleItemSquare
    {
        private Rectangle     m_InnerRect;
        private Rectangle     m_OuterRect;
        private BoxCollider2D m_BoxCollider;
        
        public override object Clone()
        {
            return new ViewMazeBackgroundIdleItemSquare();
        }

        public override void Init(Transform  _Parent, PhysicsMaterial2D _Material)
        {
            Obj = new GameObject("Background Idle Item");
            Obj.transform.SetParent(_Parent);
            m_InnerRect = Obj.AddComponentOnNewChild<Rectangle>("Background Idle Item", out _)
                .SetType(Rectangle.RectangleType.RoundedSolid)
                .SetSortingOrder(SortingOrders.BackgroundItem);
            m_OuterRect = Obj.AddComponentOnNewChild<Rectangle>("Outer Disc", out _)
                .SetType(Rectangle.RectangleType.RoundedBorder)
                .SetSortingOrder(SortingOrders.BackgroundItem);
            Shapes.Clear();
            Shapes.AddRange(new[] {m_InnerRect, m_OuterRect});
            var rb = Obj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.angularDrag = 0f;
            rb.sharedMaterial = _Material;
            Rigidbody = rb;
            var coll = Obj.AddComponent<BoxCollider2D>();
            coll.sharedMaterial = _Material;
            m_BoxCollider = coll;
        }

        public override void SetColor(Color  _Color)
        {
            m_InnerRect.Color = _Color.SetA(0.2f);
            m_OuterRect.Color = _Color.SetA(0.5f);
        }

        public void SetParams(float _Width, float _Height, float _Thickness)
        {
            m_InnerRect.SetWidth(_Width).SetHeight(_Height);
            m_OuterRect.SetWidth(_Width).SetHeight(_Height).SetThickness(_Thickness);
            Rigidbody.mass = Mathf.Sqrt(_Width * _Height);
            m_BoxCollider.size = new Vector2(_Width, _Height);
        }
    }
}