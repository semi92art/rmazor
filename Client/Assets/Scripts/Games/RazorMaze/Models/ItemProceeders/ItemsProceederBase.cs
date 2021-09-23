using System.Collections.Generic;
using System.Linq;
using Entities;
using Exceptions;
using Games.RazorMaze.Models.ProceedInfos;
using Games.RazorMaze.Views;
using Ticker;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public interface IItemsProceeder : IOnLevelStageChanged
    { }

    public abstract class ItemsProceederBase : IItemsProceeder
    {
        #region constants

        public const int StageIdle = 0; 
        
        #endregion
        
        #region nonpublic members
        
        protected IMazeItemProceedInfo KillerProceedInfo { get; set; }
        protected abstract EMazeItemType[] Types { get; }
        protected ModelSettings Settings { get; }
        protected IModelMazeData Data { get; }
        protected IModelCharacter Character { get; }
        protected IGameTicker GameTicker { get; }

        #endregion

        #region api
        
        public virtual void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    CollectItems(Data.Info); break;
                case ELevelStage.StartedOrContinued:
                    break;
                case ELevelStage.ReadyToStartOrContinue:
                    StartProceed(true, true); break;
                case ELevelStage.Paused:
                case ELevelStage.Finished:
                case ELevelStage.Unloaded:
                    StartProceed(false); break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.Stage);
            }
        }
        
        #endregion
        
        #region nonpublic methods
        
        protected ItemsProceederBase(
            ModelSettings _Settings, 
            IModelMazeData _Data,
            IModelCharacter _Character,
            IGameTicker _GameTicker)
        {
            Settings = _Settings;
            Data = _Data;
            Character = _Character;
            GameTicker = _GameTicker;
        }

        protected void CollectItems(MazeInfo _Info)
        {
            var newInfos = _Info.MazeItems
                .Where(_Item => Types.Contains(_Item.Type))
                .Select(_Item => new MazeItemProceedInfo
                {
                    Item = _Item,
                    MoveByPathDirection = EMazeItemMoveByPathDirection.Forward,
                    IsProceeding = false,
                    PauseTimer = 0,
                    BusyPositions = new List<V2Int>{_Item.Position},
                    ProceedingStage = 0
                });
            var infos = Data.ProceedInfos;
            foreach (var newInfo in newInfos)
            {
                if (!infos.ContainsKey(newInfo.Item.Type))
                    infos.Add(newInfo.Item.Type, new Dictionary<MazeItem, IMazeItemProceedInfo>());
                var dict = infos[newInfo.Item.Type];
                
                if (!dict.ContainsKey(newInfo.Item))
                    dict.Add(newInfo.Item, newInfo);
                else
                    dict[newInfo.Item] = newInfo;
            }
        }

        protected Dictionary<MazeItem, IMazeItemProceedInfo> GetProceedInfos(IEnumerable<EMazeItemType> _Types)
        {
            return _Types.SelectMany(_Type => Data.ProceedInfos[_Type]).ToDictionary(
                _Kvp => _Kvp.Key,
                _Kvp => _Kvp.Value);
        }

        private void StartProceed(bool _Proceed, bool? _ProceedKillerInfoIfTrue = null)
        {
            var infos = GetProceedInfos(Types);
            foreach (var info in infos)
            {
                info.Value.IsProceeding = _Proceed;
                info.Value.ReadyToSwitchStage = _Proceed;
            }
            if (_ProceedKillerInfoIfTrue.HasValue && !_ProceedKillerInfoIfTrue.Value && KillerProceedInfo != null)
            {
                KillerProceedInfo.IsProceeding = false;
                KillerProceedInfo = null;
            }
        }
        
        #endregion
    }
}