// ReSharper disable ClassNeverInstantiated.Global
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.CameraProviders;
using Common.Entities;
using Common.Enums;
using Common.Exceptions;
using Common.Helpers;
using Common.UI.DialogViewers;
using Common.Utils;
using Lean.Touch;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.UI.Panels;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItemGroups;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.Common.ViewLevelStageController
{
    public interface IViewLevelStageController : IOnLevelStageChanged, IInit, IOnPathCompleted
    {
        void RegisterProceeders(List<IOnLevelStageChanged> _Proceeder, int _ExecuteOrder);
    }

    public class ViewLevelStageController : InitBase, IViewLevelStageController
    {
        #region nonpublic members
        
        private static AudioClipArgs AudioClipArgsLevelStart => 
            new AudioClipArgs("level_start", EAudioClipType.GameSound);
        private static AudioClipArgs AudioClipArgsLevelComplete => 
            new AudioClipArgs("level_complete", EAudioClipType.GameSound);

        private readonly SortedDictionary<int, List<IOnLevelStageChanged>> m_ProceedersToExecuteBeforeGroups      
            = new SortedDictionary<int, List<IOnLevelStageChanged>>();
        private readonly SortedDictionary<int, List<IOnLevelStageChanged>> m_ProceedersToExecuteAfterGroups      
            = new SortedDictionary<int, List<IOnLevelStageChanged>>();

        private bool                m_FirstTimeLevelLoaded;
        private List<IViewMazeItem> m_MazeItemsCached;
        
        private bool                m_ShowRewardedOnUnload;

        #endregion

        #region inject

        private IModelGame                                    Model                              { get; }
        private IManagersGetter                               Managers                           { get; }
        private IDialogViewersController                      DialogViewersController            { get; }
        private IViewMazeItemsGroupSet                        MazeItemsGroupSet                  { get; }
        private IViewMazePathItemsGroup                       PathItemsGroup                     { get; }
        private IViewFullscreenTransitioner                   FullscreenTransitioner             { get; }
        private IViewCameraEffectsCustomAnimator              CameraEffectsCustomAnimator        { get; }
        private IViewInputTouchProceeder                      TouchProceeder                     { get; }
        private IMoneyCounter                                 MoneyCounter                       { get; }
        private ICameraProvider                               CameraProvider                     { get; }
        private ICoordinateConverter                          CoordinateConverter                { get; }
        private IViewSwitchLevelStageCommandInvoker           SwitchLevelStageCommandInvoker     { get; }
        private IViewLevelStageControllerOnLevelFinished      StageControllerOnLevelFinished     { get; }
        private IViewLevelStageControllerOnLevelUnloaded      StageControllerOnLevelUnloaded     { get; }
        private IViewLevelStageControllerOnCharacterKilled    StageControllerOnCharacterKilled   { get; }
        private IViewLevelStageControllerOnLevelLoaded        StageControllerOnLevelLoaded       { get; }
        private IViewLevelStageControllerOnReadyToUnloadLevel StageControllerOnReadyToUnload     { get; }
        private IViewLevelStageControllerOnLevelReadyToStart  StageControllerOnLevelReadyToStart { get; }

        private ViewLevelStageController(
            IModelGame                                    _Model,
            IManagersGetter                               _Managers,
            IDialogViewersController                      _DialogViewersController,
            IViewMazeItemsGroupSet                        _MazeItemsGroupSet,
            IViewMazePathItemsGroup                       _PathItemsGroup,
            IViewFullscreenTransitioner                   _FullscreenTransitioner,
            IViewCameraEffectsCustomAnimator              _CameraEffectsCustomAnimator,
            IViewInputTouchProceeder                      _TouchProceeder,
            IMoneyCounter                                 _MoneyCounter,
            ICameraProvider                               _CameraProvider,
            ICoordinateConverter                          _CoordinateConverter,
            IViewSwitchLevelStageCommandInvoker           _SwitchLevelStageCommandInvoker,
            IViewLevelStageControllerOnLevelFinished      _StageControllerOnLevelFinished,
            IViewLevelStageControllerOnLevelUnloaded      _StageControllerOnLevelUnloaded,
            IViewLevelStageControllerOnCharacterKilled    _StageControllerOnCharacterKilled,
            IViewLevelStageControllerOnLevelLoaded        _StageControllerOnLevelLoaded,
            IViewLevelStageControllerOnReadyToUnloadLevel _StageControllerOnReadyToUnload,
            IViewLevelStageControllerOnLevelReadyToStart  _StageControllerOnLevelReadyToStart)
        {
            Model                            = _Model;
            Managers                         = _Managers;
            DialogViewersController          = _DialogViewersController;
            MazeItemsGroupSet                = _MazeItemsGroupSet;
            PathItemsGroup                   = _PathItemsGroup;
            FullscreenTransitioner           = _FullscreenTransitioner;
            CameraEffectsCustomAnimator      = _CameraEffectsCustomAnimator;
            CameraProvider                   = _CameraProvider;
            CoordinateConverter              = _CoordinateConverter;
            SwitchLevelStageCommandInvoker   = _SwitchLevelStageCommandInvoker;
            StageControllerOnLevelFinished   = _StageControllerOnLevelFinished;
            StageControllerOnLevelUnloaded   = _StageControllerOnLevelUnloaded;
            StageControllerOnCharacterKilled = _StageControllerOnCharacterKilled;
            StageControllerOnLevelLoaded     = _StageControllerOnLevelLoaded;
            StageControllerOnReadyToUnload   = _StageControllerOnReadyToUnload;
            StageControllerOnLevelReadyToStart = _StageControllerOnLevelReadyToStart;
            TouchProceeder                   = _TouchProceeder;
            MoneyCounter                     = _MoneyCounter;
        }

        #endregion

        #region api

        public override void Init()
        {
            MoneyCounter.Init();
            TouchProceeder.Tap += OnTapScreenAction;
            CameraProvider.Init();
            StageControllerOnLevelReadyToStart.Init();
            Cor.Run(Cor.WaitNextFrame(CameraEffectsCustomAnimator.Init));
            FullscreenTransitioner.TransitionFinished += OnBetweenLevelTransitionFinished;
            Managers.AudioManager.InitClip(AudioClipArgsLevelStart);
            Managers.AudioManager.InitClip(AudioClipArgsLevelComplete);
            base.Init();
        }

        public void RegisterProceeders(List<IOnLevelStageChanged> _Proceeders, int _ExecuteOrder)
        {
            var dict = _ExecuteOrder < 0 ?
                m_ProceedersToExecuteBeforeGroups : m_ProceedersToExecuteAfterGroups;
            if (!dict.ContainsKey(_ExecuteOrder))
                dict.Add(_ExecuteOrder, _Proceeders);
            else 
                dict[_ExecuteOrder] = _Proceeders;
        }

        public void OnPathCompleted(V2Int _LastPath)
        {
            if (!CommonData.Release)
                return;
            Model.LevelStaging.FinishLevel(Model.LevelStaging.Arguments);
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

        private void OnTapScreenAction(LeanFinger _Finger)
        {
            if (_Finger.LastScreenPosition.y / GraphicUtils.ScreenSize.y > 0.9f)
                return;
            if (Model.LevelStaging.LevelStage != ELevelStage.Finished)
                return;
            var dialogViewerTypes = new[]
            {
                EDialogViewerType.Fullscreen,
                EDialogViewerType.Medium1,
                EDialogViewerType.Medium2
            };
            foreach (var dialogViewerType in dialogViewerTypes)
            {
                var dv = DialogViewersController.GetViewer(dialogViewerType);
                if (dv.CurrentPanel is IFinishLevelGroupDialogPanel)
                    return;
                if (dv.CurrentPanel is IPlayBonusLevelDialogPanel)
                    return;
                if (dv.CurrentPanel is IRateGameDialogPanel)
                    return;
            }
            InvokeStartUnloadingLevel(CommonInputCommandArg.ParameterScreenTap);
        }
        
        private void InvokeStartUnloadingLevel(string _Source)
        {
            var args = new Dictionary<string, object> {{CommonInputCommandArg.KeySource, _Source}};
            SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.StartUnloadingLevel, args);
        }
        
        private void OnBetweenLevelTransitionFinished(bool _Appear)
        {
            if (_Appear)
                return;
            SwitchLevelStageCommandInvoker.SwitchLevelStage(EInputCommand.ReadyToStartLevel);
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
                proceeder.Value.ForEach(_P => _P.OnLevelStageChanged(_Args));
        }
        
        private void ProceedProceedersToExecuteAfterMazeItemGroups(LevelStageArgs _Args)
        {
            foreach (var proceeder in m_ProceedersToExecuteAfterGroups)
                proceeder.Value.ForEach(_P => _P.OnLevelStageChanged(_Args));
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