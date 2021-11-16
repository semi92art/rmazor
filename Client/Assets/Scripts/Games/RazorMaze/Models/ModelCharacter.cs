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
        public EMazeMoveDirection Direction                    { get; }
        public V2Int              From                         { get; }
        public V2Int              To                           { get; }
        public V2Int              Position                     { get; }
        public Vector2            PrecisePosition              { get; }
        public Vector2            PreviousPrecisePosition      { get; }
        public float              Progress                     { get; }
        public V2Int?             ShredingerBlockPosWhoStopped { get; }

        public CharacterMovingEventArgs(
            EMazeMoveDirection _Direction, 
            V2Int _From,
            V2Int _To,
            float _Progress,
            float _PreviousProgress,
            V2Int? _ShredingerBlockPosWhoStopped)
        {
            Direction = _Direction;
            From = _From;
            To = _To;
            PrecisePosition = V2Int.Lerp(_From, _To, _Progress);
            PreviousPrecisePosition = V2Int.Lerp(_From, _To, _PreviousProgress);
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
        private IModelGameTicker GameTicker { get; }

        public ModelCharacter(
            ModelSettings _Settings, 
            IModelData _Data, 
            IPathItemsProceeder _PathItemsProceeder,
            IModelLevelStaging _LevelStaging,
            IModelGameTicker _GameTicker)
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
        public Func<List<IMazeItemProceedInfo>> GetAllProceedInfos { private get; set; }
        public event CharacterMovingHandler CharacterMoveStarted;
        public event CharacterMovingHandler CharacterMoveContinued;
        public event CharacterMovingHandler CharacterMoveFinished;

        public void Move(EMazeMoveDirection _Direction)
        {
            if (!Data.ProceedingControls)
                return;
            if (!Alive)
                return;
            if (LevelStaging.LevelStage == ELevelStage.ReadyToStart)
                LevelStaging.StartOrContinueLevel();
            
            var from = Position;
            V2Int? shredingerBlockPosWhoStopped;
            var to = GetNewPosition(from, _Direction, out shredingerBlockPosWhoStopped);
            var args = new CharacterMovingEventArgs(
                _Direction,
                from, 
                to, 
                0, 
                0f,
                shredingerBlockPosWhoStopped);
            (IsMoving, MovingInfo) = (true, args);
            CharacterMoveStarted?.Invoke(MovingInfo);
            Coroutines.Run(MoveCharacterCore(_Direction, from, to, shredingerBlockPosWhoStopped));
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.Stage == ELevelStage.Loaded)
            {
                Position = PathItemsProceeder.PathProceeds.Keys.First();
            }
            else if (_Args.Stage == ELevelStage.ReadyToStart)
                Revive(false);
            else if (_Args.Stage == ELevelStage.CharacterKilled)
                Die();
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

        #endregion
        
        #region nonpublic methods

        private V2Int GetNewPosition(V2Int _From, EMazeMoveDirection _Direction, out V2Int? _ShredingerBlockPosWhoStopped)
        {
            var nextPos = Position;
            var infos = GetAllProceedInfos();
            var dirVector = RazorMazeUtils.GetDirectionVector(_Direction, Data.Orientation);
            var pathItems = Data.Info.Path;
            while (IsNextPositionValid(
                infos,
                pathItems,
                _From,
                nextPos,
                nextPos + dirVector,
                out _ShredingerBlockPosWhoStopped))
            {
                nextPos += dirVector;
            }
            return nextPos;
        }

        private bool IsNextPositionValid(
            IReadOnlyList<IMazeItemProceedInfo> _ProceedInfos,
            IReadOnlyList<V2Int> _PathItems,
            V2Int _From,
            V2Int _CurrentPosition,
            V2Int _NextPosition, 
            out V2Int? _ShredingerBlockPosWhoStopped)
        {
            _ShredingerBlockPosWhoStopped = null;

            bool isNode = false;
            for (int i = 0; i < _PathItems.Count; i++)
            {
                if (_PathItems[i] != _NextPosition)
                    continue;
                isNode = true;
                break;
            }

            if (!isNode)
            {
                for (int i = 0; i < _ProceedInfos.Count; i++)
                {
                    var info = _ProceedInfos[i];
                    if (info.CurrentPosition != _NextPosition)
                        continue;
                    if (info.Type != EMazeItemType.Portal)
                        continue;
                    return true;
                }
                return false;
            }

            IMazeItemProceedInfo shredinger = null;
            for (int i = 0; i < _ProceedInfos.Count; i++)
            {
                var info = _ProceedInfos[i];
                if (info.Type != EMazeItemType.ShredingerBlock)
                    continue;
                if (info.CurrentPosition != _NextPosition)
                    continue;
                shredinger = info;
                break;
            }

            if (shredinger != null)
            {
                _ShredingerBlockPosWhoStopped = _NextPosition;
                return shredinger.ProceedingStage == ItemsProceederBase.StageIdle;
            }

            bool isMazeItem = false;
            for (int i = 0; i < _ProceedInfos.Count; i++)
            {
                var info = _ProceedInfos[i];
                if (info.CurrentPosition != _NextPosition)
                    continue;
                if (info.Type != EMazeItemType.Block 
                    && info.Type != EMazeItemType.TrapIncreasing
                    && info.Type != EMazeItemType.Turret)
                    continue;
                isMazeItem = true;
                break;
            }

            if (isMazeItem)
                return false;

            bool isBuzyMazeItem = false;
            for (int i = 0; i < _ProceedInfos.Count; i++)
            {
                var info = _ProceedInfos[i];
                if (info.Type != EMazeItemType.GravityBlock && info.Type != EMazeItemType.GravityBlockFree)
                    continue;
                bool busyPositionsContainNext = false;
                for (int j = 0; j < info.BusyPositions.Count; j++)
                {
                    if (info.BusyPositions[j] != _NextPosition)
                        continue;
                    busyPositionsContainNext = true;
                    break;
                }
                if (!busyPositionsContainNext)
                    continue;
                isBuzyMazeItem = true;
                break;
            }

            if (isBuzyMazeItem)
                return false;
            
            bool isPrevPortal = false;
            for (int i = 0; i < _ProceedInfos.Count; i++)
            {
                var info = _ProceedInfos[i];
                if (info.CurrentPosition != _CurrentPosition)
                    continue;
                if (info.Type != EMazeItemType.Portal)
                    continue;
                isPrevPortal = true;
                break;
            }
            
            bool isStartFromPortal = false;
            for (int i = 0; i < _ProceedInfos.Count; i++)
            {
                var info = _ProceedInfos[i];
                if (info.CurrentPosition != _From)
                    continue;
                if (info.Type != EMazeItemType.Portal)
                    continue;
                isStartFromPortal = true;
                break;
            }

            if (isPrevPortal && !isStartFromPortal)
                return false;
            return true;
        }
        
        private IEnumerator MoveCharacterCore(
            EMazeMoveDirection _Direction,
            V2Int _From,
            V2Int _To,
            V2Int? _ShredingerBlockPosWhoStopped)
        {
            int thisCount = ++m_Counter;
            int pathLength = Mathf.RoundToInt(V2Int.Distance(_From, _To));
            float lastProgress = 0f;
            yield return Coroutines.Lerp(
                0f,
                1f,
                pathLength / Settings.CharacterSpeed,
                _Progress =>
                {
                    MovingInfo = new CharacterMovingEventArgs(
                        _Direction,
                        _From,
                        _To,
                        _Progress,
                        lastProgress,  
                        _ShredingerBlockPosWhoStopped);
                    lastProgress = _Progress;
                    Position = V2Int.Round(MovingInfo.PrecisePosition);
                    CharacterMoveContinued?.Invoke(MovingInfo);
                },
                GameTicker,
                (_Stopped, _Progress) =>
                {
                    if (_Stopped) 
                        return;
                    var args = new CharacterMovingEventArgs(
                        _Direction,
                        _From,
                        _To,
                        1, 
                        lastProgress,
                        _ShredingerBlockPosWhoStopped);
                    CharacterMoveFinished?.Invoke(args);
                    Position = _To;
                    IsMoving = false;
                },
                () => thisCount != m_Counter || !Alive);
        }
        
        private void Revive(bool _WithNotify = true)
        {
            if (Alive)
                return;
            Position = PathItemsProceeder.PathProceeds.First().Key;
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