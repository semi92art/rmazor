using System;
using System.Collections;
using System.Linq;
using Entities;
using Games.RazorMaze.Models.ProceedInfos;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public interface IGravityItemsProceeder : IMovingItemsProceeder, ICharacterMoveStarted, IOnGameLoopUpdate
    {
        void OnMazeOrientationChanged();
    }
    
    public class GravityItemsProceeder : MovingItemsProceederBase, IGravityItemsProceeder
    {
        #region constants

        public const int StageDrop = 1;
        
        #endregion

        #region inject
        
        public GravityItemsProceeder(
            ModelSettings _Settings,
            IModelMazeData _Data, 
            IModelCharacter _Character,
            IGameTicker _GameTicker) 
            : base (_Settings, _Data, _Character, _GameTicker) { }
        
        #endregion
        
        #region api
        
        public override EMazeItemType[] Types => new[] {EMazeItemType.GravityBlock, EMazeItemType.GravityTrap};


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
            foreach (var info in GetProceedInfos(Types)
                .Where(_Info => _Info.IsProceeding && _Info.ProceedingStage == StageIdle))
            {
                CheckForCharacterDeath(info, info.CurrentPosition.ToVector2());
            }
        }
        
        #endregion

        #region nonpublic methods
        
        private void MoveMazeItemsGravity(MazeOrientation _Orientation, V2Int _CharacterPoint)
        {
            var dropDirection = RazorMazeUtils.GetDropDirection(_Orientation);
            foreach (var info in GetProceedInfos(Types).Where(_Info => _Info.IsProceeding))
            {
                var coroutine = MoveMazeItemGravityCoroutine(
                    (MazeItemProceedInfo)info,
                    dropDirection,
                    _CharacterPoint);
                ProceedCoroutine(coroutine);
            }
        } 
        

        private IEnumerator MoveMazeItemGravityCoroutine(
            MazeItemProceedInfo _Info,
            V2Int _DropDirection,
            V2Int _CharacterPoint)
        {
            var gravityItems = RazorMazeUtils
                .GetBlockMazeItems(Data.Info.MazeItems)
                .Where(_Item => _Item.Type == EMazeItemType.GravityBlock
                                || _Item.Type == EMazeItemType.GravityTrap)
                .ToList();
            
            yield return Coroutines.WaitWhile(
                () => _Info.ProceedingStage == StageDrop,
                () =>
                {
                    _Info.ProceedingStage = StageDrop;
                    var pos = _Info.CurrentPosition;
                    bool doMoveByPath = false;
                    V2Int? altPos = null;
                    while (RazorMazeUtils.IsValidPositionForMove(Data.Info, pos + _DropDirection))
                    {
                        pos += _DropDirection;
                        if (_CharacterPoint == pos && _Info.Type == EMazeItemType.GravityBlock)
                            altPos = pos - _DropDirection;
                        var pos1 = pos;
                        if (_Info.Path.All(_Pos => pos1 != _Pos))
                            continue;
                        int fromPosIdx = _Info.Path.IndexOf(_Info.CurrentPosition);
                        if (fromPosIdx == -1)
                        {
                            doMoveByPath = true;
                            break;
                        }
                        int toPosIds = _Info.Path.IndexOf(pos);
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
                    if (!doMoveByPath || pos == _Info.CurrentPosition)
                    {
                        _Info.ProceedingStage = StageIdle;                       
                        return;
                    }
                    var from = _Info.CurrentPosition;
                    var to = altPos ?? pos;

                    Coroutines.Run(MoveMazeItemGravityCoroutineCore(_Info, from, to));
                });
        }

        private IEnumerator MoveMazeItemGravityCoroutineCore(
            IMazeItemProceedInfo _Info,
            V2Int _From,
            V2Int _To)
        {
            var busyPositions = _Info.BusyPositions;
            InvokeMoveStarted(new MazeItemMoveEventArgs(
                _Info, _From, _To, Settings.gravityTrapSpeed, 0, busyPositions));
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
                    InvokeMoveContinued(new MazeItemMoveEventArgs(
                        _Info, _From, _To, Settings.gravityTrapSpeed, _Progress, busyPositions));
                },
                GameTicker,
                (_Stopped, _Progress) =>
                {
                    var to = !_Stopped ? _To : _Info.BusyPositions[0];  
                    _Info.CurrentPosition = to;
                    InvokeMoveFinished(new MazeItemMoveEventArgs(
                        _Info, _From, to, Settings.gravityTrapSpeed, _Progress, busyPositions, _Stopped));
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