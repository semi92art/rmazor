using Common.Extensions;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Common.BackgroundIdleItems
{
    public interface IViewMazeBackgroundIdleItemDisc : IViewMazeBackgroundIdleItem
    {
        void SetParams(float _Radius, float _Thickness);
    }
    
    public class ViewMazeBackgroundIdleItemDisc : ViewMazeBackgroundIdleItemBase, IViewMazeBackgroundIdleItemDisc
    {
        private Disc             m_InnerDisc;
        private Disc             m_OuterDisc;
        private CircleCollider2D m_CircleCollider;
        
        public override object Clone()
        {
            return new ViewMazeBackgroundIdleItemDisc();
        }

        public override void Init(Transform _Parent, PhysicsMaterial2D _Material)
        {
            Obj = new GameObject("Background Idle Item");
            Obj.transform.SetParent(_Parent);
            m_InnerDisc = Obj.AddComponentOnNewChild<Disc>("Background Idle Item", out _)
                .SetType(DiscType.Disc)
                .SetSortingOrder(SortingOrders.BackgroundItem);
            m_OuterDisc = Obj.AddComponentOnNewChild<Disc>("Outer Disc", out _)
                .SetType(DiscType.Ring)
                .SetSortingOrder(SortingOrders.BackgroundItem);
            Shapes.Clear();
            Shapes.AddRange(new[] {m_InnerDisc, m_OuterDisc});
            var rb = Obj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.angularDrag = 0f;
            rb.sharedMaterial = _Material;
            Rigidbody = rb;
            var coll = Obj.AddComponent<CircleCollider2D>();
            coll.sharedMaterial = _Material;
            m_CircleCollider = coll;
        }

        public override void SetColor(Color _Color)
        {
            m_InnerDisc.Color = _Color.SetA(0.2f);
            m_OuterDisc.Color = _Color.SetA(0.5f);
        }

        public void SetParams(float _Radius, float _Thickness)
        {
            m_InnerDisc.SetRadius(_Radius);
            m_OuterDisc.SetRadius(_Radius).SetThickness(_Thickness);
            Rigidbody.mass = _Radius;
            m_CircleCollider.radius = _Radius;
        }
    }
}