using System;
using System.Collections.Generic;
using System.Linq;
using DI.Extensions;
using Exceptions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemGravityTrap : IViewMazeItemMovingBlock { }
    
    public class ViewMazeItemGravityTrap : ViewMazeItemMovingBase, IViewMazeItemGravityTrap, IUpdateTick, IOnBackgroundColorChanged
    {
        #region constants

        private const float ShapeScale = 0.35f;

        #endregion
        
        #region nonpubilc members

        private bool m_Rotate;
        private float m_RotationSpeed;
        private Vector3 m_RotateDirection;
        private Vector3 m_Angles;
        private Vector2 m_Position;
        private Transform m_MaceTr;
        private Color m_BackColor;

        #endregion
        
        #region shapes

        protected override object[] Shapes =>
            new object[] {m_OuterDisc, m_InnerDisc}
                .Concat(m_Cones)
                .ToArray();
        private Disc m_OuterDisc;
        private Disc m_InnerDisc;
        private List<Cone> m_Cones;
        
        #endregion
        
        #region inject
        
        
        public ViewMazeItemGravityTrap(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker)
        { }
        
        #endregion
        
        #region api
        
        public override object Clone() => new ViewMazeItemGravityTrap(
            ViewSettings,
            Model,
            CoordinateConverter,
            ContainersGetter,
            GameTicker);

        public void UpdateTick()
        {
            if (!Initialized || !Activated)
                return;
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            DoRotation();
            CheckForCharacterDeath();
        }
        
        public void OnMoveStarted(MazeItemMoveEventArgs _Args)
        {
            m_RotationSpeed = _Args.Speed;
            var dir = (_Args.To - _Args.From).Normalized;
            m_Angles = Vector3.zero;
            m_RotateDirection = GetRotationDirection(dir);
            m_MaceTr.rotation = Quaternion.Euler(Vector3.zero);
            m_Rotate = true;
        }

        public void OnMoving(MazeItemMoveEventArgs _Args)
        {
            m_Position = Vector2.Lerp(_Args.From.ToVector2(), _Args.To.ToVector2(), _Args.Progress);
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(m_Position));
        }

        public void OnMoveFinished(MazeItemMoveEventArgs _Args)
        {
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(_Args.To));
            m_Rotate = false;
        }
        
        public void OnBackgroundColorChanged(Color _Color)
        {
            m_BackColor = _Color;
            m_InnerDisc.Color = _Color;
        }

        #endregion
        
        #region nonpublic methods
        
        protected override void SetShape()
        {
            Object = new GameObject("Gravity Trap");
            Object.SetParent(ContainersGetter.MazeItemsContainer.gameObject);
            Object.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));

            var go = PrefabUtilsEx.InitPrefab(
                Object.transform, "views", "gravity_trap");
            m_MaceTr = go.GetCompItem<Transform>("container");

            m_OuterDisc = go.GetCompItem<Disc>("outer disc");
            m_InnerDisc = go.GetCompItem<Disc>("inner disc");
            m_Cones = go.GetContentItem("cones").GetComponentsInChildren<Cone>().ToList();
            
            go.transform.SetLocalPosXY(Vector2.zero);
            go.transform.localScale = Vector3.one * CoordinateConverter.GetScale() * ShapeScale;
            
            base.SetShape();
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
            var ch = Model.Character;
            var cPos = ch.IsMoving ? ch.MovingInfo.PrecisePosition : ch.Position.ToVector2();
            if (Vector2.Distance(cPos, m_Position) < 1f)
                ch.RaiseDeath();
        }

        protected override void Appear(bool _Appear)
        {
            AppearingState = _Appear ? EAppearingState.Appearing : EAppearingState.Dissapearing;
            if (_Appear)
                m_InnerDisc.enabled = true;
            Coroutines.Run(Coroutines.WaitWhile(
                () => !Initialized,
                () =>
                {
                    RazorMazeUtils.DoAppearTransitionSimple(
                        _Appear,
                        GameTicker,
                        new Dictionary<object[], Func<Color>>
                        {
                            {new object[]{m_OuterDisc}.Concat(m_Cones).ToArray(), () => DrawingUtils.ColorLines},
                        },
                        Props.Position,
                        _OnFinish: () =>
                        {
                            if (!_Appear)
                            {
                                m_InnerDisc.enabled = false;
                            }
                            AppearingState = _Appear ? EAppearingState.Appeared : EAppearingState.Dissapeared;
                        });
                }));
        }

        #endregion
    }
}