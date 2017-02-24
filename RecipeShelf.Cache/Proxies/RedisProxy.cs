using RecipeShelf.Cache.Models;
using RecipeShelf.Common;
using RecipeShelf.Common.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace RecipeShelf.Cache.Proxies
{
    public sealed class RedisProxy : ICacheProxy
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly Logger<RedisProxy> _logger = new Logger<RedisProxy>();

        public RedisProxy()
        {
            _redis = ConnectionMultiplexer.Connect(Settings.CacheEndpoint, Common.Settings.LogLevel == LogLevels.Trace ? Console.Out : null);
        }

        public long Count(string setKey)
        {
            _logger.Debug("Count", $"Getting count of {setKey}");
            var db = _redis.GetDatabase();
            return db.SetLength(setKey);
        }

        public string Get(string setKey, string hashField)
        {
            _logger.Debug("Get", $"Getting {hashField} value from {setKey}");
            var value = _redis.GetDatabase().HashGet(setKey, hashField);
            if (value.IsNullOrEmpty) return string.Empty;
            return value;
        }

        public bool GetFlag(string setKey, string hashField)
        {
            _logger.Debug("GetFlag", $"Getting {hashField} flag from {setKey}");
            var value = _redis.GetDatabase().HashGet(setKey, hashField);
            if (value.IsNullOrEmpty) return false;
            return value == Constants.TRUE;
        }

        public List<Models.HashEntry> HashScan(string setKey, string hashFieldPattern)
        {
            _logger.Debug("HashScan", $"Getting {setKey} entries matching {hashFieldPattern}");
            var entries = new List<Models.HashEntry>();
            var db = _redis.GetDatabase();
            IScanningCursor cursor = null;
            long oldCursor = 0;
            while (cursor == null || cursor.Cursor > oldCursor)
            {
                if (cursor != null) oldCursor = cursor.Cursor;
                var results = db.HashScan(setKey, hashFieldPattern, 100);
                foreach (var entry in results)
                    entries.Add(new Models.HashEntry(setKey, entry.Name, entry.Value));
                cursor = (IScanningCursor)results;
            }
            return entries;
        }

        public bool IsMember(string setKey, string value)
        {
            _logger.Debug("IsMember", $"Checking if {value} is member of {setKey}");
            return _redis.GetDatabase().SetContains(setKey, value);
        }

        public string[] Members(string setKey)
        {
            _logger.Debug("Members", $"Getting {setKey} members");
            return _redis.GetDatabase().SetMembers(setKey).ToStringArray();
        }

        public string[] RandomMembers(string setKey, int count)
        {
            _logger.Debug("RandomMembers", $"Getting {count} random members from {setKey}");
            var db = _redis.GetDatabase();
            if (count == 1)
            {
                var value = db.SetRandomMember(setKey);
                if (value.IsNullOrEmpty) return new string[0];
                return new string[] { value };
            }
            return db.SetRandomMembers(setKey, count).ToStringArray();
        }

        public void Store(IEnumerable<IEntry> batch)
        {
            var db = _redis.GetDatabase();
            var transaction = db.CreateTransaction();
            var count = 0;
            foreach (var entry in batch)
            {
                if (entry is SetEntry)
                {
                    var setEntry = (SetEntry)entry;
                    foreach (var setName in setEntry.SortedSetNames)
                    {
                        if (string.IsNullOrEmpty(setName)) continue;
                        transaction.SetAddAsync(setEntry.SetPrefix, setName);
                        count++;
                    }
                    foreach (var setName in Members(setEntry.SetPrefix))
                    {
                        if (Array.BinarySearch(setEntry.SortedSetNames, setName) < 0)
                        {
                            transaction.SetRemoveAsync(setEntry.SetPrefix.Append(setName), setEntry.Value);
                            count++;
                        }
                    }
                    foreach (var setName in setEntry.SortedSetNames)
                    {
                        if (string.IsNullOrEmpty(setName)) continue;
                        transaction.SetAddAsync(setEntry.SetPrefix.Append(setName), setEntry.Value);
                        count++;
                    }
                }
                else
                {
                    var hashEntry = (Models.HashEntry)entry;
                    if (string.IsNullOrEmpty(hashEntry.Value))
                        transaction.HashDeleteAsync(hashEntry.SetKey, hashEntry.HashField);
                    else
                        transaction.HashSetAsync(hashEntry.SetKey, hashEntry.HashField, hashEntry.Value);
                    count++;
                }
            }
            _logger.Debug("Store", $"Executing transaction with {count} commands");
            transaction.Execute();
        }

        public string Combine(CombineOptions key)
        {
            var keys = new RedisKey[key.SetKeys.Length];
            for (var i = 0; i < keys.Length; i++) keys[i] = key.SetKeys[i];
            var setOp = key.Op == LogicalOperator.And ? SetOperation.Intersect : SetOperation.Union;
            _logger.Debug("Combine", $"Running {setOp.ToString()} into {key.Destination}");
            _redis.GetDatabase().SetCombineAndStore(setOp, key.Destination, keys);
            return key.Destination;
        }
    }
}
