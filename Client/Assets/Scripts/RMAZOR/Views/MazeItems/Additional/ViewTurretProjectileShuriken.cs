using System;
using System.Collections.Generic;
using Common;
using Common.Constants;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.SpawnPools;
using Common.Utils;
using RMAZOR.Views.Common;
using RMAZOR.Views.MazeItems.Props;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.MazeItems.Additional
{
    public interface IViewTurretProjectile : ICloneable, IAppear, IInit, IActivated
    {
        event UnityAction<Collider2D> WallCollision;
        IViewTurretProjectileTail     Tail                { get; }
        Transform                     ContainerTransform  { get; }
        Transform                     ProjectileTransform { get; }

        void Init(bool           _Fake);
        void SetSortingOrder(int _Order);
        void SetStencilRefId(int _RefId);
        void Show(bool           _Show);
    }
    
    public class ViewTurretProjectileShuriken : InitBase, IViewTurretProjectile
    {
        #region nonpublic members
        
        private static readonly int StencilRefId = Shader.PropertyToID("_StencilRef");
        
        private GameObject          m_Projectile;
        private ViewMazeItemProps   m_Props;
        private GameObject          m_Turret;
        private SpriteRenderer      m_BorderRenderer;
        private SpriteRenderer      m_MainRenderer;
        private Rigidbody2D         m_Rb;
        private CircleCollider2D    m_Coll;
        private bool                m_Fake;
        private bool                m_Activated;
        private CollisionDetector2D m_CollisionDetector2D;
        
        #endregion

        #region inject

        private ViewSettings                ViewSettings     { get; }
        private IContainersGetter           ContainersGetter { get; }
        private IPrefabSetManager           PrefabSetManager { get; }
        private IColorProvider              ColorProvider    { get; }
        private IRendererAppearTransitioner Transitioner     { get; }

        private ViewTurretProjectileShuriken(
            ViewSettings                _ViewSettings,
            IViewTurretProjectileTail   _Tail,
            IContainersGetter           _ContainersGetter,
            IPrefabSetManager           _PrefabSetManager,
            IColorProvider              _ColorProvider,
            IRendererAppearTransitioner _Transitioner)
        {
            ViewSettings     = _ViewSettings;
            Tail             = _Tail;
            ContainersGetter = _ContainersGetter;
            PrefabSetManager = _PrefabSetManager;
            ColorProvider    = _ColorProvider;
            Transitioner     = _Transitioner;
        }

        #endregion

        #region api

        public bool Activated
        {
            get => m_Activated;
            set
            {
                m_Activated              = value;
                m_MainRenderer.enabled   = value;
                m_BorderRenderer.enabled = value;
                m_Coll.enabled           = value && !m_Fake;
                if (value && !m_Fake) m_Rb.WakeUp();
                else                  m_Rb.Sleep();
            }
        }

        public event UnityAction<Collider2D> WallCollision;
        public IViewTurretProjectileTail     Tail                { get; }
        public EAppearingState               AppearingState      { get; private set; }
        public Transform                     ContainerTransform  => m_Projectile.transform;
        public Transform                     ProjectileTransform => m_MainRenderer.transform;

        public object Clone() => new ViewTurretProjectileShuriken(
            ViewSettings,
            Tail.Clone() as IViewTurretProjectileTail,
            ContainersGetter,
            PrefabSetManager,
            ColorProvider,
            Transitioner);
        
        public override void Init()
        {
            throw new NotSupportedException();
        }
        
        public void Init(bool _Fake)
        {
            if (Initialized) 
                return;
            ColorProvider.ColorChanged += OnColorChanged;
            InitShape(_Fake);
            Tail.Init(m_Projectile);
            base.Init();
        }

        public void Show(bool _Show)
        {
            Activated = _Show;
        }
        
        public void SetSortingOrder(int _Order)
        {
            m_MainRenderer.sortingOrder = _Order;
            m_BorderRenderer.sortingOrder = _Order - 1;
            Tail.SetSortingOrder(_Order);
        }
        
        public void SetStencilRefId(int _RefId)
        {
            m_MainRenderer.sharedMaterial.SetFloat(StencilRefId, _RefId);
            m_BorderRenderer.sharedMaterial.SetFloat(StencilRefId, _RefId);
            Tail.SetStencilRefId(_RefId);
        }

        public void Appear(bool _Appear)
        {
            Cor.Run(Cor.WaitWhile(
                () => !Initialized,
                () =>
                {
                    OnAppearStart(_Appear);
                    Transitioner.DoAppearTransition(
                        _Appear,
                        GetAppearSets(_Appear),
                        ViewSettings.betweenLevelTransitionTime,
                        () => OnAppearFinish(_Appear));
                }));
        }

        #endregion

        #region nonpublic methods
        
        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (!Activated)
                return;
            switch (_ColorId)
            {
                case ColorIds.MazeItem1:  m_MainRenderer.color = _Color;   break;
                case ColorIds.Character2: m_BorderRenderer.color = _Color; break;
            }
        }
        
        private void InitShape(bool _Fake)
        {
            m_Fake = _Fake;
            var projParent = ContainersGetter.GetContainer(ContainerNames.MazeItems);
            m_Projectile =  PrefabSetManager.InitPrefab(
                projParent, "views", "turret_projectile");
            m_Projectile.name                = "Turret Projectile" + (m_Fake ? " Fake" : string.Empty);
            m_Rb                             = m_Projectile.GetCompItem<Rigidbody2D>("rigidbody");
            m_Coll                           = m_Projectile.GetCompItem<CircleCollider2D>("collider");
            m_CollisionDetector2D            = m_Projectile.GetCompItem<CollisionDetector2D>("collider");
            m_MainRenderer                   = m_Projectile.GetCompItem<SpriteRenderer>("projectile");
            m_BorderRenderer                 = m_Projectile.GetCompItem<SpriteRenderer>("projectile_border");
            m_MainRenderer.maskInteraction   = SpriteMaskInteraction.VisibleOutsideMask;
            m_MainRenderer.color             = ColorProvider.GetColor(ColorIds.MazeItem1);
            m_BorderRenderer.color           = ColorProvider.GetColor(ColorIds.Character2);
            m_MainRenderer.maskInteraction   = SpriteMaskInteraction.None;
            m_BorderRenderer.maskInteraction = SpriteMaskInteraction.None;
            m_Coll.gameObject.layer          = LayerMask.NameToLayer("ψ Psi");
            m_CollisionDetector2D.OnTriggerEnter += OnColliderTriggerEnter;
            if (!m_Fake)
                return;
            m_Coll.enabled = false;
            m_CollisionDetector2D.enabled = false;
            m_Rb.Sleep();
        }

        private void OnColliderTriggerEnter(Collider2D _Collider)
        {
            WallCollision?.Invoke(_Collider);
        }

        protected virtual void OnAppearStart(bool _Appear)
        {
            AppearingState = _Appear ? EAppearingState.Appearing : EAppearingState.Dissapearing;
            if (!_Appear) 
                return;
            Show(!m_Fake);
        }

        protected virtual Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var projectileRenderersCol = ColorProvider.GetColor(ColorIds.MazeItem1);
            var projectileRenderersCol2 = ColorProvider.GetColor(ColorIds.Character2);
            if (!_Appear)
                projectileRenderersCol = projectileRenderersCol2 = m_MainRenderer.color;
            return new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {new Component[] {m_MainRenderer},  () => projectileRenderersCol},
                {new Component[] {m_BorderRenderer},() => projectileRenderersCol2},
            };
        }

        protected virtual void OnAppearFinish(bool _Appear)
        {
            if (!_Appear)
            {
                Activated = false;
            }
            AppearingState = _Appear ? EAppearingState.Appeared : EAppearingState.Dissapeared;
        }

        #endregion
    }
}