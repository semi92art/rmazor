using Common.Exceptions;
using RMAZOR.Models.MazeInfos;

namespace RMAZOR.Views.Utils
{
    public static class SortingOrders
    {
        public const int
            BackgroundTexture            = Path - 7,
            BackgroundItem               = Path - 6,
            AdditionalBackgroundTexture2 = Path - 5,
            AdditionalBackgroundTexture  = Path - 4,
            AdditionalBackgroundBorder   = Path - 3,
            AdditionalBackgroundCorner   = Path - 2,
            PathBackground               = Path - 1,
            Path                         = -1,
            PathLine                     = Path + 50,
            PathJoint                    = Path + 100,
            MoneyItem                    = Path + 150,
            Character                    = Path + 500,
            GameLogoBackground           = Path + 550,
            GameLogoForeground           = Path + 600;

        public static int GetBlockSortingOrder(EMazeItemType _Type)
        {
            switch (_Type)
            {
                case EMazeItemType.GravityBlock:
                case EMazeItemType.GravityTrap:
                    return Path + 151;
                case EMazeItemType.Block:
                    return Path + 152;
                case EMazeItemType.Portal:
                case EMazeItemType.TrapIncreasing:
                case EMazeItemType.TrapMoving:
                case EMazeItemType.GravityBlockFree:
                case EMazeItemType.ShredingerBlock:
                case EMazeItemType.Springboard:
                    return Path + 153;
                case EMazeItemType.Turret:
                    return Path + 200;
                case EMazeItemType.TrapReact:
                    return Path + 300;
                case EMazeItemType.MovingBlockFree:
                default: throw new SwitchCaseNotImplementedException(_Type);
            }
        }
    }
}