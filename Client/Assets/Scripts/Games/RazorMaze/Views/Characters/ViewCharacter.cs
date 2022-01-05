using System;
using System.Collections.Generic;
using System.Linq;
using Constants;
using Controllers;
using DI.Extensions;
using Entities;
using Exceptions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.MazeItems;
using Shapes;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;
using SortingOrders = Games.RazorMaze.Views.Utils.SortingOrders;

namespace Games.RazorMaze.Views.Characters
{
    public class ViewCharacter : ViewCharacterBase
    {
        #region constants
        
        private const float RelativeLocalScale = 0.98f;
        
        #endregion
        
        #region shapes
        
        private Rectangle m_HeadShape;
        private Rectangle m_Eye1Shape, m_Eye2Shape;
        private bool      m_EnableMoving;
        
        #endregion
        
        #region nonpublic members

        private static int AnimKeyStartJumping => AnimKeys.Anim3;
        private static int AnimKeyStartMove    => AnimKeys.Anim;
        private static int AnimKeyBump         => AnimKeys.Anim2;
        
        private GameObject         m_Head;
        private Animator           m_Animator;
        private EMazeMoveDirection m_PrevHorDir  = EMazeMoveDirection.Right;
        private bool               m_Activated;
        private bool               m_Initialized;
        private bool               m_ShowCharacterHeadAndTail;

        #endregion
        
        #region inject
        
        private IViewCharacterTail          Tail              { get; }
        private IViewCharacterEffector      Effector          { get; }
        private IViewAppearTransitioner     Transitioner      { get; }
        private IManagersGetter             Managers          { get; }
        private IMazeShaker                 MazeShaker        { get; }
        private IColorProvider              ColorProvider     { get; }
        private IViewInputCommandsProceeder CommandsProceeder { get; }
        private IPrefabSetManager           PrefabSetManager  { get; }

        public ViewCharacter(
            IMazeCoordinateConverter _CoordinateConverter, 
            IModelGame _Model,
            IContainersGetter _ContainersGetter,
            IViewMazeCommon _ViewMazeCommon,
            IViewCharacterTail _Tail,
            IViewCharacterEffector _Effector,
            IViewAppearTransitioner _Transitioner,
            IManagersGetter _Managers,
            IMazeShaker _MazeShaker,
            IColorProvider _ColorProvider,
            IViewInputCommandsProceeder _CommandsProceeder,
            IPrefabSetManager _PrefabSetManager) 
            : base(
                _CoordinateConverter, 
                _Model, 
                _ContainersGetter,
                _ViewMazeCommon)
        {
            Tail = _Tail;
            Effector = _Effector;
            Transitioner = _Transitioner;
            Managers = _Managers;
            MazeShaker = _MazeShaker;
            ColorProvider = _ColorProvider;
            CommandsProceeder = _CommandsProceeder;
            PrefabSetManager = _PrefabSetManager;
        }
        
        #endregion
        
        #region api
        
        public override bool Activated
        {
            get => m_Activated;
            set
            {
                if (value)
                {
                    if (!m_Initialized)
                    {
                        CommandsProceeder.Command += OnCommand;
                        ColorProvider.ColorChanged += OnColorChanged;
                        MazeShaker.Init();
                        InitPrefab();
                        m_Initialized = true;
                    }
                    UpdatePrefab();
                }
                
                m_Activated = value;
                m_HeadShape.enabled = m_Eye1Shape.enabled = m_Eye2Shape.enabled = false;

                if (!value)
                {
                    Tail.Activated = false;
                    Effector.Activated = false;
                }
            }
        }

        private void OnCommand(EInputCommand _Command, object[] _Args)
        {
            
            if (_Command != EInputCommand.KillCharacter)
                return;
            m_EnableMoving = false;

        }

        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.Character)
                m_HeadShape.Color = _Color;
            else if (_ColorId == ColorIds.Background)
                m_Eye1Shape.Color = m_Eye2Shape.Color = _Color;
        }

        public override void OnRotationAfterFinished(MazeRotationEventArgs _Args) { }

        public override void OnAllPathProceed(V2Int _LastPath)
        {
            m_HeadShape.enabled = m_Eye1Shape.enabled = m_Eye2Shape.enabled = false;
            Tail.HideTail();
            Effector.OnAllPathProceed(_LastPath);
        }

        public override void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            if (!m_EnableMoving)
                return;
            m_Animator.SetTrigger(AnimKeyStartMove);
            SetOrientation(_Args.Direction);
            Tail.ShowTail(_Args);
            Effector.OnCharacterMoveStarted(_Args);
        }

        public override void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)
        {
            if (!m_EnableMoving)
                return;
            if (!m_ShowCharacterHeadAndTail)
                return;
            var pos = CoordinateConverter.ToLocalCharacterPosition(_Args.PrecisePosition);
            SetPosition(pos);
            Tail.ShowTail(_Args);
        }

        public override void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            if (_Args.BlockOnFinish != null && _Args.BlockOnFinish.Type == EMazeItemType.Springboard)
                return;
            if (!m_EnableMoving)
                return;
            m_Animator.SetTrigger(AnimKeyBump);
            if (m_ShowCharacterHeadAndTail)
                Tail.HideTail(_Args);
            Coroutines.Run(MazeShaker.HitMazeCoroutine(_Args));
            int randClipId = 1 + Mathf.FloorToInt(5f * Random.value);
            Managers.AudioManager.PlayClip(new AudioClipArgs($"character_end_move_{randClipId}", EAudioClipType.GameSound));
            Managers.HapticsManager.PlayPreset(EHapticsPresetType.LightImpact);
            Effector.OnCharacterMoveFinished(_Args);
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            m_ShowCharacterHeadAndTail = _Args.Stage == ELevelStage.StartedOrContinued;
            if (m_Animator.IsNotNull())
                m_Animator.speed = _Args.Stage == ELevelStage.Paused ? 0f : 1f;
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    SetDefaultCharacterState(false);
                    Activated = true;
                    break;
                case ELevelStage.ReadyToStart:
                    m_EnableMoving = true;
                    SetDefaultCharacterState(true);
                    break;
                case ELevelStage.CharacterKilled:
                    m_HeadShape.enabled = m_Eye1Shape.enabled = m_Eye2Shape.enabled = false;
                    Tail.HideTail();
                    Managers.AudioManager.PlayClip(new AudioClipArgs("character_death", EAudioClipType.GameSound));
                    Managers.HapticsManager.PlayPreset(EHapticsPresetType.Failure);
                    Coroutines.Run(MazeShaker.ShakeMazeCoroutine());
                    break;
            }
            Effector.OnLevelStageChanged(_Args);
            MazeShaker.OnLevelStageChanged(_Args);
        }

        public override void Appear(bool _Appear)
        {
            if (_Appear)
            {
                m_HeadShape.enabled = m_Eye1Shape.enabled = m_Eye2Shape.enabled = true;
                m_Animator.SetTrigger(AnimKeyStartJumping);
            }
            AppearingState = _Appear ? EAppearingState.Appearing : EAppearingState.Dissapearing;
            if (_Appear)
                m_Animator.SetTrigger(AnimKeyStartJumping);
            Transitioner.DoAppearTransition(
                _Appear,
                new Dictionary<IEnumerable<Component>, Func<Color>>
                {
                    {new Component[] {m_HeadShape}, () => ColorProvider.GetColor(ColorIds.Character)},
                    {new Component[] {m_Eye1Shape, m_Eye2Shape}, () => ColorProvider.GetColor(ColorIds.Background)}
                },
                Model.Character.Position,
                () =>
                {
                    AppearingState = _Appear ? EAppearingState.Appeared : EAppearingState.Dissapeared;
                });
        }

        #endregion
        
        #region nonpublic methods
        
        private void InitPrefab()
        {
            var go = ContainersGetter.GetContainer(ContainerNames.Character).gameObject;
            var prefab = PrefabSetManager.InitPrefab(go.transform, CommonPrefabSetNames.Views, "character");
            prefab.transform.SetLocalPosXY(Vector2.zero);
            m_Head = prefab.GetContentItem("head");
            m_Animator = prefab.GetCompItem<Animator>("animator");
            m_HeadShape = prefab.GetCompItem<Rectangle>("head shape");
            m_Eye1Shape = prefab.GetCompItem<Rectangle>("eye_1");
            m_Eye2Shape = prefab.GetCompItem<Rectangle>("eye_2");
            m_HeadShape.enabled = m_Eye1Shape.enabled = m_Eye2Shape.enabled = false;
            m_HeadShape.Color = ColorProvider.GetColor(ColorIds.Character);
            m_HeadShape.SortingOrder = SortingOrders.Character;
            m_Eye1Shape.SortingOrder = SortingOrders.Character + 1;
            m_Eye2Shape.SortingOrder = SortingOrders.Character + 1;
            m_Initialized = true;
        }

        private void UpdatePrefab()
        {
            var localScale = Vector3.one * CoordinateConverter.Scale * RelativeLocalScale;
            m_Head.transform.localScale = localScale;
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
                    break;
                case EMazeMoveDirection.Right:
                case EMazeMoveDirection.Left:
                    m_PrevHorDir = _Direction;
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Direction);
            }
        }

        private void LookAtByOrientation(EMazeMoveDirection _Direction, bool _VerticalInverse, bool _LocalAngles = false)
        {
            float angle, horScale;
            switch (_Direction)
            {
                case EMazeMoveDirection.Up:    (angle, horScale) = (90f, 1f);  break;
                case EMazeMoveDirection.Right: (angle, horScale) = (0f, 1f);  break;
                case EMazeMoveDirection.Down:  (angle, horScale) = (90f, -1f); break;
                case EMazeMoveDirection.Left:  (angle, horScale) = (0f, -1f); break;
                default:
                    throw new SwitchCaseNotImplementedException(_Direction);
            }

            if (_LocalAngles)
                m_Head.transform.localEulerAngles = Vector3.forward * angle;
            else
                m_Head.transform.eulerAngles = Vector3.forward * angle;
            float vertScale = _VerticalInverse ? -1f : 1f;
            float scaleCoeff = CoordinateConverter.Scale * RelativeLocalScale;
            m_Head.transform.localScale = scaleCoeff * new Vector3(horScale, vertScale, 1f);
        }
        
        private void SetDefaultCharacterState(bool _EnableHead)
        {
            Coroutines.Run(Coroutines.WaitWhile(
                () => !m_Initialized,
                () =>
                {
                    SetPosition(CoordinateConverter.ToLocalCharacterPosition(Model.Data.Info.Path.First()));
                    LookAtByOrientation(EMazeMoveDirection.Right, false);
                    // FIXME это нужно отрефакторить будет
                    if (_EnableHead)
                    {
                        m_HeadShape.enabled = m_Eye1Shape.enabled = m_Eye2Shape.enabled = true;
                    }
                    m_Animator.SetTrigger(AnimKeyStartJumping);
                    Tail.Activated = true;
                    Tail.HideTail();
                }));
        }

        #endregion
    }
}