﻿using System;
using System.Collections.Generic;

namespace RecipeShelf.Common
{
    public static class Extensions
    {
        public static string Describe(this TimeSpan timespan)
        {
            var description = timespan.Days > 0 ? timespan.Days + (timespan.Days == 1 ? " day, " : " days, ") : string.Empty;
            if (timespan.Hours > 0) description += timespan.Hours + (timespan.Hours == 1 ? " hour, " : " hours, ");
            if (timespan.Minutes > 0) description += timespan.Minutes + (timespan.Minutes == 1 ? " minute, " : " minutes, ");
            if (timespan.Seconds > 0) description += timespan.Seconds + (timespan.Seconds == 1 ? " second and " : " seconds and ");
            return description + timespan.Milliseconds + " milliseconds";
        }

        public static string[] ToStrings<T>(this T[] enumValues) where T : struct, IConvertible
        {
            var values = new string[enumValues.Length];
            for (var i = 0; i < values.Length; i++)
                values[i] = enumValues[i].ToString();
            return values;
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
