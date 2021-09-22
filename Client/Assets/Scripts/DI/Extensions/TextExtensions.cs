using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DI.Extensions
{
    public static class TextExtensions
    {
        public static string WithSpaces(this string _Name)
        {
            if (string.IsNullOrWhiteSpace(_Name))
                return "";
            StringBuilder newText = new StringBuilder(_Name.Length * 2);
            newText.Append(_Name[0]);
            for (int i = 1; i < _Name.Length; i++)
            {
                if (char.IsUpper(_Name[i]) && _Name[i - 1] != ' ')
                    newText.Append(' ');
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
        
        public static string FirstCharToUpper(this string _Text)
        {
            return _Text.FirstCharTo(true);
        }

        public static string FirstCharToLower(this string _Text)
        {
            return _Text.FirstCharTo(false);
        }

        public static bool InRange(this string _Text, params string[] _Strings)
        {
            return _Strings.Any(_S => _Text == _S);
        }

        public static bool InRange(this char _Symbol, params char[] _Symbols)
        {
            return _Symbols.Any(_S => _Symbol == _S);
        }

        private static string FirstCharTo(this string _Text, bool _ToUpper)
        {
            switch (_Text)
            {
                case null: throw new ArgumentNullException(nameof(_Text));
                case "": throw new ArgumentException($"{nameof(_Text)} cannot be empty", nameof(_Text));
                default: return _ToUpper ? _Text.First().ToString().ToUpper() : _Text.First().ToString().ToLower() + _Text.Substring(1);
            }
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
    }
}