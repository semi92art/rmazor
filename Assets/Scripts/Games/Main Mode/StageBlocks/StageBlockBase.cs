using UnityEngine;

namespace Games.Main_Mode.StageBlocks
{
    public interface IStageBlock
    {
        void UpdateState();
    }

    public abstract class StageBlockBase : MonoBehaviour, IStageBlock
    {
        private int m_BlockIndex;

        protected void Init(int _BlockIndex)
        {
            m_BlockIndex = _BlockIndex;
        }

        public virtual void UpdateState()
        {
            
        }
    }

    public class StageBlockParameters
    {
        //public BlockObs
    }
}