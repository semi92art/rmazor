﻿using System;
using System.Collections;
using System.Collections.Generic;
using Constants;
using DI.Extensions;
using Entities;
using Exceptions;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.MazeItems;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.Characters
{
    public class ViewCharacter : ViewCharacterBase
    {
        #region constants

        private const string SoundClipNameCharacterMoveEnd = "character_move_end";
        private const string SoundClipNameCharacterDeath = "character_death";
        
        #endregion
        
        #region shapes
        
        private Rectangle m_HeadShape;
        private Rectangle m_Eye1Shape, m_Eye2Shape;
        
        #endregion
        
        #region nonpublic members

        private static int AnimKeyStartJumping => AnimKeys.Anim3;
        private static int AnimKeyStartMove => AnimKeys.Anim;
        private static int AnimKeyBump => AnimKeys.Anim2;
        
        private GameObject m_Head;
        private Animator m_Animator;
        private EMazeMoveDirection m_PrevVertDir;
        private EMazeMoveDirection m_PrevHorDir;
        private bool m_Activated;
        private bool m_Initialized;
        private Color m_BackColor;

        #endregion
        
        #region inject
        
        private IViewCharacterTail Tail { get; }
        private IViewCharacterEffector Effector { get; }
        private IGameTicker GameTicker { get; }
        private IViewAppearTransitioner Transitioner { get; }
        private IManagersGetter Managers { get; }
        private ViewSettings ViewSettings { get; }

        public ViewCharacter(
            ICoordinateConverter _CoordinateConverter, 
            IModelGame _Model,
            IContainersGetter _ContainersGetter,
            IViewMazeCommon _ViewMazeCommon,
            IViewCharacterTail _Tail,
            IViewCharacterEffector _Effector,
            IGameTicker _GameTicker,
            IViewAppearTransitioner _Transitioner,
            IManagersGetter _Managers,
            ViewSettings _ViewSettings) 
            : base(
                _CoordinateConverter, 
                _Model, 
                _ContainersGetter,
                _ViewMazeCommon)
        {
            Tail = _Tail;
            Effector = _Effector;
            GameTicker = _GameTicker;
            Transitioner = _Transitioner;
            ViewSettings = _ViewSettings;
            Managers = _Managers;
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
                        InitPrefab();
                        m_Initialized = true;
                    }
                    UpdatePrefab();
                }
                
                m_Activated = value;
                m_HeadShape.enabled = m_Eye1Shape.enabled = m_Eye2Shape.enabled = value;
                Tail.Activated = value;
                Effector.Activated = value;
                if (value)
                    m_Animator.SetTrigger(AnimKeyStartJumping);
            }
        }

        public override void OnCharacterMoveStarted(CharacterMovingEventArgs _Args)
        {
            m_Animator.SetTrigger(AnimKeyStartMove);
            SetOrientation(_Args.Direction);
            Tail.ShowTail(_Args);
        }

        public override void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            var prevPos = CoordinateConverter.ToLocalCharacterPosition(_Args.From);
            var nextPos = CoordinateConverter.ToLocalCharacterPosition(_Args.To);
            var pos = Vector2.Lerp(prevPos, nextPos, _Args.Progress);
            SetPosition(pos);
            Tail.ShowTail(_Args);
        }

        public override void OnCharacterMoveFinished(CharacterMovingEventArgs _Args)
        {
            m_Animator.SetTrigger(AnimKeyBump);
            Tail.HideTail(_Args);
            Managers.Notify(_SM => _SM.PlayClip(SoundClipNameCharacterMoveEnd));
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    SetPosition(CoordinateConverter.ToLocalCharacterPosition(Model.Character.Position));
                    Activated = true;
                    break;
                case ELevelStage.CharacterKilled:
                    m_HeadShape.enabled = m_Eye1Shape.enabled = m_Eye2Shape.enabled = false;
                    Tail.HideTail();
                    Managers.Notify(_SM => _SM.PlayClip(SoundClipNameCharacterDeath));
                    Coroutines.Run(ShakeMaze());
                    break;
                case ELevelStage.ReadyToStartOrContinue:
                    SetPosition(CoordinateConverter.ToLocalCharacterPosition(Model.Character.Position));
                    m_HeadShape.enabled = m_Eye1Shape.enabled = m_Eye2Shape.enabled = true;
                    Tail.Activated = true;
                    m_Animator.SetTrigger(AnimKeyStartJumping);
                    Tail.HideTail();
                    break;
            }
            Effector.OnLevelStageChanged(_Args);
        }

        public override void OnBackgroundColorChanged(Color _Color)
        {
            m_BackColor = _Color;
            m_Eye1Shape.Color = m_Eye2Shape.Color = _Color;
        }
        
        public override void Appear(bool _Appear)
        {
            Tail.HideTail();
            AppearingState = _Appear ? EAppearingState.Appearing : EAppearingState.Dissapearing;
            if (_Appear)
                m_Animator.SetTrigger(AnimKeyStartJumping);
            Transitioner.DoAppearTransitionSimple(
                _Appear,
                GameTicker,
                new Dictionary<object[], Func<Color>>
                {
                    {new object[] {m_HeadShape}, () => DrawingUtils.ColorCharacter},
                    {new object[] {m_Eye1Shape, m_Eye2Shape}, () => m_BackColor}
                },
                Model.Character.Position,
                () => AppearingState = _Appear ?
                    EAppearingState.Appeared : EAppearingState.Dissapeared);
        }

        #endregion
        
        #region nonpublic methods
        
        private void InitPrefab()
        {
            var go = ContainersGetter.CharacterContainer.gameObject;
            var prefab = PrefabUtilsEx.InitPrefab(go.transform, CommonPrefabSetNames.Views, "character");
            prefab.transform.SetLocalPosXY(Vector2.zero);
            m_Head = prefab.GetContentItem("head");

            m_Animator = prefab.GetCompItem<Animator>("animator");
            
            m_HeadShape = prefab.GetCompItem<Rectangle>("head shape");
            m_Eye1Shape = prefab.GetCompItem<Rectangle>("eye_1");
            m_Eye2Shape = prefab.GetCompItem<Rectangle>("eye_2");

            m_HeadShape.enabled = m_Eye1Shape.enabled = m_Eye2Shape.enabled = false;
            
            m_Initialized = true;
        }

        private void UpdatePrefab()
        {
            var localScale = Vector3.one * CoordinateConverter.GetScale() * 0.98f;
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
            float vertScale = _Inverse ? -1f : 1f;
            m_Head.transform.localScale = new Vector3(absScale.x * horScale, absScale.y * vertScale, absScale.z);
        }
        
        private IEnumerator ShakeMaze()
        {
            var defPos = ContainersGetter.MazeContainer.position;
            const float duration = 0.5f;
            yield return Coroutines.Lerp(
                1f,
                0f,
                duration,
                _Progress =>
                {
                    float amplitude = 0.25f * _Progress;
                    Vector2 res;
                    res.x = defPos.x + amplitude * Mathf.Sin(GameTicker.Time * 200f);
                    res.y = defPos.y + amplitude * Mathf.Cos(GameTicker.Time * 100f);
                    ContainersGetter.MazeContainer.position = res;
                },
                GameTicker,
                (_Finished, _Progress) =>
                {
                    ContainersGetter.MazeContainer.position = defPos;
                });
        }

        #endregion
    }
}