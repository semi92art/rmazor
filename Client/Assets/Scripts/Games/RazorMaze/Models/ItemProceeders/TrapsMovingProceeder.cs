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
    
    public interface IMovingItemsProceeder : IItemsProceeder
    {
        event MazeItemMoveHandler MazeItemMoveStarted;
        event MazeItemMoveHandler MazeItemMoveContinued;
        event MazeItemMoveHandler MazeItemMoveFinished;
    }

    public class TrapsMovingProceeder : ItemsProceederBase, IOnGameLoopUpdate, IMovingItemsProceeder
    {
        #region constants

        public const int StageMoving = 1;

        #endregion
        
        #region nonpublic members
        
        protected override EMazeItemType[] Types => new[] {EMazeItemType.TrapMoving};
        
        #endregion

        #region inject
        
        public TrapsMovingProceeder(ModelSettings _Settings, IModelMazeData _Data, IModelCharacter _Character) 
            : base(_Settings, _Data, _Character) { }
        
        #endregion
        
        #region api
        
        public event MazeItemMoveHandler MazeItemMoveStarted;
        public event MazeItemMoveHandler MazeItemMoveContinued;
        public event MazeItemMoveHandler MazeItemMoveFinished;

        public void OnGameLoopUpdate()
        {
            ProceedTrapsMoving();
        }
        
        #endregion

        #region nonpublic methods
        
        private void ProceedTrapsMoving()
        {
            var infos = GetProceedInfos(Types).Values;
            foreach (var info in infos.Where(_Info => _Info.IsProceeding))
            {
                if (info.ProceedingStage != StageIdle)
                    continue;
                CheckForCharacterDeath(info, info.Item.Position.ToVector2());
                info.PauseTimer += Time.deltaTime;
                if (info.PauseTimer < Settings.movingItemsPause)
                    continue;
                info.PauseTimer = 0;
                info.ProceedingStage = StageMoving;
                ProceedTrapMoving(info, () => info.ProceedingStage = StageIdle);
            }
        }
        
        private void ProceedTrapMoving(
            IMazeItemProceedInfo _Info, 
            UnityAction _OnFinish)
        {
            var item = _Info.Item;
            V2Int from = item.Position;
            V2Int to;
            int idx = item.Path.IndexOf(item.Position);
            var path = item.Path.ToList();
            switch (_Info.MoveByPathDirection)
            {
                case EMazeItemMoveByPathDirection.Forward:
                    if (idx == path.Count - 1)
                    {
                        idx--;
                        _Info.MoveByPathDirection = EMazeItemMoveByPathDirection.Backward;
                    }
                    else
                        idx++;
                    to = path[idx];
                    break;
                case EMazeItemMoveByPathDirection.Backward:
                    if (idx == 0)
                    {
                        idx++;
                        _Info.MoveByPathDirection = EMazeItemMoveByPathDirection.Forward;
                    }
                    else
                        idx--;
                    to = path[idx];
                    break;
                default: throw new SwitchCaseNotImplementedException(_Info.MoveByPathDirection);
            }
            Coroutines.Run(MoveTrapMovingCoroutine(_Info, from, to, _OnFinish));
        }
        
        private IEnumerator MoveTrapMovingCoroutine(
            IMazeItemProceedInfo _Info, 
            V2Int _From,
            V2Int _To,
            UnityAction _OnFinish)
        {
            _Info.IsMoving = true;
            MazeItemMoveStarted?.Invoke(new MazeItemMoveEventArgs(
                _Info.Item, _From, _To, Settings.movingItemsSpeed,0, _Info.BusyPositions));
            float distance = V2Int.Distance(_From, _To);
            yield return Coroutines.Lerp(
                0f,
                1f,
                distance / Settings.movingItemsSpeed,
                _Progress =>
                {
                    var precisePosition = V2Int.Lerp(_From, _To, _Progress);
                    _Info.Item.Position = V2Int.Round(precisePosition);
                    CheckForCharacterDeath(_Info, precisePosition);
                    MazeItemMoveContinued?.Invoke(
                        new MazeItemMoveEventArgs(_Info.Item, _From, _To, Settings.movingItemsSpeed, _Progress,
                            _Info.BusyPositions));
                },
                GameTimeProvider.Instance,
                (_Stopped, _Progress) =>
                {
                    _Info.Item.Position = _To;
                    float progress = _Stopped ? _Progress : 1;
                    _Info.IsMoving = false;
                    _OnFinish?.Invoke();
                    MazeItemMoveFinished?.Invoke(new MazeItemMoveEventArgs(
                        _Info.Item, _From, _To, Settings.movingItemsSpeed,progress, _Info.BusyPositions, _Stopped));
                });
        }
        
        private void CheckForCharacterDeath(IMazeItemProceedInfo _Info, Vector2 _ItemPrecisePosition)
        {
            if (!Character.Alive)
                return;
            var cPos = Character.IsMoving ?
                Character.MovingInfo.PrecisePosition : Character.Position.ToVector2();
            if (Vector2.Distance(cPos, _ItemPrecisePosition) + RazorMazeUtils.Epsilon > 1f)
                return;
            KillerProceedInfo = _Info;
            Character.RaiseDeath();
        }

        #endregion
    }
}