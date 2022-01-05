using System;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;
using UnityEngine;

namespace Games.RazorMaze.Models
{
    public abstract class CharacterMoveEventArgsBase : EventArgs
    {
        public EMazeMoveDirection Direction { get; }
        public V2Int              From      { get; }
        public V2Int              To        { get; }
        public float              Progress  { get; }

        protected CharacterMoveEventArgsBase(   
            EMazeMoveDirection _Direction, 
            V2Int _From,
            V2Int _To,
            float _Progress)
        {
            Direction = _Direction;
            From = _From;
            To = _To;
            Progress = _Progress;
        }
    }

    public class CharacterMovingStartedEventArgs : CharacterMoveEventArgsBase
    {
        public CharacterMovingStartedEventArgs(
            EMazeMoveDirection _Direction,
            V2Int _From, 
            V2Int _To)
            : base(_Direction, _From, _To, 0) { }
    }
    
    public class CharacterMovingContinuedEventArgs : CharacterMoveEventArgsBase
    {
        public V2Int   Position                     { get; }
        public Vector2 PrecisePosition              { get; }
        public Vector2 PreviousPrecisePosition      { get; }

        public CharacterMovingContinuedEventArgs(
            EMazeMoveDirection _Direction, 
            V2Int _From,
            V2Int _To,
            float _Progress,
            float _PreviousProgress) 
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
            EMazeMoveDirection _Direction,
            V2Int _From,
            V2Int _To,
            IMazeItemProceedInfo _BlockOnFinish,
            IMazeItemProceedInfo _BlockWhoStopped) 
            : base(_Direction, _From, _To, 1)
        {
            BlockOnFinish = _BlockOnFinish;
            BlockWhoStopped = _BlockWhoStopped;
        }
    }
}