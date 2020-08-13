using System;
using System.Linq;

namespace Clickers
{
    public static class TextExtentions
    {
        public static string FirstCharToUpper(this string input)
        {
            return input.FirstCharTo(true);
        }

        public static string FirstCharToLower(this string input)
        {
            return input.FirstCharTo(false);
        }

        private static string FirstCharTo(this string input, bool upper)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return upper ? input.First().ToString().ToUpper() : input.First().ToString().ToLower() + input.Substring(1);
            }
        }
    }
}