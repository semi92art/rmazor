using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Exceptions;
using Extensions;
using Games.RazorMaze.Models.ProceedInfos;
using UnityEngine;
using UnityEngine.Events;
using UnityGameLoopDI;
using Utils;

namespace Games.RazorMaze.Models
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
    
    public interface IMazeMovingItemsProceeder
    {
        event MazeItemMoveHandler MazeItemMoveStarted;
        event MazeItemMoveHandler MazeItemMoveContinued;
        event MazeItemMoveHandler MazeItemMoveFinished;
        void TransformItemsAfterMazeRotate(MazeOrientation _Orientation);
        void OnCharacterMoveContinued(CharacterMovingEventArgs _Args, MazeOrientation _Orientation);
        void StartProcessItems(MazeInfo _Info);
        Dictionary<MazeItem, MazeItemMovingProceedInfo> ProceedInfos { get; }
    }

    public class MazeMovingItemsProceeder : Ticker, IOnUpdate, IMazeMovingItemsProceeder
    {
        #region nonpublic members
        
        private bool m_DoProceed;
        private V2Int m_CharacterPos;
        private MazeInfo m_Info;

        #endregion
        
        #region inject
        
        private RazorMazeModelSettings Settings { get; }

        public MazeMovingItemsProceeder(RazorMazeModelSettings _Settings)
        {
            Settings = _Settings;
        }
        
        #endregion
        
        #region api
        
        public event MazeItemMoveHandler MazeItemMoveStarted;
        public event MazeItemMoveHandler MazeItemMoveContinued;
        public event MazeItemMoveHandler MazeItemMoveFinished;
        
        public Dictionary<MazeItem, MazeItemMovingProceedInfo> ProceedInfos { get; private set; }

        public void TransformItemsAfterMazeRotate(MazeOrientation _Orientation)
        {
            MoveMazeItemsGravity(_Orientation, m_CharacterPos);
        }

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args, MazeOrientation _Orientation)
        {
            var addictRaw = (_Args.To.ToVector2() - _Args.From.ToVector2()) * _Args.Progress;
            var addict = new V2Int(addictRaw);
            var newPos = _Args.From + addict;
            if (m_CharacterPos == newPos)
                return;
            m_CharacterPos = newPos;
            MoveMazeItemsGravity(_Orientation, m_CharacterPos);
        }

        public void StartProcessItems(MazeInfo _Info)
        {
            m_Info = _Info;
            ProceedInfos = _Info.MazeItems
                .Where(_Item => _Item.Type == EMazeItemType.BlockMovingGravity
                                || _Item.Type == EMazeItemType.TrapMovingGravity
                                || _Item.Type == EMazeItemType.TrapMoving)
                .ToDictionary(_Item => _Item, _Item => new MazeItemMovingProceedInfo
                {
                    Item = _Item,
                    MoveByPathDirection = EMazeItemMoveByPathDirection.Forward,
                    IsProceeding = false,
                    PauseTimer = 0,
                    BusyPositions = new List<V2Int>{_Item.Position},
                    ProceedingStage = 0
                });
            m_DoProceed = true;
        }
        
        void IOnUpdate.OnUpdate()
        {
            if (!m_DoProceed)
                return;
            foreach (var proceed in ProceedInfos.Values
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

        #endregion

        #region nonpublic methods
        
        private void MoveMazeItemsGravity(MazeOrientation _Orientation, V2Int _CharacterPoint)
        {
            var dropDirection = RazorMazeUtils.GetDropDirection(_Orientation);
            foreach (var item in ProceedInfos.Values.Where(_P => _P.Item.Type == EMazeItemType.BlockMovingGravity))
                MoveMazeItemGravity(item, dropDirection, _CharacterPoint);
            foreach (var item in ProceedInfos.Values.Where(_P => _P.Item.Type == EMazeItemType.TrapMovingGravity))
                MoveMazeItemGravity(item, dropDirection);
        }
        
        private void MoveMazeItemGravity(
            MazeItemMovingProceedInfo _Info, 
            V2Int _DropDirection,
            V2Int? _CharacterPoint = null)
        {
            Coroutines.Run(Coroutines.WaitWhile(
                () => _Info.IsProceeding,
                () =>
                {
                    _Info.IsProceeding = true;
                    var pos = _Info.Item.Position;
                    bool doMoveByPath = false;
                    V2Int? altPos = null;
                    while (RazorMazeUtils.IsValidPositionForMove(m_Info, _Info.Item, pos + _DropDirection))
                    {
                        pos += _DropDirection;
                        if (_CharacterPoint.HasValue && _CharacterPoint.Value == pos)
                            altPos = pos - _DropDirection;
                        var pos1 = pos;
                        if (_Info.Item.Path.All(_Pos => pos1 != _Pos))
                            continue;
                        int fromPosIdx = _Info.Item.Path.IndexOf(_Info.Item.Position);
                        if (fromPosIdx == -1)
                        {
                            doMoveByPath = true;
                            break;
                        }
                        int toPosIds = _Info.Item.Path.IndexOf(pos);
                        if (Math.Abs(toPosIds - fromPosIdx) > 1)
                            continue;
                        doMoveByPath = true;
                        break;
                    }

                    if (!doMoveByPath || pos == _Info.Item.Position)
                    {
                        _Info.IsProceeding = false;                       
                        return;
                    }
                    
                    var from = _Info.Item.Position;
                    var to = altPos ?? pos;

                    Coroutines.Run(MoveMazeItemGravityCoroutine(_Info, from, to));
                }));
        }

        private void ProceedMazeItemMoving(
            MazeItem _Item, 
            UnityAction _OnFinish)
        {
            V2Int from = _Item.Position;
            V2Int to;
            int idx = _Item.Path.IndexOf(_Item.Position);
            var path = _Item.Path.ToList();
            var proceeing = ProceedInfos[_Item];
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

        private IEnumerator MoveMazeItemGravityCoroutine(
            MazeItemMovingProceedInfo _Info,
            V2Int _From,
            V2Int _To)
        {
            var item = _Info.Item;
            var busyPositions = _Info.BusyPositions;
            MazeItemMoveStarted?.Invoke(new MazeItemMoveEventArgs(item, _From, _To, 0));
            var direction = (_To.ToVector2() - _From.ToVector2Int()).normalized;
            float distance = Vector2Int.Distance(_From.ToVector2Int(), _To.ToVector2Int());
            yield return Coroutines.Lerp(
                0f,
                1f,
                distance / Settings.movingItemsSpeed,
                _Progress =>
                {
                    var addict = direction * (_Progress + 0.1f) * distance;
                    busyPositions.Clear();
                    busyPositions.Add(_From + addict.ToV2IntFloor());
                    if (busyPositions[0] != _To)
                        busyPositions.Add(_From + addict.ToV2IntCeil());
                    MazeItemMoveContinued?.Invoke(new MazeItemMoveEventArgs(item, _From, _To, _Progress));
                },
                GameTimeProvider.Instance,
                (_Stopped, _Progress) =>
                {
                    var to = !_Stopped ? _To : _Info.BusyPositions[0];  
                    item.Position = to;
                    MazeItemMoveFinished?.Invoke(new MazeItemMoveEventArgs(item, _From, to, _Progress, _Stopped));
                    _Info.IsProceeding = false;
                    busyPositions.Clear();
                    busyPositions.Add(to);
                }, () => busyPositions.Any() && busyPositions.Last() == m_CharacterPos);
        }

        private IEnumerator MoveMazeItemMovingCoroutine(
            MazeItem _Item,
            V2Int _From,
            V2Int _To,
            UnityAction _OnFinish)
        {
            MazeItemMoveStarted?.Invoke(new MazeItemMoveEventArgs(_Item, _From, _To, 0));
            float distance = Vector2Int.Distance(_From.ToVector2Int(), _To.ToVector2Int());
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