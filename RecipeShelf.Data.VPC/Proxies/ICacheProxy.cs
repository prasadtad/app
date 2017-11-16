using RecipeShelf.Common.Models;
using RecipeShelf.Data.VPC.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipeShelf.Data.VPC.Proxies
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
        bool CanConnect();

        Task FlushAsync();

        Task<string> CombineAsync(CombineOptions key);

        Task<long> CountAsync(string setKey);

        Task<string> GetAsync(string setKey, string hashField);

        Task<bool> GetFlagAsync(string setKey, string hashField);

        Task<string> GetStringAsync(string setKey);

        Task<string[]> HashFieldsAsync(string setKey);

        List<HashEntry> HashScan(string setKey, string hashFieldPattern);

        Task<bool> IsMemberAsync(string setKey, string value);

        Task<string[]> MembersAsync(string setKey);

        Task<string[]> RandomMembersAsync(string setKey, int count);

        Task SetStringAsync(string setKey, string value, TimeSpan? expiry = null);

        Task StoreAsync(IEnumerable<IEntry> batch);
    }
}