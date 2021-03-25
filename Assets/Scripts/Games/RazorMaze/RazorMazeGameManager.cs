using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views;
using UnityEngine;
using Utils;
using Zenject;

namespace Games.RazorMaze
{
    public interface IGameManager
    {
        void Init();
    }
    
    public class RazorMazeGameManager : MonoBehaviour, ISingleton, IGameManager
    {
        #region singleton

        private static RazorMazeGameManager _instance;

        public static RazorMazeGameManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                var go = CommonUtils.FindOrCreateGameObject("Game Manager", out bool _WasFound);
                _instance = _WasFound ? go.GetComponent<RazorMazeGameManager>() : go.AddComponent<RazorMazeGameManager>();
                return _instance;
            }
        }

        #endregion
        
        #region nonpublic members
        
        private IGameModel           GameModel { get; set; }
        private IMazeView            MazeView { get; set; }
        private ICharacterView       CharacterView { get; set; }
        private IGameUiView          GameUiView { get; set; }
        private IInputConfigurator   InputConfigurator { get; set; }

        #endregion
    
        #region api

        [Inject]
        public void Construct(
            IGameModel _GameModel,
            IMazeView _MazeView,
            ICharacterView _CharacterView,
            IInputConfigurator _InputConfigurator,
            IGameUiView _GameUiView)
        {
            GameModel = _GameModel;
            MazeView = _MazeView;
            CharacterView = _CharacterView;
            GameUiView = _GameUiView;
            InputConfigurator = _InputConfigurator;
        }
        
        public void Init()
        {
            GameTimeProvider.Instance.Reset();

            var maze                              = GameModel.Maze;
            var mazeTransformer                   = GameModel.MazeTransformer;
            var character                         = GameModel.Character;
            var scoring                           = GameModel.Scoring;
            var levelStaging                      = GameModel.LevelStaging;
            
            maze.RotationStarted                  += MazeOnRotationStarted;
            maze.Rotation                         += MazeOnRotation;
            maze.RotationFinished                 += MazeRotationFinished;
            mazeTransformer.MazeItemMoveStarted   += MazeOnMazeItemMoveStarted;
            mazeTransformer.MazeItemMoveContinued += MazeOnMazeItemMoveContinued;
            mazeTransformer.MazeItemMoveFinished  += MazeOnMazeItemMoveFinished;

            character.HealthChanged               += CharacterOnHealthChanged;
            character.Death                       += CharacterOnDeath;
            character.MoveStarted                 += CharacterStartMove;
            character.MoveContinued               += CharacterOnMoving;
            character.MoveFinished  += CharacterOnMoveFinished;
            
            scoring.ScoreChanged                  += OnScoreChanged;
            scoring.NecessaryScoreReached         += OnNecessaryScoreReached;

            levelStaging.LevelBeforeStarted       += OnBeforeLevelStarted;
            levelStaging.LevelStarted             += OnLevelStarted;
            levelStaging.LevelFinished            += OnLevelFinished;
            
            InputConfigurator.Command             += InputConfiguratorOnCommand;
            
            MazeView.Init();
            CharacterView.Init();
            character.Init();
            InputConfigurator.ConfigureInput();
            
            //GameModel.LevelStaging.BeforeStartLevel();
        }

        private void CharacterOnMoveFinished(CharacterMovingEventArgs _Args)
        {
            CharacterView.OnMoving(_Args);
        }

        private void InputConfiguratorOnCommand(int _Value) => GameModel.InputScheduler.AddCommand((EInputCommand)_Value);
        private void CharacterOnHealthChanged(HealthPointsEventArgs _Args) => CharacterView.OnHealthChanged(_Args);
        private void CharacterOnDeath() => CharacterView.OnDeath();
        private void CharacterStartMove(CharacterMovingEventArgs _Args) => CharacterView.OnStartChangePosition(_Args);
        private void CharacterOnMoving(CharacterMovingEventArgs _Args) => CharacterView.OnMoving(_Args);
        private void MazeOnRotationStarted(MazeRotateDirection _Direction, MazeOrientation _Orientation) => MazeView.StartRotation(_Direction, _Orientation);
        private void MazeOnRotation(float _Progress) => MazeView.Rotate(_Progress);
        private void MazeRotationFinished() => MazeView.FinishRotation();
        private void MazeOnMazeItemMoveStarted(MazeItemMoveEventArgs _Args) => MazeView.OnMazeItemMoveStarted(_Args);
        private void MazeOnMazeItemMoveContinued(MazeItemMoveEventArgs _Args) => MazeView.OnMazeItemMoveContinued(_Args);
        private void MazeOnMazeItemMoveFinished(MazeItemMoveEventArgs _Args) => MazeView.OnMazeItemMoveFinished(_Args);
        public void SetLevel(int _Level) => GameModel.LevelStaging.Level = _Level;
        public void SetMazeInfo(MazeInfo _Info) => GameModel.Maze.Info = _Info;

        #endregion
    
        #region protected methods
        
        protected virtual void OnBeforeLevelStarted(LevelStateChangedArgs _Args)
        {
            GameModel.Scoring.Score = 0;
            GameUiView?.OnBeforeLevelStarted(
                _Args,
                () => GameModel.LevelStaging.StartLevel());
        }

        protected virtual void OnLevelStarted(LevelStateChangedArgs _Args) => GameUiView?.OnLevelStarted(_Args);

        protected virtual void OnLevelFinished(LevelStateChangedArgs _Args)
        {
            GameUiView?.OnLevelFinished(_Args, () =>
                {
                    GameModel.LevelStaging.Level++;
                    GameModel.LevelStaging.BeforeStartLevel();
                });
        }

        private void OnScoreChanged(int _Score) => throw new System.NotImplementedException();
        protected virtual void OnNecessaryScoreReached() => GameModel.LevelStaging.FinishLevel();

        #endregion
    
        #region engine methods

        protected virtual void OnDestroy()
        {
            var maze                          = GameModel.Maze;
            var character                     = GameModel.Character;
            var levelStaging                  = GameModel.LevelStaging;
            var scoring                       = GameModel.Scoring;
            
            maze.RotationStarted              -= MazeOnRotationStarted;
            maze.Rotation                     -= MazeOnRotation;
            maze.RotationFinished             -= MazeRotationFinished;
            
            character.HealthChanged           -= CharacterOnHealthChanged;
            character.Death                   -= CharacterOnDeath;
            character.MoveStarted               -= CharacterStartMove;
            character.MoveContinued                  -= CharacterOnMoving;
            
            scoring.ScoreChanged              -= OnScoreChanged;
            scoring.NecessaryScoreReached     -= OnNecessaryScoreReached;
            
            levelStaging.LevelBeforeStarted   -= OnBeforeLevelStarted;
            levelStaging.LevelStarted         -= OnLevelStarted;
            levelStaging.LevelFinished        -= OnLevelFinished;
        }

        #endregion
    }
}