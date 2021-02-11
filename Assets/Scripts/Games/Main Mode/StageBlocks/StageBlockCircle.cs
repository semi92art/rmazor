using Shapes;
using UnityEngine;

namespace Games.Main_Mode.StageBlocks
{
    public class StageBlockCircle : StageBlockBase
    {
        [SerializeField] private Disc disc;

        public void Init(int _Index)
        {
            base.Init(_Index);
        }
    }
}