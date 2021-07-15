using Exceptions;
using Games.RazorMaze.Models;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.Utils
{
    public static class ViewUtils
    {
        private const int PathItemOrder = 0;
        
        public static int Set { get; set; } = 1;


        public static Color ColorMain => GetFromPalette("Main");
        public static Color ColorFill => GetFromPalette("Fill");
        public static Color ColorBlock => GetFromPalette("Block");
        public static Color ColorHint => GetFromPalette("Hint");

        public static Color ColorLines => GetFromCommonPalette("Lines");
        public static Color ColorCharacter => GetFromCommonPalette("Character");
        public static Color ColorCharacterTail => GetFromCommonPalette("Character Tail");
        public static Color ColorTrap => GetFromCommonPalette("Trap");
        public static Color ColorTurret => GetFromCommonPalette("Turret");
        public static Color ColorPortal => GetFromCommonPalette("Portal");
        public static Color ColorShredinger => GetFromCommonPalette("Shredinger");
        public static Color ColorSpringboard => GetFromCommonPalette("Springboard");

        public static int GetPathSortingOrder() => PathItemOrder;

        public static int GetBlockSortingOrder(EMazeItemType _Type)
        {
            switch (_Type)
            {
                case EMazeItemType.GravityBlock:
                case EMazeItemType.GravityTrap:
                    return PathItemOrder + 3;
                case EMazeItemType.Block:
                    return PathItemOrder + 4;
                case EMazeItemType.Portal:
                case EMazeItemType.Turret:
                case EMazeItemType.TrapIncreasing:
                case EMazeItemType.TrapMoving:
                case EMazeItemType.TurretRotating:
                case EMazeItemType.ShredingerBlock:
                case EMazeItemType.Springboard:
                    return PathItemOrder + 5;
                case EMazeItemType.TrapReact:
                    return PathItemOrder + 50;
                default: throw new SwitchCaseNotImplementedException(_Type);
            }
        }

        public static int GetPathLineJointSortingOrder() => PathItemOrder + 2;
        public static int GetPathLineSortingOrder() => PathItemOrder + 1;


        private static Color GetFromCommonPalette(string _Name) =>
            ColorUtils.GetColorFromPalette("View Common", _Name);
        private static Color GetFromPalette(string _Name) =>
            ColorUtils.GetColorFromPalette(ViewSet, _Name);
        private static string ViewSet => $"View Set {Set}";

        
    }
}