using RecipeShelf.Common.Models;
using System;

namespace RecipeShelf.Cache.Models
{
    public interface IEntry
    {
    }

    public struct HashEntry : IEntry
    {
        public readonly string SetKey;

        public readonly string HashField;

        public readonly string Value;

        public HashEntry(string setKey, string hashField, string value)
        {
            SetKey = setKey;
            HashField = hashField;
            Value = value;
        }  
        
        public HashEntry(string setKey, string hashField, bool value)
        {
            SetKey = setKey;
            HashField = hashField;
            Value = value ? Constants.TRUE : Constants.FALSE;
        }      
    }

    public struct SetEntry : IEntry
    {
        public readonly string SetPrefix;

        public readonly string[] SortedSetNames;

        public readonly Id Value;

        public SetEntry(string setPrefix, string name, Id value)
        {
            SetPrefix = setPrefix;
            SortedSetNames = new string[] { name };
            Value = value;
        }

        public SetEntry(string setPrefix, string[] names, Id value)
        {
            SetPrefix = setPrefix;
            Array.Sort(names);
            SortedSetNames = names;
            Value = value;
        }

        public SetEntry(string setPrefix, bool name, Id value)
        {
            SetPrefix = setPrefix;
            SortedSetNames = new string[] { name ? Constants.TRUE : Constants.FALSE };
            Value = value;
        }
    }
}