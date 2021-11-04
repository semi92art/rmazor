using System;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.Utils;
using Ticker;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemMovingTrap : IViewMazeItemMovingBlock { }
    
    public class ViewMazeItemMovingTrap : ViewMazeItemMovingBase, IViewMazeItemMovingTrap, IUpdateTick
    {
        #region constants

        private const string SoundClipNameMoveTrap = "saw_working";
        
        #endregion
        
        #region nonpublic members
        
        private Vector2 m_Position;
        private bool m_Rotating;
        
        #endregion
        
        #region shapes

        protected override string ObjectName => "Moving Trap Block";

        private SpriteRenderer m_Saw;

        #endregion
        
        #region inject
        
        public ViewMazeItemMovingTrap(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            IMazeCoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker,
            IViewAppearTransitioner _Transitioner,
            IManagersGetter _Managers,
            IColorProvider _ColorProvider)
            : base(
                _ViewSettings,
                _Model, 
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers,
                _ColorProvider) { }
        
        #endregion
        
        #region api
        
        public override Component[] Shapes => new Component[] {m_Saw};
        
        public override object Clone() => new ViewMazeItemMovingTrap(
            ViewSettings,
            Model, 
            CoordinateConverter, 
            ContainersGetter,
            GameTicker,
            Transitioner,
            Managers,
            ColorProvider);

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

        public override void OnMoving(MazeItemMoveEventArgs _Args)
        {
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            var precisePosition = Vector2.Lerp(
                _Args.From.ToVector2(), _Args.To.ToVector2(), _Args.Progress);
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(precisePosition));
        }

        public override void OnMoveFinished(MazeItemMoveEventArgs _Args)
        {
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(_Args.To));
        }

        public void UpdateTick()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return;
            DoRotation();
        }

        #endregion

        #region nonpublic methods

        protected override void InitShape()
        {
            var saw = Object.AddComponentOnNewChild<SpriteRenderer>("Moving Trap", out _);
            saw.sprite = PrefabUtilsEx.GetObject<Sprite>("views", "moving_trap");
            saw.color = Color.white;
            saw.sortingOrder = SortingOrders.GetBlockSortingOrder(Props.Type);
            saw.enabled = false;
            var coll = Object.AddComponent<CircleCollider2D>();
            coll.radius = 0.5f;
            m_Saw = saw;
        }

        protected override void UpdateShape()
        {
            Object.transform.localScale = Vector3.one * CoordinateConverter.Scale;
            base.UpdateShape();
        }

        private void DoRotation()
        {
            if (!m_Rotating)
                return;
            float rotSpeed = ViewSettings.MovingTrapRotationSpeed * Time.deltaTime; 
            m_Saw.transform.Rotate(Vector3.forward * rotSpeed);
        }

        private void StartRotation()
        {
            Managers.Notify(_SM => _SM.PlayClip(
                SoundClipNameMoveTrap, true, _Tags: $"{GetHashCode()}"));
            m_Rotating = true;
        }

        private void StopRotation()
        {
            Managers.Notify(_SM => _SM.StopClip(
                SoundClipNameMoveTrap, _Tags: $"{GetHashCode()}"));
            m_Rotating = false;
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.MazeItem1)
                m_Saw.color = _Color;
            base.OnColorChanged(_ColorId, _Color);
        }

        #endregion
    }
}