using System;
using Common.Entities;
using mazing.common.Runtime.Entities;
using RMAZOR.Models.ProceedInfos;
using UnityEngine;

namespace RMAZOR.Models
{
    public abstract class CharacterMoveEventArgsBase : EventArgs
    {
        public EDirection Direction { get; }
        public V2Int              From      { get; }
        public V2Int              To        { get; }
        public float              Progress  { get; }

        protected CharacterMoveEventArgsBase(
            EDirection _Direction,
            V2Int              _From,
            V2Int              _To,
            float              _Progress)
        {
            Direction = _Direction;
            From = _From;
            To = _To;
            Progress = _Progress;
        }
    }

    public class CharacterMovingStartedEventArgs : CharacterMoveEventArgsBase
    {
        public bool StartFromPortal { get; }

        public CharacterMovingStartedEventArgs(
            EDirection _Direction,
            V2Int              _From,
            V2Int              _To,
            bool               _StartFromPortal)
            : base(_Direction, _From, _To, 0)
        {
            StartFromPortal = _StartFromPortal;
        }
    }
    
    public class CharacterMovingContinuedEventArgs : CharacterMoveEventArgsBase
    {
        public V2Int   Position                     { get; }
        public Vector2 PrecisePosition              { get; }
        public Vector2 PreviousPrecisePosition      { get; }

        public CharacterMovingContinuedEventArgs(
            EDirection _Direction,
            V2Int              _From,
            V2Int              _To,
            float              _Progress,
            float              _PreviousProgress) 
            : base(_Direction, _From, _To, _Progress)
        {
            PrecisePosition = V2Int.Lerp(_From, _To, _Progress);
            PreviousPrecisePosition = V2Int.Lerp(_From, _To, _PreviousProgress);
            Position = V2Int.Round(PrecisePosition);
        }
    }

    public class CharacterMovingFinishedEventArgs : CharacterMoveEventArgsBase
    {
        public IMazeItemProceedInfo BlockOnFinish   { get; }
        public IMazeItemProceedInfo BlockWhoStopped { get; }

        public CharacterMovingFinishedEventArgs(
            EDirection   _Direction,
            V2Int                _From,
            V2Int                _To,
            IMazeItemProceedInfo _BlockOnFinish,
            IMazeItemProceedInfo _BlockWhoStopped) 
            : base(_Direction, _From, _To, 1)
        {
            BlockOnFinish = _BlockOnFinish;
            BlockWhoStopped = _BlockWhoStopped;
        }
    }
}