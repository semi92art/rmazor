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
        private static readonly List<int> PossibleMazeItemTypes = Enum.GetValues(typeof(EMazeItemType))
            .Cast<int>()
            .ToList();
        
        public bool Validate(MazeInfo _Info)
        {
            if (!_Info.MazeItems.Any())
                return _Info.PathItems.Any();
            return _Info.MazeItems
                .Select(_Item => _Item.Type)
                .All(_Type => PossibleMazeItemTypes.Contains((int)_Type));
        }
    }
}