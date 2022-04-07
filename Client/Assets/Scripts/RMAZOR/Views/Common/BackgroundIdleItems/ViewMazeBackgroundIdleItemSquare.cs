using Common.Extensions;
using Common.Managers;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Common.BackgroundIdleItems
{
    public interface IViewMazeBackgroundIdleItemSquare : IViewMazeBackgroundIdleItem { }
    
    public class ViewMazeBackgroundIdleItemSquare : ViewMazeBackgroundIdleItemBase, IViewMazeBackgroundIdleItemSquare
    {
        private Rectangle     m_Rectangle;
        private Rectangle     m_Border;

        public ViewMazeBackgroundIdleItemSquare(IPrefabSetManager _PrefabSetManager)
            : base(_PrefabSetManager) { }
        
        public override object Clone()
        {
            return new ViewMazeBackgroundIdleItemSquare(PrefabSetManager);
        }

        public override void Init(Transform  _Parent, PhysicsMaterial2D _Material)
        {
            Obj =PrefabSetManager.InitPrefab(_Parent, "background", "idle_item_square");
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
            var coll = Obj.GetCompItem<Collider2D>("collider");
            coll.sharedMaterial = _Material;
        }

        public override void SetColor(Color _Color)
        {
            m_Rectangle.Color = _Color.SetA(0.2f);
            m_Border.Color = _Color.SetA(0.5f);
        }

        public override void SetParams(float _Scale, float _Thickness)
        {
            Obj.transform.SetLocalScaleXY(Vector2.one * _Scale);
            m_Border.SetThickness(0.5f * _Thickness);
            Rigidbody.mass = _Scale * _Scale;
            Obj.transform.rotation = Quaternion.Euler(0f, 0f, Random.value * 360f);
        }
    }
}