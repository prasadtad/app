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

        public readonly Id Id;

        public readonly string Value;

        public HashEntry(string setKey, Id id, string value)
        {
            SetKey = setKey;
            Id = id;
            Value = value;
        }  
        
        public HashEntry(string setKey, Id id, bool value)
        {
            SetKey = setKey;
            Id = id;
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