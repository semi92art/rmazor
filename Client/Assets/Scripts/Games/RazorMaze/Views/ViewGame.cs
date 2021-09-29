using System.Collections.Generic;
using System.Linq;
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
        public IContainersGetter ContainersGetter { get; }
        public IViewUI UI { get; }
        public IInputConfigurator InputConfigurator { get; }
        public IViewCharacter Character { get; }
        public IViewMazeCommon Common { get; }
        public IViewMazeBackground Background { get; }
        public IViewRotation Rotation { get; }
        public IViewMazePathItemsGroup PathItemsGroup { get; }
        public IViewMazeMovingItemsGroup MovingItemsGroup { get; }
        public IViewMazeTrapsReactItemsGroup TrapsReactItemsGroup { get; }
        public IViewMazeTrapsIncreasingItemsGroup TrapsIncreasingItemsGroup { get; }
        public IViewMazeTurretsGroup TurretsGroup { get; }
        public IViewMazePortalsGroup PortalsGroup { get; }
        public IViewMazeShredingerBlocksGroup ShredingerBlocksGroup { get; }
        public IViewMazeSpringboardItemsGroup SpringboardItemsGroup { get; }
        
        private IGameTicker GameTicker { get; }

        public ViewGame(
            IContainersGetter _ContainersGetter,
            IViewUI _UI,
            IInputConfigurator _InputConfigurator,
            IViewCharacter _Character,
            IViewMazeCommon _Common,
            IViewMazeBackground _Background,
            IViewRotation _Rotation,
            IViewMazePathItemsGroup _PathItemsGroup,
            IViewMazeMovingItemsGroup _MovingItemsGroup,
            IViewMazeTrapsReactItemsGroup _TrapsReactItemsGroup,
            IViewMazeTrapsIncreasingItemsGroup _TrapsIncreasingItemsGroup,
            IViewMazeTurretsGroup _TurretsGroup,
            IViewMazePortalsGroup _PortalsGroup,
            IViewMazeShredingerBlocksGroup _ShredingerBlocksGroup,
            IViewMazeSpringboardItemsGroup _SpringboardItemsGroup,
            IGameTicker _GameTicker)
        {
            ContainersGetter = _ContainersGetter;
            UI = _UI;
            InputConfigurator = _InputConfigurator;
            Character = _Character;
            Common = _Common;
            Background = _Background;
            Rotation = _Rotation;
            PathItemsGroup = _PathItemsGroup;
            MovingItemsGroup = _MovingItemsGroup;
            TrapsReactItemsGroup = _TrapsReactItemsGroup;
            TrapsIncreasingItemsGroup = _TrapsIncreasingItemsGroup;
            TurretsGroup = _TurretsGroup;
            PortalsGroup = _PortalsGroup;
            ShredingerBlocksGroup = _ShredingerBlocksGroup;
            SpringboardItemsGroup = _SpringboardItemsGroup;
            GameTicker = _GameTicker;
        }
        
        public event NoArgsHandler PreInitialized;
        public event NoArgsHandler Initialized;
        public event NoArgsHandler PostInitialized;
        
        public void PreInit()
        {
            var iBackColChangedProceeders = GetInterfaceOfProceeders<IOnBackgroundColorChanged>();
            foreach (var proceeder in iBackColChangedProceeders)
                Background.BackgroundColorChanged += proceeder.OnBackgroundColorChanged;
            
            var initProceeders = GetInterfaceOfProceeders<IPreInit>();
            int count = initProceeders.Count;
            bool[] preInited = new bool[count];
            for (int i = 0; i < count; i++)
            {
                var i1 = i;
                initProceeders[i].PreInitialized += () => preInited[i1] = true;
                initProceeders[i].PreInit();
            }
            Coroutines.Run(Coroutines.WaitWhile(
                () => preInited.Any(_PreInited => !_PreInited), 
                () => PreInitialized?.Invoke()));
        }

        public void Init()
        {
            var initProceeders = GetInterfaceOfProceeders<IInit>();
            int count = initProceeders.Count;
            bool[] initialized = new bool[count];
            for (int i = 0; i < count; i++)
            {
                var i1 = i;
                initProceeders[i].Initialized += () => initialized[i1] = true;
                initProceeders[i].Init();
            }
            
            Coroutines.Run(Coroutines.WaitWhile(
                () => initialized.Any(_Initialized => !_Initialized), 
                () => Initialized?.Invoke()));
        }
        
        public void PostInit()
        {
            var initProceeders = GetInterfaceOfProceeders<IPostInit>();
            int count = initProceeders.Count;
            bool[] postInited = new bool[count];
            for (int i = 0; i < count; i++)
            {
                var i1 = i;
                initProceeders[i].PostInitialized += () => postInited[i1] = true;
                initProceeders[i].PostInit();
            }
            Coroutines.Run(Coroutines.WaitWhile(
                () => postInited.Any(_PostInited => !_PostInited), 
                () => PostInitialized?.Invoke()));
        }

        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<IOnLevelStageChanged>();
            foreach (var proceeder in proceeders)
                proceeder.OnLevelStageChanged(_Args);
            GameTicker.Pause = _Args.Stage == ELevelStage.Paused;
        }
        
        public void OnCharacterMoveStarted(CharacterMovingEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveStarted>();
            foreach (var proceeder in proceeders)
                proceeder.OnCharacterMoveStarted(_Args);
        }

        public void OnCharacterMoveContinued(CharacterMovingEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveContinued>();
            foreach (var proceeder in proceeders)
                proceeder.OnCharacterMoveContinued(_Args);
        }
        
        public void OnCharacterMoveFinished(CharacterMovingEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveFinished>();
            foreach (var proceeder in proceeders)
                proceeder.OnCharacterMoveFinished(_Args);
        }
        
        private List<T> GetInterfaceOfProceeders<T>() where T : class
        {
            var result = new List<T>
            {
                UI                                 as T,
                Common                             as T,
                InputConfigurator                  as T,
                Character                          as T,
                Background                         as T,
                Rotation                           as T,
                PathItemsGroup                     as T,
                MovingItemsGroup                   as T,
                TrapsReactItemsGroup               as T,
                TrapsIncreasingItemsGroup          as T,
                TurretsGroup                       as T,
                PortalsGroup                       as T,
                ShredingerBlocksGroup              as T,
                SpringboardItemsGroup              as T
            }.Where(_Proceeder => _Proceeder != null).ToList();
            return result;
        }
    }
}