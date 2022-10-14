using System;
using System.Collections;
using Common.Entities;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views;
using UnityEngine;

namespace RMAZOR.Models
{
    public delegate void CharacterMovingStartedHandler(CharacterMovingStartedEventArgs _Args);
    public delegate void CharacterMovingContinuedHandler(CharacterMovingContinuedEventArgs _Args);
    public delegate void CharacterMovingFinishedHandler(CharacterMovingFinishedEventArgs _Args);

    public interface IModelCharacter : IOnLevelStageChanged, IGetAllProceedInfos
    {
        bool                                  Alive            { get; }
        V2Int                                 Position         { get; }
        bool                                  IsMoving         { get; }
        CharacterMovingContinuedEventArgs     MovingInfo       { get; }
        Func<V2Int>                           GetStartPosition { get; set; }
        event CharacterMovingStartedHandler   CharacterMoveStarted;
        event CharacterMovingContinuedHandler CharacterMoveContinued;
        event CharacterMovingFinishedHandler  CharacterMoveFinished;
        void                                  Move(EMazeMoveDirection _Direction);
        void                                  OnPortal(PortalEventArgs _Args);
        void                                  OnSpringboard(SpringboardEventArgs _Args);
    }

    public class ModelCharacter : IModelCharacter
    {
        #region nonpublic members

        private int  m_MovesCount;

        #endregion

        #region inject

        private ModelSettings                    Settings          { get; }
        private IModelData                       Data              { get; }
        private IModelLevelStaging               LevelStaging      { get; }
        private IModelGameTicker                 ModelGameTicker        { get; }
        private IModelMazeRotation               Rotation          { get; }
        private IModelCharacterPositionValidator PositionValidator { get; }

        public ModelCharacter(
            ModelSettings                    _Settings,
            IModelData                       _Data,
            IModelLevelStaging               _LevelStaging,
            IModelGameTicker                 _ModelGameTicker,
            IModelMazeRotation               _Rotation,
            IModelCharacterPositionValidator _PositionValidator)
        {
            Settings          = _Settings;
            Data              = _Data;
            LevelStaging      = _LevelStaging;
            ModelGameTicker        = _ModelGameTicker;
            Rotation          = _Rotation;
            PositionValidator = _PositionValidator;
        }

        #endregion

        #region api

        public bool                              Alive      { get; private set; } = true;
        public V2Int                             Position   { get; private set; }
        public bool                              IsMoving   { get; private set; }
        public CharacterMovingContinuedEventArgs MovingInfo { get; private set; }
        
        public Func<V2Int>                           GetStartPosition   { get;         set; }
        public Func<IMazeItemProceedInfo[]>          GetAllProceedInfos { private get; set; }
        public event CharacterMovingStartedHandler   CharacterMoveStarted;
        public event CharacterMovingContinuedHandler CharacterMoveContinued;
        public event CharacterMovingFinishedHandler  CharacterMoveFinished;

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.LevelStage)
            {
                case ELevelStage.Loaded:          Position = GetStartPosition(); break;
                case ELevelStage.ReadyToStart:    Revive(false);      break;
                case ELevelStage.CharacterKilled: Die();                         break;
            }
        }
        
        public void OnPortal(PortalEventArgs _Args)
        {
            Position = _Args.Info.Pair;
            MoveCore(_Args.Direction, true);
        }

        public void OnSpringboard(SpringboardEventArgs _Args)
        {
            Position = _Args.Info.CurrentPosition;
            MoveCore(_Args.Direction, false);
        }
        
        public void Move(EMazeMoveDirection _Direction)
        {
            MoveCore(_Direction, false);
        }
        
        #endregion
        
        #region nonpublic methods

        private void MoveCore(EMazeMoveDirection _Direction, bool _FromPortal)
        {           
            if (!Data.ProceedingControls)
                return;
            if (!Alive)
                return;
            if (LevelStaging.LevelStage == ELevelStage.ReadyToStart)
                LevelStaging.StartOrContinueLevel();
            
            var from = Position;
            var to = GetNewPosition(from, _Direction, out var blockPositionWhoStopped);
            var args = new CharacterMovingStartedEventArgs(_Direction, from, to, _FromPortal);
            IsMoving = true;
            CharacterMoveStarted?.Invoke(args);
            Cor.Run(MoveCoreCoroutine(_Direction, from, to, blockPositionWhoStopped));
            
        }
        
        private V2Int GetNewPosition(
            V2Int              _From,
            EMazeMoveDirection _Direction,
            out V2Int?         _BlockPositionWhoStopped)
        {
            var nextPos = Position;
            var infos = GetAllProceedInfos();
            var dirVector = RmazorUtils.GetDirectionVector(_Direction, Rotation.Orientation);
            while (PositionValidator.IsNextPositionValid(
                infos,
                Data.PathItems,
                _From,
                nextPos,
                nextPos + dirVector,
                out _BlockPositionWhoStopped))
            {
                nextPos += dirVector;
            }
            return nextPos;
        }

        private IEnumerator MoveCoreCoroutine(
            EMazeMoveDirection _Direction,
            V2Int              _From,
            V2Int              _To,
            V2Int?             _BlockPositionWhoStopped)
        {
            int thisCount = ++m_MovesCount;
            int pathLength = Mathf.RoundToInt(V2Int.Distance(_From, _To));
            float lastProgress = 0f;
            void OnProgress(float _P)
            {
                MovingInfo = new CharacterMovingContinuedEventArgs(
                    _Direction,
                    _From,
                    _To,
                    _P,
                    lastProgress);
                lastProgress = _P;
                Position = V2Int.Round(MovingInfo.PrecisePosition);
                CharacterMoveContinued?.Invoke(MovingInfo);
            }
            void OnFinish()
            {
                var infos = GetAllProceedInfos();
                IMazeItemProceedInfo blockOnFinish = null;
                for (int i = 0; i < infos.Length; i++)
                {
                    var info = infos[i];
                    if (info.StartPosition != _To)
                        continue;
                    blockOnFinish = info;
                    break;
                }
                IMazeItemProceedInfo blockWhoStopped = null;
                if (_BlockPositionWhoStopped.HasValue)
                {
                    for (int i = 0; i < infos.Length; i++)
                    {
                        var info = infos[i];
                        if (info.StartPosition != _BlockPositionWhoStopped.Value)
                            continue;
                        blockWhoStopped = info;
                        break;
                    }
                }
                var args = new CharacterMovingFinishedEventArgs(
                    _Direction,
                    _From,
                    _To,
                    blockOnFinish,
                    blockWhoStopped);
                CharacterMoveFinished?.Invoke(args);
                IsMoving = false;
                Position = _To;
            }
            bool BreakPredicate()
            {
                return thisCount != m_MovesCount || !Alive;
            }
            yield return Cor.Lerp(
                ModelGameTicker,
                pathLength / Settings.characterSpeed,
                _OnProgress:     OnProgress,
                _OnFinish:       OnFinish,
                _BreakPredicate: BreakPredicate,
                _FixedUpdate:    true);
        }
        
        private void Revive(bool _WithNotify = true)
        {
            if (Alive)
                return;
            Position = GetStartPosition();
            if (_WithNotify)
                LevelStaging.ReadyToStartLevel();
            Alive = true;
        }
        
        private void Die()
        {
            if (!Alive)
                return;
            IsMoving = false;
            Alive = false;
        }

        #endregion
    }
}