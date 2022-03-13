using Common.Exceptions;
using RMAZOR.Models.MazeInfos;

namespace RMAZOR.Views.Utils
{
    public static class SortingOrders
    {
        public const int
            BackgroundTexture           = Path - 6,
            BackgroundItem              = Path - 5,
            AdditionalBackgroundPolygon = Path - 4,
            AdditionalBackgroundBorder  = Path - 3,
            AdditionalBackgroundCorner  = Path - 2,
            PathBackground              = Path - 1,
            Path                        = -1,
            PathLine                    = Path + 1,
            PathJoint                   = Path + 2,
            MoneyItem                   = Path + 3,
            Character                   = Path + 200,
            GameLogoBackground          = Path + 300,
            GameLogoForeground          = Path + 301;

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
                case EMazeItemType.Turret:
                    return Path + 100;
                case EMazeItemType.TrapReact:
                    return Path + 150;
                default: throw new SwitchCaseNotImplementedException(_Type);
            }
        }
    }
}