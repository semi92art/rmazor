using System.Collections;
using System.Linq;
using Entities;
using Exceptions;
using Games.RazorMaze.Models.ProceedInfos;
using Ticker;
using UnityEngine;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze.Models.ItemProceeders
{

    public interface ITrapsMovingProceeder : IMovingItemsProceeder { }
    

    public class TrapsMovingProceeder : MovingItemsProceederBase, IUpdateTick, ITrapsMovingProceeder
    {
        #region constants

        public const int StageMoving = 1;

        #endregion

        #region inject
        
        private IPathItemsProceeder PathItemsProceeder { get; }
        
        public TrapsMovingProceeder(
            ModelSettings _Settings, 
            IModelData _Data,
            IModelCharacter _Character,
            IModelLevelStaging _LevelStaging,
            IPathItemsProceeder _PathItemsProceeder,
            IModelGameTicker _GameTicker)
            : base(_Settings, _Data, _Character, _LevelStaging, _GameTicker)
        {
            PathItemsProceeder = _PathItemsProceeder;
        }
        
        #endregion
        
        #region api
        
        public override EMazeItemType[] Types => new[] {EMazeItemType.TrapMoving};

        public void UpdateTick()
        {
            ProceedTrapsMoving();
        }
        
        #endregion

        #region nonpublic methods
        
        private void ProceedTrapsMoving()
        {
            for (int i = 0; i < ProceedInfos.Length; i++)
            {
                var info = ProceedInfos[i];
                if (!info.IsProceeding)
                    continue;
                if (info.ProceedingStage != StageIdle)
                    continue;
                CheckForCharacterDeath(info, info.CurrentPosition.ToVector2());
                info.PauseTimer += Time.deltaTime;
                if (info.PauseTimer < Settings.MovingItemsPause)
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
            V2Int from = _Info.CurrentPosition;
            V2Int to;
            int idx = _Info.Path.IndexOf(_Info.CurrentPosition);
            var path = _Info.Path.ToList();
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
            var coroutine = MoveTrapMovingCoroutine(_Info, from, to, _OnFinish);
            ProceedCoroutine(_Info, coroutine);
        }
        
        private IEnumerator MoveTrapMovingCoroutine(
            IMazeItemProceedInfo _Info, 
            V2Int _From,
            V2Int _To,
            UnityAction _OnFinish)
        {
            _Info.IsMoving = true;
            InvokeMoveStarted(new MazeItemMoveEventArgs(
                _Info, _From, _To, Settings.MovingItemsSpeed,0, _Info.BusyPositions));
            float distance = V2Int.Distance(_From, _To);
            yield return Coroutines.Lerp(
                0f,
                1f,
                distance / Settings.MovingItemsSpeed,
                _Progress =>
                {
                    var precisePosition = V2Int.Lerp(_From, _To, _Progress);
                    _Info.CurrentPosition = V2Int.Round(precisePosition);
                    CheckForCharacterDeath(_Info, precisePosition);
                    InvokeMoveContinued(new MazeItemMoveEventArgs(
                        _Info, _From, _To, Settings.MovingItemsSpeed, _Progress, _Info.BusyPositions));
                },
                GameTicker,
                (_Stopped, _Progress) =>
                {
                    _Info.CurrentPosition = _To;
                    float progress = _Stopped ? _Progress : 1;
                    _Info.IsMoving = false;
                    _OnFinish?.Invoke();
                    InvokeMoveFinished(new MazeItemMoveEventArgs(
                        _Info, _From, _To, Settings.MovingItemsSpeed,progress, _Info.BusyPositions, _Stopped));
                });
        }
        
        private void CheckForCharacterDeath(IMazeItemProceedInfo _Info, Vector2 _ItemPrecisePosition)
        {
            if (!Character.Alive || PathItemsProceeder.AllPathsProceeded)
                return;
            var cPos = Character.IsMoving ?
                Character.MovingInfo.PrecisePosition : Character.Position.ToVector2();
            if (Vector2.Distance(cPos, _ItemPrecisePosition) + RazorMazeUtils.Epsilon > 1f)
                return;
            KillerProceedInfo = _Info;
            LevelStaging.KillCharacter();
        }

        #endregion
    }
}