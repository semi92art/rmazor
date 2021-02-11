using System.Collections;
using System.Collections.Generic;
using Extensions;
using Games.Main_Mode.LevelStages;
using Games.Main_Mode.StageBlocks;
using Shapes;
using UnityEngine;

namespace Games.Main_Mode
{
    public class LevelStageCircle : LevelStageBase
    {
        protected override float Height => 10f;
        protected override float StagePosition => (Height + GapBetweenStages) * StageIndex;
        public override float Perimeter { get; }

        public static LevelStageCircle CreateRandom(Transform _Parent, int _StageIdx, int _BlocksCount)
        {
            var go = new GameObject();
            var stage = go.AddComponent<LevelStageCircle>();
            var blocks = stage.InitRandomBlocks(_BlocksCount);
            stage.Init(_StageIdx, blocks);
            
            go.transform.SetLocalPosXY(0, stage.StagePosition);
            go.SetParent(_Parent);
            return stage;
        }

        protected override IEnumerable<StageBlockBase> InitRandomBlocks(int _BlocksCount)
        {
            for (int i = 0; i < _BlocksCount; i++)
            {
                var go = new GameObject($"Block {i + 1}");
                go.SetParent(transform);
                var block = go.AddComponent<StageBlockCircle>();
                
                
                ConfigureBlock(block, i);
            }

            return null;
        }

        private void ConfigureBlock(StageBlockCircle _Block, int _BlockIdx)
        {
            
        }
    }
}