using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Exceptions;
using Games.RazorMaze.Models.ProceedInfos;
using TimeProviders;
using UnityEngine;
using UnityEngine.Events;
using UnityGameLoopDI;
using Utils;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public class MazeItemMoveEventArgs : EventArgs
    {
        public MazeItem Item { get; }
        public V2Int From { get; }
        public V2Int To { get; }
        public float Speed { get; }
        public float Progress { get; }
        public List<V2Int> BusyPositions { get; }
        public bool Stopped { get; }

        public MazeItemMoveEventArgs(
            MazeItem _Item,
            V2Int _From,
            V2Int _To, 
            float _Speed,
            float _Progress, 
            List<V2Int> _BusyPositions, bool _Stopped = false)
        {
            Item = _Item;
            From = _From;
            To = _To;
            Speed = _Speed;
            Progress = _Progress;
            BusyPositions = _BusyPositions;
            Stopped = _Stopped;
        }
    }
    
    public delegate void MazeItemMoveHandler(MazeItemMoveEventArgs Args);
    
    public interface IMovingItemsProceeder : IItemsProceeder, IOnMazeChanged
    {
        event MazeItemMoveHandler MazeItemMoveStarted;
        event MazeItemMoveHandler MazeItemMoveContinued;
        event MazeItemMoveHandler MazeItemMoveFinished;
    }

    public class MovingItemsProceeder : ItemsProceederBase, IUpdateTick, IMovingItemsProceeder
    {
        #region nonpublic members
        
        protected override EMazeItemType[] Types => new[] {EMazeItemType.TrapMoving};
        
        #endregion

        #region inject
        
        public MovingItemsProceeder(ModelSettings _Settings, IModelMazeData _Data) 
            : base(_Settings, _Data) { }
        
        #endregion
        
        #region api
        
        public event MazeItemMoveHandler MazeItemMoveStarted;
        public event MazeItemMoveHandler MazeItemMoveContinued;
        public event MazeItemMoveHandler MazeItemMoveFinished;

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
        
        public void OnMazeChanged(MazeInfo _Info)
        {
            CollectItems(_Info);
        }
        
        #endregion

        #region nonpublic methods

        void IUpdateTick.UpdateTick()
        {
            if (!Data.ProceedingMazeItems)
                return;
            ProceedMazeItemsMoving();
        }

        private void ProceedMazeItemsMoving()
        {
            foreach (var type in Types)
            {
                var infos = GetProceedInfos(type);
                foreach (var info in infos.Values.Where(_Info => !_Info.IsProceeding))
                {
                    info.PauseTimer += Time.deltaTime;
                    if (info.PauseTimer < Settings.movingItemsPause)
                        continue;
                    info.PauseTimer = 0;
                    info.IsProceeding = true;
                    ProceedMazeItemMoving(info.Item, () => info.IsProceeding = false);
                }
            }
        }
        
        private void ProceedMazeItemMoving(
            MazeItem _Item, 
            UnityAction _OnFinish)
        {
            V2Int from = _Item.Position;
            V2Int to;
            int idx = _Item.Path.IndexOf(_Item.Position);
            var path = _Item.Path.ToList();
            var proceeing = (MazeItemMovingProceedInfo) Data.ProceedInfos[Types.First()][_Item];
            switch (proceeing.MoveByPathDirection)
            {
                case EMazeItemMoveByPathDirection.Forward:
                    if (idx == path.Count - 1)
                    {
                        idx--;
                        proceeing.MoveByPathDirection = EMazeItemMoveByPathDirection.Backward;
                    }
                    else
                        idx++;
                    to = path[idx];
                    break;
                case EMazeItemMoveByPathDirection.Backward:
                    if (idx == 0)
                    {
                        idx++;
                        proceeing.MoveByPathDirection = EMazeItemMoveByPathDirection.Forward;
                    }
                    else
                        idx--;
                    to = path[idx];
                    break;
                default: throw new SwitchCaseNotImplementedException(proceeing.MoveByPathDirection);
            }
            Coroutines.Run(MoveMazeItemMovingCoroutine(_Item, from, to, _OnFinish));
        }
        
        private IEnumerator MoveMazeItemMovingCoroutine(
            MazeItem _Item,
            V2Int _From,
            V2Int _To,
            UnityAction _OnFinish)
        {
            var proceedInfo = Data.ProceedInfos[_Item.Type][_Item] as MazeItemMovingProceedInfo;
            MazeItemMoveStarted?.Invoke(new MazeItemMoveEventArgs(
                _Item, _From, _To, Settings.movingItemsSpeed,0, proceedInfo?.BusyPositions));
            float distance = V2Int.Distance(_From, _To);
            yield return Coroutines.Lerp(
                0f,
                1f,
                distance / Settings.movingItemsSpeed,
                _Progress => MazeItemMoveContinued?.Invoke(
                    new MazeItemMoveEventArgs(_Item, _From, _To, Settings.movingItemsSpeed,_Progress, proceedInfo?.BusyPositions)),
                GameTimeProvider.Instance,
                (_Stopped, _Progress) =>
                {
                    _Item.Position = _To;
                    float progress = _Stopped ? _Progress : 1;
                    _OnFinish?.Invoke();
                    MazeItemMoveFinished?.Invoke(new MazeItemMoveEventArgs(
                        _Item, _From, _To, Settings.movingItemsSpeed,progress, proceedInfo?.BusyPositions, _Stopped));
                });
        }
        
        #endregion
    }
}