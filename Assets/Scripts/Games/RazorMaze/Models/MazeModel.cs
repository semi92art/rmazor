using System.Linq;
using Entities;
using Exceptions;
using Utils;

namespace Games.RazorMaze.Models
{
    public class MazeModel : IMazeModel
    {
        #region nonpublic members
        
        private MazeInfo m_Info;
        
        #endregion
        
        #region api
        
        public ICharacterModel CharacterModel { get; }
        public event MazeInfoHandler MazeChanged;
        public event MazeOrientationHandler RotationStarted;
        public event FloatHandler Rotation;
        public event NoArgsHandler RotationFinished;
        public event ObstacleStartMoveHandler ObstacleMoveStarted;
        public event ObstacleMoveHandler ObstacleMove;
        public event ObstacleItemHandler ObstacleMoveFinished;

        
        public MazeInfo Info
        {
            get => m_Info;
            set => MazeChanged?.Invoke(m_Info = value, Orientation);
        }
        public MazeOrientation Orientation { get; private set; } = MazeOrientation.North;

        public MazeModel(ICharacterModel _CharacterModel)
        {
            CharacterModel = _CharacterModel;
        }
        
        public void Rotate(MazeRotateDirection _Direction)
        {
            int orient = (int) Orientation;
            switch (_Direction)
            {
                case MazeRotateDirection.Clockwise:
                    orient = MathUtils.ClampInverse(orient + 1, 0, 3); break;
                case MazeRotateDirection.CounterClockwise:
                    orient = MathUtils.ClampInverse(orient - 1, 0, 3); break;
            }
            Orientation = (MazeOrientation) orient;
            RotationStarted?.Invoke(_Direction, Orientation);
            Coroutines.Run(Coroutines.Lerp(
                0f, 
                1f, 
                0.2f, 
                _Val => Rotation?.Invoke(_Val),
                GameTimeProvider.Instance, 
                () =>
                {
                    RotationFinished?.Invoke();
                    MoveObstacles();
                }));
        }
        
        public void OnCharacterMoved()
        {
            MoveWallBlocksMoving();
        }
        
        #endregion
        
        #region nonpublic methods
        
        private void MoveObstacles()
        {
            MoveWallBlocksMoving();
            MoveTraps();
            MoveTrapsMoving();
        }

        private void MoveWallBlocksMoving()
        {
            var obstaclesMoving = m_Info.Obstacles
                .Where(_O => _O.Type == EObstacleType.ObstacleMoving);
            foreach (var obstacle in obstaclesMoving)
                MoveWallBlockMoving(obstacle);
        }

        private void MoveTraps()
        {
            var obstaclesTrap = m_Info.Obstacles
                .Where(_O => _O.Type == EObstacleType.Trap);
            foreach (var obstacle in obstaclesTrap)
                MoveTrap(obstacle);
        }

        private void MoveTrapsMoving()
        {
            var obstaclesTrapMoving = m_Info.Obstacles
                .Where(_O => _O.Type == EObstacleType.TrapMoving);
            foreach (var obstacle in obstaclesTrapMoving)
                MoveTrapMoving(obstacle);
        }

        private void MoveWallBlockMoving(Obstacle _Obstacle)
        {
            MoveObstacleCore(_Obstacle, true, true);
        }

        private void MoveTrap(Obstacle _Obstacle)
        {
            MoveObstacleCore(_Obstacle, false, true);
        }

        private void MoveTrapMoving(Obstacle _Obstacle)
        {
            MoveObstacleCore(_Obstacle, false, false);
        }

        private void MoveObstacleCore(Obstacle _Obstacle, bool _CheckCharacter, bool _CheckPath)
        {
            var dropVector = GetDropVector(Orientation);
            var pos = _Obstacle.Position;
            bool doMoveByPath = false;
            V2Int? altPos = null;
            while (IsValid(pos + dropVector, _Obstacle))
            {
                pos += dropVector;
                if (_CheckCharacter && CharacterModel.Position == pos)
                    altPos = pos - dropVector;
                if (!_CheckPath) 
                    continue;
                if (_Obstacle.Path.All(_Pos => pos != _Pos)) 
                    continue;
                doMoveByPath = true;
                break;
            }
            if (_CheckPath && !doMoveByPath)
                return;
            if (pos == _Obstacle.Position)
                return;
            V2Int from = _Obstacle.Position;
            V2Int to = altPos ?? pos;
            _Obstacle.Position = to;
            ObstacleMoveStarted?.Invoke(_Obstacle, from, to);
            Coroutines.Run(Coroutines.Lerp(
                0f, 
                1f, 
                0.1f, 
                _Val => ObstacleMove?.Invoke(_Obstacle, _Val),
                GameTimeProvider.Instance, 
                () => ObstacleMoveFinished?.Invoke(_Obstacle)));
        }

        private static V2Int GetDropVector(MazeOrientation _Orientation)
        {
            switch (_Orientation)
            {
                case MazeOrientation.North: return V2Int.down;
                case MazeOrientation.South: return V2Int.up;
                case MazeOrientation.East: return V2Int.right;
                case MazeOrientation.West: return V2Int.left;
                default: throw new SwitchCaseNotImplementedException(_Orientation);
            }
        }

        private bool IsValid(V2Int _Position, Obstacle _Obstacle)
        { 
            if (m_Info.Nodes.Any(_N => _N.Position == _Position))
                return true;
            return _Obstacle.Type == EObstacleType.Trap && _Obstacle.Path.Contains(_Position);
        }
        
        #endregion
    }
}