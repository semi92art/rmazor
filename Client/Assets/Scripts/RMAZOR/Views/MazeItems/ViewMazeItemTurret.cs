using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Common;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItems.Additional;
using RMAZOR.Views.MazeItems.Props;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;
using UnityEngine.Rendering;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemTurret : IViewMazeItem
    {
        void PreShoot(TurretShotEventArgs _Args);
        void Shoot(TurretShotEventArgs _Args);
        void SetProjectileSortingOrder(int _Order);
    }
    
    public class ViewMazeItemTurret : ViewMazeItemBase, IViewMazeItemTurret, IUpdateTick
    {
        #region constants

        private const float ProjectileContainerRadius = 0.4f;
        
        #endregion
        
        #region nonpublic members

        protected override string         ObjectName => "Turret Block";
        
        private static readonly int StencilRefId = Shader.PropertyToID("_StencilRef");
        private static AudioClipArgs AudioClipArgsShurikenFly =>
            new AudioClipArgs("shuriken", EAudioClipType.GameSound);
        
        private float          m_RotatingSpeed;
        private bool           m_ProjRotating;
        private Transform      m_ProjContainerTr;
        private Transform      m_ProjTr;
        private Transform      m_ProjFakeContainerTr;
        private Disc           m_Body;
        private Disc           m_ProjHolderBorder;
        private SpriteRenderer m_ProjRenderer;
        private SpriteRenderer m_ProjFakeRenderer;
        private Rectangle     m_Mask1, m_Mask2;

        #endregion
        
        #region inject

        private IViewTurretProjectileTail     ProjectileTail       { get; }
        private IViewBackground               Background           { get; }
        private IViewMazeAdditionalBackground AdditionalBackground { get; }

        public ViewMazeItemTurret(
            ViewSettings                  _ViewSettings,
            IModelGame                    _Model,
            IMazeCoordinateConverter      _CoordinateConverter,
            IContainersGetter             _ContainersGetter,
            IViewGameTicker               _GameTicker,
            IViewTurretProjectileTail     _ProjectileTail,
            IViewBackground               _Background,
            IViewBetweenLevelTransitioner _Transitioner,
            IManagersGetter               _Managers,
            IColorProvider                _ColorProvider,
            IViewInputCommandsProceeder   _CommandsProceeder,
            IViewMazeAdditionalBackground _AdditionalBackground)
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
            ProjectileTail       = _ProjectileTail;
            Background           = _Background;
            AdditionalBackground = _AdditionalBackground;
        }
        
        #endregion
        
        #region api
        
        public override Component[] Shapes => new Component[]
        {
            m_Body,
            m_ProjHolderBorder,
            m_ProjRenderer,
            m_ProjFakeRenderer
        };
        
        public override object Clone() => new ViewMazeItemTurret(
            ViewSettings,
            Model,
            CoordinateConverter, 
            ContainersGetter, 
            GameTicker,
            ProjectileTail.Clone() as IViewTurretProjectileTail, 
            Background,
            Transitioner,
            Managers,
            ColorProvider,
            CommandsProceeder,
            AdditionalBackground);

        public override bool ActivatedInSpawnPool
        {
            get => base.ActivatedInSpawnPool;
            set
            {
                m_Mask1.enabled = false;
                m_Mask2.enabled = false;
                base.ActivatedInSpawnPool = value;
            }
        }

        public override void Init(ViewMazeItemProps _Props)
        {
            base.Init(_Props);
            ProjectileTail.Init();
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            if (_Args.Stage == ELevelStage.Finished && _Args.PreviousStage != ELevelStage.Paused)
                StopProceedingTurret();
        }

        public void PreShoot(TurretShotEventArgs _Args)
        {
            Cor.Run(HandleTurretPreShootCoroutine());
        }

        public void Shoot(TurretShotEventArgs _Args)
        {
            Cor.Run(HandleTurretShootCoroutine(_Args));
        }

        public void SetProjectileSortingOrder(int _Order)
        {
            m_ProjRenderer.sortingOrder = _Order;
            ProjectileTail.SetSortingOrder(_Order);
        }

        public void UpdateTick()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return;
            if (AppearingState == EAppearingState.Dissapeared)
                return;
            m_ProjHolderBorder.DashOffset = MathUtils.ClampInverse(
                m_ProjHolderBorder.DashOffset += 2f * GameTicker.DeltaTime,
                0f, 10f);
            if (AppearingState == EAppearingState.Appearing)
                return;
            if (!m_ProjRotating)
                m_ProjTr.localEulerAngles = m_ProjFakeContainerTr.localEulerAngles;
            else
                m_ProjTr.Rotate(Vector3.forward * m_RotatingSpeed * GameTicker.DeltaTime);
        }

        #endregion
        
        #region nonpublic methods

        protected override void InitShape()
        {
            var body          = Object.gameObject.AddComponentOnNewChild<Disc>("Turret", out _);
            body.Type         = DiscType.Arc;
            body.ArcEndCaps   = ArcEndCap.Round;
            body.Color        = ColorProvider.GetColor(ColorIds.Main);
            body.SortingOrder = SortingOrders.GetBlockSortingOrder(Props.Type);
            var bhb           = Object.gameObject.AddComponentOnNewChild<Disc>("Border", out _);
            bhb.Dashed        = true;
            bhb.DashType      = DashType.Rounded;
            bhb.Color         = ColorProvider.GetColor(ColorIds.MazeItem1);
            bhb.Type          = DiscType.Ring;
            bhb.SortingOrder  = SortingOrders.GetBlockSortingOrder(Props.Type) + 1;
            bhb.DashSize      = 2f;
            var projParent = ContainersGetter.GetContainer(ContainerNames.MazeItems);
            GameObject InitProjectile(string _GameObjectName)
            {
                var go =  Managers.PrefabSetManager.InitPrefab(
                    projParent, "views", "turret_projectile");
                go.name = _GameObjectName;
                return go;
            }
            var projGo     = InitProjectile("Turret Projectile");
            m_ProjContainerTr = projGo.transform;
            m_ProjRenderer = projGo.GetCompItem<SpriteRenderer>("projectile");
            m_ProjTr = m_ProjRenderer.transform;
            
            var projFakeGo = InitProjectile("Turret Projectile Fake");
            m_ProjFakeContainerTr = projFakeGo.transform;
            m_ProjFakeRenderer = projFakeGo.GetCompItem<SpriteRenderer>("projectile");
            
            m_ProjRenderer.color = m_ProjFakeRenderer.color = ColorProvider.GetColor(ColorIds.MazeItem1);
            m_ProjRenderer.maskInteraction = m_ProjFakeRenderer.maskInteraction = SpriteMaskInteraction.None;

            var mask1 = projParent.gameObject.AddComponentOnNewChild<Rectangle>("Mask 1", out GameObject _);
            var mask2 = projParent.gameObject.AddComponentOnNewChild<Rectangle>("Mask 2", out GameObject _);
            
            mask1.BlendMode     = mask2.BlendMode     = ShapesBlendMode.Subtractive;
            mask1.RenderQueue   = mask2.RenderQueue   = -1;
            mask1.SortingOrder  = mask2.SortingOrder  = SortingOrders.AdditionalBackgroundPolygon;
            mask1.ZTest         = mask2.ZTest         = CompareFunction.Less;
            mask1.StencilComp   = mask2.StencilComp   = CompareFunction.Greater;
            mask1.StencilOpPass = mask2.StencilOpPass = StencilOp.Replace;
            mask1.enabled       = mask2.enabled       = false;
            mask1.Color         = mask2.Color         = new Color(0f, 0f, 0f, 100f / 255f);
            mask1.transform.SetPosZ(-0.1f);
            mask2.transform.SetPosZ(-0.1f);
            AdditionalBackground.GroupsCollected += SetStencilRefValues;
            (m_Body, m_ProjHolderBorder, m_Mask1, m_Mask2) = (body, bhb, mask1, mask2);
        }

        protected override void UpdateShape()
        {
            float scale = CoordinateConverter.Scale;
            m_Body.Radius = CoordinateConverter.Scale * 0.5f;
            m_Body.Thickness = ViewSettings.LineWidth * scale;
            var projectileScale = Vector3.one * scale * ProjectileContainerRadius * 0.9f;
            m_ProjContainerTr.localScale = m_ProjFakeContainerTr.localScale = projectileScale;
            var pos = CoordinateConverter.ToLocalMazeItemPosition(Props.Position);
            m_ProjContainerTr.SetLocalPosXY(pos);
            m_ProjFakeContainerTr.SetLocalPosXY(pos);
            m_Mask1.Width = m_Mask1.Height = scale;
            m_Mask2.Width = m_Mask2.Height = scale;
            m_ProjHolderBorder.Radius = scale * ProjectileContainerRadius * 0.9f;
            m_ProjHolderBorder.Thickness = ViewSettings.LineWidth * scale * 0.5f;
        }

        private void SetStencilRefValues(List<PointsGroupArgs> _Groups)
        {
            int GetGroupIndexByPoint()
            {
                if (_Groups == null)
                    return -2;
                foreach (var group in _Groups)
                {
                    if (group.Points.Contains(Props.Position))
                        return group.GroupIndex;
                }
                return -1;
            }
            
            int stencilRef = GetGroupIndexByPoint();
            Dbg.Log("stencilRef: " + stencilRef);
            if (stencilRef < 0)
                return;
            m_Mask1.StencilRefID = m_Mask2.StencilRefID = Convert.ToByte(stencilRef + 1);
            m_ProjFakeRenderer.sharedMaterial.SetFloat(StencilRefId, stencilRef);
            m_ProjRenderer.sharedMaterial.SetFloat(StencilRefId, stencilRef);
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.MazeItem1)
            {
                m_ProjHolderBorder.Color = _Color;
                m_ProjRenderer.color = _Color;
                m_ProjFakeRenderer.color = _Color;
            }
            else if (_ColorId == ColorIds.Main)
            {
                m_Body.Color = _Color;
            }
        }

        private IEnumerator HandleTurretPreShootCoroutine()
        {
            Cor.Run(IncreaseRotatingSpeed(0.1f));
            yield return ActivateRealProjectileAndOpenBarrel(0.2f);
        }
        
        private IEnumerator HandleTurretShootCoroutine(TurretShotEventArgs _Args)
        {
            Vector2 toPos = _Args.To;
            EnableProjectileMasksAndSetPositions(
                _Args.Direction + toPos, 
                toPos + _Args.Direction + 0.9f * _Args.Direction);
            Cor.Run(AnimateFakeProjectileAndCloseBarrel(0.2f));
            yield return DoShoot(_Args);
        }

        private IEnumerator IncreaseRotatingSpeed(float _Duration)
        {
            yield return Cor.Lerp(
                0f, 
                1f,
                _Duration, 
                _Progress => m_RotatingSpeed = ViewSettings.TurretProjectileRotationSpeed * _Progress,
                GameTicker);
        }

        private IEnumerator ActivateRealProjectileAndOpenBarrel(float _Delay)
        {
            yield return Cor.Delay(_Delay,
                () =>
            {
                m_ProjTr.SetGoActive(true);
                m_ProjFakeContainerTr.SetGoActive(false);
                var projectilePos = CoordinateConverter.ToLocalMazeItemPosition(Props.Position);
                m_ProjContainerTr.SetLocalPosXY(projectilePos);
                Cor.Run(HighlightBarrel(true));
                Cor.Run(OpenBarrel(true, false));
            });
        }

        private IEnumerator AnimateFakeProjectileAndCloseBarrel(float _Delay)
        {
            yield return Cor.Delay(
                _Delay,
                () =>
                {
                    Cor.Run(AnimateFakeProjectileBeforeShoot());
                    Cor.Run(HighlightBarrel(false));
                    Cor.Run(OpenBarrel(false, false));
                });
        }

        private void EnableProjectileMasksAndSetPositions(Vector2 _Mask1Pos, Vector2 _Mask2Pos)
        {
            m_Mask1.enabled = m_Mask2.enabled = true;
            m_Mask1.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(_Mask1Pos)).SetPosZ(-0.1f);
            m_Mask2.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(_Mask2Pos)).SetPosZ(-0.1f);
        }
        
        private IEnumerator AnimateFakeProjectileBeforeShoot()
        {
            m_ProjFakeContainerTr.localScale = Vector3.zero;
            m_ProjFakeContainerTr.SetGoActive(true);
            m_ProjFakeContainerTr.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            yield return Cor.Lerp(
                0f, 
                1f,
                0.2f,
                _Progress => m_ProjFakeContainerTr.localScale =
                    Vector3.one * _Progress * CoordinateConverter.Scale * ProjectileContainerRadius * 0.9f,
                GameTicker);
        }
        
        private IEnumerator OpenBarrel(bool _Open, bool _Instantly, bool _Forced = false)
        {
            float openedStart, openedEnd, closedStart, closedEnd;
            (openedStart, openedEnd) = GetBarrelDiscAngles(true);
            (closedStart, closedEnd) = GetBarrelDiscAngles(false);
            float startFrom = _Open  ? closedStart : openedStart;
            float startTo   = !_Open ? closedStart : openedStart;
            float endFrom   = _Open  ? closedEnd   : openedEnd;
            float endTo     = !_Open ? closedEnd   : openedEnd;
            if (_Open)
                m_ProjRotating = true;
            if (_Instantly && (_Forced || Model.LevelStaging.LevelStage != ELevelStage.Finished))
            {
                m_Body.AngRadiansStart = startTo;
                m_Body.AngRadiansEnd = endTo;
                yield break;
            }
            yield return Cor.Lerp(
                0f,
                1f,
                0.1f,
                _Progress =>
                {
                    m_Body.AngRadiansStart = Mathf.Lerp(startFrom, startTo, _Progress);
                    m_Body.AngRadiansEnd = Mathf.Lerp(endFrom, endTo, _Progress);
                },
                GameTicker,
                _BreakPredicate: () =>
                {
                    if (_Forced)
                        return false;
                    return Model.LevelStaging.LevelStage == ELevelStage.Finished;
                });
        }

        private IEnumerator HighlightBarrel(bool _Open, bool _Instantly = false, bool _Forced = false)
        {
            Color DefCol() => ColorProvider.GetColor(ColorIds.Main);
            var highlightCol = ColorProvider.GetColor(ColorIds.MazeItem1);
            Color StartCol() => _Open ? DefCol() : highlightCol;
            Color EndCol() => !_Open ? DefCol() : highlightCol;
            if (_Instantly && (_Forced || Model.LevelStaging.LevelStage != ELevelStage.Finished))
            {
                m_Body.Color = EndCol();
                yield break;
            }
            yield return Cor.Lerp(
                0f,
                1f,
                0.1f,
                _P =>
                {
                    m_Body.Color = Color.Lerp(StartCol(), EndCol(), _P);
                },
                GameTicker,
                _BreakPredicate: () => AppearingState != EAppearingState.Appeared
                                       || Model.LevelStaging.LevelStage == ELevelStage.Finished);
        }

        private IEnumerator DoShoot(TurretShotEventArgs _Args)
        {
            Managers.AudioManager.PlayClip(AudioClipArgsShurikenFly);
            Vector2 projectilePos = _Args.From;
            var projectilePosPrev = projectilePos;
            V2Int point;
            bool movedToTheEnd = false;
            m_ProjRotating = true;
            m_RotatingSpeed = ViewSettings.TurretProjectileRotationSpeed;
            var fullPath = RazorMazeUtils.GetFullPath(_Args.From, _Args.To);

            bool Predicate()
            {
                return ProjectileMovingPredicate(
                    fullPath, 
                    projectilePos,
                    projectilePosPrev,
                    ref movedToTheEnd);
            }
            yield return Cor.DoWhile(
                Predicate,
                () =>
                {
                    projectilePosPrev = projectilePos;
                    projectilePos += (Vector2)_Args.Direction * Model.Settings.TurretProjectileSpeed * GameTicker.FixedDeltaTime;
                    m_ProjContainerTr.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(projectilePos));
                    point = V2Int.Round(projectilePos);
                    ProjectileTail.ShowTail(_Args, projectilePos);
                    if (point == _Args.To + 2 * _Args.Direction)
                        movedToTheEnd = true;
                }, () =>
                {
                    m_ProjRotating = false;
                    m_ProjTr.SetGoActive(false);
                    ProjectileTail.HideTail();
                },
                GameTicker,
                _FixedUpdate: true);
        }

        private bool ProjectileMovingPredicate(
            ICollection<V2Int> _ProjectilePath,
            Vector2 _ProjectilePosition,
            Vector2 _PrevProjectilePosition,
            ref bool _MovedToTheEnd)
        {
            var point = (V2Int)_ProjectilePosition;
            if (Model.Character.IsMoving)
            {
                if (_ProjectilePath.Contains(point) 
                    && CheckForDeathWhileCharacterMoving(
                        _PrevProjectilePosition,
                        _ProjectilePosition,
                        Model.Character.MovingInfo.PreviousPrecisePosition,
                        Model.Character.MovingInfo.PrecisePosition))
                {
                    return false;
                }
            }
            else
            {
                if (_ProjectilePath.Contains(point) 
                    && CheckForCharacterDeathWhileCharacterNotMoving(
                        Model.Character.Position,
                        _ProjectilePosition))
                {
                    return false;
                }
            }
            return !_MovedToTheEnd;
        }

        private void StopProceedingTurret()
        {
            Cor.Run(Cor.WaitEndOfFrame(
                () =>
                {
                    Cor.Run(OpenBarrel(false, true, true));
                    Cor.Run(HighlightBarrel(false, true, true));
                }));
            m_ProjRotating = false;
        }

        private Tuple<float, float> GetBarrelDiscAngles(bool _Opened)
        {
            var dir = Props.Directions.First();
            float angleStart = 0f;
            float angleEnd = 0f;
            if (dir == V2Int.Left)
            {
                angleStart = _Opened ? -135f : -160f;
                angleEnd = _Opened ? 135f : 160f;
            }
            else if (dir == V2Int.Right)
            {
                angleStart = _Opened ? 45f : 20f;
                angleEnd = _Opened ? 315f : 340f;
            }
            else if (dir == V2Int.Up)
            {
                angleStart = _Opened ? -225f : -250f;
                angleEnd = _Opened ? 45f : 70f;
            }
            else if (dir == V2Int.Down)
            {
                angleStart = _Opened ? -45f : -70f;
                angleEnd = _Opened ? 225f : 250f;
            }
            return new Tuple<float, float>(angleStart * Mathf.Deg2Rad, angleEnd * Mathf.Deg2Rad);
        }

        protected override void OnAppearStart(bool _Appear)
        {
            if (_Appear)
            {
                m_ProjTr.SetGoActive(true);
                m_Mask1.enabled = true;
                m_Mask2.enabled = true;
                m_ProjRotating = false;
                m_ProjFakeContainerTr.SetGoActive(false);
                Cor.Run(AnimateFakeProjectileBeforeShoot());
                Cor.Run(OpenBarrel(false, true));
            }
            base.OnAppearStart(_Appear);
        }

        protected override void OnAppearFinish(bool _Appear)
        {
            if (!_Appear)
            {
                m_Mask1.enabled = false;
                m_Mask2.enabled = false;
                m_ProjRotating = false;
            }
            base.OnAppearFinish(_Appear);
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var projectileRenderers = new Component[] {m_ProjRenderer, m_ProjFakeRenderer};
            var projectileRenderersCol = _Appear ? ColorProvider.GetColor(ColorIds.MazeItem1) : m_ProjFakeRenderer.color;
            return new Dictionary<IEnumerable<Component>, Func<Color>>
            {
                {new [] {m_ProjHolderBorder}, () => ColorProvider.GetColor(ColorIds.MazeItem1)},
                {new [] {m_Body}, () => ColorProvider.GetColor(ColorIds.Main)},
                {projectileRenderers, () => projectileRenderersCol}
            };
        }

        private bool CheckForCharacterDeathWhileCharacterNotMoving(Vector2 _CharacterPos, Vector2 _ProjectilePos)
        {
            if (!IsDeathPossible())
                return false;
            bool result = Vector2.Distance(_CharacterPos, _ProjectilePos) + MathUtils.Epsilon < 0.9f;
            if (result)
                CommandsProceeder.RaiseCommand(EInputCommand.KillCharacter, null);
            return result;
        }

        private bool CheckForDeathWhileCharacterMoving(
            Vector2 _ProjectileStart,
            Vector2 _ProjectileEnd,
            Vector2 _CharacterStart,
            Vector2 _CharacterEnd)
        {
            if (!IsDeathPossible())
                return false;
            var intersection = MathUtils.LineSegementsIntersect(
                _ProjectileStart, 
                _ProjectileEnd,
                _CharacterStart,
                _CharacterEnd);
            if (intersection.HasValue)
            {
                CommandsProceeder.RaiseCommand(EInputCommand.KillCharacter, 
                    new object[] { CoordinateConverter.ToLocalMazeItemPosition(intersection.Value) });
                return true;
            }
            var projAveragePos = (_ProjectileStart + _ProjectileEnd) * 0.5f;
            var charAveragePos = (_CharacterStart + _CharacterEnd) * 0.5f;
            if (!(Vector2.Distance(projAveragePos, charAveragePos) + MathUtils.Epsilon < 0.9f)) 
                return false;
            CommandsProceeder.RaiseCommand(EInputCommand.KillCharacter, 
                new object[] { CoordinateConverter.ToLocalMazeItemPosition(charAveragePos) });
            return true;
        }

        private bool IsDeathPossible()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return false;
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return false;
            if (!Model.Character.Alive)
                return false;
            if (Model.LevelStaging.LevelStage == ELevelStage.Finished)
                return false;
            return !Model.PathItemsProceeder.AllPathsProceeded;
        }

        #endregion
    }
}