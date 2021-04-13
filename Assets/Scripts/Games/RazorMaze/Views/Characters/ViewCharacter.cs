using Constants;
using Exceptions;
using Extensions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeCommon;
using Games.RazorMaze.Views.Utils;
using Shapes;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.Characters
{
    public class ViewCharacter : IViewCharacter
    {
        #region nonpublic members

        private static int AnimKeyStartJumping => AnimKeys.Anim3;
        private static int AnimKeyStopJumping => AnimKeys.Stop;
        private static int AnimKeyStartMove => AnimKeys.Anim;
        private static int AnimKeyBump => AnimKeys.Anim2;

        private GameObject m_Head;
        private GameObject m_Character;
        private Animator m_Animator;
        private EMazeMoveDirection m_PrevVertDir;
        private EMazeMoveDirection m_PrevHorDir;
        
        #endregion
        
        #region inject
        
        private ICoordinateConverter CoordinateConverter { get; }
        private IModelMazeData Data { get; }
        private IContainersGetter ContainersGetter { get; }
        private IViewMazeCommon ViewMazeCommon { get; }
        private IViewCharacterTail Tail { get; }

        public ViewCharacter(
            ICoordinateConverter _CoordinateConverter, 
            IModelMazeData _Data, 
            IContainersGetter _ContainersGetter,
            IViewMazeCommon _ViewMazeCommon,
            IViewCharacterTail _Tail)
        {
            CoordinateConverter = _CoordinateConverter;
            Data = _Data;
            ContainersGetter = _ContainersGetter;
            ViewMazeCommon = _ViewMazeCommon;
            Tail = _Tail;
        }
        
        #endregion
        
        public void Init()
        {
            CoordinateConverter.Init(Data.Info.Size);
            InitPrefab();
            m_Animator.SetTrigger(AnimKeyStartJumping);
            var pos = CoordinateConverter.ToLocalCharacterPosition(Data.Info.Path[0]);
            SetPosition(pos);
            Tail.Init();
        }

        public void OnMovingStarted(CharacterMovingEventArgs _Args)
        {
            m_Animator.SetTrigger(AnimKeyStartMove);
            SetOrientation();
            Tail.ShowTail(_Args);
        }

        public void OnMoving(CharacterMovingEventArgs _Args)
        {
            var prevPos = CoordinateConverter.ToLocalCharacterPosition(_Args.From);
            var nextPos = CoordinateConverter.ToLocalCharacterPosition(_Args.To);
            var pos = Vector2.Lerp(prevPos, nextPos, _Args.Progress);
            SetPosition(pos);
            Tail.ShowTail(_Args);
        }

        public void OnMovingFinished(CharacterMovingEventArgs _Args)
        {
            m_Animator.SetTrigger(AnimKeyBump);
            Tail.HideTail(_Args);
        }

        public void OnDeath() { }

        public void OnHealthChanged(HealthPointsEventArgs _Args) { }
        
        private void InitPrefab()
        {
            var go = ContainersGetter.CharacterContainer.gameObject;
            var prefab = PrefabUtilsEx.InitPrefab(go.transform, CommonPrefabSetNames.Views, "character");
            prefab.transform.SetLocalPosXY(Vector2.zero);
            m_Character = prefab;
            m_Head = prefab.GetContentItem("head");
            m_Head.transform.localScale = Vector3.one * CoordinateConverter.GetScale() * 0.98f;
            prefab.GetCompItem<Rectangle>("head shape").Color = ViewUtils.ColorCharacter;
            m_Animator = prefab.GetCompItem<Animator>("animator");
        }
        
        private void SetPosition(Vector2 _Position)
        {
            ContainersGetter.CharacterContainer.localPosition = _Position;
        }

        private void SetOrientation()
        {
            var direction = Data.CharacterInfo.MoveDirection;
            
            switch (direction)
            {
                case EMazeMoveDirection.Up: 
                    LookAtByOrientation(EMazeMoveDirection.Up, m_PrevHorDir == EMazeMoveDirection.Left);
                    break;
                case EMazeMoveDirection.Down:
                    LookAtByOrientation(EMazeMoveDirection.Down, m_PrevHorDir == EMazeMoveDirection.Right);
                    break;
                case EMazeMoveDirection.Right:
                    LookAtByOrientation(EMazeMoveDirection.Right, false);
                    break;
                case EMazeMoveDirection.Left:
                    LookAtByOrientation(EMazeMoveDirection.Left, false);
                    break;
                default: throw new SwitchCaseNotImplementedException(direction);
            }

            switch (direction)
            {
                case EMazeMoveDirection.Up:
                case EMazeMoveDirection.Down:
                    m_PrevVertDir = direction;
                    break;
                case EMazeMoveDirection.Right:
                case EMazeMoveDirection.Left:
                    m_PrevHorDir = direction;
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(direction);
            }
        }

        private void LookAtByOrientation(EMazeMoveDirection _Direction, bool _Inverse)
        {
            float angle, horScale;
            float vertScale = _Inverse ? -1f : 1f;
            switch (_Direction)
            {
                case EMazeMoveDirection.Up:    angle = 90f; horScale = 1f;  break;
                case EMazeMoveDirection.Right: angle = 0f;  horScale = 1f;  break;
                case EMazeMoveDirection.Down:  angle = 90f; horScale = -1f; break;
                case EMazeMoveDirection.Left:  angle = 0f;  horScale = -1f; break;
                default:
                    throw new SwitchCaseNotImplementedException(_Direction);
            }
            
            m_Head.transform.eulerAngles = Vector3.forward * angle;
            var absScale = m_Head.transform.localScale.Abs();
            m_Head.transform.localScale = new Vector3(absScale.x * horScale, absScale.y * vertScale, absScale.z);
        }
    }
}