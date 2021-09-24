﻿using System.Collections.Generic;
using System.Linq;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Characters;
using Games.RazorMaze.Views.Common;
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
        public IViewUI UI { get; }
        public IInputConfigurator InputConfigurator { get; }
        public IViewCharacter Character { get; }
        public IViewMazeCommon Common { get; }
        public IViewMazeBackground Background { get; }
        public IViewMazeRotation Rotation { get; }
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
            IViewUI _UI,
            IInputConfigurator _InputConfigurator,
            IViewCharacter _Character,
            IViewMazeCommon _Common,
            IViewMazeBackground _Background,
            IViewMazeRotation _Rotation,
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
        
        public event NoArgsHandler Initialized;

        public void Init()
        {
            var proceeders = GetInterfaceOfProceeders<IInit>();
            int count = proceeders.Count;
            bool[] initialized = new bool[count];
            for (int i = 0; i < count; i++)
            {
                var i1 = i;
                proceeders[i].Initialized += () => initialized[i1] = true;
                proceeders[i].Init();
            }
            
            Coroutines.Run(Coroutines.WaitWhile(
                () => initialized.Any(_Initialized => !_Initialized), 
                () => Initialized?.Invoke()));
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