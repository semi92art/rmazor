using DI.Extensions;

namespace Games.RazorMaze.Views.Common
{
    public static class ColorIds
    {
        public static readonly int Border;
        public static readonly int Main;
        public static readonly int Background; //!!!
        public static readonly int BackgroundIdleItems;
        public static readonly int BackgroundCongratsItems;
        public static readonly int Character;
        public static readonly int CharacterTail;
        public static readonly int MazeItem1;
        public static readonly int MazeItem2;
        public static readonly int Path;
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
            Border                   = GetHash(nameof(Border));
            Main                     = GetHash(nameof(Main));
            Background               = GetHash(nameof(Background));
            BackgroundIdleItems      = GetHash(nameof(BackgroundIdleItems)     .WithSpaces());
            BackgroundCongratsItems  = GetHash(nameof(BackgroundCongratsItems) .WithSpaces());
            Character                = GetHash(nameof(Character));
            CharacterTail            = GetHash(nameof(CharacterTail)           .WithSpaces());
            MazeItem1                = GetHash(nameof(MazeItem1)               .WithSpaces());
            MazeItem2                = GetHash(nameof(MazeItem2)               .WithSpaces());
            Path                     = GetHash(nameof(Path));
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
    }
}