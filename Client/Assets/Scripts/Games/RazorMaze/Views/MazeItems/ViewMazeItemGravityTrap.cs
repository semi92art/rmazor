using System.Collections.Generic;
using System.Linq;
using Controllers;
using DI.Extensions;
using Entities;
using Exceptions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Models.ProceedInfos;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Shapes;
using Ticker;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemGravityTrap : IViewMazeItemMovingBlock { }
    
    public class ViewMazeItemGravityTrap : 
        ViewMazeItemMovingBase, 
        IViewMazeItemGravityTrap,
        IUpdateTick
    {
        #region constants

        private const float ShapeScale = 0.35f;

        #endregion
        
        #region nonpubilc members

        private bool      m_Rotate;
        private float     m_RotationSpeed;
        private Vector3   m_RotateDirection;
        private Vector3   m_Angles;
        private Vector2   m_Position;
        private Transform m_MaceTr;

        #endregion
        
        #region shapes

        protected override string ObjectName => "Gravity Trap Block";
        private Disc m_OuterDisc;
        private Disc m_InnerDisc;
        private List<Cone> m_Cones;

        #endregion
        
        #region inject
        
        private IMazeShaker MazeShaker { get; }
        
        public ViewMazeItemGravityTrap(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            IMazeCoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IViewGameTicker _GameTicker,
            IViewAppearTransitioner _Transitioner,
            IManagersGetter _Managers,
            IMazeShaker _MazeShaker,
            IColorProvider _ColorProvider)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers,
                _ColorProvider)
        {
            MazeShaker = _MazeShaker;
        }
        
        #endregion
        
        #region apico
        
        public override Component[] Shapes => new Component[] {m_OuterDisc}.Concat(m_Cones).ToArray();
        
        public override object Clone() => new ViewMazeItemGravityTrap(
            ViewSettings,
            Model,
            CoordinateConverter,
            ContainersGetter,
            GameTicker,
            Transitioner,
            Managers,
            MazeShaker,
            ColorProvider);

        public override bool ActivatedInSpawnPool
        {
            get => base.ActivatedInSpawnPool;
            set
            {
                m_InnerDisc.enabled = false;
                base.ActivatedInSpawnPool = value;
            }
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            if (_Args.Stage == ELevelStage.ReadyToStart || _Args.Stage == ELevelStage.Loaded)
                m_Position = Props.Position.ToVector2();
        }

        public void UpdateTick()
        {
            if (!Initialized || !ActivatedInSpawnPool)
                return;
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            DoRotation();
            CheckForCharacterDeath();
        }
        
        public override void OnMoveStarted(MazeItemMoveEventArgs _Args)
        {
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            m_RotationSpeed = _Args.Speed;
            var dir = (_Args.To - _Args.From).Normalized;
            m_Angles = Vector3.zero;
            m_RotateDirection = GetRotationDirection(dir);
            m_MaceTr.rotation = Quaternion.Euler(Vector3.zero);
            m_Rotate = true;
            Managers.AudioManager.PlayClip(GetAudioClipArgsTrapRotate(_Args.Info));
            if (!MazeShaker.ShakeMaze)
                MazeShaker.ShakeMaze = true;
        }

        public override void OnMoving(MazeItemMoveEventArgs _Args)
        {
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            m_Position = Vector2.Lerp(_Args.From.ToVector2(), _Args.To.ToVector2(), _Args.Progress);
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(m_Position));
        }

        public override void OnMoveFinished(MazeItemMoveEventArgs _Args)
        {
            base.OnMoveFinished(_Args);
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(_Args.To));
            m_Rotate = false;
            Managers.AudioManager.StopClip(GetAudioClipArgsTrapRotate(_Args.Info));
            if (MazeShaker.ShakeMaze)
                MazeShaker.ShakeMaze = false;
        }

        #endregion
        
        #region nonpublic methods
        
        protected override void InitShape()
        {
            var go = PrefabUtilsEx.InitPrefab(
                Object.transform, "views", "gravity_trap");
            m_MaceTr = go.GetCompItem<Transform>("container");
            m_OuterDisc = go.GetCompItem<Disc>("outer disc");
            m_InnerDisc = go.GetCompItem<Disc>("inner disc");
            m_Cones = go.GetContentItem("cones").GetComponentsInChildren<Cone>().ToList();
            go.transform.SetLocalPosXY(Vector2.zero);
        }

        protected override void UpdateShape()
        {
            Object.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            Object.transform.localScale = Vector3.one * CoordinateConverter.Scale * ShapeScale;
            base.UpdateShape();
        }

        private void DoRotation()
        {
            if (!m_Rotate)
                return;
            m_Angles += m_RotateDirection * Time.deltaTime * m_RotationSpeed * 100f;
            m_Angles = ClampAngles(m_Angles);
            m_MaceTr.rotation = Quaternion.Euler(m_Angles);
        }
        
        private Vector3 GetRotationDirection(Vector2 _DropDirection)
        {
            switch (Model.Data.Orientation)
            {
                case MazeOrientation.North: return new Vector3(_DropDirection.y, _DropDirection.x);
                case MazeOrientation.South: return new Vector3(-_DropDirection.y, -_DropDirection.x);
                case MazeOrientation.East: return -_DropDirection;
                case MazeOrientation.West: return _DropDirection;
                default: throw new SwitchCaseNotImplementedException(Model.Data.Orientation);
            }
        }

        private Vector3 ClampAngles(Vector3 _Angles)
        {
            float x = ClampAngle(_Angles.x);
            float y = ClampAngle(_Angles.y);
            return new Vector3(x, y, 0);
        }

        private float ClampAngle(float _Angle)
        {
            while (_Angle < 0)
                _Angle += 360f;
            while (_Angle > 360f)
                _Angle -= 360f;
            return _Angle;
        }
        
        private void CheckForCharacterDeath()
        {
            if (!Model.Character.Alive)
                return;
            if (Model.PathItemsProceeder.AllPathsProceeded)
                return;
            if (Model.LevelStaging.LevelStage == ELevelStage.Finished)
                return;
            var ch = Model.Character;
            var cPos = ch.IsMoving ? ch.MovingInfo.PrecisePosition : ch.Position.ToVector2();
            if (Vector2.Distance(cPos, m_Position) > 0.9f)
                return;
            Model.LevelStaging.KillCharacter();
        }

        protected override void InitWallBlockMovingPaths() { }

        protected override void OnAppearStart(bool _Appear)
        {
            if (_Appear)
                m_InnerDisc.enabled = true;
            base.OnAppearStart(_Appear);
        }

        protected override void OnAppearFinish(bool _Appear)
        {
            if (!_Appear)
                m_InnerDisc.enabled = false;
            base.OnAppearFinish(_Appear);
        }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.MazeItem1)
            {
                m_OuterDisc.Color = _Color;
                foreach (var cone in m_Cones)
                    cone.Color = _Color;
            }
            else if (_ColorId == ColorIds.Background)
                m_InnerDisc.Color = _Color;
        }

        private AudioClipArgs GetAudioClipArgsTrapRotate(IMazeItemProceedInfo _Info)
        {
            return new AudioClipArgs("mace_roll", EAudioClipType.Sound, _Loop: true, _Id: _Info.GetHashCode().ToString());
        }

        #endregion
    }
}