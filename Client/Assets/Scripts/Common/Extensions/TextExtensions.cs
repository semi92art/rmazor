using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Common.Extensions
{
    public static class TextExtensions
    {
        #region api

        public static string WithSpaces(this string _Name)
        {
            if (string.IsNullOrWhiteSpace(_Name))
                return "";
            var newText = new StringBuilder(_Name.Length * 2);
            newText.Append(_Name[0]);
            for (int i = 1; i < _Name.Length; i++)
            {
                if ((char.IsUpper(_Name[i])
                     || char.IsDigit(_Name[i]))
                    && _Name[i - 1] != ' ')
                {
                    newText.Append(' ');
                }
                newText.Append(_Name[i]);
            }
            return newText.ToString();
        }
        
        public static string GetFileName(this string _Path, bool _WithExtension)
        {
            _Path =  _Path.Replace('/', '\\');
            string nameWithExtension = _Path.Substring(_Path.LastIndexOf('\\') + 1,
                    _Path.Length - 1 - _Path.LastIndexOf('\\'));
            return _WithExtension ? nameWithExtension : 
                nameWithExtension.Split('.')[0];
        }
        
        public static bool EqualsIgnoreCase(this string _Text, string _Other)
        {
            return string.Compare(_Text, _Other, StringComparison.OrdinalIgnoreCase) == 0;
        }
        
        public static string FirstCharToUpper(this string _Text, CultureInfo _Culture)
        {
            return _Text.FirstCharTo(true, _Culture);
        }

        public static string FirstCharToLower(this string _Text, CultureInfo _Culture)
        {
            return _Text.FirstCharTo(false, _Culture);
        }

        public static bool InRange(this string _Text, params string[] _Strings)
        {
            return _Strings.Any(_S => _Text == _S);
        }

        public static bool InRange(this char _Symbol, params char[] _Symbols)
        {
            return _Symbols.Any(_S => _Symbol == _S);
        }

        public static string ToNumeric(this long _Value)
        {
            return _Value.ToString("N0", CultureInfo.CreateSpecificCulture("en-US"));
        }
        
        public static string ToNumeric(this ulong _Value)
        {
            return ToNumeric((long) _Value);
        }

        public static string ToNumeric(this int _Value)
        {
            return ((long) _Value).ToNumeric();
        }
        
        public static string Shortened(this string _Text, int _Length, bool _Ellipsis = true)
        {
            return _Text.Substring(0, _Length) + (_Ellipsis ? "..." : string.Empty);
        }

        #endregion

        #region nonpublic methods
        
        private static string FirstCharTo(this string _Text, bool _ToUpper, CultureInfo _Culture)
        {
            char first = _ToUpper ? char.ToUpper(_Text[0], _Culture) : char.ToLower(_Text[0], _Culture);
            return first + _Text.Substring(1);
        }

        #endregion
        

    }
}