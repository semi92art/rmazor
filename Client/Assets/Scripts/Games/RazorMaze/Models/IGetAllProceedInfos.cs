using System;
using System.Collections.Generic;
using Games.RazorMaze.Models.ProceedInfos;

namespace Games.RazorMaze.Models
{
    public interface IGetAllProceedInfos
    {
        Func<IMazeItemProceedInfo[]> GetAllProceedInfos { set; }
    }
}