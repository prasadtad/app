using RecipeShelf.Common.Models;
using System;
using System.Collections.Generic;

namespace RecipeShelf.Common
{
    public static class Extensions
    {
        public static string[] ToStrings<T>(this T[] enumValues) where T : struct, IConvertible
        {
            var values = new string[enumValues.Length];
            for (var i = 0; i < values.Length; i++)
                values[i] = enumValues[i].ToString();
            return values;
        }

        public static Id[] ToIds(this string[] strings)
        {
            var ids = new Id[strings.Length];
            for (var i = 0; i < ids.Length; i++) ids[i] = new Id(strings[i]);
            return ids;
        }

        public static string[] ToStrings(this Id[] ids)
        {
            var strings = new string[ids.Length];
            for (var i = 0; i < ids.Length; i++) strings[i] = ids[i].Value;
            return strings;
        }

        /// <see cref="http://csharpindepth.com/ViewNote.aspx?NoteID=186"/>
        /// <see cref="http://www.javamex.com/tutorials/collections/hash_function_technical_2.shtml"/> 
        public static int GetHashCode(this IEnumerable<object> items)
        {
            unchecked
            {
                int hash = 0;
                foreach (var foo in items)
                    hash = (hash << 5) - hash + foo.GetHashCode();
                return hash;
            }
        }
    }
}
