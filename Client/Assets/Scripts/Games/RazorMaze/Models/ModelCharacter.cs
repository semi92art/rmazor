﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Models.ProceedInfos;
using Games.RazorMaze.Views;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Models
{
    public class CharacterMovingEventArgs : EventArgs
    {
        public EMazeMoveDirection Direction { get; }
        public V2Int From { get; }
        public V2Int To { get; }
        public V2Int Position { get; }
        public Vector2 PrecisePosition { get; }
        public float Progress { get; }

        public CharacterMovingEventArgs(EMazeMoveDirection _Direction, V2Int _From, V2Int _To, float _Progress)
        {
            Direction = _Direction;
            From = _From;
            To = _To;
            PrecisePosition = V2Int.Lerp(_From, _To, _Progress);
            Position = V2Int.Round(PrecisePosition);
            Progress = _Progress;
        }
    }
    
    public delegate void CharacterMovingHandler(CharacterMovingEventArgs _Args);

    public interface IModelCharacter : IInit, IOnLevelStageChanged
    {
        bool Alive { get; }
        V2Int Position { get; }
        bool IsMoving { get; }
        CharacterMovingEventArgs MovingInfo { get; }
        Func<EMazeItemType, IEnumerable<IMazeItemProceedInfo>> GetProceedInfos { get; set; }
        event CharacterMovingHandler CharacterMoveStarted;
        event CharacterMovingHandler CharacterMoveContinued;
        event CharacterMovingHandler CharacterMoveFinished;
        event BoolHandler AliveOrDeath;
        event V2IntHandler PositionSet;
        void Move(EMazeMoveDirection _Direction);
        void OnPortal(PortalEventArgs _Args);
        void OnSpringboard(SpringboardEventArgs _Args);
        void RaiseDeath();
    }

    public class ModelCharacter : IModelCharacter
    {
        #region nonpublic members

        private int m_Counter;

        #endregion

        #region inject

        private ModelSettings Settings { get; }
        private IModelMazeData Data { get; }
        private IPathItemsProceeder PathItemsProceeder { get; }
        private IInputScheduler InputScheduler { get; }
        private ILevelStagingModel LevelStagingModel { get; }
        private IGameTicker GameTicker { get; }

        public ModelCharacter(
            ModelSettings _Settings, 
            IModelMazeData _Data, 
            IPathItemsProceeder _PathItemsProceeder,
            IInputScheduler _InputScheduler,
            ILevelStagingModel _LevelStagingModel,
            IGameTicker _GameTicker)
        {
            Settings = _Settings;
            Data = _Data;
            PathItemsProceeder = _PathItemsProceeder;
            InputScheduler = _InputScheduler;
            LevelStagingModel = _LevelStagingModel;
            GameTicker = _GameTicker;
        }

        #endregion

        #region api

        public bool Alive { get; private set; }
        public V2Int Position { get; private set; }
        public bool IsMoving { get; private set; }
        public CharacterMovingEventArgs MovingInfo { get; private set; }
        public Func<EMazeItemType, IEnumerable<IMazeItemProceedInfo>> GetProceedInfos { get; set; }
        public event CharacterMovingHandler CharacterMoveStarted;
        public event CharacterMovingHandler CharacterMoveContinued;
        public event CharacterMovingHandler CharacterMoveFinished;
        public event BoolHandler AliveOrDeath;
        public event V2IntHandler PositionSet;


        public void Init()
        {
            Alive = true;
            Initialized?.Invoke();
        }

        public event NoArgsHandler Initialized;

        public void Move(EMazeMoveDirection _Direction)
        {
            if (!Data.ProceedingControls)
                return;
            if (!Alive)
                return;
            if (LevelStagingModel.LevelStage == ELevelStage.ReadyToStartOrContinue)
                LevelStagingModel.StartOrContinueLevel();
            
            var from = Position;
            var to = GetNewPosition(from, _Direction);
            (IsMoving, MovingInfo) = (true, new CharacterMovingEventArgs(_Direction, from, to, 0));
            CharacterMoveStarted?.Invoke(MovingInfo);
            Coroutines.Run(MoveCharacterCore(_Direction, from, to));
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            bool unlockMovement = _Args.Stage == ELevelStage.StartedOrContinued 
                                  || _Args.Stage == ELevelStage.ReadyToStartOrContinue;
            InputScheduler.UnlockMovement(unlockMovement);

            if (_Args.Stage == ELevelStage.Loaded)
            {
                Position = Data.Info.Path.First();
                PositionSet?.Invoke(Position);
            }
            else if (_Args.Stage == ELevelStage.ReadyToStartOrContinue)
                Revive();
        }
        
        public void OnPortal(PortalEventArgs _Args)
        {
            Position = _Args.Info.Pair;
            Move(_Args.Direction);
        }

        public void OnSpringboard(SpringboardEventArgs _Args)
        {
            Move(_Args.Direction);
        }

        public void RaiseDeath()
        {
            Die();
        }

        #endregion
        
        #region nonpublic methods

        private V2Int GetNewPosition(V2Int _From, EMazeMoveDirection _Direction)
        {
            var nextPos = Position;
            var dirVector = RazorMazeUtils.GetDirectionVector(_Direction, Data.Orientation);
            while (IsNextPositionValid(_From, nextPos, nextPos + dirVector, Data.Info))
                nextPos += dirVector;
            return nextPos;
        }

        private bool IsNextPositionValid(V2Int _From, V2Int _CurrentPosition, V2Int _NextPosition, MazeInfo _Info)
        {
            bool isNode = _Info.Path.Any(_PathItem => _PathItem == _NextPosition);

            if (!isNode)
            {
                bool isPortal = _Info.MazeItems
                    .Any(_O => _O.Position == _NextPosition && _O.Type == EMazeItemType.Portal);
                return isPortal;
            }
            
            var shredinger = GetProceedInfos(EMazeItemType.ShredingerBlock)
                .FirstOrDefault(_Inf => _Inf.CurrentPosition == _NextPosition);

            if (shredinger != null)
                return shredinger.ProceedingStage != ShredingerBlocksProceeder.StageClosed;

            bool isMazeItem = _Info.MazeItems.Any(_O => 
                _O.Position == _NextPosition
                && (_O.Type == EMazeItemType.Block
                    || _O.Type == EMazeItemType.TrapIncreasing
                    || _O.Type == EMazeItemType.Turret));

            if (isMazeItem)
                return false;
            
            bool isBuzyMazeItem = GetProceedInfos(EMazeItemType.GravityBlock)
                .Where(_Inf => _Inf.Type == EMazeItemType.GravityBlock)
                .Any(_Inf => _Inf.BusyPositions.Contains(_NextPosition));

            if (isBuzyMazeItem)
                return false;

            bool isPrevPortal = _Info.MazeItems
                .Any(_O => _O.Position == _CurrentPosition && _O.Type == EMazeItemType.Portal);
            bool isStartFromPortal = _Info.MazeItems
                .Any(_O => _O.Position == _From && _O.Type == EMazeItemType.Portal);

            if (isPrevPortal && !isStartFromPortal)
                return false;
            return true;
        }
        
        private IEnumerator MoveCharacterCore(EMazeMoveDirection _Direction, V2Int _From, V2Int _To)
        {
            int thisCount = ++m_Counter;
            int pathLength = Mathf.RoundToInt(V2Int.Distance(_From, _To));
            yield return Coroutines.Lerp(
                0f,
                1f,
                pathLength / Settings.characterSpeed,
                _Progress =>
                {
                    MovingInfo = new CharacterMovingEventArgs(_Direction, _From, _To, _Progress);
                    Position = V2Int.Round(MovingInfo.PrecisePosition);
                    CharacterMoveContinued?.Invoke(new CharacterMovingEventArgs(_Direction, _From, _To, _Progress));
                },
                GameTicker,
                (_Stopped, _Progress) =>
                {
                    if (!_Stopped)
                    {
                        CharacterMoveFinished?.Invoke(new CharacterMovingEventArgs(_Direction, _From, _To, 1));
                        Position = _To;
                        IsMoving = false;
                    }
                },
                () => thisCount != m_Counter || !Alive);
        }
        
        private void Revive()
        {
            if (Alive)
                return;
            Position = PathItemsProceeder.PathProceeds.First().Key;
            PositionSet?.Invoke(Position);
            InputScheduler.UnlockMovement(true);
            AliveOrDeath?.Invoke(true);
            Alive = true;
        }
        
        private void Die()
        {
            if (!Alive)
                return;
            InputScheduler.UnlockMovement(false);
            AliveOrDeath?.Invoke(false);
            IsMoving = false;
            Alive = false;
        }
        
        #endregion
    }
}