using System.Collections.Generic;
using Common.Extensions;

namespace Common
{
    public class ColorIdsCommon
    {
        public static readonly int UI;
        public static readonly int UiDialogItemNormal;
        public static readonly int UiDialogBackground;
        public static readonly int UiBorder;
        public static readonly int UiText;
        public static readonly int UiBackground;
        public static readonly int UiItemHighlighted;
        public static readonly int UiStartLogo;
        

        static ColorIdsCommon()
        {
            UI                       = GetHash(nameof(UI));
            UiDialogItemNormal       = GetHash(nameof(UiDialogItemNormal).WithSpaces());
            UiDialogBackground       = GetHash(nameof(UiDialogBackground).WithSpaces());
            UiBorder                 = GetHash(nameof(UiBorder)          .WithSpaces());
            UiText                   = GetHash(nameof(UiText)            .WithSpaces());
            UiBackground             = GetHash(nameof(UiBackground)      .WithSpaces());
            UiItemHighlighted        = GetHash(nameof(UiItemHighlighted) .WithSpaces());
            UiStartLogo              = GetHash(nameof(UiStartLogo)       .WithSpaces());
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
            {UI,                       nameof(UI)},
            {UiDialogItemNormal,       nameof(UiDialogItemNormal).WithSpaces()},
            {UiDialogBackground,       nameof(UiDialogBackground).WithSpaces()},
            {UiBorder,                 nameof(UiBorder).WithSpaces()},
            {UiText,                   nameof(UiText).WithSpaces()},
            {UiBackground,             nameof(UiBackground).WithSpaces()},
            {UiItemHighlighted,        nameof(UiItemHighlighted).WithSpaces()},
            {UiStartLogo,              nameof(UiStartLogo).WithSpaces()}
        };
    }
}