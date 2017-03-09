using System;

namespace RecipeShelf.Data.VPC.Models
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

        public HashEntry(string setKey, string hashField)
        {
            SetKey = setKey;
            HashField = hashField;
            Value = null;
        }
    }

    public struct SetEntry : IEntry
    {
        public readonly string SetPrefix;

        public readonly string[] SortedSetNames;

        public readonly string Value;

        public SetEntry(string setPrefix, string name, string value)
        {
            SetPrefix = setPrefix;
            SortedSetNames = new string[] { name };
            Value = value;
        }

        public SetEntry(string setPrefix, string[] names, string value)
        {
            SetPrefix = setPrefix;
            Array.Sort(names);
            SortedSetNames = names;
            Value = value;
        }

        public SetEntry(string setPrefix, bool name, string value)
        {
            SetPrefix = setPrefix;
            SortedSetNames = new string[] { name ? Constants.TRUE : Constants.FALSE };
            Value = value;
        }

        public SetEntry(string setPrefix, string value)
        {
            SetPrefix = setPrefix;
            SortedSetNames = null;
            Value = value;
        }
    }
}