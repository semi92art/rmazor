﻿using System.Collections.Generic;
using Common;
using Common.Constants;
using Common.Entities;
using Common.Enums;
using Common.Helpers;
using Common.Managers;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RMAZOR.Views.Characters
{
    public class ViewCharacter : ViewCharacterBase, IFixedUpdateTick
    {
        #region constants

        private const int AudioCharacterEndMoveCount = 5;

        #endregion
        
        #region nonpublic members

        private bool    m_EnableMoving;
        private bool    m_Activated;
        private Vector2 m_NewPosition;
        private bool    m_IsMoving;

        #endregion
        
        #region inject

        private   IViewCharacterHead          Head              { get; }
        protected IViewCharacterTail          Tail              { get; }
        private   IViewCharacterLegs          Legs              { get; }
        private   IViewCharacterEffector      Effector          { get; }
        private   IManagersGetter             Managers          { get; }
        private   IMazeShaker                 MazeShaker        { get; }
        private   IViewInputCommandsProceeder CommandsProceeder { get; }
        protected IViewGameTicker             ViewGameTicker    { get; }

        private ViewCharacter(
            IViewCharacterHead          _Head,
            IViewCharacterTail          _Tail,
            IViewCharacterLegs          _Legs,
            IViewCharacterEffector      _Effector,
            ICoordinateConverter        _CoordinateConverter,
            IModelGame                  _Model,
            IContainersGetter           _ContainersGetter,
            IManagersGetter             _Managers,
            IMazeShaker                 _MazeShaker,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewGameTicker             _ViewGameTicker) 
            : base(
                _CoordinateConverter, 
                _Model, 
                _ContainersGetter)
        {
            Head              = _Head;
            Tail              = _Tail;
            Legs              = _Legs;
            Effector          = _Effector;
            Managers          = _Managers;
            MazeShaker        = _MazeShaker;
            CommandsProceeder = _CommandsProceeder;
            ViewGameTicker    = _ViewGameTicker;
        }
        
        #endregion
        
        #region api

        public override EAppearingState AppearingState => Head.AppearingState;
        
        public override bool Activated
        {
            get => m_Activated;
            set
            {
                m_Activated        = value;
                Head.Activated     = value;
                Tail.Activated     = value;
                Legs.Activated     = value;
                Effector.Activated = value;
            }
        }
        
        public override void Init()
        {
            if (Initialized)
                return;
            ViewGameTicker.Register(this);
            CommandsProceeder.Command += OnCommand;
            Managers.AudioManager.InitClip(GetCharacterDeadAudioClipArgs());
            for (int i = 1; i <= AudioCharacterEndMoveCount; i++)
            {
                var args = GetCharacterEndMoveAudioClipArgs(i);
                Managers.AudioManager.InitClip(args);
            }
            Tail.GetCharacterObjects = GetObjects;
            Head.Init();
            Tail.Init();
            Legs.Init();
            MazeShaker.Init();
            base.Init();
        }

        public override ViewCharacterInfo GetObjects()
        {
            return new ViewCharacterInfo(Head.Transform, Head.Colliders);
        }

        public override void OnRotationFinished(MazeRotationEventArgs _Args)
        {
            Head.OnRotationFinished(_Args);
            Legs.OnRotationFinished(_Args);
        }

        public override void OnPathCompleted(V2Int _LastPath)
        {
            Head.OnPathCompleted(_LastPath);
            Tail.OnPathCompleted(_LastPath);
            Legs.OnPathCompleted(_LastPath);
            Effector.OnPathCompleted(_LastPath);
        }

        public override void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            if (!m_EnableMoving)
                return;
            var pos = CoordinateConverter.ToLocalCharacterPosition(_Args.From);
            SetPosition(pos);
            m_IsMoving = true;
            Head.OnCharacterMoveStarted(_Args);
            Tail.OnCharacterMoveStarted(_Args);
            Legs.OnCharacterMoveStarted(_Args);
            Effector.OnCharacterMoveStarted(_Args);
            CommandsProceeder.LockCommands(RmazorUtils.MoveAndRotateCommands, nameof(IViewCharacter));
        }

        public override void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)
        {
            if (!m_EnableMoving)
                return;
            m_NewPosition = CoordinateConverter.ToLocalCharacterPosition(_Args.PrecisePosition);
            Tail    .OnCharacterMoveContinued(_Args);
            Legs    .OnCharacterMoveContinued(_Args);
            Effector.OnCharacterMoveContinued(_Args);
        }

        public override void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            if (!m_EnableMoving)
                return;
            if (_Args.BlockOnFinish != null &&
                (_Args.BlockOnFinish.Type == EMazeItemType.Springboard
                 || _Args.BlockOnFinish.Type == EMazeItemType.Portal))
            {
                return;
            }
            m_IsMoving = false;
            var pos = CoordinateConverter.ToLocalCharacterPosition(_Args.To);
            SetPosition(pos);
            CommandsProceeder.UnlockCommands(RmazorUtils.MoveAndRotateCommands, nameof(IViewCharacter));
            Head    .OnCharacterMoveFinished(_Args);
            Tail    .OnCharacterMoveFinished(_Args);
            Legs    .OnCharacterMoveFinished(_Args);
            Effector.OnCharacterMoveFinished(_Args);
            Cor.Run(MazeShaker.HitMazeCoroutine(_Args));
            int randClipId = 1 + Mathf.FloorToInt(AudioCharacterEndMoveCount * Random.value);
            Managers.AudioManager.PlayClip(GetCharacterEndMoveAudioClipArgs(randClipId));
            Managers.HapticsManager.PlayPreset(EHapticsPresetType.RigidImpact);
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded:
                    SetStartOrRevivePosition();
                    Activated = true;
                    break;
                case ELevelStage.ReadyToStart:
                    if (!CommonData.Release)
                        CommandsProceeder.UnlockCommands(RmazorUtils.MoveAndRotateCommands, nameof(IViewCharacter));
                    if (_Args.PreviousStage == ELevelStage.Paused 
                        && _Args.PrePreviousStage == ELevelStage.CharacterKilled)
                    {
                        SetStartOrRevivePosition();
                    }
                    m_EnableMoving = true;
                    break;
                case ELevelStage.CharacterKilled:
                    m_IsMoving = false;
                    Managers.AudioManager.PlayClip(GetCharacterDeadAudioClipArgs());
                    Managers.HapticsManager.PlayPreset(EHapticsPresetType.Failure);
                    Cor.Run(MazeShaker.ShakeMazeCoroutine(1f, 0.5f));
                    break;
            }
            Head      .OnLevelStageChanged(_Args);
            Tail      .OnLevelStageChanged(_Args);
            Legs      .OnLevelStageChanged(_Args);
            Effector  .OnLevelStageChanged(_Args);
            MazeShaker.OnLevelStageChanged(_Args);
        }

        public override void Appear(bool _Appear)
        {
            Head.Appear(_Appear);
            Legs.Appear(_Appear);
        }
        
        public void FixedUpdateTick()
        {
            if (!m_IsMoving)
                return;
            SetPosition(m_NewPosition);
        }

        #endregion
        
        #region nonpublic methods

        private void OnCommand(EInputCommand _Command, Dictionary<string, object> _Args)
        {
            if (_Command != EInputCommand.KillCharacter)
                return;
            m_EnableMoving = false;
        }
        
        private void SetStartOrRevivePosition()
        {
            SetPosition(CoordinateConverter.ToLocalCharacterPosition(Model.Character.Position));
        }

        private static AudioClipArgs GetCharacterDeadAudioClipArgs()
        {
            return new AudioClipArgs("character_death", EAudioClipType.GameSound, 1f);
        }

        private static AudioClipArgs GetCharacterEndMoveAudioClipArgs(int _Index)
        {
            return new AudioClipArgs($"character_end_move_{_Index}", EAudioClipType.GameSound);
        }
        
        private void SetPosition(Vector2 _Position)
        {
            ContainersGetter.GetContainer(ContainerNames.Character).localPosition = _Position;
        }

        #endregion
    }
}