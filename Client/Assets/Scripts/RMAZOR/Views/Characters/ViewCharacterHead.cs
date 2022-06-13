using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.SpawnPools;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Views.Common;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Characters
{
    public interface IViewCharacterHead :
        IActivated,
        IOnLevelStageChanged,
        ICharacterMoveStarted,
        ICharacterMoveContinued,
        ICharacterMoveFinished,
        IAppear
    {
        Transform    Transform { get; }
        Collider2D[] Colliders { get; }
        void         OnRotationFinished(MazeRotationEventArgs _Args);
        void         OnAllPathProceed(V2Int                   _LastPath);
    }
    
    public class ViewCharacterHead : IViewCharacterHead
    {
        #region constants

        private const float RelativeLocalScale = 0.8f;
        
        #endregion

        #region nonpublic members

        private static int AnimKeyStartMove    => AnimKeys.Anim;
        private static int AnimKeyStartMove2   => AnimKeys.Anim4;
        private static int AnimKeyBump         => AnimKeys.Anim2;
        private static int AnimKeyStartJumping => AnimKeys.Anim3;

        private MazeOrientation    m_MazeOrientation;
        private EMazeMoveDirection m_PrevHorDir = EMazeMoveDirection.Right;
        private GameObject         m_Head;
        private GameObject         m_Border;
        private Animator           m_Animator;
        private CircleCollider2D   m_HeadCollider;
        private Rectangle          m_HeadShape, m_BorderShape;
        private Rectangle          m_Eye1Shape, m_Eye2Shape;
        private bool               m_Activated;
        private bool               m_Initialized;
        private bool               m_HorizontalScaleInverse;
        private bool               m_VerticalScaleInverse;

        #endregion

        #region inject

        private IColorProvider                ColorProvider            { get; }
        private IContainersGetter             ContainersGetter         { get; }
        private IPrefabSetManager             PrefabSetManager         { get; }
        private IMazeCoordinateConverter      CoordinateConverter      { get; }
        private IViewBetweenLevelTransitioner BetweenLevelTransitioner { get; }

        private ViewCharacterHead(
            IColorProvider                _ColorProvider,
            IContainersGetter             _ContainersGetter,
            IPrefabSetManager             _PrefabSetManager,
            IMazeCoordinateConverter      _CoordinateConverter,
            IViewBetweenLevelTransitioner _BetweenLevelTransitioner)
        {
            ColorProvider            = _ColorProvider;
            ContainersGetter         = _ContainersGetter;
            PrefabSetManager         = _PrefabSetManager;
            CoordinateConverter      = _CoordinateConverter;
            BetweenLevelTransitioner = _BetweenLevelTransitioner;
        }
        
        #endregion

        #region api
        
        public EAppearingState AppearingState { get; private set; }
        
        public bool Activated
        {
            get => m_Activated;
            set
            {
                if (value)
                {
                    if (!m_Initialized)
                    {
                        ColorProvider.ColorChanged += OnColorChanged;
                        InitPrefab();
                        m_Initialized = true;
                    }
                    UpdatePrefab();
                }
                ActivateShapes(false);
                m_Activated = value;
            }
        }

        public Transform    Transform => m_Head.transform;
        public Collider2D[] Colliders => new Collider2D[] {m_HeadCollider};

        public void OnRotationFinished(MazeRotationEventArgs _Args)
        {
            m_MazeOrientation = _Args.NextOrientation;
        }

        public void OnAllPathProceed(V2Int _LastPath)
        {
            ActivateShapes(false);
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (m_Animator.IsNotNull())
                m_Animator.speed = _Args.LevelStage == ELevelStage.Paused ? 0f : 1f;
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded:
                    m_MazeOrientation = MazeOrientation.North;
                    SetDefaultState();
                    break;
                case ELevelStage.ReadyToStart:
                    ActivateShapes(true);
                    break;
                case ELevelStage.CharacterKilled:
                    ActivateShapes(false);
                    m_Animator.SetTrigger(AnimKeyStartJumping);
                    break;
            }
        }
        
        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            SetOrientation(_Args.Direction);
            int animKey = AnimKeyStartMove;
            if (m_HorizontalScaleInverse && !m_VerticalScaleInverse
                || !m_HorizontalScaleInverse && m_VerticalScaleInverse)
            {
                animKey = AnimKeyStartMove2;
            }
            m_Animator.SetTrigger(animKey);
        }
        
        public void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args) { }
        
        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            m_Animator.SetTrigger(AnimKeyBump);
        }
        
        public void Appear(bool _Appear)
        {
            if (_Appear)
            {
                ActivateShapes(true);
                m_Animator.SetTrigger(AnimKeyStartJumping);
            }
            AppearingState = _Appear ? EAppearingState.Appearing : EAppearingState.Dissapearing;
            if (_Appear)
                m_Animator.SetTrigger(AnimKeyStartJumping);
            var charCol = ColorProvider.GetColor(ColorIds.Character);
            var charCol2 = ColorProvider.GetColor(ColorIds.Character2);
            BetweenLevelTransitioner.DoAppearTransition(
                _Appear,
                new Dictionary<IEnumerable<Component>, Func<Color>>
                {
                    {new Component[] {m_HeadShape}, () => charCol},
                    {new Component[] {m_BorderShape, m_Eye1Shape, m_Eye2Shape}, () => charCol2},
                },
                () =>
                {
                    AppearingState = _Appear ? EAppearingState.Appeared : EAppearingState.Dissapeared;
                });
        }

        #endregion

        #region nonpublic methods

        private void OnColorChanged(int _ColorId, Color _Color)
        {
            switch (_ColorId)
            {
                case ColorIds.Character:
                    m_HeadShape.SetColor(_Color);
                    break;
                case ColorIds.Character2:
                    m_BorderShape.SetColor(_Color);
                    m_Eye1Shape  .SetColor(_Color);
                    m_Eye2Shape  .SetColor(_Color);
                    break;
            }
        }
        
        private void InitPrefab()
        {
            var contGo = ContainersGetter.GetContainer(ContainerNames.Character).gameObject;
            var go = PrefabSetManager.InitPrefab(
                contGo.transform, 
                CommonPrefabSetNames.Views,
                "character");
            go.transform.SetLocalPosXY(Vector2.zero);
            m_Head = go.GetContentItem("head");
            m_Border = go.GetContentItem("border");
            m_Animator = go.GetCompItem<Animator>("animator");
            m_HeadCollider = go.GetCompItem<CircleCollider2D>("collider");
            m_HeadShape = go.GetCompItem<Rectangle>("head shape")
                .SetColor(ColorProvider.GetColor(ColorIds.Character))
                .SetSortingOrder(SortingOrders.Character);
            m_Eye1Shape = go.GetCompItem<Rectangle>("eye_1").SetSortingOrder(SortingOrders.Character + 1);
            m_Eye2Shape = go.GetCompItem<Rectangle>("eye_2").SetSortingOrder(SortingOrders.Character + 1);
            m_BorderShape = go.GetCompItem<Rectangle>("border").SetSortingOrder(SortingOrders.Character - 1);
            m_HeadShape.enabled = m_Eye1Shape.enabled = m_Eye2Shape.enabled = false;
        }
        
        private void UpdatePrefab()
        {
            var localScale = Vector2.one * CoordinateConverter.Scale * RelativeLocalScale;
            m_Head.transform.SetLocalScaleXY(localScale);
            m_Border.transform.SetLocalScaleXY(localScale);
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
        
        private void LookAtByOrientation(EMazeMoveDirection _Direction, bool _VerticalInverse)
        {
            float angle, horScale;
            (angle, horScale) = _Direction switch
            {
                EMazeMoveDirection.Up    => (90f, 1f),
                EMazeMoveDirection.Right => (0f, 1f),
                EMazeMoveDirection.Down  => (90f, -1f),
                EMazeMoveDirection.Left  => (0f, -1f),
                _                        => throw new SwitchCaseNotImplementedException(_Direction)
            };
            m_HorizontalScaleInverse = horScale < 0f;
            m_VerticalScaleInverse = _VerticalInverse;
            
            var localRot = Quaternion.Euler(Vector3.forward * (angle + GetMazeAngleByCurrentOrientation()));
            m_Head.transform.localRotation = localRot;
            m_Border.transform.localRotation = localRot;
            float vertScale = _VerticalInverse ? -1f : 1f;
            float scaleCoeff = CoordinateConverter.Scale * RelativeLocalScale;
            var localScale = scaleCoeff * new Vector3(horScale, vertScale, 1f);
            m_Head.transform.localScale = localScale;
            m_Border.transform.localScale = localScale;
        }
        
        private void SetDefaultState()
        {
            LookAtByOrientation(EMazeMoveDirection.Right, false);
            m_Animator.SetTrigger(AnimKeyStartJumping);
        }

        private void ActivateShapes(bool _Active)
        {
            m_HeadShape.enabled   = _Active;
            m_BorderShape.enabled = _Active;
            m_Eye1Shape.enabled   = _Active;
            m_Eye2Shape.enabled   = _Active;
        }
        
        private float GetMazeAngleByCurrentOrientation()
        {
            return m_MazeOrientation switch
            {
                MazeOrientation.North => 0f,
                MazeOrientation.East  => 90f,
                MazeOrientation.South => 180f,
                MazeOrientation.West  => 270f,
                _                     => throw new SwitchExpressionException(m_MazeOrientation)
            };
        }

        #endregion
    }
}