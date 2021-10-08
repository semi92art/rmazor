using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.InputConfigurators;
using Games.RazorMaze.Views.MazeItemGroups;
using Games.RazorMaze.Views.Rotation;
using Games.RazorMaze.Views.UI;
using Ticker;
using Utils;

namespace Games.RazorMaze.Views
{
    public class ViewGame : IViewGame
    {
        #region constants

        private const string SoundClipNameLevelStart = "level_start";
        private const string SoundClipNameLevelComplete = "level_complete";
        
        #endregion
        
        public IContainersGetter ContainersGetter { get; }
        public IViewUI UI { get; }
        public IInputConfigurator InputConfigurator { get; }
        public IViewCharacter Character { get; }
        public IViewMazeCommon Common { get; }
        public IViewMazeBackground Background { get; }
        public IViewMazeRotation MazeRotation { get; }
        public IViewMazePathItemsGroup PathItemsGroup { get; }
        public IViewMazeMovingItemsGroup MovingItemsGroup { get; }
        public IViewMazeTrapsReactItemsGroup TrapsReactItemsGroup { get; }
        public IViewMazeTrapsIncItemsGroup TrapsIncItemsGroup { get; }
        public IViewMazeTurretsGroup TurretsGroup { get; }
        public IViewMazePortalsGroup PortalsGroup { get; }
        public IViewMazeShredingerBlocksGroup ShredingerBlocksGroup { get; }
        public IViewMazeSpringboardItemsGroup SpringboardItemsGroup { get; }
        
        private IGameTicker GameTicker { get; }
        public IManagersGetter Managers { get; }

        public ViewGame(
            IContainersGetter _ContainersGetter,
            IViewUI _UI,
            IInputConfigurator _InputConfigurator,
            IViewCharacter _Character,
            IViewMazeCommon _Common,
            IViewMazeBackground _Background,
            IViewMazeRotation _MazeRotation,
            IViewMazePathItemsGroup _PathItemsGroup,
            IViewMazeMovingItemsGroup _MovingItemsGroup,
            IViewMazeTrapsReactItemsGroup _TrapsReactItemsGroup,
            IViewMazeTrapsIncItemsGroup _TrapsIncItemsGroup,
            IViewMazeTurretsGroup _TurretsGroup,
            IViewMazePortalsGroup _PortalsGroup,
            IViewMazeShredingerBlocksGroup _ShredingerBlocksGroup,
            IViewMazeSpringboardItemsGroup _SpringboardItemsGroup,
            IGameTicker _GameTicker,
            IManagersGetter _Managers)
        {
            ContainersGetter = _ContainersGetter;
            UI = _UI;
            InputConfigurator = _InputConfigurator;
            Character = _Character;
            Common = _Common;
            Background = _Background;
            MazeRotation = _MazeRotation;
            PathItemsGroup = _PathItemsGroup;
            MovingItemsGroup = _MovingItemsGroup;
            TrapsReactItemsGroup = _TrapsReactItemsGroup;
            TrapsIncItemsGroup = _TrapsIncItemsGroup;
            TurretsGroup = _TurretsGroup;
            PortalsGroup = _PortalsGroup;
            ShredingerBlocksGroup = _ShredingerBlocksGroup;
            SpringboardItemsGroup = _SpringboardItemsGroup;
            GameTicker = _GameTicker;
            Managers = _Managers;
        }
        
        public event NoArgsHandler PreInitialized;
        public event NoArgsHandler Initialized;
        public event NoArgsHandler PostInitialized;
        
        public void PreInit()
        {
            var iBackColChangedProceeders = 
                RazorMazeUtils.GetInterfaceOfProceeders<IOnBackgroundColorChanged>(GetProceeders());
            foreach (var proceeder in iBackColChangedProceeders)
                Background.BackgroundColorChanged += proceeder.OnBackgroundColorChanged;
            
            RazorMazeUtils.CallInits<IPreInit>(GetProceeders(),
                _PreInitedArray =>
                {
                    Coroutines.Run(Coroutines.WaitWhile(
                        () => _PreInitedArray.Any(_PreInited => !_PreInited), 
                        () => PreInitialized?.Invoke()));
                });
        }

        public void Init()
        {
            RazorMazeUtils.CallInits<IInit>(GetProceeders(),
                _InitedArray =>
                {
                    Coroutines.Run(Coroutines.WaitWhile(
                        () => _InitedArray.Any(_Inited => !_Inited), 
                        () => Initialized?.Invoke()));
                });
        }
        
        public void PostInit()
        {
            RazorMazeUtils.CallInits<IPostInit>(GetProceeders(),
                _PostInitedArray =>
                {
                    Coroutines.Run(Coroutines.WaitWhile(
                        () => _PostInitedArray.Any(_PostInited => !_PostInited), 
                        () => PostInitialized?.Invoke()));
                });
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            var proceeders =
                RazorMazeUtils.GetInterfaceOfProceeders<IOnLevelStageChanged>(GetProceeders());
            foreach (var proceeder in proceeders)
                proceeder.OnLevelStageChanged(_Args);
            GameTicker.Pause = _Args.Stage == ELevelStage.Paused;

            if (_Args.Stage == ELevelStage.Loaded)
                Managers.Notify(_SM => _SM.PlayClip(SoundClipNameLevelStart));
            else if (_Args.Stage == ELevelStage.Finished)
                Managers.Notify(_SM => _SM.PlayClip(SoundClipNameLevelComplete));
        }
        
        public void OnCharacterMoveStarted(CharacterMovingEventArgs _Args)
        {
            var proceeders = 
                RazorMazeUtils.GetInterfaceOfProceeders<ICharacterMoveStarted>(GetProceeders());
            foreach (var proceeder in proceeders)
                proceeder.OnCharacterMoveStarted(_Args);
        }

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            var proceeders =
                RazorMazeUtils.GetInterfaceOfProceeders<ICharacterMoveContinued>(GetProceeders());
            foreach (var proceeder in proceeders)
                proceeder.OnCharacterMoveContinued(_Args);
        }
        
        public void OnCharacterMoveFinished(CharacterMovingEventArgs _Args)
        {
            var proceeders =
                RazorMazeUtils.GetInterfaceOfProceeders<ICharacterMoveFinished>(GetProceeders());
            foreach (var proceeder in proceeders)
                proceeder.OnCharacterMoveFinished(_Args);
        }
        
        
        
        private List<object> GetProceeders()
        {
            var result = new List<object>
            {
                UI,                         
                Common,
                InputConfigurator,
                Character,
                Background,
                MazeRotation,
                PathItemsGroup,
                MovingItemsGroup,
                TrapsReactItemsGroup,
                TrapsIncItemsGroup,
                TurretsGroup,
                PortalsGroup,
                ShredingerBlocksGroup,
                SpringboardItemsGroup    
            }.Where(_Proceeder => _Proceeder != null)
                .ToList();
            return result;
        }
    }
}