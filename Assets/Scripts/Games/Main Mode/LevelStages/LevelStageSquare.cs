using System.Collections.Generic;
using Extensions;
using Games.Main_Mode.BlockObstacles;
using Games.Main_Mode.StageBlocks;
using UnityEngine;

namespace Games.Main_Mode.LevelStages
{
    public class LevelStageSquare : LevelStageBase
    {
        public override float Height { get; }

        public static LevelStageSquare Create(
            Transform _Parent, 
            int _StageIdx, 
            IEnumerable<StageBlockProps> _BlocksProps,
            float _VerticalPosition)
        {
            var go = new GameObject();
            var stage = go.AddComponent<LevelStageSquare>();
            stage.Init(_StageIdx, _BlocksProps);
            go.transform.SetLocalPosXY(0, _VerticalPosition);
            go.SetParent(_Parent);
            return stage;
        }
        
        protected override IEnumerable<StageBlockBase> InitBlocks(
            IEnumerable<StageBlockProps> _BlocksProps)
        {
            var blocks = new List<StageBlockBase>();
            foreach (var props in _BlocksProps)
            {
                var block = StageBlockCircle.Create(transform, props);
                blocks.Add(block);
            }
            return blocks;
        }

        protected override void InitCenterBlock()
        {
            throw new System.NotImplementedException();
        }
    }
}