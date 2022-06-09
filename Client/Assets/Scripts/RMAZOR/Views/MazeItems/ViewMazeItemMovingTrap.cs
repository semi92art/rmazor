using Common;
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
using RMAZOR.Views.Utils;
using UnityEngine;

namespace RMAZOR.Views.MazeItems
{
    public interface IViewMazeItemMovingTrap : IViewMazeItemMovingBlock { }
    
    public class ViewMazeItemMovingTrap : ViewMazeItemMovingBase, IViewMazeItemMovingTrap, IUpdateTick, IFixedUpdateTick
    {
        #region nonpublic members

        private bool m_Rotating;

        #endregion
        
        #region shapes

        protected override string ObjectName => "Moving Trap Block";

        private SpriteRenderer m_Saw;
        private Vector2        m_PrecisePosition;

        #endregion
        
        #region inject
        
        public ViewMazeItemMovingTrap(
            ViewSettings                  _ViewSettings,
            IModelGame                    _Model,
            IMazeCoordinateConverter      _CoordinateConverter,
            IContainersGetter             _ContainersGetter,
            IViewGameTicker               _GameTicker,
            IViewBetweenLevelTransitioner _Transitioner,
            IManagersGetter               _Managers,
            IColorProvider                _ColorProvider,
            IViewInputCommandsProceeder   _CommandsProceeder)
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
            var saw = Object.AddComponentOnNewChild<SpriteRenderer>("Moving Trap", out _);
            saw.sprite = Managers.PrefabSetManager.GetObject<Sprite>("views", "moving_trap");
            saw.color = Color.white;
            saw.sortingOrder = GetSortingOrder();
            saw.enabled = false;
            var coll = Object.AddComponent<CircleCollider2D>();
            coll.radius = 0.5f;
            m_Saw = saw;
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
            if (_ColorId == ColorIds.MazeItem1)
                m_Saw.color = _Color;
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
                var deathPos = GetPerpendicular(cPosPrev, cPos, m_PrecisePosition);
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

        private static Vector2 GetPerpendicular(Vector2 _SegStart, Vector2 _SegEnd, Vector2 _P)
        {
            float x1 = _SegStart.x;
            float y1 = _SegStart.y;
            float x2 = _SegEnd.x;
            float y2 = _SegEnd.y;
            float x3 = _P.x;
            float y3 = _P.y;
            float k = ((y2-y1) * (x3-x1) - (x2-x1) * (y3-y1)) / ((y2-y1) * (y2-y1) + (x2-x1) * (x2-x1));
            float x4 = x3 - k * (y2 - y1);
            float y4 = y3 + k * (x2 - x1);
            return new Vector2(x4, y4);
        }

        #endregion
    }
}