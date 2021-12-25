using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views;
using Games.RazorMaze.Views.InputConfigurators;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using Zenject;

namespace Games.RazorMaze.Controllers
{
    public interface IGameController : IInit
    {
        IModelGame Model { get; }
        IViewGame View { get; }
    }
    
    public class GameController : MonoBehaviour, IGameController
    {
        #region factory
        
        public static IGameController CreateInstance()
        {
            var go = CommonUtils.FindOrCreateGameObject("Game Manager", out bool wasFound);
            var instance = wasFound ? go.GetComponent<GameController>() 
                : go.AddComponent<GameController>();
            return instance;
        }

        #endregion

        #region inject
        
        public  IModelGame         Model    { get; private set; }
        public  IViewGame          View     { get; private set; }
        private CommonGameSettings Settings { get; set; }

        [Inject]
        public void Inject(
            IModelGame _Model,
            IViewGame _View,
            CommonGameSettings _Settings)
        {
            Model = _Model;
            View = _View;
            Settings = _Settings;
        }

        #endregion
        
        #region api

        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;
        
        public void Init()
        {
            DefineSrDebuggerInitialization();
            
            bool modelInitialized = false;
            bool viewInitialized = false;
            Model.Initialize += () => modelInitialized = true;
            View.Initialize += () => viewInitialized = true;
            
            Model.Init();

            Model.PathItemsProceeder.AllPathsProceededEvent           += View.Character.OnAllPathProceed;
            Model.PathItemsProceeder.AllPathsProceededEvent           += View.LevelStageController.OnAllPathProceed;
            Model.PathItemsProceeder.PathProceedEvent                 += View.PathItemsGroup.OnPathProceed;
            Model.MazeRotation.RotationStarted                        += View.MazeRotation.OnRotationStarted;
            Model.MazeRotation.RotationFinished                       += View.MazeRotation.OnRotationFinished;
            
            Model.GravityItemsProceeder.MazeItemMoveStarted           += View.GravityItemsGroup.OnMazeItemMoveStarted;
            Model.GravityItemsProceeder.MazeItemMoveStarted           += View.UI.GameControls.OnMazeItemMoveStarted;
            Model.GravityItemsProceeder.MazeItemMoveContinued         += View.GravityItemsGroup.OnMazeItemMoveContinued;
            Model.GravityItemsProceeder.MazeItemMoveFinished          += View.GravityItemsGroup.OnMazeItemMoveFinished;
            Model.GravityItemsProceeder.MazeItemMoveFinished          += View.UI.GameControls.OnMazeItemMoveFinished;
            
            Model.TrapsMovingProceeder.MazeItemMoveStarted            += View.MovingItemsGroup.OnMazeItemMoveStarted;
            Model.TrapsMovingProceeder.MazeItemMoveStarted            += View.UI.GameControls.OnMazeItemMoveStarted;
            Model.TrapsMovingProceeder.MazeItemMoveContinued          += View.MovingItemsGroup.OnMazeItemMoveContinued;
            Model.TrapsMovingProceeder.MazeItemMoveFinished           += View.MovingItemsGroup.OnMazeItemMoveFinished;
            Model.TrapsMovingProceeder.MazeItemMoveFinished           += View.UI.GameControls.OnMazeItemMoveFinished;
            
            Model.TrapsReactProceeder.TrapReactStageChanged           += View.TrapsReactItemsGroup.OnMazeTrapReactStageChanged;
            Model.TrapsIncreasingProceeder.TrapIncreasingStageChanged += View.TrapsIncItemsGroup.OnMazeTrapIncreasingStageChanged;
            Model.TurretsProceeder.TurretShoot                        += View.TurretsGroup.OnTurretShoot;
            Model.PortalsProceeder.PortalEvent                        += View.PortalsGroup.OnPortalEvent;
            Model.ShredingerBlocksProceeder.ShredingerBlockEvent      += View.ShredingerBlocksGroup.OnShredingerBlockEvent;
            Model.SpringboardProceeder.SpringboardEvent               += View.SpringboardItemsGroup.OnSpringboardEvent;
            Model.Character.CharacterMoveStarted                      += View.OnCharacterMoveStarted;
            Model.Character.CharacterMoveContinued                    += View.OnCharacterMoveContinued;
            Model.Character.CharacterMoveFinished                     += View.OnCharacterMoveFinished;
            Model.LevelStaging.LevelStageChanged                      += View.OnLevelStageChanged;
            
            View.CommandsProceeder.Command                            += Model.InputScheduler.AddCommand;
            View.MazeRotation.RotationFinished                        += Model.MazeRotation.OnRotationFinished;
            
            View.Init();
            
            Coroutines.Run(Coroutines.WaitWhile(
                () => !modelInitialized || !viewInitialized,
                () =>
                {
                    Initialize?.Invoke();
                    Initialized = true;
                }));
        }

        #endregion

        #region nonpublic members

        private void DefineSrDebuggerInitialization()
        {
            if (Settings.SrDebuggerOn)
                OnSrDebugInitialized();
            else if (View.InputController.TouchProceeder is ViewInputTouchProceederWithSRDebugInit proceeder)
                proceeder.OnSrDebugInitialized = OnSrDebugInitialized;
        }

        private void OnSrDebugInitialized()
        {
            SROptions.Init(Model, View);
        }

        #endregion

        #region engine methods

        protected virtual void OnDestroy()
        {
            if (Model == null || View == null)
                return;
            Model.PathItemsProceeder.AllPathsProceededEvent           -= View.Character.OnAllPathProceed;
            Model.PathItemsProceeder.AllPathsProceededEvent           -= View.LevelStageController.OnAllPathProceed;
            Model.PathItemsProceeder.PathProceedEvent                 -= View.PathItemsGroup.OnPathProceed;
            Model.MazeRotation.RotationStarted                        -= View.MazeRotation.OnRotationStarted;
            Model.MazeRotation.RotationFinished                       += View.MazeRotation.OnRotationFinished;
            
            Model.GravityItemsProceeder.MazeItemMoveStarted           -= View.MovingItemsGroup.OnMazeItemMoveStarted;
            Model.GravityItemsProceeder.MazeItemMoveStarted           -= View.UI.GameControls.OnMazeItemMoveStarted;
            Model.GravityItemsProceeder.MazeItemMoveContinued         -= View.MovingItemsGroup.OnMazeItemMoveContinued;
            Model.GravityItemsProceeder.MazeItemMoveFinished          -= View.MovingItemsGroup.OnMazeItemMoveFinished;
            Model.GravityItemsProceeder.MazeItemMoveFinished          -= View.UI.GameControls.OnMazeItemMoveFinished;
            
            Model.TrapsMovingProceeder.MazeItemMoveStarted            -= View.MovingItemsGroup.OnMazeItemMoveStarted;
            Model.TrapsMovingProceeder.MazeItemMoveStarted            -= View.UI.GameControls.OnMazeItemMoveStarted;
            Model.TrapsMovingProceeder.MazeItemMoveContinued          -= View.MovingItemsGroup.OnMazeItemMoveContinued;
            Model.TrapsMovingProceeder.MazeItemMoveFinished           -= View.MovingItemsGroup.OnMazeItemMoveFinished;
            Model.TrapsMovingProceeder.MazeItemMoveFinished           -= View.UI.GameControls.OnMazeItemMoveFinished;
            
            Model.TrapsReactProceeder.TrapReactStageChanged           -= View.TrapsReactItemsGroup.OnMazeTrapReactStageChanged;
            Model.TrapsIncreasingProceeder.TrapIncreasingStageChanged -= View.TrapsIncItemsGroup.OnMazeTrapIncreasingStageChanged;
            Model.TurretsProceeder.TurretShoot                        -= View.TurretsGroup.OnTurretShoot;
            Model.PortalsProceeder.PortalEvent                        -= View.PortalsGroup.OnPortalEvent;
            Model.ShredingerBlocksProceeder.ShredingerBlockEvent      -= View.ShredingerBlocksGroup.OnShredingerBlockEvent;
            Model.SpringboardProceeder.SpringboardEvent               -= View.SpringboardItemsGroup.OnSpringboardEvent;
            Model.Character.CharacterMoveStarted                      -= View.OnCharacterMoveStarted;
            Model.Character.CharacterMoveContinued                    -= View.OnCharacterMoveContinued;
            Model.Character.CharacterMoveFinished                     -= View.OnCharacterMoveFinished;
            Model.LevelStaging.LevelStageChanged                      -= View.OnLevelStageChanged;
            
            View.CommandsProceeder.Command                            -= Model.InputScheduler.AddCommand;
            View.MazeRotation.RotationFinished                        -= Model.MazeRotation.OnRotationFinished;
        }

        #endregion
    }
}