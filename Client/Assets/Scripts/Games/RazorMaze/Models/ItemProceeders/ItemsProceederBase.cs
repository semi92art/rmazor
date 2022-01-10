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
        IMazeItemProceedInfo[] ProceedInfos { get; }
        EMazeItemType[]        Types        { get; }
    }

    public abstract class ItemsProceederBase : IItemsProceeder
    {
        #region types

        private class ViewMazeItemCoroutines
        {
            public IEnumerator[] Coroutines { get; private set; }
            public int           Count      { get; private set; }

            public ViewMazeItemCoroutines()
            {
                ClearArray();
            }

            public void AddCoroutine(IEnumerator _Coroutine)
            {
                Coroutines[Count++] = _Coroutine;
            }

            public void ClearArray()
            {
                Coroutines = new IEnumerator[500];
                Count = 0;
            }
        }

        #endregion
        
        #region nonpublic members
        
        private readonly Dictionary<IMazeItemProceedInfo, ViewMazeItemCoroutines> m_CoroutinesDict =
            new Dictionary<IMazeItemProceedInfo, ViewMazeItemCoroutines>();
        
        #endregion
        
        #region constants

        public const int StageIdle = 0; 
        
        #endregion
        
        #region inject
        
        protected ModelSettings      Settings     { get; }
        protected IModelData         Data         { get; }
        protected IModelCharacter    Character    { get; }
        protected IModelLevelStaging LevelStaging { get; }
        protected IModelGameTicker   GameTicker   { get; }
        
        protected ItemsProceederBase(
            ModelSettings _Settings, 
            IModelData _Data,
            IModelCharacter _Character,
            IModelLevelStaging _LevelStaging,
            IModelGameTicker _GameTicker)
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
        public IMazeItemProceedInfo[] ProceedInfos { get; private set; } = new IMazeItemProceedInfo[0];

        public virtual void OnLevelStageChanged(LevelStageArgs _Args)
        {
            switch (_Args.Stage)
            {
                case ELevelStage.Loaded:
                    CollectItems(Data.Info);
                    ClearCoroutines();
                    break;
                case ELevelStage.ReadyToStart when _Args.PreviousStage == ELevelStage.CharacterKilled:
                    break;
                case ELevelStage.ReadyToStart when _Args.PreviousStage != ELevelStage.Paused
                                                   && _Args.PreviousStage != ELevelStage.CharacterKilled:
                    StartProceed();
                    break;
                case ELevelStage.ReadyToStart when _Args.PreviousStage == ELevelStage.Paused:
                case ELevelStage.StartedOrContinued:
                    ContinueProceed();
                    break;
                case ELevelStage.Paused:
                    PauseProceed();
                    break;
                case ELevelStage.Finished when _Args.PreviousStage != ELevelStage.Paused:
                    ClearCoroutines();
                    break;
                case ELevelStage.Finished:
                case ELevelStage.ReadyToUnloadLevel: 
                case ELevelStage.Unloaded:
                case ELevelStage.CharacterKilled:
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.Stage);
            }
        }
        
        #endregion
        
        #region nonpublic methods
        
        private void CollectItems(MazeInfo _Info)
        {
            var infos = new List<IMazeItemProceedInfo>();

            var newInfos = _Info.MazeItems
                .Where(_Item => Types.Contains(_Item.Type))
                .Select(_Item =>
                {
                    IMazeItemProceedInfo res = new MazeItemProceedInfo
                    {
                        MoveByPathDirection = EMoveByPathDirection.Forward,
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
                m_CoroutinesDict.Add(newInfo, new ViewMazeItemCoroutines());
                if (!infos.Contains(newInfo))
                    infos.Add(newInfo);
            }
            ProceedInfos = infos.ToArray();
        }
        
        private void StartProceed()
        {
            foreach (var info in ProceedInfos)
            {
                info.IsProceeding = true;
                info.ReadyToSwitchStage = true;
                info.CurrentPosition = info.StartPosition;
                info.ProceedingStage = StageIdle;
            }
        }
        
        private void ContinueProceed()
        {
            foreach (var info in ProceedInfos)
                info.IsProceeding = true;
        }

        private void PauseProceed()
        {
            foreach (var info in ProceedInfos)
                info.IsProceeding = false;
        }

        private void FinishProceed()
        {
            var infos = ProceedInfos;
            foreach (var info in infos)
            {
                info.IsProceeding = false;
                info.ReadyToSwitchStage = false;
            }
        }

        protected void ProceedCoroutine(IMazeItemProceedInfo _ProceedInfo, IEnumerator _Coroutine)
        {
            m_CoroutinesDict[_ProceedInfo].AddCoroutine(_Coroutine);
            Coroutines.Run(_Coroutine);
        }
        
        private void ClearCoroutines()
        {
            foreach (var coroutines in m_CoroutinesDict
                .Select(_Kvp => _Kvp.Value))
            {
                foreach (var coroutine in coroutines.Coroutines)
                    Coroutines.Stop(coroutine);
                coroutines.ClearArray();
            }
        }
        
        protected static bool PathContainsItem(V2Int _From, V2Int _To, V2Int _Point)
        {
            var fullPath = RazorMazeUtils.GetFullPath(_From, _To);
            return fullPath.Contains(_Point);
        }
        
        #endregion
    }
}