using Common.Exceptions;
using RMAZOR.Models;
using RMAZOR.Models.MazeInfos;

namespace RMAZOR.Views.Utils
{
    public static class SortingOrders
    {
        public const int BackgroundItem = Path - 1;
        public const int Path           = -1;
        public const int PathLine       = Path + 1;
        public const int PathJoint      = Path + 2;
        public const int Character      = Path + 400;
        
        public static int GetBlockSortingOrder(EMazeItemType _Type)
        {
            switch (_Type)
            {
                case EMazeItemType.GravityBlock:
                case EMazeItemType.GravityTrap:
                    return Path + 3;
                case EMazeItemType.Block:
                    return Path + 4;
                case EMazeItemType.Portal:
                case EMazeItemType.TrapIncreasing:
                case EMazeItemType.TrapMoving:
                case EMazeItemType.GravityBlockFree:
                case EMazeItemType.ShredingerBlock:
                case EMazeItemType.Springboard:
                    return Path + 5;
                case EMazeItemType.TrapReact:
                    return Path + 200;
                case EMazeItemType.Turret:
                    return Path + 300;
                default: throw new SwitchCaseNotImplementedException(_Type);
            }
        }
    }
}