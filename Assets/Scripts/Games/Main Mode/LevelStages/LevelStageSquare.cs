using System.Collections.Generic;
using Extensions;
using Games.Main_Mode.LevelStages;
using Games.Main_Mode.StageBlocks;
using UnityEngine;

namespace Games.Main_Mode
{
    public class LevelStageSquare : LevelStageBase
    {
        protected override float Height { get; }
        protected override float StagePosition { get; }
        public override float Perimeter { get; }
        
        public static LevelStageSquare Create(Transform _Parent, int _StageIdx, int _BlocksCount)
        {
            var go = new GameObject();
            var stage = go.AddComponent<LevelStageSquare>();
            //stage.Init(_StageIdx, _BlocksCount);
            go.SetParent(_Parent);
            return stage;
        }

        
        protected override IEnumerable<StageBlockBase> InitRandomBlocks(int _BlocksCount)
        {
            throw new System.NotImplementedException();
        }
    }
}