using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entities;
using Exceptions;
using Games.RazorMaze.Models.ProceedInfos;
using Games.RazorMaze.Views;
using Ticker;
using Utils;

namespace Games.RazorMaze.Models.ItemProceeders
{
    public interface IItemsProceeder : IOnLevelStageChanged
    {
        Dictionary<EMazeItemType, List<IMazeItemProceedInfo>> ProceedInfos { get; }
        EMazeItemType[] Types { get; }
    }

    public abstract class ItemsProceederBase : IItemsProceeder
    {
        #region nonpublic members
        
        private readonly Queue<IEnumerator> m_Coroutines = new Queue<IEnumerator>();
        protected IMazeItemProceedInfo KillerProceedInfo { get; set; }
        
        #endregion
        
        #region constants

        public const int StageIdle = 0; 
        
        #endregion
        
        #region inject
        
        protected ModelSettings Settings { get; }
        protected IModelData Data { get; }
        protected IModelCharacter Character { get; }
        protected IModelLevelStaging LevelStaging { get; }
        protected IGameTicker GameTicker { get; }
        
        protected ItemsProceederBase(
            ModelSettings _Settings, 
            IModelData _Data,
            IModelCharacter _Character,
            IModelLevelStaging _LevelStaging,
            IGameTicker _GameTicker)
        {
            Settings = _Settings;
            Data = _Data;
            Character = _Character;
            LevelStaging = _LevelStaging;
            GameTicker = _GameTicker;
            GameTicker.Register(this);
        }

        #endregion

        #region api
        
        public abstract EMazeItemType[] Types { get; }
        public Dictionary<EMazeItemType, List<IMazeItemProceedInfo>> ProceedInfos { get; } =
            new Dictionary<EMazeItemType, List<IMazeItemProceedInfo>>();
        
        public virtual void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    CollectItems(Data.Info); break;
                case ELevelStage.StartedOrContinued:
                    ContinueProceed();
                    break;
                case ELevelStage.ReadyToStartOrContinue:
                    StartProceed(true); break;
                case ELevelStage.Paused:
                    PauseProceed(); break;
                case ELevelStage.Unloaded:
                case ELevelStage.Finished:
                case ELevelStage.CharacterKilled:
                    FinishProceed(true); break;
                case ELevelStage.ReadyToUnloadLevel: break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.Stage);
            }
            
            if (_Args.Stage == ELevelStage.Loaded 
                ||_Args.Stage == ELevelStage.ReadyToStartOrContinue)
            {
                foreach (var coroutine in m_Coroutines)
                    Coroutines.Stop(coroutine);
                m_Coroutines.Clear();
            }
        }
        
        #endregion
        
        #region nonpublic methods
        
        private void CollectItems(MazeInfo _Info)
        {
            ProceedInfos.Clear();
            foreach (var type in Types)
                ProceedInfos.Add(type, new List<IMazeItemProceedInfo>());

            var newInfos = _Info.MazeItems
                .Where(_Item => Types.Contains(_Item.Type))
                .Select(_Item =>
                {
                    IMazeItemProceedInfo res = new MazeItemProceedInfo
                    {
                        MoveByPathDirection = EMazeItemMoveByPathDirection.Forward,
                        IsProceeding = false,
                        PauseTimer = 0,
                        BusyPositions = new List<V2Int> {_Item.Position},
                        ProceedingStage = 0,
                        CurrentPosition = _Item.Position,
                        NextPosition = _Item.Position
                    };
                    res.SetItem(_Item);
                    return res;
                });
            foreach (var newInfo in newInfos)
            {
                var list = ProceedInfos[newInfo.Type];
                if (!list.Contains(newInfo))
                    list.Add(newInfo);
            }
        }

        protected IEnumerable<IMazeItemProceedInfo> GetProceedInfos(IEnumerable<EMazeItemType> _Types)
        {
            return _Types.SelectMany(_Type => 
                ProceedInfos.ContainsKey(_Type) ? ProceedInfos[_Type] : new List<IMazeItemProceedInfo>());
        }
        
        private void StartProceed(bool? _ProceedKillerInfoIfTrue = null)
        {
            var infos = GetProceedInfos(Types);
            foreach (var info in infos)
            {
                info.IsProceeding = true;
                info.ReadyToSwitchStage = true;
                info.CurrentPosition = info.StartPosition;
                info.ProceedingStage = StageIdle;
            }
            if (_ProceedKillerInfoIfTrue.HasValue
                && !_ProceedKillerInfoIfTrue.Value 
                && KillerProceedInfo != null)
            {
                KillerProceedInfo.IsProceeding = false;
                KillerProceedInfo = null;
            }
        }
        
        private void ContinueProceed()
        {
            foreach (var info in GetProceedInfos(Types))
                info.IsProceeding = true;
        }

        private void PauseProceed()
        {
            foreach (var info in GetProceedInfos(Types))
                info.IsProceeding = false;
        }

        private void FinishProceed(bool _DropInfo)
        {
            var infos = GetProceedInfos(Types);
            foreach (var info in infos)
            {
                info.IsProceeding = false;
                info.ReadyToSwitchStage = false;
                if (_DropInfo)
                {
                    info.CurrentPosition = info.StartPosition;
                }
            }
        }

        protected void ProceedCoroutine(IEnumerator _Coroutine)
        {
            m_Coroutines.Enqueue(_Coroutine);
            Coroutines.Run(_Coroutine);
        }
        
        protected static bool PathContainsItem(V2Int _From, V2Int _To, V2Int _Point)
        {
            var fullPath = RazorMazeUtils.GetFullPath(_From, _To);
            return fullPath.Contains(_Point);
        }
        
        #endregion
    }
}