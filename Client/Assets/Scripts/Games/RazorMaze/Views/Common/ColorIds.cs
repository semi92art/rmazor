using System.Collections.Generic;
using DI.Extensions;

namespace Games.RazorMaze.Views.Common
{
    public static class ColorIds
    {
        public static readonly int Main;
        public static readonly int Background; //!!!
        public static readonly int BackgroundIdleItems;
        public static readonly int BackgroundCongratsItems;
        public static readonly int Character;
        public static readonly int CharacterTail;
        public static readonly int MazeItem1;
        public static readonly int MazeItem2;
        public static readonly int MoneyItem;
        public static readonly int UI;
        public static readonly int UiDialogItemNormal;
        public static readonly int UiDialogItemPressed;
        public static readonly int UiDialogItemDisabled;
        public static readonly int UiDialogItemSelected;
        public static readonly int UiDialogBackground;
        public static readonly int UiBorderDefault;
        public static readonly int UiTextDefault;

        static ColorIds()
        {
            Main                     = GetHash(nameof(Main));
            Background               = GetHash(nameof(Background));
            BackgroundIdleItems      = GetHash(nameof(BackgroundIdleItems)     .WithSpaces());
            BackgroundCongratsItems  = GetHash(nameof(BackgroundCongratsItems) .WithSpaces());
            Character                = GetHash(nameof(Character));
            CharacterTail            = GetHash(nameof(CharacterTail)           .WithSpaces());
            MazeItem1                = GetHash(nameof(MazeItem1)               .WithSpaces());
            MazeItem2                = GetHash(nameof(MazeItem2)               .WithSpaces());
            MoneyItem                = GetHash(nameof(MoneyItem)               .WithSpaces());
            UI                       = GetHash(nameof(UI));
            UiDialogItemNormal       = GetHash(nameof(UiDialogItemNormal)      .WithSpaces());
            UiDialogItemPressed      = GetHash(nameof(UiDialogItemPressed)     .WithSpaces());
            UiDialogItemDisabled     = GetHash(nameof(UiDialogItemDisabled)    .WithSpaces());
            UiDialogItemSelected     = GetHash(nameof(UiDialogItemSelected)    .WithSpaces());
            UiDialogBackground       = GetHash(nameof(UiDialogBackground)      .WithSpaces());
            UiBorderDefault          = GetHash(nameof(UiBorderDefault)         .WithSpaces());
            UiTextDefault            = GetHash(nameof(UiTextDefault)           .WithSpaces());
        }

        public static int GetHash(string _S)
        {
            return _S.GetHashCode();
        }

        public static string GetColorNameById(int _Id)
        {
            return ColorNamesDict[_Id];
        }
        
        private static Dictionary<int, string> ColorNamesDict => new Dictionary<int, string>
        {
            {Main, nameof(Main).WithSpaces()},
            {Background, nameof(Background).WithSpaces()},
            {BackgroundIdleItems, nameof(BackgroundIdleItems).WithSpaces()},
            {BackgroundCongratsItems, nameof(BackgroundCongratsItems).WithSpaces()},
            {Character, nameof(Character).WithSpaces()},
            {CharacterTail, nameof(CharacterTail).WithSpaces()},
            {MazeItem1, nameof(MazeItem1).WithSpaces()},
            {MazeItem2, nameof(MazeItem2).WithSpaces()},
            {MoneyItem, nameof(MoneyItem).WithSpaces()},
            {UI, nameof(UI)},
            {UiDialogItemNormal, nameof(UiDialogItemNormal).WithSpaces()},
            {UiDialogItemPressed, nameof(UiDialogItemPressed).WithSpaces()},
            {UiDialogItemDisabled, nameof(UiDialogItemDisabled).WithSpaces()},
            {UiDialogItemSelected, nameof(UiDialogItemSelected).WithSpaces()},
            {UiDialogBackground, nameof(UiDialogBackground).WithSpaces()},
            {UiBorderDefault, nameof(UiBorderDefault).WithSpaces()},
            {UiTextDefault, nameof(UiTextDefault).WithSpaces()}
        };
    }
}