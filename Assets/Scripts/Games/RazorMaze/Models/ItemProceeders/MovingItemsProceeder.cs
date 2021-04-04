using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Exceptions;
using Games.RazorMaze.Models.ProceedInfos;
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
        public float Progress { get; }
        public bool Stopped { get; }

        public MazeItemMoveEventArgs(MazeItem _Item, V2Int _From, V2Int _To, float _Progress, bool _Stopped = false)
        {
            Item = _Item;
            From = _From;
            To = _To;
            Progress = _Progress;
            Stopped = _Stopped;
        }
    }
    
    public delegate void MazeItemMoveHandler(MazeItemMoveEventArgs Args);
    
    public interface IMovingItemsProceeder : IOnMazeChanged
    {
        event MazeItemMoveHandler MazeItemMoveStarted;
        event MazeItemMoveHandler MazeItemMoveContinued;
        event MazeItemMoveHandler MazeItemMoveFinished;
    }

    public class MovingItemsProceeder : Ticker, IUpdateTick, IMovingItemsProceeder
    {
        #region inject
        
        private RazorMazeModelSettings Settings { get; }
        private IModelMazeData Data { get; }

        public MovingItemsProceeder(
            RazorMazeModelSettings _Settings,
            IModelMazeData _Data)
        {
            Settings = _Settings;
            Data = _Data;
        }
        
        #endregion
        
        #region api
        
        public event MazeItemMoveHandler MazeItemMoveStarted;
        public event MazeItemMoveHandler MazeItemMoveContinued;
        public event MazeItemMoveHandler MazeItemMoveFinished;
        

        public void OnMazeChanged(MazeInfo _Info)
        {
            CollectItems(_Info);
        }
        
        #endregion

        #region nonpublic methods

        private void CollectItems(MazeInfo _Info)
        {
            var infos = _Info.MazeItems
                .Where(_Item =>_Item.Type == EMazeItemType.TrapMoving)
                .Select(_Item => new MazeItemMovingProceedInfo
                {
                    Item = _Item,
                    MoveByPathDirection = EMazeItemMoveByPathDirection.Forward,
                    IsProceeding = false,
                    PauseTimer = 0,
                    BusyPositions = new List<V2Int>{_Item.Position},
                    ProceedingStage = 0
                });
            foreach (var info in infos)
            {
                if (Data.ProceedInfos.ContainsKey(info.Item))
                    Data.ProceedInfos[info.Item] = info;
                else
                    Data.ProceedInfos.Add(info.Item, info);
            }
        }
        
        void IUpdateTick.UpdateTick()
        {
            if (!Data.ProceedingMazeItems)
                return;
            ProceedMazeItemsMoving();
        }

        private void ProceedMazeItemsMoving()
        {
            foreach (var proceed in Data.ProceedInfos.Values
                .Where(_P => !_P.IsProceeding && _P.Item.Type == EMazeItemType.TrapMoving))
            {
                proceed.PauseTimer += Time.deltaTime;
                if (proceed.PauseTimer < Settings.movingItemsPause)
                    continue;
                proceed.PauseTimer = 0;
                proceed.IsProceeding = true;
                ProceedMazeItemMoving(proceed.Item, () => proceed.IsProceeding = false);
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
            var proceeing = (MazeItemMovingProceedInfo) Data.ProceedInfos[_Item];
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
            MazeItemMoveStarted?.Invoke(new MazeItemMoveEventArgs(_Item, _From, _To, 0));
            float distance = V2Int.Distance(_From, _To);
            yield return Coroutines.Lerp(
                0f,
                1f,
                distance / Settings.movingItemsSpeed,
                _Progress => MazeItemMoveContinued?.Invoke(new MazeItemMoveEventArgs(_Item, _From, _To, _Progress)),
                GameTimeProvider.Instance,
                (_Stopped, _Progress) =>
                {
                    _Item.Position = _To;
                    float progress = _Stopped ? _Progress : 1;
                    _OnFinish?.Invoke();
                    MazeItemMoveFinished?.Invoke(new MazeItemMoveEventArgs(_Item, _From, _To, progress, _Stopped));
                });
        }
        
        #endregion
    }
}