﻿using System.Linq;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.MazeItems;
using Managers;
using Managers.Audio;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Games.RazorMaze.Views.Characters
{
    public class ViewCharacter : ViewCharacterBase
    {
        #region constants

        private const int AudioCharacterEndMoveCount = 5;

        #endregion
        
        
        #region nonpublic members

        private bool m_EnableMoving;
        private bool m_Activated;
        private bool m_Initialized;

        #endregion
        
        #region inject

        private IViewCharacterHead          Head              { get; }
        private IViewCharacterTail          Tail              { get; }
        private IViewCharacterEffector      Effector          { get; }
        private IManagersGetter             Managers          { get; }
        private IMazeShaker                 MazeShaker        { get; }
        private IViewInputCommandsProceeder CommandsProceeder { get; }

        public ViewCharacter(
            IViewCharacterHead          _Head,
            IViewCharacterTail          _Tail,
            IViewCharacterEffector      _Effector,
            IMazeCoordinateConverter    _CoordinateConverter, 
            IModelGame                  _Model,
            IContainersGetter           _ContainersGetter,
            IViewMazeCommon             _ViewMazeCommon,
            IManagersGetter             _Managers,
            IMazeShaker                 _MazeShaker,
            IViewInputCommandsProceeder _CommandsProceeder) 
            : base(
                _CoordinateConverter, 
                _Model, 
                _ContainersGetter,
                _ViewMazeCommon)
        {
            Head              = _Head;
            Tail              = _Tail;
            Effector          = _Effector;
            Managers          = _Managers;
            MazeShaker        = _MazeShaker;
            CommandsProceeder = _CommandsProceeder;
        }
        
        #endregion
        
        #region api
        
        public override EAppearingState AppearingState => Head.AppearingState;
        
        public override bool Activated
        {
            get => m_Activated;
            set
            {
                if (value)
                {
                    if (!m_Initialized)
                    {
                        Init();
                        MazeShaker.Init();
                        m_Initialized = true;
                    }
                }
                m_Activated = value;
                Head.Activated = value;
                Tail.Activated = value;
                Effector.Activated = value;
            }
        }

        private void OnCommand(EInputCommand _Command, object[] _Args)
        {
            if (_Command != EInputCommand.KillCharacter)
                return;
            m_EnableMoving = false;
        }
        
        public override void OnRotationAfterFinished(MazeRotationEventArgs _Args) { }

        public override void OnAllPathProceed(V2Int _LastPath)
        {
            Head.OnAllPathProceed(_LastPath);
            Tail.OnAllPathProceed(_LastPath);
            Effector.OnAllPathProceed(_LastPath);
        }

        public override void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            if (!m_EnableMoving)
                return;
            Head.OnCharacterMoveStarted(_Args);
            Tail.OnCharacterMoveStarted(_Args);
            Effector.OnCharacterMoveStarted(_Args);
        }

        public override void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)
        {
            if (!m_EnableMoving)
                return;
            var pos = CoordinateConverter.ToLocalCharacterPosition(_Args.PrecisePosition);
            SetPosition(pos);
            Tail.OnCharacterMoveContinued(_Args);
        }

        public override void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            if (!m_EnableMoving)
                return;
            if (_Args.BlockOnFinish != null && _Args.BlockOnFinish.Type == EMazeItemType.Springboard)
                return;
            Head.OnCharacterMoveFinished(_Args);
            Tail.OnCharacterMoveFinished(_Args);
            Effector.OnCharacterMoveFinished(_Args);
            Coroutines.Run(MazeShaker.HitMazeCoroutine(_Args));
            int randClipId = 1 + Mathf.FloorToInt(AudioCharacterEndMoveCount * Random.value);
            Managers.AudioManager.PlayClip(GetCharacterEndMoveArgs(randClipId));
            Managers.HapticsManager.PlayPreset(EHapticsPresetType.LightImpact);
        }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            Dbg.Log(_Args.Stage);
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    SetDefaultPosition();
                    Activated = true;
                    break;
                case ELevelStage.ReadyToStart:
                    SetDefaultPosition();
                    m_EnableMoving = true;
                    break;
                case ELevelStage.CharacterKilled:
                    Managers.AudioManager.PlayClip(GetCharacterDeadArgs());
                    Managers.HapticsManager.PlayPreset(EHapticsPresetType.Failure);
                    Coroutines.Run(MazeShaker.ShakeMazeCoroutine());
                    break;
            }
            Head.OnLevelStageChanged(_Args);
            Tail.OnLevelStageChanged(_Args);
            Effector.OnLevelStageChanged(_Args);
            MazeShaker.OnLevelStageChanged(_Args);
        }

        public override void Appear(bool _Appear)
        {
            Head.Appear(_Appear);
        }

        #endregion
        
        #region nonpublic methods

        private void Init()
        {
            CommandsProceeder.Command += OnCommand;
            Managers.AudioManager.InitClip(GetCharacterDeadArgs());
            for (int i = 1; i <= AudioCharacterEndMoveCount; i++)
            {
                var args = GetCharacterEndMoveArgs(i);
                Managers.AudioManager.InitClip(args);
            }
        }
        
        private void SetDefaultPosition()
        {
            SetPosition(CoordinateConverter.ToLocalCharacterPosition(Model.Data.Info.Path.First()));
        }

        private static AudioClipArgs GetCharacterDeadArgs()
        {
            return new AudioClipArgs("character_death", EAudioClipType.GameSound, 1f);
        }

        private static AudioClipArgs GetCharacterEndMoveArgs(int _Index)
        {
            return new AudioClipArgs($"character_end_move_{_Index}", EAudioClipType.GameSound);
        }

        #endregion
    }
}