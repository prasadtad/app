using RecipeShelf.Common.Models;
using System;
using System.Text.RegularExpressions;

namespace RecipeShelf.Cache
{
    public static class Extensions
    {
        private readonly static Regex _nonPunctuationLowerCaseRegex = new Regex("[^-a-z0-9\\s]+", RegexOptions.Compiled);

        public static string Append(this KeyType type, string name)
        {
            return $"{type}{Constants.SEPERATOR}{name}";
        }

        public static string Append(this string prefix, string name)
        {
            return $"{prefix}{Constants.SEPERATOR}{name}";
        }

        public static string Append(this string prefix, bool flag)
        {
            return $"{prefix}{Constants.SEPERATOR}{(flag ? Constants.TRUE : Constants.FALSE)}";
        }

        public static string[] ToLowerCaseWords(this string text)
        {
            return _nonPunctuationLowerCaseRegex.Replace(text.ToLower(), "").Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
