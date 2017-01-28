using RecipeShelf.Common.Models;

namespace RecipeShelf.Cache
{
    public static class Extensions
    {
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
    }
}
