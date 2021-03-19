using System.Linq;
using Entities;
using Exceptions;
using Utils;

namespace Games.RazorMaze.Models
{
    public class MazeModel : IMazeModel
    {
        public event MazeInfoHandler MazeChanged;
        public event MazeOrientationHandler RotationStarted;
        public event FloatHandler Rotation;
        public event NoArgsHandler RotationFinished;
        public event ObstacleStartMoveHandler ObstacleMoveStarted;
        public event ObstacleMoveHandler ObstacleMove;
        public event ObstacleItemHandler ObstacleMoveFinished;

        private MazeInfo m_Info;

        public MazeInfo Info
        {
            get => m_Info;
            set
            {
                m_Info = value;
                MazeChanged?.Invoke(m_Info, Orientation);
            }
        }
        public MazeOrientation Orientation { get; private set; } = MazeOrientation.North;

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

        private void MoveObstacles()
        {
            var obstaclesMoving = m_Info.Obstacles
                .Where(_O => _O.Type == EObstacleType.ObstacleMoving);
            foreach (var obstacle in obstaclesMoving)
                MoveWallBlockMoving(obstacle);

            var obstaclesTrap = m_Info.Obstacles
                .Where(_O => _O.Type == EObstacleType.Trap);
            foreach (var obstacle in obstaclesTrap)
                MoveTrap(obstacle);

            var obstaclesTrapMoving = m_Info.Obstacles
                .Where(_O => _O.Type == EObstacleType.TrapMoving);
            foreach (var obstacle in obstaclesTrapMoving)
                MoveTrapMoving(obstacle);
        }

        private void MoveWallBlockMoving(Obstacle _Obstacle)
        {
            var dropVector = GetDropVector(Orientation);
            var pos = _Obstacle.Position;
            bool doMove = false;
            while (IsOnNode(pos + dropVector))
            {
                pos += dropVector;
                if (_Obstacle.Path.All(_Pos => pos != _Pos)) 
                    continue;
                doMove = true;
                break;
            }
            if (!doMove)
                return;
            V2Int from = _Obstacle.Position;
            _Obstacle.Position = pos;
            ObstacleMoveStarted?.Invoke(_Obstacle, from, pos);
            Coroutines.Run(Coroutines.Lerp(
                0f, 
                1f, 
                0.1f, 
                _Val => ObstacleMove?.Invoke(_Obstacle, _Val),
                GameTimeProvider.Instance, 
                () => ObstacleMoveFinished?.Invoke(_Obstacle)));
        }

        private void MoveTrap(Obstacle _Obstacle)
        {
            var dropVector = GetDropVector(Orientation);
            
        }

        private void MoveTrapMoving(Obstacle _Obstacle)
        {
            var dropVector = GetDropVector(Orientation);
            
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

        private bool IsOnNode(V2Int _Position)
        {
            return m_Info.Nodes.Any(_N => _N.Position == _Position);
        }
    }
}