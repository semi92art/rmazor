using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Extensions;
using Games.RazorMaze.Models.ProceedInfos;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Models
{
    public interface IMazeGravityItemsProceeder : IMazeMovingItemsProceeder
    {
        void OnMazeOrientationChanged();
        void OnCharacterMoveContinued(CharacterMovingEventArgs _Args);
    }
    
    public class MazeGravityItemsProceeder : IMazeGravityItemsProceeder
    {
        #region nonpublic members
        
        private V2Int m_CharacterPosCheck;
        
        #endregion
        
        #region inject
        
        private RazorMazeModelSettings Settings { get; }
        private IModelMazeData Data { get; }

        public MazeGravityItemsProceeder(
            RazorMazeModelSettings _Settings,
            IModelMazeData _Data)
        {
            Settings = _Settings;
            Data = _Data;
        }
        
        #endregion
        
        public event MazeItemMoveHandler MazeItemMoveStarted;
        public event MazeItemMoveHandler MazeItemMoveContinued;
        public event MazeItemMoveHandler MazeItemMoveFinished;

        public void OnMazeChanged(MazeInfo _Info)
        {
            var infos = _Info.MazeItems
                .Where(_Item => _Item.Type == EMazeItemType.BlockMovingGravity
                                || _Item.Type == EMazeItemType.TrapMovingGravity)
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
            
            MoveMazeItemsGravity(Data.Orientation, Data.CharacterInfo.Position);
        }

        public void OnMazeOrientationChanged()
        {
            MoveMazeItemsGravity(Data.Orientation, Data.CharacterInfo.Position);
        }
        
        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            if (Data.CharacterInfo.Position == _Args.Current)
                return;
            m_CharacterPosCheck = _Args.Current;
            MoveMazeItemsGravity(Data.Orientation, _Args.Current);
        }
        
        #region nonpublic members

        
        private void MoveMazeItemsGravity(MazeOrientation _Orientation, V2Int _CharacterPoint)
        {
            var dropDirection = RazorMazeUtils.GetDropDirection(_Orientation);
            foreach (var item in Data.ProceedInfos.Values.Where(_P => _P.Item.Type == EMazeItemType.BlockMovingGravity))
                MoveMazeItemGravity((MazeItemMovingProceedInfo)item, dropDirection, _CharacterPoint);
            foreach (var item in Data.ProceedInfos.Values.Where(_P => _P.Item.Type == EMazeItemType.TrapMovingGravity))
                MoveMazeItemGravity((MazeItemMovingProceedInfo)item, dropDirection);
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
                    while (RazorMazeUtils.IsValidPositionForMove(Data.Info, _Info.Item, pos + _DropDirection))
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
                }, () => busyPositions.Any() && busyPositions.Last() == m_CharacterPosCheck);
        }

        #endregion
    }
}