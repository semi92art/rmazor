using System.Collections.Generic;
using Common.Enums;

namespace Common.Utils
{
    public static class LocalizationUtils
    {
        private static readonly Dictionary<ELanguage, string> LanguageTitles = new Dictionary<ELanguage, string>
        {
            {ELanguage.English,   "English"},
            {ELanguage.Portugal,  "Portugués"},
            {ELanguage.Spanish,   "Español"},
            {ELanguage.Russian,   "Русский"},
            {ELanguage.German,    "Deutsch"},
            {ELanguage.Japaneese, "日本語"},
            {ELanguage.Korean,    "한국어"}
        };

        public static string GetLanguageTitle(ELanguage _Language)
        {
            return LanguageTitles[_Language];
        }
    }
}