using System.Collections.Generic;
using System.Linq;
using Common;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.MazeItemGroups;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageController : IOnLevelStageChanged, IInit, IOnPathCompleted
    {
        void RegisterProceeder(IOnLevelStageChanged _Proceeder, int _ExecuteOrder);
    }

    public class ViewLevelStageController : InitBase, IViewLevelStageController
    {
        #region nonpublic members
        
        private readonly SortedDictionary<int, IOnLevelStageChanged> m_ProceedersToExecuteBeforeGroups      
            = new SortedDictionary<int, IOnLevelStageChanged>();
        private readonly SortedDictionary<int, IOnLevelStageChanged> m_ProceedersToExecuteAfterGroups      
            = new SortedDictionary<int, IOnLevelStageChanged>();

        private bool                m_FirstTimeLevelLoaded;
        private List<IViewMazeItem> m_MazeItemsCached;
        
        private bool                m_ShowRewardedOnUnload;

        #endregion

        #region inject

        private IManagersGetter                               Managers                               { get; }
        private IViewMazeItemsGroupSet                        MazeItemsGroupSet                      { get; }
        private IViewMazePathItemsGroup                       PathItemsGroup                         { get; }
        private IViewFullscreenTransitioner                   FullscreenTransitioner                 { get; }
        private IViewCameraEffectsCustomAnimator              CameraEffectsCustomAnimator            { get; }
        private ICameraProvider                               CameraProvider                         { get; }
        private ICoordinateConverter                          CoordinateConverter                    { get; }
        private IViewLevelStageSwitcher                       LevelStageSwitcher                     { get; }
        private IViewLevelStageControllerOnLevelFinished      StageControllerOnLevelFinished         { get; }
        private IViewLevelStageControllerOnLevelUnloaded      StageControllerOnLevelUnloaded         { get; }
        private IViewLevelStageControllerOnCharacterKilled    StageControllerOnCharacterKilled       { get; }
        private IViewLevelStageControllerOnLevelLoaded        StageControllerOnLevelLoaded           { get; }
        private IViewLevelStageControllerOnReadyToUnloadLevel StageControllerOnReadyToUnload         { get; }
        private IViewLevelStageControllerOnLevelReadyToStart  StageControllerOnLevelReadyToStart     { get; }
        private IViewLevelStageControllerOnExitLevelStaging   LevelStageControllerOnExitLevelStaging { get; }

        private ViewLevelStageController(
            IManagersGetter                               _Managers,
            IViewMazeItemsGroupSet                        _MazeItemsGroupSet,
            IViewMazePathItemsGroup                       _PathItemsGroup,
            IViewFullscreenTransitioner                   _FullscreenTransitioner,
            IViewCameraEffectsCustomAnimator              _CameraEffectsCustomAnimator,
            ICameraProvider                               _CameraProvider,
            ICoordinateConverter                          _CoordinateConverter,
            IViewLevelStageSwitcher                       _LevelStageSwitcher,
            IViewLevelStageControllerOnLevelFinished      _StageControllerOnLevelFinished,
            IViewLevelStageControllerOnLevelUnloaded      _StageControllerOnLevelUnloaded,
            IViewLevelStageControllerOnCharacterKilled    _StageControllerOnCharacterKilled,
            IViewLevelStageControllerOnLevelLoaded        _StageControllerOnLevelLoaded,
            IViewLevelStageControllerOnReadyToUnloadLevel _StageControllerOnReadyToUnload,
            IViewLevelStageControllerOnLevelReadyToStart  _StageControllerOnLevelReadyToStart,
            IViewLevelStageControllerOnExitLevelStaging   _LevelStageControllerOnExitLevelStaging)
        {
            Managers                               = _Managers;
            MazeItemsGroupSet                      = _MazeItemsGroupSet;
            PathItemsGroup                         = _PathItemsGroup;
            FullscreenTransitioner                 = _FullscreenTransitioner;
            CameraEffectsCustomAnimator            = _CameraEffectsCustomAnimator;
            CameraProvider                         = _CameraProvider;
            CoordinateConverter                    = _CoordinateConverter;
            LevelStageSwitcher                     = _LevelStageSwitcher;
            StageControllerOnLevelFinished         = _StageControllerOnLevelFinished;
            StageControllerOnLevelUnloaded         = _StageControllerOnLevelUnloaded;
            StageControllerOnCharacterKilled       = _StageControllerOnCharacterKilled;
            StageControllerOnLevelLoaded           = _StageControllerOnLevelLoaded;
            StageControllerOnReadyToUnload         = _StageControllerOnReadyToUnload;
            StageControllerOnLevelReadyToStart     = _StageControllerOnLevelReadyToStart;
            LevelStageControllerOnExitLevelStaging = _LevelStageControllerOnExitLevelStaging;
        }

        #endregion

        #region api

        public override void Init()
        {
            CameraProvider                    .Init();
            StageControllerOnLevelReadyToStart.Init();
            StageControllerOnReadyToUnload    .Init();
            StageControllerOnLevelFinished    .Init();
            Cor.Run(Cor.WaitNextFrame(CameraEffectsCustomAnimator.Init));
            FullscreenTransitioner.TransitionFinished += OnBetweenLevelTransitionFinished;
            base.Init();
        }

        public void RegisterProceeder(IOnLevelStageChanged _Proceeder, int _ExecuteOrder)
        {
            var dict = _ExecuteOrder < 0 ?
                m_ProceedersToExecuteBeforeGroups : m_ProceedersToExecuteAfterGroups;
            if (!dict.ContainsKey(_ExecuteOrder))
                dict.Add(_ExecuteOrder, _Proceeder);
            else 
                dict[_ExecuteOrder] = _Proceeder;
        }

        public void OnPathCompleted(V2Int _LastPath)
        {
            if (!MazorCommonData.Release)
                return;
            LevelStageSwitcher.SwitchLevelStage(EInputCommand.FinishLevel);
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            CommonDataRmazor.LastLevelStageArgs = _Args;
            SetCameraProviderProps(_Args);
            ProceedProceedersToExecuteBeforeMazeItemGroups(_Args);
            MazeItemsGroupSet.OnLevelStageChanged(_Args);
            ProceedProceedersToExecuteAfterMazeItemGroups(_Args);
            switch (_Args.LevelStage)
            {
                case ELevelStage.None:
                    LevelStageControllerOnExitLevelStaging.OnExitLevelStaging(_Args);
                    break;
                case ELevelStage.Loaded:
                    StageControllerOnLevelLoaded.OnLevelLoaded(_Args, GetMazeItems(_Args));
                    break;
                case ELevelStage.ReadyToStart:
                    StageControllerOnLevelReadyToStart.OnLevelReadyToStart(_Args);
                    break;
                case ELevelStage.StartedOrContinued:
                    Managers.AudioManager.UnmuteAudio(EAudioClipType.GameSound);
                    break;
                case ELevelStage.Paused:
                    break;
                case ELevelStage.Finished:
                    StageControllerOnLevelFinished.OnLevelFinished(_Args);
                    break;
                case ELevelStage.ReadyToUnloadLevel:
                    StageControllerOnReadyToUnload.OnReadyToUnloadLevel(_Args, GetMazeItems(_Args));
                    break;
                case ELevelStage.Unloaded:
                    StageControllerOnLevelUnloaded.OnLevelUnloaded(_Args);
                    break;
                case ELevelStage.CharacterKilled:
                    StageControllerOnCharacterKilled.OnCharacterKilled(_Args, GetMazeItems(_Args));
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Args.LevelStage);
            }
            CameraEffectsCustomAnimator.OnLevelStageChanged(_Args);
        }

        #endregion

        #region nonpublic methods
        
        private void OnBetweenLevelTransitionFinished(bool _Appear)
        {
            if (_Appear)
                return;
            LevelStageSwitcher.SwitchLevelStage(EInputCommand.ReadyToStartLevel);
        }

        private void SetCameraProviderProps(LevelStageArgs _Args)
        {
            if (_Args.LevelStage != ELevelStage.Loaded)
                return;
            CameraProvider.GetMazeBounds = CoordinateConverter.GetMazeBounds;
            CameraProvider.GetConverterScale = () => CoordinateConverter.Scale;
            CameraProvider.UpdateState();
        }

        private void ProceedProceedersToExecuteBeforeMazeItemGroups(LevelStageArgs _Args)
        {
            foreach (var proceeder in m_ProceedersToExecuteBeforeGroups)
                proceeder.Value.OnLevelStageChanged(_Args);
        }
        
        private void ProceedProceedersToExecuteAfterMazeItemGroups(LevelStageArgs _Args)
        {
            foreach (var proceeder in m_ProceedersToExecuteAfterGroups)
                proceeder.Value.OnLevelStageChanged(_Args);
        }

        private IReadOnlyCollection<IViewMazeItem> GetMazeItems(LevelStageArgs _Args)
        {
            var mazeItems = _Args.LevelStage == ELevelStage.Loaded ? 
                m_MazeItemsCached = GetMazeAndPathItems() : m_MazeItemsCached;
            mazeItems.AddRange(PathItemsGroup.PathItems);
            return mazeItems;
        }
        
        private List<IViewMazeItem> GetMazeAndPathItems()
        {
            var mazeItems = new List<IViewMazeItem>();
            var mazeItemGroups = MazeItemsGroupSet.GetGroups()
                .Where(_P => _P != null);
            foreach (var group in mazeItemGroups)
                mazeItems.AddRange(group.GetActiveItems());
            mazeItems.AddRange(PathItemsGroup.PathItems);
            return mazeItems;
        }

        #endregion
    }
}