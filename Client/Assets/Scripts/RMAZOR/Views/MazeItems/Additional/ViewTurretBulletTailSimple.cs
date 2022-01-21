using System;
using System.Collections.Generic;
using Common.Constants;
using Common.Entities;
using Common.Extensions;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Common;
using RMAZOR.Views.ContainerGetters;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

namespace RMAZOR.Views.MazeItems.Additional
{
    public interface IViewTurretProjectileTail : ICloneable
    {
        void Init();
        void ShowTail(TurretShotEventArgs _Args, Vector2 _ProjectilePosition);
        void HideTail();
        void SetSortingOrder(int _Order);
    }
    
    // FIXME хвост не виден, нужно доработать
    public class ViewTurretProjectileTailSimple : IViewTurretProjectileTail
    {
        #region constants

        private const float MaxTailLength = 1f;
        
        #endregion
        
        #region nonpublic members

        private Triangle m_Tail;
        private Polygon  m_Mask;
        private bool     m_Activated;
        private V2Int    m_From, m_To;
        
        #endregion
        
        #region inject

        private IMazeCoordinateConverter CoordinateConverter { get; }
        private IContainersGetter        ContainersGetter    { get; }
        private IColorProvider           ColorProvider       { get; }

        public ViewTurretProjectileTailSimple(
            IMazeCoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IColorProvider _ColorProvider)
        {
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter = _ContainersGetter;
            ColorProvider = _ColorProvider;
        }
        
        #endregion

        #region api

        public void Init()
        {
            InitShape();
        }

        public void ShowTail(TurretShotEventArgs _Args, Vector2 _ProjectilePosition)
        {
            if (_Args.From != m_From || _Args.To != m_To)
            {
                m_From = _Args.From;
                m_To = _Args.To;
                UpdateMaskShape(_Args.To, _Args.Direction);
            }
            m_Tail.enabled = true;
            var dir = (_Args.To - _Args.From).Normalized;
            var orth = new Vector2(dir.y, dir.x);
            var b = _ProjectilePosition - dir * 0.2f + orth * 0.3f;
            var c = _ProjectilePosition - dir * 0.2f - orth * 0.3f;
            var d = (b + c) * 0.5f;
            var a = Vector2.Distance(_Args.From, d) < MaxTailLength ? _Args.From : d - dir * MaxTailLength;
            m_Tail.A = CoordinateConverter.ToLocalMazeItemPosition(a);
            m_Tail.B = CoordinateConverter.ToLocalMazeItemPosition(b);
            m_Tail.C = CoordinateConverter.ToLocalMazeItemPosition(c);
        }

        public void HideTail()
        {
            m_Tail.enabled = false;
        }

        public void SetSortingOrder(int _Order)
        {
            m_Tail.SortingOrder = _Order;
            m_Mask.SortingOrder = _Order - 1;
        }

        public object Clone()
        {
            return new ViewTurretProjectileTailSimple(CoordinateConverter, ContainersGetter, ColorProvider);
        }

        #endregion

        #region nonpublic methods

        private void InitShape()
        {
            var go = new GameObject("Turret Bullet Tail");
            go.SetParent(ContainersGetter.GetContainer(ContainerNames.MazeItems));
            m_Tail = go.AddComponent<Triangle>();
            m_Tail.Color = ColorProvider.GetColor(ColorIds.MazeItem1).SetA(0.3f);
            m_Tail.Roundness = 0.1f;
            m_Tail.BlendMode = ShapesBlendMode.Transparent;
            m_Tail.ZTest = CompareFunction.Disabled;
            m_Tail.StencilComp = CompareFunction.Greater;
            m_Tail.StencilOpPass = StencilOp.Keep;
            m_Tail.gameObject.transform.SetLocalPosXY(Vector3.zero);
            
            var go2 = new GameObject("Turret Bullet Tail Mask");
            go2.SetParent(ContainersGetter.GetContainer(ContainerNames.MazeItems));
            m_Mask = go2.AddComponent<Polygon>();
            m_Mask.Color = Color.white.SetA(0.1f);
            m_Mask.BlendMode = ShapesBlendMode.Transparent;
            m_Mask.ZTest = CompareFunction.Disabled;
            m_Mask.StencilComp = CompareFunction.Disabled;
            m_Mask.StencilOpPass = StencilOp.Replace;
            m_Mask.gameObject.transform.SetLocalPosXY(Vector3.zero);
            
            m_Tail.enabled = false;
            m_Mask.enabled = false;
        }

        private void UpdateMaskShape(V2Int _To, V2Int _Direction)
        {
            const float width = 3f;
            var orth = new V2Int(_Direction.Y, _Direction.X);
            var a = (Vector2)_To + 0.5f * _Direction + 0.5f * orth;
            var b = (Vector2)_To + 0.5f * _Direction - 0.5f * orth;
            var c = b + width * _Direction;
            var d = a + width * _Direction;
            a = CoordinateConverter.ToLocalMazeItemPosition(a);
            b = CoordinateConverter.ToLocalMazeItemPosition(b);
            c = CoordinateConverter.ToLocalMazeItemPosition(c);
            d = CoordinateConverter.ToLocalMazeItemPosition(d);
            m_Mask.points = new List<Vector2> {a, b, c, d};
        }

        #endregion
    }

    public class ViewTurretProjectileTailFake : IViewTurretProjectileTail
    {
        public object Clone() => new ViewTurretProjectileTailFake();
        public void   Init() { }
        public void   ShowTail(TurretShotEventArgs _Args, Vector2 _ProjectilePosition) { }
        public void   HideTail()                                                       { }
        public void   SetSortingOrder(int _Order)                                      { }
    }
}