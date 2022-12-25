using System;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Coordinate_Converters;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;
using MathUtils = mazing.common.Runtime.Utils.MathUtils;

namespace RMAZOR.Views.MazeItems.Additional
{
    public interface IViewTurretProjectileTail : ICloneable
    {
        void Init(Transform               _Parent, GameObject _ProjectileObject);
        void ShowTail(TurretShotEventArgs _Args);
        void HideTail();
        void SetSortingOrder(int _Order);
        void SetStencilRefId(int _RefId);
    }
    
    public class ViewTurretProjectileTail : InitBase, IViewTurretProjectileTail, IUpdateTick
    {
        #region nonpublic members

        private List<Line> m_Renderers;
        private Transform  m_TailTr;
        private bool       m_Activated;
        private V2Int      m_From, m_To;
        private bool       m_IsShowing;
        private GameObject m_ProjectileObject;

        #endregion
        
        #region inject

        private IPrefabSetManager    PrefabSetManager    { get; }
        private ICoordinateConverter CoordinateConverter { get; }
        private IContainersGetter    ContainersGetter    { get; }
        private IColorProvider       ColorProvider       { get; }
        private IViewGameTicker      ViewGameTicker      { get; }

        private ViewTurretProjectileTail(
            IPrefabSetManager _PrefabSetManager,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter    _ContainersGetter,
            IColorProvider       _ColorProvider,
            IViewGameTicker      _ViewGameTicker)
        {
            PrefabSetManager = _PrefabSetManager;
            CoordinateConverter = _CoordinateConverter;
            ContainersGetter    = _ContainersGetter;
            ColorProvider       = _ColorProvider;
            ViewGameTicker      = _ViewGameTicker;
        }
        
        #endregion

        #region api
        
        public void Init(Transform _Parent, GameObject _ProjectileObject)
        {
            if (Initialized)
                return;
            ViewGameTicker.Register(this);
            m_ProjectileObject = _ProjectileObject;
            InitShape(_Parent);
            base.Init();
        }

        public void ShowTail(TurretShotEventArgs _Args)
        {
            m_IsShowing = true;
            foreach (var line in m_Renderers)
                line.enabled = true;
            var dir = _Args.Direction.Normalized;
            m_TailTr.localEulerAngles = Vector3.forward * GetAngle(dir);
        }

        public void HideTail()
        {
            m_IsShowing = false;
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
            PrefabSetManager,
            CoordinateConverter,
            ContainersGetter,
            ColorProvider,
            ViewGameTicker);
        
        public void UpdateTick()
        {
            if (m_IsShowing)
                m_TailTr.localPosition = m_ProjectileObject.transform.localPosition;
        }

        #endregion

        #region nonpublic methods

        private void InitShape(Transform _Parent)
        {
            var go = PrefabSetManager.InitPrefab(
                _Parent, "views", "turret_projectile_tail");
            m_TailTr = go.GetCompItem<Transform>("tail");
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
}