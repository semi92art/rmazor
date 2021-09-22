using System.Collections.Generic;
using DI.Extensions;
using Exceptions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using TimeProviders;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemGravityTrap : IViewMazeItemMovingBlock { }
    
    public class ViewMazeItemGravityTrap : ViewMazeItemBase, IViewMazeItemGravityTrap, IUpdateTick
    {
        #region constants

        private const float ShapeScale = 0.35f;

        #endregion
        
        #region nonpubilc members

        private bool m_Rotate;
        private float m_RotationSpeed;
        private Vector3 m_RotateDirection;
        private Vector3 m_Angles;
        private Transform m_Mace;
        private Vector2 m_Position;

        #endregion
        
        #region shapes

        protected override object[] Shapes => new object[] {m_MaceRenderer, m_MaceDisc};
        private MeshRenderer m_MaceRenderer;
        private Disc m_MaceDisc;
        
        #endregion
        
        #region inject
        
        public ViewMazeItemGravityTrap(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTimeProvider _GameTimeProvider,
            IGameTicker _GameTicker)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTimeProvider,
                _GameTicker) { }
        
        #endregion
        
        #region api
        
        public override object Clone() => new ViewMazeItemGravityTrap(
            ViewSettings,
            Model,
            CoordinateConverter,
            ContainersGetter,
            GameTimeProvider, 
            GameTicker);

        public override bool Activated
        {
            get => m_Activated;
            set
            {
                m_Activated = value;
                m_Mace.SetGoActive(value);
            }
        }

        public void UpdateTick()
        {
            if (!Initialized || !Activated)
                return;
            if (!m_Rotate)
                return;
            m_Angles += m_RotateDirection * Time.deltaTime * m_RotationSpeed;
            m_Angles = ClampAngles(m_Angles);
            m_Mace.rotation = Quaternion.Euler(m_Angles);
        }
        
        public void OnMoveStarted(MazeItemMoveEventArgs _Args)
        {
            m_RotationSpeed = _Args.Speed;
            var dir = (_Args.To - _Args.From).Normalized;
            m_Angles = Vector3.zero;
            m_RotateDirection = GetRotationDirection(dir);
            m_Mace.rotation = Quaternion.Euler(Vector3.zero);
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

        #endregion
        
        #region nonpublic methods
        
        protected override void SetShape()
        {
            Object = new GameObject("Gravity Trap");
            Object.SetParent(ContainersGetter.MazeItemsContainer.gameObject);
            Object.transform.SetLocalPosXY(CoordinateConverter.ToLocalMazeItemPosition(Props.Position));

            var go = PrefabUtilsEx.InitPrefab(
                Object.transform, "views", "gravity_trap");
            m_Mace = go.GetCompItem<Transform>("container");

            m_MaceRenderer = go.GetCompItem<MeshRenderer>("renderer");
            m_MaceDisc = go.GetCompItem<Disc>("disc");
            
            go.transform.SetLocalPosXY(Vector2.zero);
            go.transform.localScale = Vector3.one * CoordinateConverter.GetScale() * ShapeScale;
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
        
        #endregion
    }
}