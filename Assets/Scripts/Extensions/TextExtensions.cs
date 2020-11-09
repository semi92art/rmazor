using System;
using System.Linq;

namespace Extensions
{
    public static class TextExtensions
    {
        public static string FirstCharToUpper(this string _Text)
        {
            return _Text.FirstCharTo(true);
        }

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