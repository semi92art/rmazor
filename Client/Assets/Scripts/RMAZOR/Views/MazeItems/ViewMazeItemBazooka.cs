using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.Entities;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.SpawnPools;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Characters;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemBazooka : IViewMazeItem, IGetViewCharacterInfo
    {
        void OnAllPathProceed(V2Int           _LastPath);
        void OnBazookaAppear(BazookaEventArgs _Args);
        void OnBazookaShot(BazookaEventArgs   _Args);
    }
    
    public class ViewMazeItemBazooka : ViewMazeItemBase, IViewMazeItemBazooka, IUpdateTick, IFixedUpdateTick
    {
        #region constants

        private const float  ScaleCoefficient       = 0.8f;
        private const int    TailItemsCount         = 50;
        private const string TailItemsContainerName = "Bazooka Projectile Container";

        #endregion
        
        #region nonpublic members

        protected override string ObjectName => "Bazooka Block";

        private static readonly int StencilRefId = Shader.PropertyToID("_StencilRef");

        private readonly BehavioursSpawnPool<Disc> m_TailItemsPool = new BehavioursSpawnPool<Disc>();

        private SpriteRenderer m_SpearRend;
        private SpriteRenderer m_SpearBorderRend;
        private Rectangle      m_Mask1, m_Mask2;
        private Disc           m_OuterDisc;
        private Disc           m_OuterDiscBorder;
        private Disc           m_AdditionalOuterSpearMaskTop;
        private Disc           m_AdditionalOuterSpearMaskBottom;
        private Transform      m_ProjectileContainerOnAwait;
        private Transform      m_ProjectileContainerOnFly;
        private Transform      m_ProjectileTailSpawnPoint;
        private Rigidbody2D    m_ProjectileRb;
        private Transform      m_LookAtHelper;
        private bool           m_LookAtCharacter;
        private bool           m_ProjectileIsFlying;
        private bool           m_CanSpawnTailItems;
        
        #endregion

        #region inject
        
        private IPrefabSetManager PrefabSetManager { get; }
        private ICameraProvider   CameraProvider   { get; }

        public ViewMazeItemBazooka(
            ViewSettings                  _ViewSettings,
            IModelGame                    _Model,
            IMazeCoordinateConverter      _CoordinateConverter,
            IContainersGetter             _ContainersGetter,
            IViewGameTicker               _GameTicker,
            IViewBetweenLevelTransitioner _Transitioner,
            IManagersGetter               _Managers,
            IColorProvider                _ColorProvider,
            IViewInputCommandsProceeder   _CommandsProceeder,
            IPrefabSetManager             _PrefabSetManager,
            ICameraProvider               _CameraProvider) 
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers,
                _ColorProvider, 
                _CommandsProceeder)
        {
            PrefabSetManager = _PrefabSetManager;
            CameraProvider   = _CameraProvider;
        }

        #endregion

        #region api

        public System.Func<ViewCharacterInfo> GetViewCharacterInfo { private get; set; }

        public override Component[] Shapes => new Component[0];
        
        public override object Clone() => 
            new ViewMazeItemBazooka(
                ViewSettings,
                Model,
                CoordinateConverter, 
                ContainersGetter,
                GameTicker,
                Transitioner,
                Managers,
                ColorProvider,
                CommandsProceeder,
                PrefabSetManager,
                CameraProvider);

        public void OnAllPathProceed(V2Int _LastPath)
        {
            m_SpearRend.enabled = false;
            m_SpearBorderRend.enabled = false;
            m_Mask1.enabled = false;
            m_Mask2.enabled = false;
        }

        public void OnBazookaAppear(BazookaEventArgs _Args)
        {
            Cor.Run(AppearCoroutine());
        }

        public void OnBazookaShot(BazookaEventArgs _Args)
        {
            ShootProjectile();
        }
        
        public void UpdateTick()
        {
            if (!Model.Character.Alive)
                return;
            if (Model.PathItemsProceeder.AllPathsProceeded)
                return;
            if (Model.LevelStaging.LevelStage == ELevelStage.Finished)
                return;
            if (m_ProjectileIsFlying)
            {
                if (!IsInsideOfScreenBounds(m_ProjectileRb.position))
                    StopProjectile();
            }
            else if (m_LookAtCharacter)
            {
                LookAtCharacter();
            }
        }
        
        public void FixedUpdateTick()
        {
            if (m_CanSpawnTailItems)
                SpawnTailItemsOnFixedUpdate();
        }

        #endregion

        #region nonpublic methods
        
        protected override void InitShape()
        {
            InitBazookaWithProjectile();
            InitTailItemsPool();
        }

        private void InitBazookaWithProjectile()
        {
            const byte stencilRefId = 230;
            var go = PrefabSetManager.InitPrefab(
                Object.transform, "views", "bazooka");
            m_ProjectileContainerOnAwait = go.GetCompItem<Transform>("container");
            int sortingOrder = SortingOrders.GetBlockSortingOrder(Props.Type);
            var collisionDetector = go.GetCompItem<CollisionDetector2D>("collision_detector");
            m_ProjectileRb = go.GetCompItem<Rigidbody2D>("projectile");
            m_LookAtHelper = go.GetCompItem<Transform>("look_at_helper");
            m_ProjectileTailSpawnPoint = go.GetCompItem<Transform>("projectile_tail_spawn_point");
            collisionDetector.OnTriggerEnter += CheckForCharacterDeath;
            collisionDetector.gameObject.layer = LayerMask.NameToLayer("ζ Dzeta");
            m_OuterDisc = go.GetCompItem<Disc>("outer_disc")
                .SetSortingOrder(sortingOrder - 1)
                .SetZTest(CompareFunction.LessEqual)
                .SetStencilComp(CompareFunction.Always)
                .SetStencilOpPass(StencilOp.Replace)
                .SetStencilRefId(stencilRefId);
            m_OuterDiscBorder = go.GetCompItem<Disc>("outer_disc_border")
                .SetSortingOrder(sortingOrder - 2)
                .SetZTest(CompareFunction.LessEqual)
                .SetStencilComp(CompareFunction.Always)
                .SetStencilOpPass(StencilOp.Replace)
                .SetStencilRefId(stencilRefId);
            m_AdditionalOuterSpearMaskTop = go.GetCompItem<Disc>("additional_outer_spear_mask_top")
                .SetSortingOrder(sortingOrder - 2)
                .SetZTest(CompareFunction.LessEqual)
                .SetStencilComp(CompareFunction.Always)
                .SetStencilOpPass(StencilOp.Replace)
                .SetStencilRefId(stencilRefId + 2);
            m_AdditionalOuterSpearMaskBottom = go.GetCompItem<Disc>("additional_outer_spear_mask_bottom")
                .SetSortingOrder(sortingOrder - 2)
                .SetZTest(CompareFunction.LessEqual)
                .SetStencilComp(CompareFunction.Always)
                .SetStencilOpPass(StencilOp.Replace)
                .SetStencilRefId(stencilRefId + 2);
            m_Mask1 = go.GetCompItem<Rectangle>("mask_1")
                .SetSortingOrder(sortingOrder - 1)
                .SetZTest(CompareFunction.LessEqual)
                .SetStencilComp(CompareFunction.Always)
                .SetStencilOpPass(StencilOp.Replace)
                .SetStencilRefId(stencilRefId);
            m_Mask2 = go.GetCompItem<Rectangle>("mask_2")
                .SetSortingOrder(sortingOrder - 1)
                .SetZTest(CompareFunction.LessEqual)
                .SetStencilComp(CompareFunction.Always)
                .SetStencilOpPass(StencilOp.Replace)
                .SetStencilRefId(stencilRefId);
            m_SpearRend = go.GetCompItem<SpriteRenderer>("spear");
            m_SpearRend.sortingOrder = sortingOrder;
            m_SpearRend.material.SetFloat(StencilRefId, stencilRefId);
            m_SpearBorderRend = go.GetCompItem<SpriteRenderer>("spear_border");
            m_SpearBorderRend.sortingOrder = sortingOrder - 1;
            m_SpearBorderRend.material.SetFloat(StencilRefId, stencilRefId);
        }
        
        private void InitTailItemsPool()
        {
            int sortingOrder = SortingOrders.GetBlockSortingOrder(Props.Type);
            m_ProjectileContainerOnFly = ContainersGetter.GetContainer(TailItemsContainerName);
            for (int i = 0; i < TailItemsCount; i++)
            {
                var tailItem = m_ProjectileContainerOnFly.AddComponentOnNewChild<Disc>("Bazooka Tail Item", out _)
                    .SetSortingOrder(sortingOrder);
                m_TailItemsPool.Add(tailItem);
            }
        }

        protected override void UpdateShape()
        {
            Object.transform.SetParent(null);
            float scale = CoordinateConverter.Scale;
            var mazeSize = Model.Data.Info.Size;
            var posRaw = new Vector2(mazeSize.X * 0.5f - 0.5f, mazeSize.Y + 1f);
            var pos = CoordinateConverter.ToGlobalMazeItemPosition(posRaw);
            Object.transform.SetLocalPosXY(pos);
            m_ProjectileRb.transform
                .SetParentEx(m_ProjectileContainerOnAwait)
                .SetLocalPosXY(Vector2.zero)
                .SetLocalScaleXY(scale * ScaleCoefficient * Vector2.one)
                .SetGoActive(false)
                .rotation = Quaternion.Euler(Vector3.forward * 180f);
            m_ProjectileContainerOnAwait.transform.localRotation = Quaternion.Euler(Vector3.zero);
            foreach (var tailItem in m_TailItemsPool)
                tailItem.SetRadius(scale * ScaleCoefficient * 0.1f);
            m_LookAtCharacter = false;
            m_Mask1.transform.SetLocalScaleXY(scale * ScaleCoefficient * Vector2.one);
            m_Mask2.transform.SetLocalScaleXY(scale * ScaleCoefficient * Vector2.one);
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            switch (_ColorId)
            {
                case ColorIds.Main:
                    m_OuterDiscBorder.SetColor(_Color);
                    break;
                case ColorIds.Background1:
                    m_OuterDisc.SetColor(_Color);
                    break;
                case ColorIds.MazeItem1:
                    m_SpearRend.color = _Color;
                    if (m_TailItemsPool.Any())
                        foreach (var tailItem in m_TailItemsPool)
                            tailItem.SetColor(_Color.SetA(tailItem.Color.a));
                    break;
            }
        }

        private void CheckForCharacterDeath(Collider2D _Collider)
        {
            if (!Model.Character.Alive)
                return;
            if (Model.PathItemsProceeder.AllPathsProceeded)
                return;
            if (Model.LevelStaging.LevelStage == ELevelStage.Finished)
                return;
            if (!m_ProjectileIsFlying)
                return;
            var charColls = GetViewCharacterInfo().Colliders;
            for (int i = 0; i < charColls.Length; i++)
            {
                if (_Collider != charColls[i])
                    continue;
                CommandsProceeder.RaiseCommand(EInputCommand.KillCharacter, null);
                break;
            }
        }

        private Vector3 m_CharacterPositionLerped;
        
        private void LookAtCharacter()
        {
            var projTr = m_ProjectileRb.transform;
            var charPos = GetViewCharacterInfo().Transform.position;
            if (m_CharacterPositionLerped == default)
                m_CharacterPositionLerped = new Vector3(projTr.position.x, charPos.y);
            else
            {
                m_CharacterPositionLerped = Vector3.Lerp(m_CharacterPositionLerped, charPos,
                    GameTicker.DeltaTime * ViewSettings.bazookaRotationSpeed);
            }
            m_LookAtHelper.LookAt2D(m_CharacterPositionLerped);
            var correctRotation = Quaternion.Euler(Vector3.forward * (m_LookAtHelper.eulerAngles.z - 90f));
            m_ProjectileRb.transform.rotation = correctRotation;
        }

        private IEnumerator AppearCoroutine()
        {
            float scale = CoordinateConverter.Scale;
            m_Mask1.transform
                .SetParentEx(m_ProjectileRb.transform)
                .SetLocalPosXY(Vector2.up * 0.28f);
            m_Mask2.transform
                .SetParentEx(m_ProjectileRb.transform)
                .SetLocalPosXY(Vector2.up * 0.35f);
            const float duration = 0.5f;
            m_OuterDisc.SetRadius(0f).enabled = true;
            m_OuterDiscBorder.SetThickness(ScaleCoefficient * scale * 0.1f).SetRadius(0f).enabled = true;
            m_AdditionalOuterSpearMaskTop.SetRadius(0f).SetThickness(0f).enabled = true;
            m_AdditionalOuterSpearMaskBottom.SetRadius(0f).SetThickness(0f).enabled = true;
            m_ProjectileRb.SetGoActive(true);
            float finalRadius = ScaleCoefficient * scale * 1.5f;
            m_AdditionalOuterSpearMaskTop.SetRadius(0f).SetThickness(2f * 15f);
            m_AdditionalOuterSpearMaskBottom.SetRadius(0f).SetThickness(2f * 10f);
            yield return Cor.Lerp(
                GameTicker,
                duration,
                _OnProgress: _P =>
                {
                    float radius = finalRadius * _P;
                    m_OuterDisc.SetRadius(radius);
                    m_AdditionalOuterSpearMaskTop.SetRadius(radius).SetThickness(2f * (15f - radius));
                    m_AdditionalOuterSpearMaskBottom.SetRadius(radius).SetThickness(2f * (10f - radius));
                    m_OuterDiscBorder.SetRadius(radius);
                },
                 _OnFinish: () => m_LookAtCharacter = true);
        }
        
        private IEnumerator DisappearCoroutine()
        {
            const float delay = 0.5f;
            const float duration = 0.5f;
            float time = GameTicker.Time;
            while (time + delay > GameTicker.Time)
                yield return new WaitForEndOfFrame();
            yield return Cor.Lerp(
                GameTicker,
                duration,
                _OnProgress: _P =>
                {
                    float radius = ScaleCoefficient * CoordinateConverter.Scale * (1f - _P) * 1.5f;
                    m_OuterDisc.SetRadius(radius);
                    m_OuterDiscBorder.SetRadius(radius);
                },
                _OnFinish: () =>
                {
                    m_OuterDisc.SetRadius(0f).enabled = false;
                    m_OuterDiscBorder.SetRadius(0f).enabled = false;
                    m_AdditionalOuterSpearMaskTop.SetRadius(0f).SetThickness(0f).enabled = false;
                    m_AdditionalOuterSpearMaskBottom.SetRadius(0f).SetThickness(0f).enabled = false;
                });
        }

        private void ShootProjectile()
        {
            m_AdditionalOuterSpearMaskBottom.enabled = false;
            m_Mask1.enabled            = true;
            m_Mask2.enabled            = true;
            m_ProjectileIsFlying      = true;
            m_LookAtCharacter         = false;
            var projTr      = m_ProjectileRb.transform;
            var direction     = (m_CharacterPositionLerped - projTr.position).normalized;
            float scale = CoordinateConverter.Scale;
            var startPos = m_ProjectileRb.position;
            m_Mask1.transform.SetParent(Object.transform);
            m_Mask2.transform.SetParent(Object.transform);
            Cor.Run(Cor.Lerp(
                GameTicker,
                0.5f,
                _OnProgress: _P =>
                {
                    var newPos = startPos - (Vector2)direction * scale * _P;
                    m_ProjectileRb.MovePosition(newPos);
                },
                _BreakPredicate: () => !Model.Character.Alive
                                       || Model.PathItemsProceeder.AllPathsProceeded
                                       || Model.LevelStaging.LevelStage == ELevelStage.Finished 
                                       || !m_ProjectileIsFlying,
                _OnFinishEx: (_Broken, _Progress) =>
                {
                    if (_Broken)
                    {
                        Cor.Run(DisappearCoroutine());
                        return;
                    }
                    projTr.SetParentEx(m_ProjectileContainerOnFly).SetGoActive(true);
                    m_ProjectileRb.transform.SetParent(m_ProjectileContainerOnFly);
                    m_ProjectileRb.AddForce(direction * ViewSettings.bazookaProjectileSpeed);
                    m_CanSpawnTailItems = true;
                    Cor.Run(DisappearCoroutine());
                    Cor.Run(Cor.WaitWhile(() =>
                    {
                        var projCenter = m_SpearRend.bounds.center;
                        return projCenter.y > m_Mask1.transform.position.y;
                    },
                    () =>
                    {
                        m_Mask1.transform
                            .SetParent(m_ProjectileRb.transform);
                    }));
                }));
        }

        private float m_SpawnTailItemsTimer;

        private void SpawnTailItemsOnFixedUpdate()
        {
            bool canSpawn = m_ProjectileTailSpawnPoint.transform.position.y <
                            m_ProjectileContainerOnAwait.transform.position.y;
            if (!canSpawn)
                return;
            
            const float spawnDelta = 0.02f;
            m_SpawnTailItemsTimer += GameTicker.FixedDeltaTime;
            if (m_SpawnTailItemsTimer < spawnDelta)
                return;
            m_SpawnTailItemsTimer -= spawnDelta;
            
            var item = m_TailItemsPool.FirstInactive;
            m_TailItemsPool.Activate(item);
            item.transform.SetPosXY(m_ProjectileTailSpawnPoint.transform.position);
            Cor.Run(Cor.Lerp(
                GameTicker,
                0.5f,
                _OnProgress: _P => item.Color = item.Color.SetA(1f - _P),
                _OnFinish: () => m_TailItemsPool.Deactivate(item)));
            if (!m_ProjectileIsFlying)
                m_CanSpawnTailItems = false;
        }
        
        private void StopProjectile()
        {
            m_ProjectileRb.velocity = Vector2.zero;
            m_ProjectileRb.SetGoActive(false);
            m_ProjectileRb.transform.SetParent(m_ProjectileContainerOnFly);
            m_ProjectileIsFlying = false;
            m_AdditionalOuterSpearMaskBottom.enabled = true;
        }

        protected override Dictionary<IEnumerable<Component>, System.Func<Color>> GetAppearSets(bool _Appear)
        {
            var mainCol = ColorProvider.GetColor(ColorIds.Main);
            var back1Col = ColorProvider.GetColor(ColorIds.Background1);
            var mazeItemCol = ColorProvider.GetColor(ColorIds.MazeItem1);
            return new Dictionary<IEnumerable<Component>, System.Func<Color>>
            {
                {new [] {m_SpearRend}, () => mazeItemCol},
                {m_TailItemsPool, () => mazeItemCol},
                {new [] {m_SpearBorderRend}, () => Color.black},
                {new[] {m_OuterDiscBorder}, () => mainCol},
                {new[] {m_OuterDisc}, () => back1Col}
            };
        }

        protected override void OnAppearFinish(bool _Appear)
        {
            base.OnAppearFinish(_Appear);
            if (!_Appear)
                return;
            m_OuterDisc.enabled = false;
            m_OuterDiscBorder.enabled = false;
            m_AdditionalOuterSpearMaskTop.enabled = false;
            m_AdditionalOuterSpearMaskBottom.enabled = false;
            m_Mask1.enabled = false;
            m_Mask2.enabled = false;
            m_TailItemsPool.DeactivateAll();
        }

        private bool IsInsideOfScreenBounds(Vector2 _Position)
        {
            var screenBds = GraphicUtils.GetVisibleBounds(CameraProvider.MainCamera);
            var padding = Vector2.one * 5f;
            var min = screenBds.min;
            var max = screenBds.max;
            return _Position.x > min.x - padding.x 
                   && _Position.y > min.y - padding.y
                   && _Position.x < max.x + padding.x
                   && _Position.y < max.y + padding.y;
        }

        #endregion
    }
}