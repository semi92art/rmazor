using DI.Extensions;

namespace Games.RazorMaze.Views.Common
{
    public static class ColorIds
    {
        public static readonly int Border;
        public static readonly int Corner;
        public static readonly int Background; //!!!
        public static readonly int BackgroundItems;
        public static readonly int Character;
        public static readonly int CharacterTail;
        public static readonly int MazeItem;
        public static readonly int MazeItemPathLine;
        public static readonly int MazeItemPathJoint;
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
            Border               = GetHash(nameof(Border));
            Corner               = GetHash(nameof(Corner));
            Background           = GetHash(nameof(Background));
            BackgroundItems      = GetHash(nameof(BackgroundItems)     .WithSpaces());
            Character            = GetHash(nameof(Character));
            CharacterTail        = GetHash(nameof(CharacterTail)       .WithSpaces());
            MazeItem             = GetHash(nameof(MazeItem)            .WithSpaces());
            MazeItemPathLine     = GetHash(nameof(MazeItemPathLine)    .WithSpaces());
            MazeItemPathJoint    = GetHash(nameof(MazeItemPathJoint)   .WithSpaces());
            Path                 = GetHash(nameof(Path));
            UI                   = GetHash(nameof(UI));
            UiDialogItemNormal   = GetHash(nameof(UiDialogItemNormal)  .WithSpaces());
            UiDialogItemPressed  = GetHash(nameof(UiDialogItemPressed) .WithSpaces());
            UiDialogItemDisabled = GetHash(nameof(UiDialogItemDisabled).WithSpaces());
            UiDialogItemSelected = GetHash(nameof(UiDialogItemSelected).WithSpaces());
            UiDialogBackground   = GetHash(nameof(UiDialogBackground)  .WithSpaces());
            UiBorderDefault      = GetHash(nameof(UiBorderDefault)     .WithSpaces());
            UiTextDefault        = GetHash(nameof(UiTextDefault)       .WithSpaces());
        }

        public static int GetHash(string _S)
        {
            return _S.GetHashCode();
        }
    }
}