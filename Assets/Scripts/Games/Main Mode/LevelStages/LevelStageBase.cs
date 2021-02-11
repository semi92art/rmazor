using System.Collections.Generic;
using Games.Main_Mode.BlockObstacles;
using Games.Main_Mode.StageBlocks;
using UnityEngine;

namespace Games.Main_Mode.LevelStages
{
    public enum LevelStageType
    {
        Circle,
        Square
    }

    public abstract class LevelStageBase : MonoBehaviour
    {
        protected const float GapBetweenStages = 5f;
        protected abstract float Height { get; }
        protected abstract float StagePosition { get; }
        public abstract float Perimeter { get; }
        
        protected int StageIndex { get; private set; }
        protected IEnumerable<StageBlockBase> Blocks { get; private set; }
        

        protected virtual void Init(int _StageIdx, IEnumerable<StageBlockBase> _Blocks)
        {
            StageIndex = _StageIdx;
            Blocks = _Blocks;
        }

        protected abstract IEnumerable<StageBlockBase> InitRandomBlocks(int _BlocksCount);
    }
}