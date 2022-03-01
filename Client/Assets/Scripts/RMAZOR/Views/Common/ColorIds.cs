using System.Collections.Generic;
using Common;
using Common.Extensions;

namespace RMAZOR.Views.Common
{
    public static class ColorIds
    {
        public static readonly int Main;
        public static readonly int Background1;
        public static readonly int Background2;
        public static readonly int BackgroundIdleItems;
        public static readonly int Character;
        public static readonly int CharacterTail;
        public static readonly int MazeItem1;
        public static readonly int MazeItem2;
        public static readonly int MoneyItem;

        static ColorIds()
        {
            Main                     = ColorIdsCommon.GetHash(nameof(Main));
            Background1               = ColorIdsCommon.GetHash(nameof(Background1).WithSpaces());
            Background2              = ColorIdsCommon.GetHash(nameof(Background2)        .WithSpaces());
            BackgroundIdleItems      = ColorIdsCommon.GetHash(nameof(BackgroundIdleItems).WithSpaces());
            Character                = ColorIdsCommon.GetHash(nameof(Character));
            CharacterTail            = ColorIdsCommon.GetHash(nameof(CharacterTail)      .WithSpaces());
            MazeItem1                = ColorIdsCommon.GetHash(nameof(MazeItem1)          .WithSpaces());
            MazeItem2                = ColorIdsCommon.GetHash(nameof(MazeItem2)          .WithSpaces());
            MoneyItem                = ColorIdsCommon.GetHash(nameof(MoneyItem)          .WithSpaces());
        }

        public static string GetColorNameById(int _Id)
        {
            return ColorNamesDict[_Id];
        }
        
        private static Dictionary<int, string> ColorNamesDict => new Dictionary<int, string>
        {
            {Main,                     nameof(Main)},
            {Background1,               nameof(Background1).WithSpaces()},
            {Background2,               nameof(Background2)       .WithSpaces()},
            {BackgroundIdleItems,      nameof(BackgroundIdleItems).WithSpaces()},
            {Character,                nameof(Character)},
            {CharacterTail,            nameof(CharacterTail)      .WithSpaces()},
            {MazeItem1,                nameof(MazeItem1)          .WithSpaces()},
            {MazeItem2,                nameof(MazeItem2)          .WithSpaces()},
            {MoneyItem,                nameof(MoneyItem)          .WithSpaces()}
        };
    }
}