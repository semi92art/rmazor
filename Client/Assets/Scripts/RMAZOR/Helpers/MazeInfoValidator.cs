using System;
using System.Collections.Generic;
using System.Linq;
using RMAZOR.Models.MazeInfos;

namespace RMAZOR.Helpers
{
    public interface IMazeInfoValidator
    {
        bool Validate(MazeInfo _Info);
    }
    
    public class MazeInfoValidator : IMazeInfoValidator
    {
        private static readonly List<EMazeItemType> PossibleMazeItemTypes = Enum.GetValues(typeof(EMazeItemType))
            .Cast<EMazeItemType>()
            .ToList();
        
        public bool Validate(MazeInfo _Info)
        {
            return _Info.MazeItems
                .Select(_Item => _Item.Type)
                .All(_Type => PossibleMazeItemTypes.Contains(_Type));
        }
    }
}