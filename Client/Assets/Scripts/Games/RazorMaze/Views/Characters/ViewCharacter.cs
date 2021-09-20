using System.Linq;
using Constants;
using Exceptions;
using Extensions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Utils;
using Shapes;
using TimeProviders;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.Characters
{
    public class ViewCharacter : ViewCharacterBase
    {
        #region nonpublic members

        private static int AnimKeyStartJumping => AnimKeys.Anim3;
        private static int AnimKeyStartMove => AnimKeys.Anim;
        private static int AnimKeyBump => AnimKeys.Anim2;

        private GameObject m_Head;
        private Animator m_Animator;
        private EMazeMoveDirection m_PrevVertDir;
        private EMazeMoveDirection m_PrevHorDir;
        private bool m_FirstMoveDone;
        private bool m_Activated;
        private bool m_Initialized;
        
        #endregion
        
        #region inject
        
        private IModelMazeData Data { get; }
        private IViewCharacterTail Tail { get; }
        private IViewCharacterEffector Effector { get; }
        private IGameTimeProvider GameTimeProvider { get; }
        private ViewSettings ViewSettings { get; }

        public ViewCharacter(
            ICoordinateConverter _CoordinateConverter, 
            IModelMazeData _Data, 
            IContainersGetter _ContainersGetter,
            IViewMazeCommon _ViewMazeCommon,
            IViewCharacterTail _Tail,
            IViewCharacterEffector _Effector,
            IGameTimeProvider _GameTimeProvider,
            ViewSettings _ViewSettings) : base(_CoordinateConverter, _Data, _ContainersGetter, _ViewMazeCommon)
        {
            Data = _Data;
            Tail = _Tail;
            Effector = _Effector;
            GameTimeProvider = _GameTimeProvider;
            ViewSettings = _ViewSettings;
        }
        
        #endregion
        
        #region api
        
        public override bool Activated
        {
            get => m_Activated;
            set
            {
                m_Activated = value;
                m_Head.SetActive(value);
                Tail.Activated = value;
                Effector.Activated = value;
            }
        }
        
        public override void Init()
        {
            bool m_TailInitialized = false;
            bool m_DeathEffectorInitialized = false;
            Tail.Initialized += () => m_TailInitialized = true;
            Effector.Initialized += () => m_DeathEffectorInitialized = true;
            Tail.Init();
            Effector.Init();
            
            InitPrefab();
            m_Animator.SetTrigger(AnimKeyStartJumping);
            if (!ViewSettings.StartPathItemFilledOnStart)
                UnfillStartPathItem();
            Coroutines.Run(Coroutines.WaitWhile(
                () => !m_TailInitialized || !m_DeathEffectorInitialized,
                () =>
                {
                    base.Init();
                    m_Initialized = true;
                }));
        }
        
        public override void OnMovingStarted(CharacterMovingEventArgs _Args)
        {
            m_Animator.SetTrigger(AnimKeyStartMove);
            SetOrientation(_Args.Direction);
            Tail.ShowTail(_Args);
            if (!m_FirstMoveDone && ViewSettings.StartPathItemFilledOnStart)
                UnfillStartPathItem();
            m_FirstMoveDone = true;
        }

        public override void OnMoving(CharacterMovingEventArgs _Args)
        {
            var prevPos = CoordinateConverter.ToLocalCharacterPosition(_Args.From);
            var nextPos = CoordinateConverter.ToLocalCharacterPosition(_Args.To);
            var pos = Vector2.Lerp(prevPos, nextPos, _Args.Progress);
            SetPosition(pos);
            Tail.ShowTail(_Args);
        }

        public override void OnMovingFinished(CharacterMovingEventArgs _Args)
        {
            m_Animator.SetTrigger(AnimKeyBump);
            Tail.HideTail(_Args);
        }
        
        public override void OnRevivalOrDeath(bool _Alive)
        {
            Coroutines.Run(Coroutines.WaitWhile(
                () => !m_Initialized,
                () =>
                {
                    m_Head.SetActive(_Alive);
                    Tail.Activated = _Alive;
                    Effector.OnRevivalOrDeath(_Alive);
                    if (!_Alive) 
                        return;
                    m_Animator.SetTrigger(AnimKeyStartJumping);
                    Tail.HideTail();
                }));
        }
        
        #endregion
        
        #region nonpublic methods
        
        private void InitPrefab()
        {
            var go = ContainersGetter.CharacterContainer.gameObject;
            var prefab = PrefabUtilsEx.InitPrefab(go.transform, CommonPrefabSetNames.Views, "character");
            prefab.transform.SetLocalPosXY(Vector2.zero);
            m_Head = prefab.GetContentItem("head");
            var localScale = Vector3.one * CoordinateConverter.GetScale() * 0.98f;
            m_Head.transform.localScale = localScale;
            
            prefab.GetCompItem<Rectangle>("head shape").Color = DrawingUtils.ColorCharacter;
            m_Animator = prefab.GetCompItem<Animator>("animator");
            var eye1 = prefab.GetCompItem<Rectangle>("eye_1");
            var eye2 = prefab.GetCompItem<Rectangle>("eye_2");
            eye1.Color = eye2.Color = DrawingUtils.ColorBack;
        }

        private void SetOrientation(EMazeMoveDirection _Direction)
        {
            switch (_Direction)
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
                default: throw new SwitchCaseNotImplementedException(_Direction);
            }

            switch (_Direction)
            {
                case EMazeMoveDirection.Up:
                case EMazeMoveDirection.Down:
                    m_PrevVertDir = _Direction;
                    break;
                case EMazeMoveDirection.Right:
                case EMazeMoveDirection.Left:
                    m_PrevHorDir = _Direction;
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Direction);
            }
        }

        private void LookAtByOrientation(EMazeMoveDirection _Direction, bool _Inverse)
        {
            float angle, horScale;
            float vertScale = _Inverse ? -1f : 1f;
            switch (_Direction)
            {
                case EMazeMoveDirection.Up:    (angle, horScale) = (90f, 1f);  break;
                case EMazeMoveDirection.Right: (angle, horScale) = (0f, 1f);  break;
                case EMazeMoveDirection.Down:  (angle, horScale) = (90f, -1f); break;
                case EMazeMoveDirection.Left:  (angle, horScale) = (0f, -1f); break;
                default:
                    throw new SwitchCaseNotImplementedException(_Direction);
            }
            
            m_Head.transform.eulerAngles = Vector3.forward * angle;
            var absScale = m_Head.transform.localScale.Abs();
            m_Head.transform.localScale = new Vector3(absScale.x * horScale, absScale.y * vertScale, absScale.z);
        }
        
        private void UnfillStartPathItem()
        {
            ViewMazeCommon.MazeItems.Single(_Item => _Item.Props.IsStartNode).Proceeding = true;
        }

        #endregion
    }
}