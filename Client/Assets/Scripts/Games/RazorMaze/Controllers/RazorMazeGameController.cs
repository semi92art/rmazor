using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views;
using UnityEngine;
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
        private ICoordinateConverter CoordinateConverter { get; set; }

        [Inject] public void Inject(
            IModelGame _Model,
            IViewGame _View,
            ICoordinateConverter _CoordinateConverter) =>
            (Model, View, CoordinateConverter) =
            (_Model, _View, _CoordinateConverter);
        
        #endregion
        
        #region api

        public event NoArgsHandler Initialized;
        
        public void Init()
        {
            Model.Init();
            View.Init();
            
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
            
            pathItemsProceeder.PathProceedEvent                 += View.PathItemsGroup.OnPathProceed;
            
            rotation.RotationStarted                            += View.MazeRotation.StartRotation;
            View.MazeRotation.RotationFinished                  += Model.MazeRotation.OnRotationFinished;
            
            trapsMovingProceeder.MazeItemMoveStarted            += OnMazeItemMoveStarted;
            trapsMovingProceeder.MazeItemMoveContinued          += View.MovingItemsGroup.OnMazeItemMoveContinued;
            trapsMovingProceeder.MazeItemMoveFinished           += OnMazeItemMoveFinished;

            gravityItemsProceeder.MazeItemMoveStarted           += OnMazeItemMoveStarted;
            gravityItemsProceeder.MazeItemMoveContinued         += View.MovingItemsGroup.OnMazeItemMoveContinued;
            gravityItemsProceeder.MazeItemMoveFinished          += OnMazeItemMoveFinished;
            
            trapsReactProceeder.TrapReactStageChanged           += View.TrapsReactItemsGroup.OnMazeTrapReactStageChanged;
            trapsIncreasingProceeder.TrapIncreasingStageChanged += View.TrapsIncItemsGroup.OnMazeTrapIncreasingStageChanged;
            turretsProceeder.TurretShoot                        += View.TurretsGroup.OnTurretShoot;
            portalsProceeder.PortalEvent                        += View.PortalsGroup.OnPortalEvent;
            shredingerProceeder.ShredingerBlockEvent            += View.ShredingerBlocksGroup.OnShredingerBlockEvent;
            springboardProceeder.SpringboardEvent               += View.SpringboardItemsGroup.OnSpringboardEvent;

            character.CharacterMoveStarted                      += View.OnCharacterMoveStarted;
            character.CharacterMoveContinued                    += View.OnCharacterMoveContinued;
            character.CharacterMoveFinished                     += View.OnCharacterMoveFinished;
            
            levelStaging.LevelStageChanged                      += View.OnLevelStageChanged;
            View.InputConfigurator.Command                      += Model.InputScheduler.AddCommand;

            Model.Data.ProceedingControls = true;
            Initialized?.Invoke();
        }
        
        #endregion

        #region event methods
        
        private void OnMazeItemMoveStarted(MazeItemMoveEventArgs _Args)
        {
            View.MovingItemsGroup.OnMazeItemMoveStarted(_Args);
            if (_Args.Info.Type == EMazeItemType.GravityBlock)
                View.InputConfigurator.Locked = true;
        }

        private void OnMazeItemMoveFinished(MazeItemMoveEventArgs _Args)
        {
            View.MovingItemsGroup.OnMazeItemMoveFinished(_Args);
            if (_Args.Info.Type == EMazeItemType.GravityBlock)
                View.InputConfigurator.Locked = false;
        }

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
            
            pathItemsProceeder.PathProceedEvent                 -= View.PathItemsGroup.OnPathProceed;
            
            rotation.RotationStarted                            -= View.MazeRotation.StartRotation;
            
            movingItemsProceeder.MazeItemMoveStarted            -= OnMazeItemMoveStarted;
            movingItemsProceeder.MazeItemMoveContinued          -= View.MovingItemsGroup.OnMazeItemMoveContinued;
            movingItemsProceeder.MazeItemMoveFinished           -= OnMazeItemMoveFinished;

            gravityItemsProceeder.MazeItemMoveStarted           -= OnMazeItemMoveStarted;
            gravityItemsProceeder.MazeItemMoveContinued         -= View.MovingItemsGroup.OnMazeItemMoveContinued;
            gravityItemsProceeder.MazeItemMoveFinished          -= OnMazeItemMoveFinished;
            
            trapsReactProceeder.TrapReactStageChanged           -= View.TrapsReactItemsGroup.OnMazeTrapReactStageChanged;
            trapsIncreasingProceeder.TrapIncreasingStageChanged -= View.TrapsIncItemsGroup.OnMazeTrapIncreasingStageChanged;
            turretsProceeder.TurretShoot                        -= View.TurretsGroup.OnTurretShoot;
            portalsProceeder.PortalEvent                        -= View.PortalsGroup.OnPortalEvent;
            shredingerProceeder.ShredingerBlockEvent            -= View.ShredingerBlocksGroup.OnShredingerBlockEvent;
            springboardProceeder.SpringboardEvent               -= View.SpringboardItemsGroup.OnSpringboardEvent;

            character.CharacterMoveStarted                      -= View.OnCharacterMoveStarted;
            character.CharacterMoveContinued                    -= View.OnCharacterMoveContinued;
            character.CharacterMoveFinished                     -= View.OnCharacterMoveFinished;
            
            levelStaging.LevelStageChanged                      -= View.OnLevelStageChanged;
            View.InputConfigurator.Command                      -= Model.InputScheduler.AddCommand;
        }

        #endregion
    }
}