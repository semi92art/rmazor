using System;
using System.Collections.Generic;
using Common;
using Common.Extensions;
using Common.Helpers;
using Common.Providers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemMovingTrap : IViewMazeItemMovingBlock { }
    
    public class ViewMazeItemMovingTrap :
        ViewMazeItemMovingBase, 
        IViewMazeItemMovingTrap,
        IUpdateTick,
        IFixedUpdateTick
    {
        #region nonpublic members

        private bool m_Rotating;

        #endregion
        
        #region shapes

        protected override string ObjectName => "Moving Trap Block";

        private SpriteRenderer   m_Saw, m_Border;
        private Vector2          m_PrecisePosition;
        private CircleCollider2D m_Collider;

        #endregion
        
        #region inject

        private ViewMazeItemMovingTrap(
            ViewSettings                _ViewSettings,
            IModelGame                  _Model,
            ICoordinateConverter  _CoordinateConverter,
            IContainersGetter           _ContainersGetter,
            IViewGameTicker             _GameTicker,
            IViewFullscreenTransitioner _Transitioner,
            IManagersGetter             _Managers,
            IColorProvider              _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder)
            : base(
                _ViewSettings,
                _Model, 
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers,
                _ColorProvider,
                _CommandsProceeder) { }
        
        #endregion
        
        #region api
        
        public override Component[] Renderers => new Component[] {m_Saw};
        
        public override object Clone() => new ViewMazeItemMovingTrap(
            ViewSettings,
            Model, 
            CoordinateConverter, 
            ContainersGetter,
            GameTicker,
            Transitioner,
            Managers,
            ColorProvider,
            CommandsProceeder);

        public override EProceedingStage ProceedingStage
        {
            get => base.ProceedingStage;
            set
            {
                base.ProceedingStage = value;
                if (value == EProceedingStage.Inactive)
                    StopRotation();
                else
                    StartRotation();
            }
        }

        public override void OnMoveStarted(MazeItemMoveEventArgs _Args) { }

        public override void OnMoving(MazeItemMoveEventArgs _Args)
        {
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            m_PrecisePosition = Vector2.Lerp(_Args.From, _Args.To, _Args.Progress);
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(m_PrecisePosition));
        }

        public override void OnMoveFinished(MazeItemMoveEventArgs _Args) { }

        public void UpdateTick()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return;
            DoRotation();
        }

        public void FixedUpdateTick()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return;
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            CheckForCharacterDeath();
        }
        
        #endregion

        #region nonpublic methods

        protected override void InitShape()
        {
            var saw = Object.AddComponentOnNewChild<SpriteRenderer>("Moving Trap", out var sawObject);
            saw.sprite = Managers.PrefabSetManager.GetObject<Sprite>("views", "moving_trap");
            saw.color = ColorProvider.GetColor(ColorIds.MazeItem1);
            saw.sortingOrder = GetSortingOrder();
            saw.enabled = false;
            m_Saw = saw;
            var border = sawObject.AddComponentOnNewChild<SpriteRenderer>("Moving Trap Border", out _);
            border.sprite = Managers.PrefabSetManager.GetObject<Sprite>("views", "moving_trap_border");
            border.color = ColorProvider.GetColor(ColorIds.Main);
            border.sortingOrder = GetSortingOrder() - 1;
            border.enabled = false;
            m_Border = border;
            m_Collider = Object.AddComponent<CircleCollider2D>();
            m_Collider.radius = 0.5f;
        }

        protected override void UpdateShape()
        {
            m_PrecisePosition = Props.Position;
            Object.transform.localScale = Vector3.one * CoordinateConverter.Scale;
            base.UpdateShape();
        }

        private void DoRotation()
        {
            if (!m_Rotating)
                return;
            float rotSpeed = ViewSettings.movingTrapRotationSpeed * GameTicker.DeltaTime; 
            m_Saw.transform.Rotate(Vector3.forward * rotSpeed);
        }

        private void StartRotation()
        {
            m_Rotating = true;
        }

        private void StopRotation()
        {
            m_Rotating = false;
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            switch (_ColorId)
            {
                case ColorIds.Main:      m_Border.color = _Color; break;
                case ColorIds.MazeItem1: m_Saw.color    = _Color; break;
            }
            base.OnColorChanged(_ColorId, _Color);
        }
        
        private void CheckForCharacterDeath()
        {
            if (!Model.Character.Alive)
                return;
            if (Model.PathItemsProceeder.AllPathsProceeded)
                return;
            if (Model.LevelStaging.LevelStage == ELevelStage.Finished)
                return;
            if (Model.Character.IsMoving)
            {
                var cPos = Model.Character.MovingInfo.PrecisePosition;
                var cPosPrev = Model.Character.MovingInfo.PreviousPrecisePosition;
                float dist = MathUtils.MinDistanceBetweenPointAndSegment(
                    cPosPrev, 
                    cPos,
                    m_PrecisePosition);
                if (dist + MathUtils.Epsilon > 1f)
                    return;
                CommandsProceeder.RaiseCommand(EInputCommand.KillCharacter, 
                    null);
            }
            else
            {
                Vector2 cPos = Model.Character.Position;
                if (Vector2.Distance(cPos, m_PrecisePosition) + MathUtils.Epsilon > 1f)
                    return;
                CommandsProceeder.RaiseCommand(EInputCommand.KillCharacter, null);
            }
        }

        protected override void OnAppearStart(bool _Appear)
        {
            base.OnAppearStart(_Appear);
            if (!_Appear)
                return;
            m_Border.enabled = true;
            m_Collider.enabled = true;
        }

        protected override void OnAppearFinish(bool _Appear)
        {
            base.OnAppearFinish(_Appear);
            if (_Appear)
                return;
            m_Border.enabled = false;
            m_Collider.enabled = false;
        }

        protected override Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear)
        {
            var sets = base.GetAppearSets(_Appear);
            var borderCol = ColorProvider.GetColor(ColorIds.Main);
            sets.Add(new [] {m_Border}, () => borderCol);
            return sets;
        }

        #endregion
    }
}