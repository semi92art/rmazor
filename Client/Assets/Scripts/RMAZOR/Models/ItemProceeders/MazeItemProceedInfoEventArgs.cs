using RMAZOR.Models.ProceedInfos;

namespace RMAZOR.Models.ItemProceeders
{
    public abstract class MazeItemProceedInfoEventArgs
    {
        public IMazeItemProceedInfo Info { get; }

        protected MazeItemProceedInfoEventArgs(IMazeItemProceedInfo _Info)
        {
            Info = _Info;
        }
    }
}