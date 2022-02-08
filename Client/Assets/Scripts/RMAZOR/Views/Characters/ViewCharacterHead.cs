using System;
using System.Collections.Generic;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using Common.SpawnPools;
using Common.Utils;
using GameHelpers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Common;
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.Helpers;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.Utils;
using Shapes;
using UnityEngine;

namespace RMAZOR.Views.Characters
{
    public interface IViewCharacterHead :
        IActivated,
        IOnLevelStageChanged,
        ICharacterMoveStarted,
        ICharacterMoveFinished,
        IAppear
    {
        void OnAllPathProceed(V2Int _LastPath);
    }
    
    public class ViewCharacterHead : IViewCharacterHead
    {
        #region constants

        private const float RelativeLocalScale = 0.98f;
        
        #endregion

        #region nonpublic members
        
        private static int AnimKeyStartJumping => AnimKeys.Anim3;
        private static int AnimKeyStartMove    => AnimKeys.Anim;
        private static int AnimKeyBump         => AnimKeys.Anim2;
        
        private EMazeMoveDirection m_PrevHorDir = EMazeMoveDirection.Right;
        private GameObject         m_Head;
        private Animator           m_Animator;
        private Rectangle          m_HeadShape;
        private Rectangle          m_Eye1Shape, m_Eye2Shape;
        private bool               m_Activated;
        private bool               m_Initialized;

        #endregion

        #region inject

        private IModelGame               Model               { get; }
        private IColorProvider           ColorProvider       { get; }
        private IContainersGetter        ContainersGetter    { get; }
        private IPrefabSetManager        PrefabSetManager    { get; }
        private IMazeCoordinateConverter CoordinateConverter { get; }
        private IViewAppearTransitioner  AppearTransitioner  { get; }

        public ViewCharacterHead(
            IModelGame _Model,
            IColorProvider _ColorProvider,
            IContainersGetter _ContainersGetter,
            IPrefabSetManager _PrefabSetManager,
            IMazeCoordinateConverter _CoordinateConverter,
            IViewAppearTransitioner _AppearTransitioner)
        {
            Model = _Model;
            ColorProvider = _ColorProvider;
            ContainersGetter = _ContainersGetter;
            PrefabSetManager = _PrefabSetManager;
            CoordinateConverter = _CoordinateConverter;
            AppearTransitioner = _AppearTransitioner;
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

                m_HeadShape.enabled = m_Eye1Shape.enabled = m_Eye2Shape.enabled = false;
                m_Activated = value;
            }
        }

        public void OnAllPathProceed(V2Int _LastPath)
        {
            m_HeadShape.enabled = m_Eye1Shape.enabled = m_Eye2Shape.enabled = false;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (m_Animator.IsNotNull())
                m_Animator.speed = _Args.Stage == ELevelStage.Paused ? 0f : 1f;
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    SetDefaultState(false);
                    break;
                case ELevelStage.ReadyToStart:
                    SetDefaultState(true);
                    break;
            }
            if (_Args.Stage == ELevelStage.CharacterKilled)
                m_HeadShape.enabled = m_Eye1Shape.enabled = m_Eye2Shape.enabled = false;
        }
        
        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            SetOrientation(_Args.Direction);
            m_Animator.SetTrigger(AnimKeyStartMove);
        }
        
        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            m_Animator.SetTrigger(AnimKeyBump);
        }
        
        public void Appear(bool _Appear)
        {
            if (_Appear)
            {
                m_HeadShape.enabled = m_Eye1Shape.enabled = m_Eye2Shape.enabled = true;
                m_Animator.SetTrigger(AnimKeyStartJumping);
            }
            AppearingState = _Appear ? EAppearingState.Appearing : EAppearingState.Dissapearing;
            if (_Appear)
                m_Animator.SetTrigger(AnimKeyStartJumping);
            AppearTransitioner.DoAppearTransition(
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

        private void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.Character)
                m_HeadShape.Color = _Color;
            else if (_ColorId == ColorIds.Background)
                m_Eye1Shape.Color = m_Eye2Shape.Color = _Color;
        }
        
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
        
        private void SetDefaultState(bool _EnableHead)
        {
            Cor.Run(Cor.WaitWhile(
                () => !m_Initialized,
                () =>
                {
                    LookAtByOrientation(EMazeMoveDirection.Right, false);
                    // FIXME это нужно отрефакторить будет
                    if (_EnableHead)
                        m_HeadShape.enabled = m_Eye1Shape.enabled = m_Eye2Shape.enabled = true;
                    m_Animator.SetTrigger(AnimKeyStartJumping);
                }));
        }

        #endregion
    }
}