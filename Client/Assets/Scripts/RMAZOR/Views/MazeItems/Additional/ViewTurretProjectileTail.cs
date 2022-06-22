using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using RMAZOR.Models.ItemProceeders;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;
using MathUtils = Common.Utils.MathUtils;

namespace RMAZOR.Views.MazeItems.Additional
{
    public interface IViewTurretProjectileTail : ICloneable
    {
        IEnumerable<Component> Renderers { get; }
        void                   Init(GameObject              _Projectile);
        void                   ShowTail(TurretShotEventArgs _Args, Vector2 _ProjectilePosition);
        void                   HideTail();
        void                   SetSortingOrder(int _Order);
        void                   SetStencilRefId(int _RefId);
    }
    
    public class ViewTurretProjectileTail : InitBase, IViewTurretProjectileTail
    {
        #region nonpublic members

        private List<Line> m_Renderers;
        private Transform  m_TailTr;
        private bool       m_Activated;
        private V2Int      m_From, m_To;
        
        #endregion
        
        #region inject

        private IMazeCoordinateConverter CoordinateConverter { get; }
        private IContainersGetter        ContainersGetter    { get; }
        private IColorProvider           ColorProvider       { get; }

        private ViewTurretProjectileTail(
            IMazeCoordinateConverter _CoordinateConverter,
            IContainersGetter        _ContainersGetter,
            IColorProvider           _ColorProvider)
        {
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter    = _ContainersGetter;
            ColorProvider       = _ColorProvider;
        }
        
        #endregion

        #region api

        public IEnumerable<Component> Renderers => m_Renderers;

        public void Init(GameObject _Projectile)
        {
            if (Initialized)
                return;
            InitShape(_Projectile);
            base.Init();
        }

        public void ShowTail(TurretShotEventArgs _Args, Vector2 _ProjectilePosition)
        {
            if (_Args.From == _ProjectilePosition)
            {
                foreach (var line in m_Renderers)
                    line.enabled = true;
            }
            var dir = _Args.Direction.Normalized;
            m_TailTr.localEulerAngles = Vector3.forward * GetAngle(dir);
        }

        public void HideTail()
        {
            foreach (var line in m_Renderers)
                line.enabled = false;
        }

        public void SetSortingOrder(int _Order)
        {
            foreach (var line in m_Renderers)
                line.SetSortingOrder(_Order);
        }

        public void SetStencilRefId(int _RefId)
        {
            foreach (var line in m_Renderers)
                line.SetStencilRefId(Convert.ToByte(_RefId));
        }

        public object Clone() => new ViewTurretProjectileTail(
            CoordinateConverter,
            ContainersGetter,
            ColorProvider);

        #endregion

        #region nonpublic methods

        private void InitShape(GameObject _Projectile)
        {
            m_TailTr = _Projectile.GetCompItem<Transform>("tail");
            m_Renderers = m_TailTr.GetComponentsInChildren<Line>().ToList();
            foreach (var line in m_Renderers)
                line.SetStencilComp(CompareFunction.Equal);
        }

        private static float GetAngle(Vector2 _Direction)
        {
            if (Mathf.Abs(_Direction.x) < MathUtils.Epsilon)
                return _Direction.y > 0 ? 90f : 270f;
            return _Direction.x > 0 ? 0f : 180f;
        }

        #endregion
    }

    public class ViewTurretProjectileTailFake : IViewTurretProjectileTail
    {
        public IEnumerable<Component> Renderers => new Component[] { };
        public object                 Clone()  => new ViewTurretProjectileTailFake();
        
        public void Init(GameObject              _Projectile)                        { }
        public void ShowTail(TurretShotEventArgs _Args, Vector2 _ProjectilePosition) { }
        public void HideTail()                  { }
        public void SetSortingOrder(int _Order) { }
        public void SetStencilRefId(int _RefId) { }
    }
}