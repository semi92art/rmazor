using System;
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
        public V2Int? ShredingerBlockPosWhoStopped { get; }

        public CharacterMovingEventArgs(EMazeMoveDirection _Direction, V2Int _From, V2Int _To, float _Progress, V2Int? _ShredingerBlockPosWhoStopped)
        {
            Direction = _Direction;
            From = _From;
            To = _To;
            PrecisePosition = V2Int.Lerp(_From, _To, _Progress);
            Position = V2Int.Round(PrecisePosition);
            Progress = _Progress;
            ShredingerBlockPosWhoStopped = _ShredingerBlockPosWhoStopped;
        }
    }
    
    public delegate void CharacterMovingHandler(CharacterMovingEventArgs _Args);

    public interface IModelCharacter : IOnLevelStageChanged, IGetAllProceedInfos
    {
        bool Alive { get; }
        V2Int Position { get; }
        bool IsMoving { get; }
        CharacterMovingEventArgs MovingInfo { get; }
        event CharacterMovingHandler CharacterMoveStarted;
        event CharacterMovingHandler CharacterMoveContinued;
        event CharacterMovingHandler CharacterMoveFinished;
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
        private IModelData Data { get; }
        private IPathItemsProceeder PathItemsProceeder { get; }
        private IModelLevelStaging LevelStaging { get; }
        private IGameTicker GameTicker { get; }

        public ModelCharacter(
            ModelSettings _Settings, 
            IModelData _Data, 
            IPathItemsProceeder _PathItemsProceeder,
            IModelLevelStaging _LevelStaging,
            IGameTicker _GameTicker)
        {
            Settings = _Settings;
            Data = _Data;
            PathItemsProceeder = _PathItemsProceeder;
            LevelStaging = _LevelStaging;
            GameTicker = _GameTicker;
        }

        #endregion

        #region api

        public bool Alive { get; private set; } = true;
        public V2Int Position { get; private set; }
        public bool IsMoving { get; private set; }
        public CharacterMovingEventArgs MovingInfo { get; private set; }
        public Func<IEnumerable<IMazeItemProceedInfo>> GetAllProceedInfos { private get; set; }
        public event CharacterMovingHandler CharacterMoveStarted;
        public event CharacterMovingHandler CharacterMoveContinued;
        public event CharacterMovingHandler CharacterMoveFinished;

        public void Move(EMazeMoveDirection _Direction)
        {
            if (!Data.ProceedingControls)
                return;
            if (!Alive)
                return;
            if (LevelStaging.LevelStage == ELevelStage.ReadyToStartOrContinue)
                LevelStaging.StartOrContinueLevel();
            
            var from = Position;
            V2Int? shredingerBlockPosWhoStopped;
            var to = GetNewPosition(from, _Direction, out shredingerBlockPosWhoStopped);
            (IsMoving, MovingInfo) = (true, new CharacterMovingEventArgs(_Direction, from, to, 0, shredingerBlockPosWhoStopped));
            CharacterMoveStarted?.Invoke(MovingInfo);
            Coroutines.Run(MoveCharacterCore(_Direction, from, to, shredingerBlockPosWhoStopped));
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Loaded)
            {
                Position = PathItemsProceeder.PathProceeds.Keys.First();
            }
            else if (_Args.Stage == ELevelStage.ReadyToStartOrContinue)
                Revive(false);
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

        private V2Int GetNewPosition(V2Int _From, EMazeMoveDirection _Direction, out V2Int? _ShredingerBlockPosWhoStopped)
        {
            var nextPos = Position;
            var dirVector = RazorMazeUtils.GetDirectionVector(_Direction, Data.Orientation);
            while (IsNextPositionValid(_From, nextPos, nextPos + dirVector, out _ShredingerBlockPosWhoStopped))
                nextPos += dirVector;
            return nextPos;
        }

        private bool IsNextPositionValid(V2Int _From, V2Int _CurrentPosition, V2Int _NextPosition, out V2Int? _ShredingerBlockPosWhoStopped)
        {
            var proceedInfos = GetAllProceedInfos().ToList();
            _ShredingerBlockPosWhoStopped = null;
            bool isNode = PathItemsProceeder.PathProceeds.Keys
                .Any(_PathItem => _PathItem == _NextPosition);
            if (!isNode)
            {
                bool isPortal = GetAllProceedInfos()
                    .Any(_O => _O.CurrentPosition == _NextPosition 
                               && _O.Type == EMazeItemType.Portal);
                return isPortal;
            }
            
            var shredinger = GetAllProceedInfos()
                .FirstOrDefault(
                    _Info => _Info.Type == EMazeItemType.ShredingerBlock
                             && _Info.CurrentPosition == _NextPosition);

            if (shredinger != null)
            {
                _ShredingerBlockPosWhoStopped = _NextPosition;
                return shredinger.ProceedingStage == ItemsProceederBase.StageIdle;
            }

            bool isMazeItem = proceedInfos.Any(_O => 
                _O.CurrentPosition == _NextPosition
                && (_O.Type == EMazeItemType.Block
                    || _O.Type == EMazeItemType.TrapIncreasing
                    || _O.Type == EMazeItemType.Turret));

            if (isMazeItem)
                return false;
            
            bool isBuzyMazeItem = GetAllProceedInfos()
                .Where(_Info => _Info.Type == EMazeItemType.GravityBlock)
                .Any(_Info => _Info.BusyPositions.Contains(_NextPosition));

            if (isBuzyMazeItem)
                return false;

            bool isPrevPortal = proceedInfos
                .Any(_O => _O.CurrentPosition == _CurrentPosition && _O.Type == EMazeItemType.Portal);
            bool isStartFromPortal = proceedInfos
                .Any(_O => _O.CurrentPosition == _From && _O.Type == EMazeItemType.Portal);

            if (isPrevPortal && !isStartFromPortal)
                return false;
            return true;
        }
        
        private IEnumerator MoveCharacterCore(EMazeMoveDirection _Direction, V2Int _From, V2Int _To, V2Int? _ShredingerBlockPosWhoStopped)
        {
            int thisCount = ++m_Counter;
            int pathLength = Mathf.RoundToInt(V2Int.Distance(_From, _To));
            yield return Coroutines.Lerp(
                0f,
                1f,
                pathLength / Settings.CharacterSpeed,
                _Progress =>
                {
                    MovingInfo = new CharacterMovingEventArgs(_Direction, _From, _To, _Progress, _ShredingerBlockPosWhoStopped);
                    Position = V2Int.Round(MovingInfo.PrecisePosition);
                    CharacterMoveContinued?.Invoke(new CharacterMovingEventArgs(_Direction, _From, _To, _Progress, _ShredingerBlockPosWhoStopped));
                },
                GameTicker,
                (_Stopped, _Progress) =>
                {
                    if (!_Stopped)
                    {
                        CharacterMoveFinished?.Invoke(new CharacterMovingEventArgs(_Direction, _From, _To, 1, _ShredingerBlockPosWhoStopped));
                        Position = _To;
                        IsMoving = false;
                    }
                },
                () => thisCount != m_Counter || !Alive);
        }
        
        private void Revive(bool _WithNotify = true)
        {
            if (Alive)
                return;
            Position = PathItemsProceeder.PathProceeds.First().Key;
            if (_WithNotify)
                LevelStaging.ReadyToStartOrContinueLevel();
            Alive = true;
        }
        
        private void Die()
        {
            if (!Alive)
                return;
            LevelStaging.KillCharacter();
            IsMoving = false;
            Alive = false;
        }

        #endregion
    }
}