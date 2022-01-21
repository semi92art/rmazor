using System;
using RMAZOR.Models.ProceedInfos;

namespace RMAZOR.Models
{
    public interface IGetAllProceedInfos
    {
        Func<IMazeItemProceedInfo[]> GetAllProceedInfos { set; }
    }
}