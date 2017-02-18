using RecipeShelf.Cache.Models;
using RecipeShelf.Common.Models;
using System;
using System.Collections.Generic;

namespace RecipeShelf.Cache.Proxies
{
    public struct CombineOptions
    {
        public readonly LogicalOperator Op;

        public readonly string Destination;

        public readonly string[] SetKeys;

        public CombineOptions(LogicalOperator op, string setPrefix, string[] setNames)
        {
            if (setNames.Length == 0) throw new ArgumentException("Count is zero", "setNames");
            Op = op;
            SetKeys = new string[setNames.Length];
            for (var i = 0; i < SetKeys.Length; i++)
                SetKeys[i] = setPrefix.Append(setNames[i]);
            Destination = string.Join(op == LogicalOperator.And ? "&" : "|", SetKeys);
        }

        public CombineOptions(LogicalOperator op, string[] setKeys)
        {
            if (setKeys.Length == 0) throw new ArgumentException("Count is zero", "setKeys");
            Op = op;
            SetKeys = setKeys;
            Destination = string.Join(op == LogicalOperator.And ? "&" : "|", SetKeys);
        }
    }

    public interface ICacheProxy
    {
        string Combine(CombineOptions options);

        long Count(string setKey);

        string Get(string setKey, string hashField);

        bool GetFlag(string setKey, string hashField);

        List<HashEntry> HashScan(string setKey, string hashFieldPattern);

        Id[] Ids(string setKey);

        bool IsMember(string vegan, Id id);

        string[] Members(string setKey);

        Id[] RandomIds(string setKey, int count);

        string[] RandomMembers(string setKey, int count);

        void Store(IEnumerable<IEntry> batch);
    }
}