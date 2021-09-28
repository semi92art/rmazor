using System;
using System.Collections.Generic;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;
using Ticker;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public class MazeItemMoveEventArgs : EventArgs
    {
        public IMazeItemProceedInfo Info { get; }
        public V2Int From { get; }
        public V2Int To { get; }
        public float Speed { get; }
        public float Progress { get; }
        public List<V2Int> BusyPositions { get; }
        public bool Stopped { get; }

        public MazeItemMoveEventArgs(
            IMazeItemProceedInfo _Info,
            V2Int _From,
            V2Int _To, 
            float _Speed,
            float _Progress, 
            List<V2Int> _BusyPositions, bool _Stopped = false)
        {
            Info = _Info;
            From = _From;
            To = _To;
            Speed = _Speed;
            Progress = _Progress;
            BusyPositions = _BusyPositions;
            Stopped = _Stopped;
        }
    }
    
    public delegate void MazeItemMoveHandler(MazeItemMoveEventArgs Args);
    
    public interface IMovingItemsProceeder : IItemsProceeder
    {
        event MazeItemMoveHandler MazeItemMoveStarted;
        event MazeItemMoveHandler MazeItemMoveContinued;
        event MazeItemMoveHandler MazeItemMoveFinished;
    }
    
    public abstract class MovingItemsProceederBase : ItemsProceederBase, IMovingItemsProceeder
    {
        protected MovingItemsProceederBase(
            ModelSettings _Settings, 
            IModelData _Data, 
            IModelCharacter _Character,
            IGameTicker _GameTicker) 
            : base(_Settings, _Data, _Character, _GameTicker) { }

        public event MazeItemMoveHandler MazeItemMoveStarted;
        public event MazeItemMoveHandler MazeItemMoveContinued;
        public event MazeItemMoveHandler MazeItemMoveFinished;

        protected void InvokeMoveStarted(MazeItemMoveEventArgs _Args)
        {
            MazeItemMoveStarted?.Invoke(_Args);
        }
        
        protected void InvokeMoveContinued(MazeItemMoveEventArgs _Args)
        {
            MazeItemMoveContinued?.Invoke(_Args);
        }
        
        protected void InvokeMoveFinished(MazeItemMoveEventArgs _Args)
        {
            MazeItemMoveFinished?.Invoke(_Args);
        }
    }
}