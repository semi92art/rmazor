using System;
using System.Collections;
using System.Linq;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;
using TimeProviders;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public interface IGravityItemsProceeder : IMovingItemsProceeder, ICharacterMoveStarted, IOnGameLoopUpdate
    {
        void OnMazeOrientationChanged();
    }
    
    public class GravityItemsProceeder : ItemsProceederBase, IGravityItemsProceeder
    {
        #region constants

        public const int StageDrop = 1;
        
        #endregion
        
        #region nonpublic members

        protected override EMazeItemType[] Types => new[] {EMazeItemType.GravityBlock, EMazeItemType.GravityTrap};
        
        #endregion
        
        #region inject
        
        public GravityItemsProceeder(ModelSettings _Settings, IModelMazeData _Data, IModelCharacter _Character) 
            : base (_Settings, _Data, _Character) { }
        
        #endregion
        
        public event MazeItemMoveHandler MazeItemMoveStarted;
        public event MazeItemMoveHandler MazeItemMoveContinued;
        public event MazeItemMoveHandler MazeItemMoveFinished;

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            if (_Args.Stage == ELevelStage.StartedOrContinued)
                MoveMazeItemsGravity(Data.Orientation, Character.Position);
        }

        public void OnMazeOrientationChanged()
        {
            MoveMazeItemsGravity(Data.Orientation, Character.Position);
        }
        
        public void OnCharacterMoveStarted(CharacterMovingEventArgs _Args)
        {
            MoveMazeItemsGravity(Data.Orientation, _Args.To);
        }
        
        public void OnGameLoopUpdate()
        {
            var infos = GetProceedInfos(Types).Values;
            foreach (var info in infos.Where(_Info => _Info.IsProceeding && _Info.ProceedingStage == StageIdle))
            {
                CheckForCharacterDeath(info, info.Item.Position.ToVector2());
            }
        }

        #region nonpublic methods
        
        private void MoveMazeItemsGravity(MazeOrientation _Orientation, V2Int _CharacterPoint)
        {
            var dropDirection = RazorMazeUtils.GetDropDirection(_Orientation);
            foreach (var info in GetProceedInfos(Types).Values
                    .Where(_Info => _Info.IsProceeding))
                MoveMazeItemGravity((MazeItemProceedInfo)info, dropDirection, _CharacterPoint);
        } 
        
        private void MoveMazeItemGravity(
            MazeItemProceedInfo _Info, 
            V2Int _DropDirection,
            V2Int _CharacterPoint)
        {
            var gravityItems = RazorMazeUtils
                .GetBlockMazeItems(Data.Info.MazeItems)
                .Where(_Item => _Item.Type == EMazeItemType.GravityBlock
                                || _Item.Type == EMazeItemType.GravityTrap)
                .ToList();
            Coroutines.Run(Coroutines.WaitWhile(
                () => _Info.ProceedingStage == StageDrop,
                () =>
                {
                    _Info.ProceedingStage = StageDrop;
                    var pos = _Info.Item.Position;
                    bool doMoveByPath = false;
                    V2Int? altPos = null;
                    while (RazorMazeUtils.IsValidPositionForMove(Data.Info, pos + _DropDirection))
                    {
                        pos += _DropDirection;
                        if (_CharacterPoint == pos && _Info.Item.Type == EMazeItemType.GravityBlock)
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
                    if (!doMoveByPath)
                    {
                        if (gravityItems.Any(_Item => _Item.Position == pos + _DropDirection))
                            doMoveByPath = true;
                    }
                    if (!doMoveByPath || pos == _Info.Item.Position)
                    {
                        _Info.ProceedingStage = StageIdle;                       
                        return;
                    }
                    var from = _Info.Item.Position;
                    var to = altPos ?? pos;

                    Coroutines.Run(MoveMazeItemGravityCoroutine(_Info, from, to));
                }));
        }
        
        private IEnumerator MoveMazeItemGravityCoroutine(
            MazeItemProceedInfo _Info,
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
                    busyPositions.Add(_From + V2Int.Floor(addict));
                    if (busyPositions[0] != _To)
                        busyPositions.Add(_From + V2Int.Ceil(addict));
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
                    _Info.ProceedingStage = StageIdle;
                    busyPositions.Clear();
                    busyPositions.Add(to);
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