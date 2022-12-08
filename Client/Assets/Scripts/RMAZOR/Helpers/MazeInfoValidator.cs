using System;
using System.Collections.Generic;
using System.Linq;
using RMAZOR.Models.MazeInfos;

namespace RMAZOR.Helpers
{
    public interface IMazeInfoValidator
    {
        bool Validate(MazeInfo _Info, out string _Error);
        bool ValidateRaw(string _InfoRaw, out string _Error);
    }
    
    public class MazeInfoValidator : IMazeInfoValidator
    {
        private static readonly List<int> PossibleMazeItemTypes = Enum.GetValues(typeof(EMazeItemType))
            .Cast<int>()
            .ToList();
        
        public bool Validate(MazeInfo _Info, out string _Error)
        {
            bool success = true;
            _Error = null;
            if (_Info == null)
            {
                _Error = "Maze info is null.";
                return false;
            }
            if (!_Info.MazeItems.Any())
            {
                success = _Info.PathItems.Any();
                if (!success)
                    _Error = "MazeInfo does not contain maze and path items at all.";
                return success;
            }
            var mazeItemTypesInt = 
                _Info.MazeItems
                .Select(_Item => _Item.Type)
                .Select(_Item => (int)_Item)
                .Distinct()
                .ToList();
            var impossibleMazeItemTypesInt = new List<int>();
            foreach (int mazeItemTypeInt in mazeItemTypesInt)
            {
                if (PossibleMazeItemTypes.Contains(mazeItemTypeInt)) 
                    continue;
                impossibleMazeItemTypesInt.Add(mazeItemTypeInt);
                success = false;
            }
            if (!success)
            {
                _Error = "MazeInfo contains maze items of invalid types: " +
                         string.Join(',', impossibleMazeItemTypesInt);
            }
            return success;
        }

        public bool ValidateRaw(string _InfoRaw, out string _Error)
        {
            _Error = null;
            if (string.IsNullOrEmpty(_InfoRaw))
            {
                _Error = "Maze info is null.";
                return false;
            }
            return true;
        }
    }
}