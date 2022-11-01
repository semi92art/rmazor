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
using StansAssets.Foundation.Extensions;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.MazeItems.Additional
{
    public interface IViewTurretProjectile : ICloneable, IAppear, IInit, IActivated
    {
        event UnityAction<Collider2D> WallCollision;
        IViewTurretProjectileTail     Tail     { get; }
        Quaternion                    Rotation { get; set; }

        void Init(bool              _Fake);
        void SetSortingOrder(int    _Order);
        void SetStencilRefId(int    _RefId);
        void Show(bool              _Show);
        void SetVelocity(Vector2    _Velocity);
        void SetPosition(Vector2    _Position);
        void SetScale(Vector2       _Scale);
    }
    
    public class ViewTurretProjectileShuriken : InitBase, IViewTurretProjectile
    {
        #region nonpublic members
        
        private static readonly int StencilRefId = Shader.PropertyToID("_StencilRef");
        
        private GameObject          m_ProjectileObj;
        private ViewMazeItemProps   m_Props;
        private GameObject          m_Turret;
        private SpriteRenderer      m_MainRenderer;
        private Rigidbody2D         m_Rb;
        private CircleCollider2D    m_Coll;
        private bool                m_Fake;
        private bool                m_Activated;
        private CollisionDetector2D m_CollisionDetector2D;
        
        #endregion

        #region inject

        private ViewSettings                  ViewSettings     { get; }
        private IContainersGetter             ContainersGetter { get; }
        private IPrefabSetManager             PrefabSetManager { get; }
        private IColorProvider                ColorProvider    { get; }
        private IRendererAppearTransitioner   Transitioner     { get; }
        private IViewTurretProjectileTail     TailReal         { get; }
        private IViewTurretProjectileTailFake TailFake         { get; }

        private ViewTurretProjectileShuriken(
            ViewSettings                _ViewSettings,
            IViewTurretProjectileTail   _TailReal,
            IViewTurretProjectileTailFake _TailFake,
            IContainersGetter           _ContainersGetter,
            IPrefabSetManager           _PrefabSetManager,
            IColorProvider              _ColorProvider,
            IRendererAppearTransitioner _Transitioner)
        {
            ViewSettings     = _ViewSettings;
            TailReal         = _TailReal;
            TailFake         = _TailFake;
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
                m_Activated            = value;
                m_MainRenderer.enabled = value;
                if (m_Fake)
                    return;
                m_Coll.enabled         = value;
                if (value) m_Rb.WakeUp();
                else m_Rb.Sleep();
            }
        }

        public event UnityAction<Collider2D> WallCollision;
        public IViewTurretProjectileTail     Tail           => m_Fake ? TailFake : TailReal;
        public EAppearingState               AppearingState { get; private set; }

        public Quaternion Rotation
        {
            get => m_MainRenderer.transform.localRotation;
            set => m_MainRenderer.transform.localRotation = value;
        }
        
        public object Clone() => new ViewTurretProjectileShuriken(
            ViewSettings,
            Tail.Clone() as IViewTurretProjectileTail,
            TailFake.Clone() as IViewTurretProjectileTailFake,
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
            m_Fake = _Fake;
            InitShape(_Fake);
            if (!_Fake)
                TailReal.Init(m_ProjectileObj.transform, m_MainRenderer.gameObject);
            base.Init();
        }

        public void Show(bool _Show)
        {
            Activated = _Show;
        }

        public void SetVelocity(Vector2 _Velocity)
        {
            m_Rb.velocity = _Velocity;
        }

        public void SetPosition(Vector2 _Position)
        {
            m_ProjectileObj.transform.SetLocalPosXY(_Position);
            m_MainRenderer.transform.SetLocalPosXY(Vector2.zero);
        }

        public void SetScale(Vector2 _Scale)
        {
            m_ProjectileObj.transform.SetLossyScale(_Scale);
        }

        public void SetSortingOrder(int _Order)
        {
            m_MainRenderer.sortingOrder = _Order;
            Tail.SetSortingOrder(_Order);
        }
        
        public void SetStencilRefId(int _RefId)
        {
            m_MainRenderer.sharedMaterial.SetFloat(StencilRefId, _RefId);
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
                case ColorIds.MazeItem1: 
                    m_MainRenderer.color = _Color; 
                    break;
            }
        }
        
        private void InitShape(bool _Fake)
        {
            var projParent = ContainersGetter.GetContainer(ContainerNames.MazeItems);
            m_ProjectileObj =  PrefabSetManager.InitPrefab(
                projParent, "views", "turret_projectile");
            m_ProjectileObj.name                = "Turret Projectile" + (m_Fake ? " Fake" : string.Empty);
            m_Rb                             = m_ProjectileObj.GetCompItem<Rigidbody2D>("rigidbody");
            m_Coll                           = m_ProjectileObj.GetCompItem<CircleCollider2D>("collider");
            m_CollisionDetector2D            = m_ProjectileObj.GetCompItem<CollisionDetector2D>("collider");
            m_MainRenderer                   = m_ProjectileObj.GetCompItem<SpriteRenderer>("projectile");
            var borderRenderer                 = m_ProjectileObj.GetCompItem<SpriteRenderer>("projectile_border");
            borderRenderer.enabled = false;
            m_MainRenderer.maskInteraction   = SpriteMaskInteraction.VisibleOutsideMask;
            m_MainRenderer.color             = ColorProvider.GetColor(ColorIds.MazeItem1);
            m_MainRenderer.maskInteraction   = SpriteMaskInteraction.None;
            m_Coll.gameObject.layer          = LayerMask.NameToLayer(LayerNamesCommon.Psi);
            m_CollisionDetector2D.OnTriggerEnter += OnColliderTriggerEnter;
            if (!(m_Fake = _Fake))
                return;
            m_Rb.DestroySafe();
            m_Coll.DestroySafe();
            m_CollisionDetector2D.DestroySafe();
            // m_Coll.enabled = false;
            // m_CollisionDetector2D.enabled = false;
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
            if (!_Appear)
                projectileRenderersCol = m_MainRenderer.color;
            return new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {new Component[] {m_MainRenderer}, () => projectileRenderersCol},
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