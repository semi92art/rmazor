using System;
using Common.Entities;
using Common.Ticker;
using RMAZOR.Models.ProceedInfos;

namespace RMAZOR.Models.ItemProceeders
{
    public class MazeItemMoveEventArgs : EventArgs
    {
        public IMazeItemProceedInfo Info     { get; }
        public V2Int                From     { get; }
        public V2Int                To       { get; }
        public float                Speed    { get; }
        public float                Progress { get; }

        public MazeItemMoveEventArgs(
            IMazeItemProceedInfo _Info,
            V2Int                _From,
            V2Int                _To, 
            float                _Speed,
            float                _Progress)
        {
            Info = _Info;
            From = _From;
            To = _To;
            Speed = _Speed;
            Progress = _Progress;
        }
    }
    
    public delegate void MazeItemMoveHandler(MazeItemMoveEventArgs _Args);
    
    public interface IMovingItemsProceeder : IItemsProceeder
    {
        event MazeItemMoveHandler MazeItemMoveStarted;
        event MazeItemMoveHandler MazeItemMoveContinued;
        event MazeItemMoveHandler MazeItemMoveFinished;
    }
    
    public abstract class MovingItemsProceederBase : ItemsProceederBase, IMovingItemsProceeder
    {
        #region inject
        
        protected MovingItemsProceederBase(
            ModelSettings      _Settings,
            IModelData         _Data,
            IModelCharacter    _Character,
            IModelGameTicker   _GameTicker,
            IModelMazeRotation _Rotation) 
            : base(
                _Settings, 
                _Data, 
                _Character,
                _GameTicker, 
                _Rotation) { }

        #endregion

        #region api
        
        public event MazeItemMoveHandler MazeItemMoveStarted;
        public event MazeItemMoveHandler MazeItemMoveContinued;
        public event MazeItemMoveHandler MazeItemMoveFinished;

        #endregion

        #region nonpublic methods
        
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

        #endregion
    }
}