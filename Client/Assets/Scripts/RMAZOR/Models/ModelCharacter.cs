// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery

using System;
using System.Collections;
using System.Collections.Generic;
using Common.Entities;
using Common.Ticker;
using Common.Utils;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.MazeInfos;
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

        private int m_Counter;

        #endregion

        #region inject

        private ModelSettings      Settings     { get; }
        private IModelData         Data         { get; }
        private IModelLevelStaging LevelStaging { get; }
        private IModelGameTicker   GameTicker   { get; }
        private IModelMazeRotation Rotation     { get; }

        public ModelCharacter(
            ModelSettings      _Settings,
            IModelData         _Data,
            IModelLevelStaging _LevelStaging,
            IModelGameTicker   _GameTicker,
            IModelMazeRotation _Rotation)
        {
            Settings     = _Settings;
            Data         = _Data;
            LevelStaging = _LevelStaging;
            GameTicker   = _GameTicker;
            Rotation = _Rotation;
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
            Move(_Args.Direction);
        }

        public void OnSpringboard(SpringboardEventArgs _Args)
        {
            Position = _Args.Info.CurrentPosition;
            Move(_Args.Direction);
        }
        
        public void Move(EMazeMoveDirection _Direction)
        {
            if (!Data.ProceedingControls)
                return;
            if (!Alive)
                return;
            if (LevelStaging.LevelStage == ELevelStage.ReadyToStart)
                LevelStaging.StartOrContinueLevel();
            
            var from = Position;
            var to = GetNewPosition(from, _Direction, out var blockPositionWhoStopped);
            var args = new CharacterMovingStartedEventArgs(_Direction, from, to);
            IsMoving = true;
            CharacterMoveStarted?.Invoke(args);
            Cor.Run(MoveCharacterCore(_Direction, from, to, blockPositionWhoStopped));
        }
        
        #endregion
        
        #region nonpublic methods

        private V2Int GetNewPosition(V2Int _From, EMazeMoveDirection _Direction, out V2Int? _BlockPositionWhoStopped)
        {
            var nextPos = Position;
            var infos = GetAllProceedInfos();
            var dirVector = RmazorUtils.GetDirectionVector(_Direction, Rotation.Orientation);
            while (IsNextPositionValid(
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

        private static bool IsNextPositionValid(
            IReadOnlyList<IMazeItemProceedInfo> _ProceedInfos,
            IReadOnlyList<V2Int>                _PathItems,
            V2Int                               _From,
            V2Int                               _CurrentPosition,
            V2Int                               _NextPosition,
            out V2Int?                          _BlockPositionWhoStopped)
        {
            _BlockPositionWhoStopped = null;
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
                _BlockPositionWhoStopped = _NextPosition;
                return shredinger.ProceedingStage == ModelCommonData.StageIdle;
            }
            IMazeItemProceedInfo diode = null;
            for (int i = 0; i < _ProceedInfos.Count; i++)
            {
                var info = _ProceedInfos[i];
                if (info.Type != EMazeItemType.Diode)
                    continue;
                if (info.CurrentPosition != _NextPosition)
                    continue;
                diode = info;
                break;
            }
            if (diode != null)
            {
                _BlockPositionWhoStopped = _NextPosition;
                bool nextPosIsInvalid = diode.Direction == -_NextPosition + _CurrentPosition;
                return !nextPosIsInvalid;
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
            bool isPrevSpringboard = false;
            if (_CurrentPosition != _From)
            {
                for (int i = 0; i < _ProceedInfos.Count; i++)
                {
                    var info = _ProceedInfos[i];
                    if (info.CurrentPosition != _CurrentPosition)
                        continue;
                    if (info.Type != EMazeItemType.Springboard)
                        continue;
                    isPrevSpringboard = true;
                    break;
                }
            }
            if (isPrevSpringboard)
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
            return !isPrevPortal || isStartFromPortal;
        }
        
        private IEnumerator MoveCharacterCore(
            EMazeMoveDirection _Direction,
            V2Int _From,
            V2Int _To,
            V2Int? _BlockPositionWhoStopped)
        {
            int thisCount = ++m_Counter;
            int pathLength = Mathf.RoundToInt(V2Int.Distance(_From, _To));
            float lastProgress = 0f;
            yield return Cor.Lerp(
                GameTicker,
                pathLength / Settings.characterSpeed,
                _OnProgress: _P =>
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
                },
                _OnFinish: () =>
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
                },
                _BreakPredicate: () => thisCount != m_Counter || !Alive,
                _FixedUpdate: true);
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