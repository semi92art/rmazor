using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using RMAZOR.Views.Common;
using RMAZOR.Views.MazeItems.Props;
using UnityEngine;

namespace RMAZOR.Views.MazeItems.Additional
{
    public interface IViewTurretProjectile : ICloneable
    {
        IViewTurretProjectileTail Tail         { get; }
        IEnumerable<Component>    Renderers    { get; }
        Transform                 Transform    { get; }
        SpriteRenderer            MainRenderer { get; }
        void                      SetSortingOrder(int _Order);
        void                      SetStencilRefId(int _RefId);
        void                      Init();
    }
    
    public class ViewTurretProjectileShuriken : IViewTurretProjectile
    {
        #region nonpublic members
        
        private static readonly int StencilRefId = Shader.PropertyToID("_StencilRef");
        
        private GameObject        m_Projectile;
        private ViewMazeItemProps m_Props;
        private GameObject        m_Turret;
        
        #endregion

        #region inject
        
        private IContainersGetter ContainersGetter { get; }
        private IPrefabSetManager PrefabSetManager { get; }
        private IColorProvider    ColorProvider    { get; }

        private ViewTurretProjectileShuriken(
            IViewTurretProjectileTail _Tail,
            IContainersGetter         _ContainersGetter,
            IPrefabSetManager         _PrefabSetManager,
            IColorProvider            _ColorProvider)
        {
            Tail             = _Tail;
            ContainersGetter = _ContainersGetter;
            PrefabSetManager = _PrefabSetManager;
            ColorProvider    = _ColorProvider;
        }

        #endregion

        #region api
        
        public IViewTurretProjectileTail Tail         { get; }
        public IEnumerable<Component>    Renderers    => new Component[] {MainRenderer}.Concat(Tail.Renderers);
        public Transform                 Transform    => m_Projectile.transform;
        public SpriteRenderer            MainRenderer { get; private set; }

        public object Clone() => new ViewTurretProjectileShuriken(
            Tail.Clone() as IViewTurretProjectileTail,
            ContainersGetter,
            PrefabSetManager,
            ColorProvider);
        
        public void Init()
        {
            InitShape();
            Tail.Init(m_Projectile);
        }

        public void SetSortingOrder(int _Order)
        {
            MainRenderer.sortingOrder = _Order;
            Tail.SetSortingOrder(_Order);
        }
        
        public void SetStencilRefId(int _RefId)
        {
            MainRenderer.sharedMaterial.SetFloat(StencilRefId, _RefId);
            Tail.SetStencilRefId(_RefId);
        }


        #endregion

        #region nonpublic methods
        
        private void InitShape()
        {
            var projParent = ContainersGetter.GetContainer(ContainerNames.MazeItems);
            m_Projectile =  PrefabSetManager.InitPrefab(
                projParent, "views", "turret_projectile");
            m_Projectile.name = "Turret Projectile";
            MainRenderer = m_Projectile.GetCompItem<SpriteRenderer>("projectile");
            MainRenderer.color = ColorProvider.GetColor(ColorIds.MazeItem1);
            MainRenderer.maskInteraction = SpriteMaskInteraction.None;
        }

        #endregion
    }
}