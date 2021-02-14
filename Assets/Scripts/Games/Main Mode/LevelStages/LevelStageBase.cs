using System.Collections.Generic;
using System.Linq;
using Games.Main_Mode.StageBlocks;
using UnityEngine;

namespace Games.Main_Mode.LevelStages
{
    public enum LevelStageType
    {
        Circle,
        Square
    }

    public abstract class LevelStageBase : MonoBehaviour, IUpdateState
    {
        public abstract float Height { get; }

        protected int StageIndex { get; private set; }
        protected List<StageBlockBase> Blocks { get; private set; }
        

        protected void Init(int _StageIdx, 
            IEnumerable<StageBlockProps> _BlocksProps)
        {
            StageIndex = _StageIdx;
            var blocks = InitBlocks(_BlocksProps);
            InitCenterBlock();
            Blocks = blocks.ToList();
        }

        protected abstract IEnumerable<StageBlockBase> InitBlocks(IEnumerable<StageBlockProps> _BlocksProps);

        protected abstract void InitCenterBlock();
        public void UpdateState(float _DeltaTime)
        {
            foreach (var block in Blocks)
            {
                block.UpdateState(_DeltaTime);
            }
        }
    }
}