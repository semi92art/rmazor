﻿using Common.Extensions;
using Common.Managers;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Common.BackgroundIdleItems
{
    public interface IViewMazeBackgroundIdleItemTriangle : IViewMazeBackgroundIdleItem { }
    
    public class ViewMazeBackgroundIdleItemTriangle : 
        ViewMazeBackgroundIdleItemBase, 
        IViewMazeBackgroundIdleItemTriangle
    {
        private Triangle          m_Triangle;
        private Triangle          m_Border;

        public ViewMazeBackgroundIdleItemTriangle(IPrefabSetManager _PrefabSetManager)
            : base(_PrefabSetManager) { }
        
        public override object Clone()
        {
            return new ViewMazeBackgroundIdleItemTriangle(PrefabSetManager);
        }

        public override void Init(Transform  _Parent, PhysicsMaterial2D _Material)
        {
            Obj =PrefabSetManager.InitPrefab(_Parent, "background", "idle_item_triangle");
            m_Triangle = Obj.GetCompItem<Triangle>("triangle")
                .SetSortingOrder(SortingOrders.BackgroundItem);
            m_Triangle.Border = false;
            m_Border = Obj.GetCompItem<Triangle>("border")
                .SetSortingOrder(SortingOrders.BackgroundItem);
            m_Border.Border = true;
            Shapes.Clear();
            Shapes.AddRange(new[] {m_Triangle, m_Border});
            var rb = Obj.GetCompItem<Rigidbody2D>("rigidbody");
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.None;
            rb.angularDrag = 0f;
            rb.sharedMaterial = _Material;
            Rigidbody = rb;
            var coll = Obj.GetCompItem<Collider2D>("collider");
            coll.sharedMaterial = _Material;
        }

        public override void SetColor(Color  _Color)
        {
            m_Triangle.Color = _Color.SetA(0.2f);
            m_Border.Color = _Color.SetA(0.5f);
        }

        public override void SetParams(float _Scale, float _Thickness)
        {
            Obj.transform.SetLocalScaleXY(Vector2.one * _Scale);
            m_Border.Thickness = _Thickness * 0.5f;
            Rigidbody.mass = GetTriangleArea(m_Triangle.A, m_Triangle.B, m_Triangle.C) * _Scale;
            Obj.transform.rotation = Quaternion.Euler(0f, 0f, Random.value * 360f);
        }

        private static float GetTriangleArea(Vector2 _A, Vector2 _B, Vector2 _C)
        {
            float ab = Vector2.Distance(_A, _B);
            float bc = Vector2.Distance(_B, _C);
            float ca = Vector2.Distance(_C, _A);
            float p = (ab + bc + ca) * 0.5f;
            return Mathf.Sqrt(p * (p - ab) * (p - bc) * (p - ca));
        }
    }
}