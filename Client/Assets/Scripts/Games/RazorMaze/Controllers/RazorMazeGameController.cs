using Games.RazorMaze.Models;
using Games.RazorMaze.Views;
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

        [Inject]
        public void Inject(
            IModelGame _Model,
            IViewGame _View)
        {
            Model = _Model;
            View = _View;
        }

        #endregion
        
        #region api

        public event UnityAction Initialized;
        
        public void Init()
        {
            bool modelInitialized = false;
            bool viewInitialized = false;
            Model.Initialized += () => modelInitialized = true;
            View.Initialized += () => viewInitialized = true;
            
            Model.Init();

            Model.PathItemsProceeder.PathProceedEvent                 += View.PathItemsGroup.OnPathProceed;
            Model.MazeRotation.RotationStarted                        += View.MazeRotation.StartRotation;
            Model.GravityItemsProceeder.MazeItemMoveStarted           += View.MovingItemsGroup.OnMazeItemMoveStarted;
            Model.GravityItemsProceeder.MazeItemMoveContinued         += View.MovingItemsGroup.OnMazeItemMoveContinued;
            Model.GravityItemsProceeder.MazeItemMoveFinished          += View.MovingItemsGroup.OnMazeItemMoveFinished;
            Model.TrapsMovingProceeder.MazeItemMoveStarted            += View.MovingItemsGroup.OnMazeItemMoveStarted;
            Model.TrapsMovingProceeder.MazeItemMoveContinued          += View.MovingItemsGroup.OnMazeItemMoveContinued;
            Model.TrapsMovingProceeder.MazeItemMoveFinished           += View.MovingItemsGroup.OnMazeItemMoveFinished;
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
            
            View.InputConfigurator.Command                            += Model.InputScheduler.AddCommand;
            View.MazeRotation.RotationFinished                        += Model.MazeRotation.OnRotationFinished;
            
            View.Init();
            
            Coroutines.Run(Coroutines.WaitWhile(
                () => !modelInitialized || !viewInitialized,
                () => Initialized?.Invoke()));
        }
        
        #endregion

        #region engine methods

        protected virtual void OnDestroy()
        {
            Model.PathItemsProceeder.PathProceedEvent                 -= View.PathItemsGroup.OnPathProceed;
            Model.MazeRotation.RotationStarted                        -= View.MazeRotation.StartRotation;
            Model.GravityItemsProceeder.MazeItemMoveStarted           -= View.MovingItemsGroup.OnMazeItemMoveStarted;
            Model.GravityItemsProceeder.MazeItemMoveContinued         -= View.MovingItemsGroup.OnMazeItemMoveContinued;
            Model.GravityItemsProceeder.MazeItemMoveFinished          -= View.MovingItemsGroup.OnMazeItemMoveFinished;
            Model.TrapsMovingProceeder.MazeItemMoveStarted            -= View.MovingItemsGroup.OnMazeItemMoveStarted;
            Model.TrapsMovingProceeder.MazeItemMoveContinued          -= View.MovingItemsGroup.OnMazeItemMoveContinued;
            Model.TrapsMovingProceeder.MazeItemMoveFinished           -= View.MovingItemsGroup.OnMazeItemMoveFinished;
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
            
            View.InputConfigurator.Command                            -= Model.InputScheduler.AddCommand;
            View.MazeRotation.RotationFinished                        -= Model.MazeRotation.OnRotationFinished;
        }

        #endregion
    }
}