using Common.Extensions;
using Common.Managers;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Common.BackgroundIdleItems
{
    public interface IViewMazeBackgroundIdleItemDisc : IViewMazeBackgroundIdleItem { }
    
    public class ViewMazeBackgroundIdleItemDisc :
        ViewMazeBackgroundIdleItemBase, 
        IViewMazeBackgroundIdleItemDisc
    {
        #region nonpublic members
        
        private Disc m_Disc;
        private Disc m_Border;

        #endregion

        #region inject
        
        private ViewMazeBackgroundIdleItemDisc(
            IPrefabSetManager          _PrefabSetManager,
            ICoordinateConverter _CoordinateConverter) 
            : base(_PrefabSetManager, _CoordinateConverter) { }

        #endregion

        #region api
        
        public override object Clone()
        {
            return new ViewMazeBackgroundIdleItemDisc(PrefabSetManager, CoordinateConverter);
        }

        public override void Init(Transform _Parent, PhysicsMaterial2D _Material)
        {
            Obj = PrefabSetManager.InitPrefab(_Parent, "background", "idle_item_disc");
            m_Disc = Obj.GetCompItem<Disc>("disc")
                .SetType(DiscType.Disc)
                .SetSortingOrder(SortingOrders.BackgroundItem);
            m_Border = Obj.GetCompItem<Disc>("border")
                .SetType(DiscType.Ring)
                .SetSortingOrder(SortingOrders.BackgroundItem);
            Shapes.Clear();
            Shapes.AddRange(new[] {m_Disc, m_Border});
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
            m_Disc.Color = _Color.SetA(0.2f);
            m_Border.Color = _Color.SetA(0.5f);
        }

        public override void SetParams(float _Scale, float _Thickness)
        {
            Obj.transform.SetLocalScaleXY(Vector2.one * _Scale);
            m_Border.SetThickness(_Thickness);
            Rigidbody.mass = _Scale;
        }

        #endregion
    }
}