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

        protected bool    EnableMoving;
        private   bool    m_Activated;
        private   Vector2 m_NewPosition;
        private   bool    m_IsMoving;

        #endregion
        
        #region inject

        private   IViewCharacterHead          Head              { get; }
        protected IViewCharacterTail          Tail              { get; }
        private   IViewCharacterEffector      Effector          { get; }
        private   IManagersGetter             Managers          { get; }
        private   IMazeShaker                 MazeShaker        { get; }
        private   IViewInputCommandsProceeder CommandsProceeder { get; }
        protected   IViewGameTicker             ViewGameTicker    { get; }

        protected ViewCharacter(
            IViewCharacterHead          _Head,
            IViewCharacterTail          _Tail,
            IViewCharacterEffector      _Effector,
            ICoordinateConverter        _CoordinateConverter, 
            IModelGame                  _Model,
            IContainersGetter           _ContainersGetter,
            IManagersGetter             _Managers,
            IMazeShaker                 _MazeShaker,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewGameTicker _ViewGameTicker) 
            : base(
                _CoordinateConverter, 
                _Model, 
                _ContainersGetter)
        {
            Head              = _Head;
            Tail              = _Tail;
            Effector          = _Effector;
            Managers          = _Managers;
            MazeShaker        = _MazeShaker;
            CommandsProceeder = _CommandsProceeder;
            ViewGameTicker = _ViewGameTicker;
        }
        
        #endregion
        
        #region api

        public override EAppearingState AppearingState => Head.AppearingState;
        
        public override bool Activated
        {
            get => m_Activated;
            set
            {
                m_Activated = value;
                Head.Activated = value;
                Tail.Activated = value;
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
        }

        public override void OnAllPathProceed(V2Int _LastPath)
        {
            Head.OnAllPathProceed(_LastPath);
            Tail.OnAllPathProceed(_LastPath);
            Effector.OnAllPathProceed(_LastPath);
        }

        public override void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            if (!EnableMoving)
                return;
            m_IsMoving = true;
            Head.OnCharacterMoveStarted(_Args);
            Tail.OnCharacterMoveStarted(_Args);
            Effector.OnCharacterMoveStarted(_Args);
            CommandsProceeder.LockCommands(RmazorUtils.MoveAndRotateCommands, nameof(IViewCharacter));
        }

        public override void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)
        {
            if (!EnableMoving)
                return;
            m_NewPosition = CoordinateConverter.ToLocalCharacterPosition(_Args.PrecisePosition);
            Tail.OnCharacterMoveContinued(_Args);
        }

        public override void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            if (!EnableMoving)
                return;
            if (_Args.BlockOnFinish != null && _Args.BlockOnFinish.Type == EMazeItemType.Springboard)
                return;
            m_IsMoving = false;
            var pos = CoordinateConverter.ToLocalCharacterPosition(_Args.To);
            SetPosition(pos);
            CommandsProceeder.UnlockCommands(RmazorUtils.MoveAndRotateCommands, nameof(IViewCharacter));
            Head.OnCharacterMoveFinished(_Args);
            Tail.OnCharacterMoveFinished(_Args);
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
                    SetDefaultPosition();
                    Activated = true;
                    break;
                case ELevelStage.ReadyToStart:
                    SetDefaultPosition();
                    EnableMoving = true;
                    break;
                case ELevelStage.CharacterKilled:
                    Managers.AudioManager.PlayClip(GetCharacterDeadAudioClipArgs());
                    Managers.HapticsManager.PlayPreset(EHapticsPresetType.Failure);
                    Cor.Run(MazeShaker.ShakeMazeCoroutine(1f, 0.5f));
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

        private void OnCommand(EInputCommand _Command, object[] _Args)
        {
            if (_Command != EInputCommand.KillCharacter)
                return;
            EnableMoving = false;
        }
        
        private void SetDefaultPosition()
        {
            SetPosition(CoordinateConverter.ToLocalCharacterPosition(Model.Data.Info.PathItems[0].Position));
        }

        private static AudioClipArgs GetCharacterDeadAudioClipArgs()
        {
            return new AudioClipArgs("character_death", EAudioClipType.GameSound, 1f);
        }

        private static AudioClipArgs GetCharacterEndMoveAudioClipArgs(int _Index)
        {
            return new AudioClipArgs($"character_end_move_{_Index}", EAudioClipType.GameSound);
        }
        
        protected void SetPosition(Vector2 _Position)
        {
            ContainersGetter.GetContainer(ContainerNames.Character).localPosition = _Position;
        }

        #endregion

        public void FixedUpdateTick()
        {
            if (!m_IsMoving)
                return;
            SetPosition(m_NewPosition);
        }
    }
}