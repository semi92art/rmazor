﻿using System.Collections.Generic;
using System.Linq;
using Common.Extensions;

namespace Common
{
    public static partial class ColorIds
    {
        public const int UI                 = 1;
        public const int UiDialogItemNormal = 2;
        public const int UiDialogBackground = 3;
        public const int UiBorder           = 4;
        public const int UiText             = 5;
        public const int UiBackground       = 6;
        public const int UiItemHighlighted  = 7;
        
        public const int Main           = 101;
        public const int Background1    = 102;
        public const int Background2    = 103;
        public const int Character      = 104;
        public const int Character2     = 105;
        public const int CharacterTail  = 106;
        public const int MazeItem1      = 107;
        public const int MoneyItem      = 108;
        public const int PathFill       = 109;
        public const int PathItem       = 110;
        public const int PathBackground = 111;
        

        public static int GetColorIdByName(string _ColorName)
        {
            var kvp = ColorNamesDict.FirstOrDefault(
                _Kvp => _Kvp.Value == _ColorName);
            return kvp.Key;
        }
        
        public static string GetColorNameById(int _Id)
        {
            return ColorNamesDict[_Id];
        }
        
        private static Dictionary<int, string> ColorNamesDict => new Dictionary<int, string>
        {
            {UI,                 nameof(UI)},
            {UiDialogItemNormal, nameof(UiDialogItemNormal).WithSpaces()},
            {UiDialogBackground, nameof(UiDialogBackground).WithSpaces()},
            {UiBorder,           nameof(UiBorder)          .WithSpaces()},
            {UiText,             nameof(UiText)            .WithSpaces()},
            {UiBackground,       nameof(UiBackground)      .WithSpaces()},
            {UiItemHighlighted,  nameof(UiItemHighlighted) .WithSpaces()},
            {Main,               nameof(Main)              .WithSpaces()},
            {Background1,        nameof(Background1)       .WithSpaces()},
            {Background2,        nameof(Background2)       .WithSpaces()},
            {Character,          nameof(Character)         .WithSpaces()},
            {Character2,         nameof(Character2)        .WithSpaces()},
            {CharacterTail,      nameof(CharacterTail)     .WithSpaces()},
            {MazeItem1,          nameof(MazeItem1)         .WithSpaces()},
            {MoneyItem,          nameof(MoneyItem)         .WithSpaces()},
            {PathFill,           nameof(PathFill)          .WithSpaces()},
            {PathItem,           nameof(PathItem)          .WithSpaces()},
            {PathBackground,     nameof(PathBackground)    .WithSpaces()},
        };
    }
}