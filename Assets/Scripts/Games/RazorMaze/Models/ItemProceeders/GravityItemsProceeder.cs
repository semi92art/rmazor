using System;
using System.Collections;
using System.Linq;
using Entities;
using Extensions;
using Games.RazorMaze.Models.ProceedInfos;
using TimeProviders;
using Utils;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public interface IGravityItemsProceeder : IMovingItemsProceeder, ICharacterMoveStarted
    {
        void OnMazeOrientationChanged();
    }
    
    public class GravityItemsProceeder : ItemsProceederBase, IGravityItemsProceeder
    {
        #region nonpublic members

        protected override EMazeItemType[] Types => new[] {EMazeItemType.GravityBlock, EMazeItemType.GravityTrap};
        
        #endregion
        
        #region inject
        
        public GravityItemsProceeder(ModelSettings _Settings, IModelMazeData _Data) 
            : base (_Settings, _Data) { }
        
        #endregion
        
        public event MazeItemMoveHandler MazeItemMoveStarted;
        public event MazeItemMoveHandler MazeItemMoveContinued;
        public event MazeItemMoveHandler MazeItemMoveFinished;

        public void OnMazeChanged(MazeInfo _Info)
        {
            CollectItems(_Info);
        }

        public void OnMazeOrientationChanged()
        {
            if (!Data.ProceedingMazeItems)
                return;
            MoveMazeItemsGravity(Data.Orientation, Data.CharacterInfo.Position);
        }
        
        public void OnCharacterMoveStarted(CharacterMovingEventArgs _Args)
        {
            if (!Data.ProceedingMazeItems)
                return;
            MoveMazeItemsGravity(Data.Orientation, _Args.To);
        }

        #region nonpublic members

        
        private void MoveMazeItemsGravity(MazeOrientation _Orientation, V2Int _CharacterPoint)
        {
            var dropDirection = RazorMazeUtils.GetDropDirection(_Orientation);
            foreach (var item in GetProceedInfos(EMazeItemType.GravityBlock).Values)
                MoveMazeItemGravity((MazeItemMovingProceedInfo)item, dropDirection, _CharacterPoint);
            foreach (var item in GetProceedInfos(EMazeItemType.GravityTrap).Values)
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
                    
                    while (RazorMazeUtils.IsValidPositionForMove(
                        Data.Info, 
                        _Info.Item,
                        pos + _DropDirection))
                    {
                        pos += _DropDirection;

                        if (_CharacterPoint.HasValue && _CharacterPoint.Value == pos )
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
            MazeItemMoveStarted?.Invoke(new MazeItemMoveEventArgs(
                item, _From, _To, Settings.gravityTrapSpeed, 0, busyPositions));
            var direction = (_To - _From).Normalized;
            float distance = V2Int.Distance(_From, _To);
            yield return Coroutines.Lerp(
                0f,
                1f,
                distance / Settings.gravityTrapSpeed,
                _Progress =>
                {
                    var addict = direction * (_Progress + 0.1f) * distance;
                    busyPositions.Clear();
                    busyPositions.Add(_From + addict.ToV2IntFloor());
                    if (busyPositions[0] != _To)
                        busyPositions.Add(_From + addict.ToV2IntCeil());
                    MazeItemMoveContinued?.Invoke(new MazeItemMoveEventArgs(
                        item, _From, _To, Settings.gravityTrapSpeed, _Progress, busyPositions));
                },
                GameTimeProvider.Instance,
                (_Stopped, _Progress) =>
                {
                    var to = !_Stopped ? _To : _Info.BusyPositions[0];  
                    item.Position = to;
                    MazeItemMoveFinished?.Invoke(new MazeItemMoveEventArgs(
                        item, _From, to, Settings.gravityTrapSpeed, _Progress, busyPositions, _Stopped));
                    _Info.IsProceeding = false;
                    busyPositions.Clear();
                    busyPositions.Add(to);
                });
        }

        #endregion
    }
}