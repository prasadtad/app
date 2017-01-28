using RecipeShelf.Cache.Models;
using RecipeShelf.Common;
using RecipeShelf.Common.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace RecipeShelf.Cache.Proxies
{
    public class RedisProxy : ICacheProxy
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

        public string Get(string setKey, Id id)
        {
            _logger.Debug("Get", $"Getting {id.Value} value from {setKey}");
            var value = _redis.GetDatabase().HashGet(setKey, id.Value);
            if (value.IsNullOrEmpty) return string.Empty;
            return value;
        }

        public bool GetFlag(string setKey, Id id)
        {
            _logger.Debug("GetFlag", $"Getting {id.Value} flag from {setKey}");
            var value = _redis.GetDatabase().HashGet(setKey, id.Value);
            if (value.IsNullOrEmpty) return false;
            return value == Constants.TRUE;
        }

        public Id[] Ids(string setKey)
        {
            _logger.Debug("Ids", $"Getting {setKey} ids");
            var values = _redis.GetDatabase().SetMembers(setKey);
            var ids = new Id[values.Length];
            for (var i = 0; i < ids.Length; i++) ids[i] = new Id(values[i]);
            return ids;
        }

        public bool IsMember(string setKey, Id id)
        {
            _logger.Debug("IsMember", $"Checking if {id.Value} is member of {setKey}");
            return _redis.GetDatabase().SetContains(setKey, id.Value);
        }

        public string[] Members(string setKey)
        {
            _logger.Debug("Members", $"Getting {setKey} members");
            return _redis.GetDatabase().SetMembers(setKey).ToStringArray();
        }

        public Id[] RandomIds(string setKey, int count)
        {
            _logger.Debug("RandomIds", $"Getting {count} random members from {setKey}");
            var db = _redis.GetDatabase();
            if (count == 1)
            {
                var value = db.SetRandomMember(setKey);
                if (value.IsNullOrEmpty) return new Id[0];
                return new Id[] { new Id(value) };
            }
            var values = db.SetRandomMembers(setKey, count);
            var ids = new Id[values.Length];
            for (var i = 0; i < ids.Length; i++) ids[i] = new Id(values[i]);
            return ids;
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
                            transaction.SetRemoveAsync(setEntry.SetPrefix.Append(setName), setEntry.Value.Value);
                            count++;
                        }
                    }
                    foreach (var setName in setEntry.SortedSetNames)
                    {
                        if (string.IsNullOrEmpty(setName)) continue;
                        transaction.SetAddAsync(setEntry.SetPrefix.Append(setName), setEntry.Value.Value);
                        count++;
                    }
                }
                else
                {
                    var hashEntry = (Models.HashEntry)entry;
                    if (string.IsNullOrEmpty(hashEntry.Value))
                        transaction.HashDeleteAsync(hashEntry.SetKey, hashEntry.Id.Value);
                    else
                        transaction.HashSetAsync(hashEntry.SetKey, hashEntry.Id.Value, hashEntry.Value);
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
