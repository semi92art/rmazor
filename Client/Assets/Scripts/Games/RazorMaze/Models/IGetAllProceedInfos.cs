using System;
using Games.RazorMaze.Models.ProceedInfos;

namespace Games.RazorMaze.Models
{
    public interface IGetAllProceedInfos
    {
        Func<IMazeItemProceedInfo[]> GetAllProceedInfos { set; }
    }
}