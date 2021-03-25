using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using Entities;
using Exceptions;
using Extensions;
using Games.RazorMaze.Models;
using ModestTree;
using UnityEngine;
using UnityEngine.Events;
using UnityGameLoopDI;
using Utils;

namespace Games.RazorMaze
{
    public interface IMazeTransformer
    {
        event MazeItemMoveHandler MazeItemMoveStarted;
        event MazeItemMoveHandler MazeItemMoveContinued;
        event MazeItemMoveHandler MazeItemMoveFinished;
        void TransformItemsAfterMazeRotate(MazeOrientation _Orientation);
        void OnCharacterMoveContinued(CharacterMovingEventArgs _Args, MazeOrientation _Orientation);
        void StartProcessItems(MazeInfo _Info);
        Dictionary<MazeItem, MazeItemProceeding> MovableProceeds { get; }
        Dictionary<MazeItem, MazeItemProceeding> GravityProceeds { get; }
    }

    public class MazeTransformer : Ticker, IOnUpdate, IMazeTransformer
    {
        #region nonpublic members
        
        private bool m_DoProceed;
        private V2Int m_CharacterPos;
        private MazeInfo m_Info;

        #endregion
        
        #region inject
        
        private RazorMazeModelSettings Settings { get; }

        public MazeTransformer(RazorMazeModelSettings _Settings)
        {
            Settings = _Settings;
        }
        
        #endregion
        
        #region api
        
        public event MazeItemMoveHandler MazeItemMoveStarted;
        public event MazeItemMoveHandler MazeItemMoveContinued;
        public event MazeItemMoveHandler MazeItemMoveFinished;
        
        public Dictionary<MazeItem, MazeItemProceeding> MovableProceeds { get; private set; }
        public Dictionary<MazeItem, MazeItemProceeding> GravityProceeds { get; private set; }
        public void OnCharacterMoved(CharacterMovingEventArgs _Args)
        {
            m_CharacterPos = _Args.To;
        }

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
            MovableProceeds = _Info.MazeItems
                .Where(_Item => _Item.Type == EMazeItemType.TrapMoving)
                .ToDictionary(_Item => _Item, _Item => new MazeItemProceeding
                {
                    Item = _Item,
                    MoveByPathDirection = EMazeItemMoveByPathDirection.Forward
                });
            GravityProceeds = _Info.MazeItems
                .Where(_Item => _Item.Type == EMazeItemType.BlockMovingGravity
                                || _Item.Type == EMazeItemType.TrapMovingGravity)
                .ToDictionary(_Item => _Item, _Item => new MazeItemProceeding
                {
                    Item = _Item,
                    MoveByPathDirection = EMazeItemMoveByPathDirection.Forward
                });
            m_DoProceed = true;
        }
        
        public void OnUpdate()
        {
            if (!m_DoProceed)
                return;
            foreach (var proceed in MovableProceeds.Values.Where(_P => !_P.IsProceeding))
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
            foreach (var item in GravityProceeds.Values.Where(_P => _P.Item.Type == EMazeItemType.BlockMovingGravity))
                MoveMazeItemGravity(item, dropDirection, _CharacterPoint);
            foreach (var item in GravityProceeds.Values.Where(_P => _P.Item.Type == EMazeItemType.TrapMovingGravity))
                MoveMazeItemGravity(item, dropDirection);
        }
        
        private void MoveMazeItemGravity(
            MazeItemProceeding _MazeItemProceeding, 
            V2Int _DropDirection,
            V2Int? _CharacterPoint = null)
        {
            Coroutines.Run(Coroutines.WaitWhile(
                () => _MazeItemProceeding.IsProceeding,
                () =>
                {
                    _MazeItemProceeding.IsProceeding = true;
                    var pos = _MazeItemProceeding.Item.Position;
                    bool doMoveByPath = false;
                    V2Int? altPos = null;
                    while (RazorMazeUtils.IsValidPositionForMove(m_Info, _MazeItemProceeding.Item, pos + _DropDirection))
                    {
                        pos += _DropDirection;
                        if (_CharacterPoint.HasValue && _CharacterPoint.Value == pos)
                            altPos = pos - _DropDirection;
                        if (_MazeItemProceeding.Item.Path.All(_Pos => pos != _Pos))
                            continue;
                        int fromPosIdx = _MazeItemProceeding.Item.Path.IndexOf(_MazeItemProceeding.Item.Position);
                        int toPosIds = _MazeItemProceeding.Item.Path.IndexOf(pos);
                        if (Math.Abs(toPosIds - fromPosIdx) > 1)
                            continue;
                        doMoveByPath = true;
                        break;
                    }

                    if (!doMoveByPath || pos == _MazeItemProceeding.Item.Position)
                    {
                        _MazeItemProceeding.IsProceeding = false;                       
                        return;
                    }
                    
                    var from = _MazeItemProceeding.Item.Position;
                    var to = altPos ?? pos;

                    Coroutines.Run(MoveMazeItemGravityCore(_MazeItemProceeding, from, to));
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
            var proceeing = MovableProceeds[_Item];
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
            Coroutines.Run(MoveMazeItemMoving(_Item, from, to, _OnFinish));
        }

        private IEnumerator MoveMazeItemGravityCore(
            MazeItemProceeding _Proceeding,
            V2Int _From,
            V2Int _To)
        {
            var item = _Proceeding.Item;
            var busyPositions = _Proceeding.BusyPositions;
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
                (_Breaked, _Progress) =>
                {
                    var to = !_Breaked ? _To : _Proceeding.BusyPositions[0];  
                    item.Position = to;
                    MazeItemMoveFinished?.Invoke(new MazeItemMoveEventArgs(item, _From, to, _Progress, _Breaked));
                    _Proceeding.IsProceeding = false;
                    busyPositions.Clear();
                    busyPositions.Add(to);
                }, () => busyPositions.Any() && busyPositions.Last() == m_CharacterPos);
        }

        private IEnumerator MoveMazeItemMoving(
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
                (_Breaked, _Progress) =>
                {
                    _Item.Position = _To;
                    _OnFinish?.Invoke();
                    MazeItemMoveFinished?.Invoke(new MazeItemMoveEventArgs(_Item, _From, _To, _Progress));
                });
        }

        #endregion
    }
    
    public class MazeItemProceeding
    {
        public MazeItem Item { get; set; }
        public bool IsProceeding { get; set; }
        public float PauseTimer { get; set; }
        public EMazeItemMoveByPathDirection MoveByPathDirection { get; set; }
        public List<V2Int> BusyPositions { get; } = new List<V2Int>();
    }
}