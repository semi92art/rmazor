using System;
using System.Collections;
using System.Linq;
using Entities;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Models.ProceedInfos;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Models
{
    public class CharacterMovingEventArgs : EventArgs
    {
        public V2Int From { get; }
        public V2Int To { get; }
        public V2Int Current { get; }
        public float Progress { get; }

        public CharacterMovingEventArgs(V2Int _From, V2Int _To, V2Int _Current, float _Progress)
        {
            From = _From;
            To = _To;
            Current = _Current;
            Progress = _Progress;
        }
    }
    
    public delegate void HealthPointsChangedHandler(HealthPointsEventArgs _Args);
    public delegate void CharacterMovingHandler(CharacterMovingEventArgs _Args);

    public interface IModelCharacter : IInit
    {
        event CharacterMovingHandler CharacterMoveStarted;
        event CharacterMovingHandler CharacterMoveContinued;
        event CharacterMovingHandler CharacterMoveFinished;
        event HealthPointsChangedHandler HealthChanged;
        event NoArgsHandler Death;
        void Move(MazeMoveDirection _Direction);
        void OnMazeChanged(MazeInfo _Info);
        void OnPortal(PortalEventArgs _Args);
    }
    
    public class ModelCharacter : IModelCharacter
    {
        #region nonpublic members

        private int m_Counter;
        
        #endregion
        
        #region inject

        private RazorMazeModelSettings Settings { get; }
        private IModelMazeData Data { get; }

        public ModelCharacter(RazorMazeModelSettings _Settings, IModelMazeData _Data)
        {
            Settings = _Settings;
            Data = _Data;
        }
        
        #endregion
        
        #region api

        public event CharacterMovingHandler CharacterMoveStarted;
        public event CharacterMovingHandler CharacterMoveContinued;
        public event CharacterMovingHandler CharacterMoveFinished;
        public event HealthPointsChangedHandler HealthChanged;
        public event NoArgsHandler Death;
        

        public void Init()
        {
            Data.CharacterInfo.HealthPoints = 1;
        }
        
        public void Move(MazeMoveDirection _Direction)
        {
            if (!Data.ProceedingControls)
                return;
            Data.CharacterInfo.MoveDirection = _Direction;
            var from = Data.CharacterInfo.Position;
            var to = GetNewPosition(_Direction);
            var cor = MoveCharacterCore(from, to);
            Coroutines.Run(cor);
        }

        public void OnMazeChanged(MazeInfo _Info)
        {
            Data.CharacterInfo.Position = Data.Info.Path[0];
        }

        public void OnPortal(PortalEventArgs _Args)
        {
            Data.CharacterInfo.Position = _Args.Item.Pair;
            Move(Data.CharacterInfo.MoveDirection);
        }

        #endregion
        
        #region nonpublic methods

        private V2Int GetNewPosition(MazeMoveDirection _Direction)
        {
            var nextPos = Data.CharacterInfo.Position;
            var dirVector = RazorMazeUtils.GetDirectionVector(_Direction, Data.Orientation);
            while (ValidPosition(nextPos + dirVector, Data.Info))
                nextPos += dirVector;
            return nextPos;
        }

        private bool ValidPosition(V2Int _Position, MazeInfo _Info)
        {
            bool isNode = _Info.Path.Any(_PathItem => _PathItem == _Position);
            bool isMazeItem = _Info.MazeItems.Any(_O => 
                _O.Position == _Position
                && (_O.Type == EMazeItemType.Block
                    || _O.Type == EMazeItemType.TrapIncreasing
                    || _O.Type == EMazeItemType.Turret));
            bool isBuzyMazeItem = Data.ProceedInfos.Values
                .Where(_Proceed => _Proceed.Item.Type == EMazeItemType.BlockMovingGravity)
                .Any(_Proceed => (_Proceed as MazeItemMovingProceedInfo).BusyPositions.Contains(_Position));
            bool isPortal = _Info.MazeItems
                .Any(_O => _O.Position == _Position && _O.Type == EMazeItemType.Portal);
            return (isNode && !isMazeItem && !isBuzyMazeItem) || isPortal;
        }
        
        private IEnumerator MoveCharacterCore(V2Int _From, V2Int _To)
        {
            m_Counter++;
            int thisCount = m_Counter;
            
            CharacterMoveStarted?.Invoke(new CharacterMovingEventArgs(_From, _To, _From,0));
            int pathLength = Mathf.RoundToInt(V2Int.Distance(_From, _To));
            yield return Coroutines.Lerp(
                0f,
                1f,
                pathLength / Settings.characterSpeed,
                _Progress =>
                {
                    var addictRaw = (_To.ToVector2() - _From.ToVector2()) * _Progress;
                    var addict = new V2Int(addictRaw);
                    var newPos = _From + addict;
                    Data.CharacterInfo.Position = newPos;
                    CharacterMoveContinued?.Invoke(new CharacterMovingEventArgs(_From, _To, newPos, _Progress));
                },
                GameTimeProvider.Instance,
                (_Stopped, _Progress) => CharacterMoveFinished?
                    .Invoke(new CharacterMovingEventArgs(_From, _To, _To, _Progress)),
                () => thisCount != m_Counter);
        }
        
        #endregion
    }
}