using System.Collections.Generic;
using Exceptions;
using Games.Main_Mode.BlockObstacles;
using UnityEngine;

namespace Games.Main_Mode.StageBlocks
{
    public interface IUpdateState
    {
        void UpdateState(float _DeltaTime);
    }

    public abstract class StageBlockBase : MonoBehaviour, IUpdateState
    {
        protected int BlockIndex { get; private set; }
        protected readonly List<BlockObstacleBase> Obstacles = new List<BlockObstacleBase>();

        protected virtual void Init(StageBlockProps _Props)
        {
            SetZone(_Props.BlockIndex, _Props.ZoneColor, _Props.BorderColor);
            BlockIndex = _Props.BlockIndex;

            foreach (var obstacleProps in _Props.ObstaclesProps)
            {
                obstacleProps.GetPosition = GetObstaclePosition;
                obstacleProps.GetAngle = GetObstacleAngle;
                BlockObstacleBase obstacle;
                switch (obstacleProps.Type)
                {
                    case BlockObstacleType.Simple:
                        obstacle = BlockObstacleSimple.Create(transform, obstacleProps);
                        break;
                    case BlockObstacleType.Static:
                        obstacle = BlockObstacleStatic.Create(transform, obstacleProps);
                        break;
                    default: throw new SwitchCaseNotImplementedException(obstacleProps.Type);
                }
                Obstacles.Add(obstacle);
            }
        }

        public virtual void UpdateState(float _DeltaTime)
        {
            foreach (var obstacle in Obstacles)
            {
                obstacle.UpdateState(_DeltaTime);
            }
        }

        protected abstract float GetObstacleAngle(float _RelativeRawPosition);
        protected abstract Vector2 GetObstaclePosition(float _RelativeRawPosition);
        protected abstract void SetZone(int _BlockIndex, Color _ZoneColor, Color _BorderColor);
    }

    public class StageBlockProps
    {
        public Color ZoneColor { get; set; }
        public Color BorderColor { get; set; }
        public int BlockIndex { get; set; }
        public IEnumerable<BlockObstacleProps> ObstaclesProps { get; set; }
    }
}