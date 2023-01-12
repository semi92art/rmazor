using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Extensions;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Characters;
using RMAZOR.Views.Common;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemHammer : IViewMazeItem, IGetViewCharacterInfo
    {
        void OnHammerShot(HammerShotEventArgs _Args);
    }
    
    public class ViewMazeItemHammer : ViewMazeItemBase, IViewMazeItemHammer
    {
        #region constants

        private const float Shot90DegTime        = 0.1f;
        private const float TailAlpha            = 0.3f;
        private const int   ParticlesThrowerSize = 20;

        #endregion

        #region nonpublic members

        private AudioClipArgs AudioClipInfoHammerShot =>
            new AudioClipArgs(
                "hammer_shot",
                EAudioClipType.GameSound, 
                0.3f, 
                _Id: m_ShotAngle.ToString());
        
        protected override string ObjectName => "Hammer Block";
        
        private List<ShapeRenderer> m_MainShapes;
        private List<ShapeRenderer> m_AdditionalShapes;
        private List<ShapeRenderer> m_AdditionalShapes2;
        private List<ShapeRenderer> m_AdditionalShapes3;
        private Disc                m_Tail;
        private Transform           m_Side1, m_Side2;
        private Collider2D          m_Collider;

        private          Transform           m_HammerContainer;
        private          bool                m_ProceedShots;
        private          int                 m_ShotAngle;
        private          bool                m_Clockwise;
        
        #endregion

        #region inject
        
        private IPrefabSetManager                   PrefabSetManager               { get; }
        private IMazeShaker                         Shaker                         { get; }
        private IViewParticlesThrower               ParticlesThrower               { get; }
        private IViewSwitchLevelStageCommandInvoker SwitchLevelStageCommandInvoker { get; }

        private ViewMazeItemHammer(
            ViewSettings                        _ViewSettings,
            IModelGame                          _Model,
            ICoordinateConverter                _CoordinateConverter,
            IContainersGetter                   _ContainersGetter,
            IViewGameTicker                     _GameTicker,
            IRendererAppearTransitioner         _Transitioner,
            IManagersGetter                     _Managers,
            IColorProvider                      _ColorProvider,
            IViewInputCommandsProceeder         _CommandsProceeder,
            IPrefabSetManager                   _PrefabSetManager,
            IMazeShaker                         _Shaker,
            IViewParticlesThrower               _ParticlesThrower,
            IViewSwitchLevelStageCommandInvoker _SwitchLevelStageCommandInvoker) 
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
            PrefabSetManager               = _PrefabSetManager;
            Shaker                         = _Shaker;
            ParticlesThrower               = _ParticlesThrower;
            SwitchLevelStageCommandInvoker = _SwitchLevelStageCommandInvoker;
        }

        #endregion

        #region api

        public override bool ActivatedInSpawnPool
        {
            get => base.ActivatedInSpawnPool;
            set
            {
                base.ActivatedInSpawnPool = value;
                if (value) 
                    return;
                foreach (var shape in m_MainShapes)
                    shape.enabled = false;
                foreach (var shape in m_AdditionalShapes)
                    shape.enabled = false;
                foreach (var shape in m_AdditionalShapes2)
                    shape.enabled = false;
                foreach (var shape in m_AdditionalShapes3)
                    shape.enabled = false;
                m_Tail.enabled = false;
            }
        }

        public override void Init()
        {
            InitParticlesThrower();
            base.Init();
        }

        public override object Clone() => 
            new ViewMazeItemHammer(
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
                Shaker,
                ParticlesThrower.Clone() as IViewParticlesThrower,
                SwitchLevelStageCommandInvoker);
        
        public Func<ViewCharacterInfo> GetViewCharacterInfo { private get; set; }
        
        public override Component[] Renderers => m_MainShapes.Cast<Component>().ToArray();
        
        public void OnHammerShot(HammerShotEventArgs _Args)
        {
            if (!m_ProceedShots)
                return;
            Cor.Run(ShotCoroutine(_Args.Back));
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            m_ProceedShots = _Args.LevelStage switch
            {
                ELevelStage.Loaded   => true,
                ELevelStage.Finished => false,
                _                    => m_ProceedShots
            };
        }

        #endregion

        #region nonpublic methods

        protected override void InitShape()
        {
            var go = PrefabSetManager.InitPrefab(
                Object.transform, CommonPrefabSetNames.Views, "hammer");
            m_HammerContainer = go.GetCompItem<Transform>("container");
            int sortingOrder = SortingOrders.GetBlockSortingOrder(Props.Type);
            var collisionDetector = go.GetCompItem<CollisionDetector2D>("collision_detector");
            collisionDetector.OnTriggerEnter += CheckForCharacterDeath;
            collisionDetector.gameObject.layer = LayerMask.NameToLayer(LayerNamesCommon.Dzeta);
            m_MainShapes = go.GetCompItem<Transform>("main_shapes")
                .GetComponentsInChildren<ShapeRenderer>()
                .Select(_Shape => _Shape.SetSortingOrder(sortingOrder))
                .ToList();
            m_AdditionalShapes = go.GetCompItem<Transform>("additional_shapes")
                .GetComponentsInChildren<ShapeRenderer>()
                .Select(_Shape => _Shape.SetSortingOrder(sortingOrder + 1))
                .ToList();
            m_AdditionalShapes2 = go.GetCompItem<Transform>("additional_shapes_2")
                .GetComponentsInChildren<ShapeRenderer>()
                .Select(_Shape => _Shape.SetSortingOrder(sortingOrder))
                .ToList();
            m_AdditionalShapes3 = go.GetCompItem<Transform>("additional_shapes_3")
                .GetComponentsInChildren<ShapeRenderer>()
                .Select(_Shape => _Shape.SetSortingOrder(sortingOrder + 1))
                .ToList();
            m_Tail = go.GetCompItem<Disc>("tail").SetSortingOrder(sortingOrder - 1);
            m_Side1 = go.GetCompItem<Transform>("side_1");
            m_Side2 = go.GetCompItem<Transform>("side_2");
            m_Collider = go.GetCompItem<Collider2D>("collider");
        }

        private void InitParticlesThrower()
        {
            int sortingOrder = SortingOrders.GetBlockSortingOrder(Props.Type);
            ParticlesThrower.ParticleType = EParticleType.Bubbles;
            ParticlesThrower.SetPoolSize(ParticlesThrowerSize);
            ParticlesThrower.Init();
            ParticlesThrower.SetSortingOrder(sortingOrder + 3);
        }

        protected override void UpdateShape()
        {
            var pos = CoordinateConverter.ToLocalMazeItemPosition(Props.Position);
            Object.transform.SetLocalPosXY(pos);
            Object.transform.SetLocalScaleXY(CoordinateConverter.Scale * Vector3.one);
            Object.transform.localRotation = GetHammerLocalRotation();
            m_ShotAngle = int.Parse(Props.Args[0].Split(':')[1]);
            m_Clockwise = Props.Args[1].Split(':')[1] == "true";
            m_Tail.Color = ColorProvider.GetColor(ColorIds.MazeItem1).SetA(TailAlpha);
            SetHammerTailAngles(0f, 0f, false);
            SetHammerAngle(0f);
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            switch (_ColorId)
            {
                case ColorIds.MazeItem1:
                    foreach (var shape in m_MainShapes)
                        shape.SetColor(_Color);
                    m_Tail.SetColor(_Color.SetA(TailAlpha));
                    break;
                case ColorIds.Background2:
                    foreach (var shape in m_AdditionalShapes)
                        shape.SetColor(_Color);
                    break;
                case ColorIds.Main:
                    foreach (var shape in m_AdditionalShapes2)
                        shape.SetColor(_Color);
                    foreach (var shape in m_AdditionalShapes3)
                        shape.SetColor(_Color);
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
            var charColls = GetViewCharacterInfo().Colliders;
            for (int i = 0; i < charColls.Length; i++)
            {
                if (_Collider != charColls[i])
                    continue;
                SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.KillCharacter);
                break;
            }
        }

        private Quaternion GetHammerLocalRotation()
        {
            float angle = default;
            var dir = Props.Directions.First();
            if (dir == V2Int.Up)         angle = 0f;
            else if (dir == V2Int.Right) angle = -90f;
            else if (dir == V2Int.Down)  angle = -180f;
            else if (dir == V2Int.Left)  angle = -270f;
            return Quaternion.Euler(Vector3.forward * angle);
        }

        private IEnumerator ShotCoroutine(bool _Back)
        {
            float coeff      = m_Clockwise ? -1f : 1f;
            float startAngle = _Back ? m_ShotAngle * coeff : 0f;
            float endAngle   = !_Back ? m_ShotAngle * coeff : 0f;
            yield return Cor.Lerp(
                    GameTicker,
                    Shot90DegTime * m_ShotAngle / 90f,
                    _OnProgress: _P =>
                    {
                        float angle = Mathf.Lerp(startAngle, endAngle, _P);
                        SetHammerAngle(angle);
                        SetHammerTailAngles(startAngle, angle, _Back);
                    });
            Managers.AudioManager.PlayClip(AudioClipInfoHammerShot);
            Cor.Run(Shaker.ShakeMazeCoroutine(0.05f, 0.1f));
            ThrowParticlesOnShot(_Back);
            Cor.Run(ShotFinishCoroutine(startAngle, endAngle));
            yield return Cor.Lerp(
                GameTicker,
                0.7f * Shot90DegTime * m_ShotAngle / 90f,
                _OnProgress: _P =>
                {
                    float angle = Mathf.Lerp(startAngle, endAngle, _P);
                    SetHammerTailAngles(angle, endAngle, _Back);
                });
        }

        private IEnumerator ShotFinishCoroutine(float _StartAngle, float _EndAngle)
        {
            float addictAngle = _EndAngle > _StartAngle ? -5f : 5f;
            yield return Cor.Lerp(
                GameTicker,
                0.03f,
                _OnProgress: _P =>
                {
                    float angle = Mathf.Lerp(_EndAngle, _EndAngle + addictAngle, _P);
                    SetHammerAngle(angle);
                },
                _ProgressFormula: _P => _P < 0.5f ? 2f * _P : 2f * (1f - _P));
        }
        
        private void SetHammerAngle(float _Angle)
        {
            m_HammerContainer.localRotation = Quaternion.Euler(Vector3.forward * _Angle);
        }

        private void SetHammerTailAngles(float _StartAngle, float _EndAngle, bool _Back)
        {
            _StartAngle += 90f;
            _EndAngle += 90f;
            m_Tail.AngRadiansStart = (!_Back ? _StartAngle : _EndAngle) * Mathf.Deg2Rad;
            m_Tail.AngRadiansEnd = (_Back ? _StartAngle : _EndAngle) * Mathf.Deg2Rad;
        }
        
        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var sets = new Dictionary<IEnumerable<Component>, Func<Color>>();
            var additCol = ColorProvider.GetColor(ColorIds.Background2);
            var additCol2 = ColorProvider.GetColor(ColorIds.Main);
            sets.Add(m_AdditionalShapes, () => additCol);
            sets.Add(m_AdditionalShapes2, () => additCol2);
            sets.Add(m_AdditionalShapes3, () => additCol2);
            return sets;
        }
        
        protected override void OnAppearStart(bool _Appear)
        {
            base.OnAppearStart(_Appear);
            if (!_Appear) 
                return;
            foreach (var shape in m_AdditionalShapes)
                ActivateRenderer(shape, true);
            foreach (var shape in m_AdditionalShapes2)
                ActivateRenderer(shape, true);
            foreach (var shape in m_AdditionalShapes3)
                ActivateRenderer(shape, true);
            ActivateRenderer(m_Tail, true);
            m_Collider.enabled = true;
        }

        protected override void OnAppearFinish(bool _Appear)
        {
            base.OnAppearFinish(_Appear);
            if (_Appear) 
                return;
            foreach (var shape in m_AdditionalShapes)
                ActivateRenderer(shape, false);
            foreach (var shape in m_AdditionalShapes2)
                ActivateRenderer(shape, false);
            foreach (var shape in m_AdditionalShapes3)
                ActivateRenderer(shape, false);
            ActivateRenderer(m_Tail, false);
            m_Collider.enabled = false;
        }
        
        private void ThrowParticlesOnShot(bool _Back)
        {
            const float directSpeedCoefficient = 4f;
            float GetDirectSpeedAddict(float _DirectionCoordinate)
            {
                return -_DirectionCoordinate * directSpeedCoefficient;
            }
            var sideTr = _Back ^ !m_Clockwise ? m_Side1 : m_Side2;
            Vector2 moveDir = sideTr.TransformDirection(sideTr.up);
            for (int i = 0; i < ParticlesThrowerSize; i++)
            {
                float orthDirCoeff = i % 2 == 0 ? 1f : -1f;
                var speedVector = new Vector2(
                    GetDirectSpeedAddict(moveDir.x),
                    GetDirectSpeedAddict(moveDir.y)) 
                + new Vector2((UnityEngine.Random.value - 0.5f),
                    (UnityEngine.Random.value - 0.5f)) * 3f;
                float orthCoeff2 = m_Clockwise ? 1f : -1f;
                var orthDir = 0.5f * new Vector2(moveDir.y, moveDir.x) * orthDirCoeff * orthCoeff2;
                var pos = (Vector2)sideTr.position + orthDir * (UnityEngine.Random.value * 0.8f);
                float randScale = 0.2f + 0.15f * UnityEngine.Random.value;
                ParticlesThrower.ThrowParticle(pos, speedVector, randScale, 0.15f);
            }
        }

        #endregion
    }
}