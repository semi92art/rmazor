using System.Collections.Generic;
using System.Linq;
using Entities;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views;
using Ticker;
using UnityEngine;
using Utils;
using Zenject;

namespace Games.RazorMaze.Controllers
{
    public interface IGameController : IInit, IPreInit, IPostInit
    {
        IModelGame Model { get; }
        IViewGame View { get; }
    }
    
    public class RazorMazeGameController : MonoBehaviour, IGameController
    {
        #region factory
        
        public static IGameController CreateInstance()
        {
            var go = CommonUtils.FindOrCreateGameObject("Game Manager", out bool _WasFound);
            var instance = _WasFound ? go.GetComponent<RazorMazeGameController>() 
                : go.AddComponent<RazorMazeGameController>();
            return instance;
        }

        #endregion

        #region inject
        
        public IModelGame Model { get; private set; }
        public IViewGame  View { get; private set; }
        public ILevelsLoader LevelsLoader { get; private set; }
        public IGameTicker GameTicker { get; private set; }
        public IUITicker UITicker { get; private set; }

        [Inject] public void Inject(
            IModelGame _Model,
            IViewGame _View, 
            ILevelsLoader _LevelsLoader,
            IGameTicker _GameTicker,
            IUITicker _UITicker) =>
            (Model, View, LevelsLoader, GameTicker, UITicker) =
            (_Model, _View, _LevelsLoader, _GameTicker, _UITicker);
        
        #endregion
        
        #region api

        public event NoArgsHandler PreInitialized;
        public event NoArgsHandler Initialized;
        public event NoArgsHandler PostInitialized;
        
        public void PreInit()
        {
            GameTicker.Reset();

            var rotation                                        = Model.MazeRotation;
            var pathItemsProceeder                              = Model.PathItemsProceeder;
            var trapsMovingProceeder                            = Model.TrapsMovingProceeder;
            var gravityItemsProceeder                           = Model.GravityItemsProceeder;
            var trapsReactProceeder                             = Model.TrapsReactProceeder;
            var trapsIncreasingProceeder                        = Model.TrapsIncreasingProceeder;
            var portalsProceeder                                = Model.PortalsProceeder;
            var turretsProceeder                                = Model.TurretsProceeder;
            var shredingerProceeder                             = Model.ShredingerBlocksProceeder;
            var springboardProceeder                            = Model.SpringboardProceeder;
            var character                                       = Model.Character;
            var levelStaging                                    = Model.LevelStaging;
            
            pathItemsProceeder.PathProceedEvent                 += DataOnPathProceedEvent;
            
            rotation.RotationStarted                            += OnMazeRotationStarted;
            rotation.Rotation                                   += OnMazeRotation;
            rotation.RotationFinished                           += OnMazeRotationFinished;
            
            trapsMovingProceeder.MazeItemMoveStarted            += OnMazeItemMoveStarted;
            trapsMovingProceeder.MazeItemMoveContinued          += OnMazeItemMoveContinued;
            trapsMovingProceeder.MazeItemMoveFinished           += OnMazeItemMoveFinished;

            gravityItemsProceeder.MazeItemMoveStarted           += OnMazeItemMoveStarted;
            gravityItemsProceeder.MazeItemMoveContinued         += OnMazeItemMoveContinued;
            gravityItemsProceeder.MazeItemMoveFinished          += OnMazeItemMoveFinished;
            
            trapsReactProceeder.TrapReactStageChanged           += OnMazeTrapReactStageChanged;
            trapsIncreasingProceeder.TrapIncreasingStageChanged += OnMazeTrapIncreasingStageChanged;
            turretsProceeder.TurretShoot                        += OnTurretShoot;
            portalsProceeder.PortalEvent                        += OnPortalEvent;
            shredingerProceeder.ShredingerBlockEvent            += OnShredingerBlockEvent;
            springboardProceeder.SpringboardEvent               += OnSpringboardEvent;

            character.AliveOrDeath                              += OnCharacterAliveOrDeath;
            character.CharacterMoveStarted                      += View.OnCharacterMoveStarted;
            character.CharacterMoveContinued                    += View.OnCharacterMoveContinued;
            character.CharacterMoveFinished                     += View.OnCharacterMoveFinished;
            character.PositionSet                               += OnCharacterPositionSet;
            
            levelStaging.LevelStageChanged                      += View.OnLevelStageChanged;
            
            View.InputConfigurator.Command                      += OnInputCommand;
            View.Common.GameLoopUpdate                          += OnGameLoopUpdate;
            
            var iPreInits = GetInterfaceOfProceeders<IPreInit>();
            int count = iPreInits.Count;
            bool[] preInited = new bool[count];
            for (int i = 0; i < count; i++)
            {
                var i1 = i;
                iPreInits[i].PreInitialized += () => preInited[i1] = true;
                iPreInits[i].PreInit();
            }
            Coroutines.Run(Coroutines.WaitWhile(
                () => preInited.Any(_PreInitialized => !_PreInitialized), 
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
            Model.Data.ProceedingControls = true;
            
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
                () => postInited.Any(_PostInitialized => !_PostInitialized), 
                () => PostInitialized?.Invoke()));
        }

        #endregion

        #region event methods
        
        private void OnGameLoopUpdate() => Model.Data.OnGameLoopUpdate();
        private void DataOnPathProceedEvent(V2Int _PathItem) => View.PathItemsGroup.OnPathProceed(_PathItem);
        private void OnInputCommand(int _Value)
        {
            Model.InputScheduler.AddCommand(_Value);
        }

        private void OnCharacterAliveOrDeath(bool _Alive) => View.Character.OnRevivalOrDeath(_Alive);
        
        private void OnCharacterPositionSet(V2Int _Value) => View.Character.OnPositionSet(_Value);

        private void OnMazeRotationStarted(MazeRotateDirection _Direction, MazeOrientation _Orientation) => View.Rotation.StartRotation(_Direction, _Orientation);
        private void OnMazeRotation(float _Progress) => View.Rotation.Rotate(_Progress);
        private void OnMazeRotationFinished(MazeRotateDirection _Direction, MazeOrientation _Orientation) => View.Rotation.FinishRotation();
        
        private void OnMazeItemMoveStarted(MazeItemMoveEventArgs _Args)
        {
            View.MovingItemsGroup.OnMazeItemMoveStarted(_Args);
            if (_Args.Info.Type == EMazeItemType.GravityBlock)
                View.InputConfigurator.Locked = true;
        }

        private void OnMazeItemMoveContinued(MazeItemMoveEventArgs _Args)
        {
            View.MovingItemsGroup.OnMazeItemMoveContinued(_Args);
        }

        private void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args)
        {
            View.MovingItemsGroup.OnMazeItemMoveFinished(_Args);
            if (_Args.Info.Type == EMazeItemType.GravityBlock)
                View.InputConfigurator.Locked = false;
        }

        private void OnMazeTrapReactStageChanged(MazeItemTrapReactEventArgs _Args) => View.TrapsReactItemsGroup.OnMazeTrapReactStageChanged(_Args);
        private void OnMazeTrapIncreasingStageChanged(MazeItemTrapIncreasingEventArgs _Args) => View.TrapsIncreasingItemsGroup.OnMazeTrapIncreasingStageChanged(_Args);
        private void OnTurretShoot(TurretShotEventArgs _Args) => View.TurretsGroup.OnTurretShoot(_Args);
        private void OnPortalEvent(PortalEventArgs _Args) => View.PortalsGroup.OnPortalEvent(_Args);
        private void OnShredingerBlockEvent(ShredingerBlockArgs _Args) => View.ShredingerBlocksGroup.OnShredingerBlockEvent(_Args);
        private void OnSpringboardEvent(SpringboardEventArgs _Args) => View.SpringboardItemsGroup.OnSpringboardEvent(_Args);
        
        #endregion
        
        #region engine methods

        protected virtual void OnDestroy()
        {
            var rotation                                        = Model.MazeRotation;
            var pathItemsProceeder                              = Model.PathItemsProceeder;
            var movingItemsProceeder                            = Model.TrapsMovingProceeder;
            var gravityItemsProceeder                           = Model.GravityItemsProceeder;
            var trapsReactProceeder                             = Model.TrapsReactProceeder;
            var trapsIncreasingProceeder                        = Model.TrapsIncreasingProceeder;
            var portalsProceeder                                = Model.PortalsProceeder;
            var turretsProceeder                                = Model.TurretsProceeder;
            var shredingerProceeder                             = Model.ShredingerBlocksProceeder;
            var springboardProceeder                            = Model.SpringboardProceeder;
            var character                                       = Model.Character;
            var levelStaging                                    = Model.LevelStaging;
            
            pathItemsProceeder.PathProceedEvent                 -= DataOnPathProceedEvent;
            
            rotation.RotationStarted                            -= OnMazeRotationStarted;
            rotation.Rotation                                   -= OnMazeRotation;
            rotation.RotationFinished                           -= OnMazeRotationFinished;
            
            movingItemsProceeder.MazeItemMoveStarted            -= OnMazeItemMoveStarted;
            movingItemsProceeder.MazeItemMoveContinued          -= OnMazeItemMoveContinued;
            movingItemsProceeder.MazeItemMoveFinished           -= OnMazeItemMoveFinished;

            gravityItemsProceeder.MazeItemMoveStarted           -= OnMazeItemMoveStarted;
            gravityItemsProceeder.MazeItemMoveContinued         -= OnMazeItemMoveContinued;
            gravityItemsProceeder.MazeItemMoveFinished          -= OnMazeItemMoveFinished;
            
            trapsReactProceeder.TrapReactStageChanged           -= OnMazeTrapReactStageChanged;
            trapsIncreasingProceeder.TrapIncreasingStageChanged -= OnMazeTrapIncreasingStageChanged;
            turretsProceeder.TurretShoot                        -= OnTurretShoot;
            portalsProceeder.PortalEvent                        -= OnPortalEvent;
            shredingerProceeder.ShredingerBlockEvent            -= OnShredingerBlockEvent;
            springboardProceeder.SpringboardEvent               -= OnSpringboardEvent;

            character.AliveOrDeath                              -= OnCharacterAliveOrDeath;
            character.CharacterMoveStarted                      -= View.OnCharacterMoveStarted;
            character.CharacterMoveContinued                    -= View.OnCharacterMoveContinued;
            character.CharacterMoveFinished                     -= View.OnCharacterMoveFinished;
            character.PositionSet                               -= OnCharacterPositionSet;
            
            levelStaging.LevelStageChanged                      -= View.OnLevelStageChanged;
            
            View.InputConfigurator.Command                      -= OnInputCommand;
            View.Common.GameLoopUpdate                          -= OnGameLoopUpdate;
        }
        
        private List<T> GetInterfaceOfProceeders<T>() where T : class
        {
            var result = new List<T>
            {
                Model as T,
                View as T
            }.Where(_Proceeder => _Proceeder != null).ToList();
            return result;
        }

        #endregion
    }
}