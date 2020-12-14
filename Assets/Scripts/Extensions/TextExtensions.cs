using System;
using System.Linq;

namespace Extensions
{
    public static class TextExtensions
    {
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
/// <summary>
/// //yjf
/// </summary>
/// <param name="_Text"></param>
/// <returns></returns>
        public static string FirstCharToLower(this string _Text)
        {
            return _Text.FirstCharTo(false);
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
    }
}