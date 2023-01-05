using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common.Constants;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Providers;
using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;
using UnityEngine;

namespace RMAZOR.Views.Characters.Head
{
    public abstract class ViewCharacterHeadBase : InitBase, IViewCharacterHead
    {
        #region constants

        private const float RelativeLocalScale = 0.8f;
        
        #endregion

        #region nonpublic members
        
        private static int _AnimKeyStartJumping2 = Animator.StringToHash("anim5");

        private static int AnimKeyStartMove    => AnimKeys.Anim;
        private static int AnimKeyStartMove2   => AnimKeys.Anim4;
        private static int AnimKeyBump         => AnimKeys.Anim2;
        private static int AnimKeyStartJumping1 => AnimKeys.Anim3;
        private static int AnimKeyStartJumping2 => _AnimKeyStartJumping2;
        
        protected abstract string PrefabName { get; }

        private   EMazeOrientation m_LastMazeOrientation;
        private   EDirection       m_PrevHorDir = EDirection.Right;
        protected GameObject       PrefabObj;
        private   GameObject       m_HeadObj;
        private   GameObject       m_BorderObj;
        private   Animator         m_Animator;
        private   CircleCollider2D m_HeadCollider;

        private bool             m_Activated;
        private bool             m_HorizontalScaleInverse;
        private bool             m_VerticalScaleInverse;
        
        #endregion

        #region inject

        private   ViewSettings                ViewSettings        { get; }
        protected IColorProvider              ColorProvider       { get; }
        private   IContainersGetter           ContainersGetter    { get; }
        private   IPrefabSetManager           PrefabSetManager    { get; }
        private   ICoordinateConverter        CoordinateConverter { get; }
        private   IRendererAppearTransitioner AppearTransitioner  { get; }

        protected ViewCharacterHeadBase(
            ViewSettings                _ViewSettings,
            IColorProvider              _ColorProvider,
            IContainersGetter           _ContainersGetter,
            IPrefabSetManager           _PrefabSetManager,
            ICoordinateConverter        _CoordinateConverter,
            IRendererAppearTransitioner _AppearTransitioner)
        {
            ViewSettings        = _ViewSettings;
            ColorProvider       = _ColorProvider;
            ContainersGetter    = _ContainersGetter;
            PrefabSetManager    = _PrefabSetManager;
            CoordinateConverter = _CoordinateConverter;
            AppearTransitioner  = _AppearTransitioner;
        }
        
        #endregion

        #region api
        
        public EAppearingState AppearingState { get; private set; }
        public Transform       Transform      => m_HeadObj.transform;
        public Collider2D[]    Colliders      => new Collider2D[] {m_HeadCollider};

        public bool Activated
        {
            get => m_Activated;
            set
            {
                if (value)
                    UpdatePrefab();
                ActivateShapes(false);
                m_Activated = value;
            }
        }
        
        public override void Init()
        {
            if (Initialized)
                return;
            ColorProvider.ColorChanged += OnColorChanged;
            InitPrefab();
            base.Init();
        }

        public void OnRotationFinished(MazeRotationEventArgs _Args)
        {
            m_LastMazeOrientation = _Args.NextOrientation;
        }

        public void OnPathCompleted(V2Int _LastPath)
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
                    m_LastMazeOrientation = EMazeOrientation.North;
                    SetOrientation(EDirection.Right, false);
                    break;
                case ELevelStage.ReadyToStart when 
                    _Args.PreviousStage == ELevelStage.Paused 
                    && _Args.PrePreviousStage == ELevelStage.CharacterKilled:
                {
                    SetOrientation(EDirection.Right, false, EMazeOrientation.North);
                    ActivateShapes(true);
                }
                    break;
                case ELevelStage.CharacterKilled:
                    ActivateShapes(false);
                    m_Animator.SetTrigger(AnimKeyStartJumping1);
                    break;
            }
        }
        
        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            SetOrientation(_Args.Direction, false);
            int animKey = AnimKeyStartMove;
            if (m_HorizontalScaleInverse && !m_VerticalScaleInverse
                || !m_HorizontalScaleInverse && m_VerticalScaleInverse)
            {
                animKey = AnimKeyStartMove2;
            }
            m_Animator.SetTrigger(animKey);
        }
        
        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            SetOrientation(_Args.Direction, true);
            m_Animator.SetTrigger(AnimKeyBump);
        }
        
        public void Appear(bool _Appear)
        {
            if (_Appear)
            {
                ActivateShapes(true);
                m_Animator.SetTrigger(AnimKeyStartJumping1);
            }
            AppearingState = _Appear ? EAppearingState.Appearing : EAppearingState.Dissapearing;
            if (_Appear)
                m_Animator.SetTrigger(AnimKeyStartJumping1);
            AppearTransitioner.DoAppearTransition(
                _Appear,
                GetAppearSets(_Appear),
                ViewSettings.betweenLevelTransitionTime,
                () =>
                {
                    AppearingState = _Appear ? EAppearingState.Appeared : EAppearingState.Dissapeared;
                });
        }

        #endregion

        #region nonpublic methods

        protected abstract void OnColorChanged(int _ColorId, Color _Color);
        
        protected virtual void InitPrefab()
        {
            var container = ContainersGetter.GetContainer(ContainerNamesMazor.Character);
            var go = PrefabSetManager.InitPrefab(
                container, 
                "characters",
                PrefabName);
            go.transform.SetLocalPosXY(Vector2.zero);
            PrefabObj      = go;
            m_HeadObj      = go.GetContentItem("head");
            m_BorderObj    = go.GetContentItem("border");
            m_Animator     = go.GetCompItem<Animator>("animator");
            m_HeadCollider = go.GetCompItem<CircleCollider2D>("collider");
            m_HeadCollider.gameObject.layer = LayerMask.NameToLayer(LayerNamesCommon.Gamma);
        }
        
        private void UpdatePrefab()
        {
            var localScale = Vector2.one * CoordinateConverter.Scale * RelativeLocalScale;
            m_HeadObj.transform.SetLocalScaleXY(localScale);
            m_BorderObj.transform.SetLocalScaleXY(localScale);
            SetOrientation(EDirection.Right, false);
            m_Animator.SetTrigger(AnimKeyStartJumping1);
        }
        
        private void SetOrientation(
            EDirection _Direction,
            bool               _OnFinish,
            EMazeOrientation?   _Orientation = null)
        {
            void LookAtByOrientationFinal(bool _VerticalInverse)
            {
                if (!_OnFinish)
                    LookAtByOrientationOnMoveStart(_Direction, _VerticalInverse, _Orientation);
                else 
                    LookAtByOrientationOnMoveFinish(_Direction);
            }
            bool verticalInverse = _Direction switch
            {
                EDirection.Left  => false,
                EDirection.Right => false,
                EDirection.Down  => m_PrevHorDir == EDirection.Right,
                EDirection.Up    => m_PrevHorDir == EDirection.Left,
                _                        => throw new SwitchExpressionException(_Direction)
            };
            LookAtByOrientationFinal(verticalInverse);
            if (!_OnFinish &&(_Direction == EDirection.Right || _Direction == EDirection.Left))
                m_PrevHorDir = _Direction;
        }
        
        private void LookAtByOrientationOnMoveStart(
            EDirection _Direction,
            bool               _VerticalInverse,
            EMazeOrientation?   _Orientation = null)
        {
            float angle, horScale;
            (angle, horScale) = _Direction switch
            {
                EDirection.Left  => (0f, -1f),
                EDirection.Right => (0f, 1f),
                EDirection.Down  => (90f, -1f),
                EDirection.Up    => (90f, 1f),
                _                        => throw new SwitchCaseNotImplementedException(_Direction)
            };
            m_HorizontalScaleInverse = horScale < 0f;
            m_VerticalScaleInverse = _VerticalInverse;
            var localRot = Quaternion.Euler(
                Vector3.forward * (angle + GetMazeAngleByCurrentOrientation(_Orientation)));
            m_HeadObj.transform.localRotation = localRot;
            m_BorderObj.transform.localRotation = localRot;
            float vertScale = _VerticalInverse ? -1f : 1f;
            float scaleCoeff = CoordinateConverter.Scale * RelativeLocalScale;
            var localScale = scaleCoeff * new Vector3(horScale, vertScale, 1f);
            m_HeadObj.transform.localScale = localScale;
            m_BorderObj.transform.localScale = localScale;
        }
        
        private void LookAtByOrientationOnMoveFinish(
            EDirection _Direction,
            EMazeOrientation? _Orientation = null)
        {
            float angle, vertScale;
            (angle, vertScale) = _Direction switch
            {
                EDirection.Left  => (90f, -1f),
                EDirection.Right => (90f, 1f),
                EDirection.Down  => (0f, 1f),
                EDirection.Up    => (0f, -1f),
                _                        => throw new SwitchCaseNotImplementedException(_Direction)
            };
            var localRot = Quaternion.Euler(
                Vector3.forward * (angle + GetMazeAngleByCurrentOrientation(_Orientation)));
            m_HeadObj.transform.localRotation = localRot;
            m_BorderObj.transform.localRotation = localRot;
            float scaleCoeff = CoordinateConverter.Scale * RelativeLocalScale;
            var localScale = scaleCoeff * new Vector3(1f, vertScale, 1f);
            m_HeadObj.transform.localScale = localScale;
            m_BorderObj.transform.localScale = localScale;
        }

        private float GetMazeAngleByCurrentOrientation(EMazeOrientation? _Orientation)
        {
            var oritentation = _Orientation ?? m_LastMazeOrientation;
            return oritentation switch
            {
                EMazeOrientation.North => 0f,
                EMazeOrientation.East  => 90f,
                EMazeOrientation.South => 180f,
                EMazeOrientation.West  => 270f,
                _                     => throw new SwitchExpressionException(oritentation)
            };
        }

        protected abstract void ActivateShapes(bool _Active);

        protected abstract Dictionary<IEnumerable<Component>, Func<Color>> GetAppearSets(bool _Appear);

        #endregion
    }
}