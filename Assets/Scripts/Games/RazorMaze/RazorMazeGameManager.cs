using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views;
using UnityEngine;
using UnityEngine.EventSystems;
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
        
        private IGameModel         GameModel { get; set; }
        private IMazeView          MazeView { get; set; }
        private ICharacterView     CharacterView { get; set; }
        private IGameUiView        GameUiView { get; set; }
        private IInputConfigurator InputConfigurator { get; set; }
        
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

            var maze                          = GameModel.Maze;
            var character                     = GameModel.Character;
            var scoring                       = GameModel.Scoring;
            var levelStaging                  = GameModel.LevelStaging;
            
            maze.OnRotationStarted            += MazeOnRotationStarted;
            maze.OnRotation                   += MazeOnRotation;
            maze.OnRotationFinished           += MazeOnRotationFinished;

            character.OnHealthChanged         += CharacterOnHealthChanged;
            character.OnDeath                 += CharacterOnDeath;
            character.OnStartChangePosition   += CharacterOnStartChangePosition;
            character.OnMoving                += CharacterOnMoving;
            
            scoring.OnScoreChanged            += OnScoreChanged;
            scoring.OnNecessaryScoreReached   += OnNecessaryScoreReached;

            levelStaging.OnLevelBeforeStarted += OnBeforeLevelStarted;
            levelStaging.OnLevelStarted       += OnLevelStarted;
            levelStaging.OnLevelFinished      += OnLevelFinished;
            
            InputConfigurator.OnCommand       += InputConfiguratorOnCommand;
            
            MazeView.Init(GameModel.Maze);
            CharacterView.Init(GameModel.Maze.Info);
            character.Init(new HealthPointsEventArgs(1));
            InputConfigurator.ConfigureInput();
            
            //GameModel.LevelStaging.BeforeStartLevel();
        }

        private void InputConfiguratorOnCommand(int _Value)
        {
            var character = GameModel.Character;
            var maze = GameModel.Maze;
            switch (_Value)
            {
                case (int)EInputCommand.MoveLeft:               character.Move(MoveDirection.Left);                break;
                case (int)EInputCommand.MoveRight:              character.Move(MoveDirection.Right);               break;
                case (int)EInputCommand.MoveUp:                 character.Move(MoveDirection.Up);                  break;
                case (int)EInputCommand.MoveDown:               character.Move(MoveDirection.Down);                break;
                case (int)EInputCommand.RotateClockwise:        maze.Rotate(MazeRotateDirection.Clockwise);        break;
                case (int)EInputCommand.RotateCounterClockwise: maze.Rotate(MazeRotateDirection.CounterClockwise); break;
            }
        }

        private void CharacterOnHealthChanged(HealthPointsEventArgs _Args)
        {
            CharacterView.OnHealthChanged(_Args);
        }
        
        private void CharacterOnDeath()
        {
            CharacterView.OnDeath();
        }

        private void CharacterOnStartChangePosition(V2Int _PrevPos, V2Int _NextPos)
        {
            CharacterView.OnStartChangePosition(_PrevPos, _NextPos);
        }

        private void CharacterOnMoving(float _Progress)
        {
            CharacterView.OnMoving(_Progress);
        }

        private void MazeOnRotationStarted(MazeRotateDirection _Direction, MazeOrientation _Orientation)
        {
            MazeView.FinishRotation(_Direction, _Orientation);
        }

        private void MazeOnRotation(float _Progress)
        {
            MazeView.Rotate(_Progress);
        }
        
        private void MazeOnRotationFinished(MazeRotateDirection _Direction, MazeOrientation _Orientation)
        {
            MazeView.StartRotation(_Direction, _Orientation);
        }

        public void SetLevel(int _Level)
        {
            GameModel.LevelStaging.Level = _Level;
        }

        public void SetMazeInfo(MazeInfo _Info)
        {
            GameModel.Maze.Info = _Info;
        }
        
        #endregion
    
        #region protected methods
        
        protected virtual void OnBeforeLevelStarted(LevelStateChangedArgs _Args)
        {
            GameModel.Scoring.Score = 0;
            GameUiView?.OnBeforeLevelStarted(
                _Args,
                () => GameModel.LevelStaging.StartLevel());
        }

        protected virtual void OnLevelStarted(LevelStateChangedArgs _Args)
        {
            GameUiView?.OnLevelStarted(_Args);
        }

        protected virtual void OnLevelFinished(LevelStateChangedArgs _Args)
        {
            GameUiView?.OnLevelFinished(_Args, () =>
                {
                    GameModel.LevelStaging.Level++;
                    GameModel.LevelStaging.BeforeStartLevel();
                });
        }

        private void OnScoreChanged(int _Score)
        {
            throw new System.NotImplementedException();
        }
        
        protected virtual void OnNecessaryScoreReached()
        {
            GameModel.LevelStaging.FinishLevel();
        }

        #endregion
    
        #region engine methods

        protected virtual void OnDestroy()
        {
            var maze                          = GameModel.Maze;
            var character                     = GameModel.Character;
            var levelStaging                  = GameModel.LevelStaging;
            var scoring                       = GameModel.Scoring;
            
            maze.OnRotationStarted            -= MazeOnRotationStarted;
            maze.OnRotation                   -= MazeOnRotation;
            maze.OnRotationFinished           -= MazeOnRotationFinished;
            
            character.OnHealthChanged         -= CharacterOnHealthChanged;
            character.OnDeath                 -= CharacterOnDeath;
            character.OnStartChangePosition   -= CharacterOnStartChangePosition;
            character.OnMoving                -= CharacterOnMoving;
            
            scoring.OnScoreChanged            -= OnScoreChanged;
            scoring.OnNecessaryScoreReached   -= OnNecessaryScoreReached;
            
            levelStaging.OnLevelBeforeStarted -= OnBeforeLevelStarted;
            levelStaging.OnLevelStarted       -= OnLevelStarted;
            levelStaging.OnLevelFinished      -= OnLevelFinished;
        }

        #endregion
    }
}