using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Enums;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Characters;
using RMAZOR.Views.Common;
using RMAZOR.Views.Helpers;
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

        private const float Shot90DegTime = 0.1f;

        #endregion

        #region nonpublic members

        protected override string              ObjectName => "Hammer Block";
        
        private List<ShapeRenderer> m_MainShapes;
        private List<ShapeRenderer> m_AdditionalShapes;

        private          Transform           m_HammerContainer;
        private          bool                m_ProceedShots;
        private          int                 m_ShotAngle;
        private          bool                m_Clockwise;
        
        #endregion

        #region inject
        private IPrefabSetManager PrefabSetManager { get; }
        private IMazeShaker       Shaker           { get; }

        public ViewMazeItemHammer(
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
            IMazeShaker                   _Shaker) 
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
            Shaker           = _Shaker;
        }

        #endregion

        #region api

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
                Shaker);
        
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
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    

                    m_ProceedShots = true;
                    break;
                case ELevelStage.Finished:
                    m_ProceedShots = false;
                    break;
            }
        }

        #endregion

        #region nonpublic methods

        protected override void InitShape()
        {
            var go = PrefabSetManager.InitPrefab(
                Object.transform, "views", "hammer");
            m_HammerContainer = go.GetCompItem<Transform>("container");
            int sortingOrder = SortingOrders.GetBlockSortingOrder(Props.Type);
            var collisionDetector = go.GetCompItem<CollisionDetector2D>("collision_detector");
            collisionDetector.OnTriggerEnter += CheckForCharacterDeath;
            collisionDetector.gameObject.layer = LayerMask.NameToLayer("ζ Dzeta");
            m_MainShapes = go.GetCompItem<Transform>("main_shapes")
                .GetComponentsInChildren<ShapeRenderer>()
                .Select(_Shape => _Shape.SetSortingOrder(sortingOrder))
                .ToList();
            m_AdditionalShapes = go.GetCompItem<Transform>("additional_shapes")
                .GetComponentsInChildren<ShapeRenderer>()
                .Select(_Shape => _Shape.SetSortingOrder(sortingOrder + 1))
                .ToList();
        }

        protected override void UpdateShape()
        {
            var pos = CoordinateConverter.ToLocalMazeItemPosition(Props.Position);
            Object.transform.SetLocalPosXY(pos);
            Object.transform.SetLocalScaleXY(CoordinateConverter.Scale * Vector3.one);
            Object.transform.localRotation = GetHammerLocalRotation();
            m_ShotAngle = int.Parse(Props.Args[0].Split(':')[1]);
            m_Clockwise = Props.Args[1].Split(':')[1] == "true";
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            switch (_ColorId)
            {
                case ColorIds.MazeItem1:
                    foreach (var shape in m_MainShapes)
                        shape.SetColor(_Color);
                    break;
                case ColorIds.Background2:
                    foreach (var shape in m_AdditionalShapes)
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
                CommandsProceeder.RaiseCommand(EInputCommand.KillCharacter, null);
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
                },
                _OnFinish: () =>
                {
                    Managers.AudioManager.PlayClip( GetAudioClipInfoHammerShot());
                    Cor.Run(Shaker.ShakeMazeCoroutine(0.05f, 0.1f));
                    Cor.Run(ShotFinishCoroutine(startAngle, endAngle));
                });
        }

        private IEnumerator ShotFinishCoroutine(float _StartAngle, float _EndAngle)
        {
            float addichAngle = _EndAngle > _StartAngle ? -5f : 5f; 
            yield return Cor.Lerp(
                GameTicker,
                0.03f,
                _OnProgress: _P =>
                {
                    float angle = Mathf.Lerp(_EndAngle, _EndAngle + addichAngle, _P);
                    SetHammerAngle(angle);     
                },
                _ProgressFormula: _P => _P < 0.5f ? 2f * _P : 2f * (1f - _P));
        }
        
        private void SetHammerAngle(float _Angle)
        {
            m_HammerContainer.localRotation = Quaternion.Euler(Vector3.forward * _Angle);
        }

        protected override void OnAppearStart(bool _Appear)
        {
            base.OnAppearStart(_Appear);
            if (!_Appear) 
                return;
            foreach (var shape in m_AdditionalShapes)
                ActivateRenderer(shape, true);
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var sets = base.GetAppearSets(_Appear);
            var additCol = ColorProvider.GetColor(ColorIds.Background2);
            sets.Add(m_AdditionalShapes, () => additCol);
            return sets;
        }

        protected override void OnAppearFinish(bool _Appear)
        {
            base.OnAppearFinish(_Appear);
            if (_Appear) 
                return;
            foreach (var shape in m_AdditionalShapes)
                ActivateRenderer(shape, false);
        }

        private AudioClipArgs GetAudioClipInfoHammerShot()
        {
            return new AudioClipArgs("hammer_shot", EAudioClipType.GameSound, 0.3f, _Id: m_ShotAngle.ToString());
        }

        #endregion
    }
}