using RecipeShelf.Data.VPC.Models;
using RecipeShelf.Common.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RecipeShelf.Data.VPC.Proxies
{
    public sealed class RedisProxy : ICacheProxy
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly ILogger<RedisProxy> _logger;
        private readonly DataVPCSettings _settings;

        public RedisProxy(ILogger<RedisProxy> logger, IOptions<DataVPCSettings> optionsAccessor)
        {
            _logger = logger;
            _settings = optionsAccessor.Value;
            _redis = ConnectionMultiplexer.Connect(_settings.CacheEndpoint);
        }

        public long Count(string setKey)
        {
            _logger.LogDebug("Getting count of {SetKey}", setKey);
            var db = _redis.GetDatabase();
            return db.SetLength(setKey);
        }

        public string Get(string setKey, string hashField)
        {
            _logger.LogDebug("Getting {HashField} value from {SetKey}", hashField, setKey);
            var value = _redis.GetDatabase().HashGet(setKey, hashField);
            if (value.IsNullOrEmpty) return string.Empty;
            return value;
        }

        public bool GetFlag(string setKey, string hashField)
        {
            _logger.LogDebug("Getting {HashField} flag from {SetKey}", hashField, setKey);
            var value = _redis.GetDatabase().HashGet(setKey, hashField);
            if (value.IsNullOrEmpty) return false;
            return value == Constants.TRUE;
        }

        public string[] HashFields(string setKey)
        {
            _logger.LogDebug("Getting {SetKey} hash fields", setKey);
            return _redis.GetDatabase().HashKeys(setKey).ToStringArray();
        }

        public List<Models.HashEntry> HashScan(string setKey, string hashFieldPattern)
        {
            _logger.LogDebug("Getting {SetKey} entries matching {HashFieldPattern}", setKey, hashFieldPattern);
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
            _logger.LogDebug("Checking if {Value} is member of {SetKey}", value, setKey);
            return _redis.GetDatabase().SetContains(setKey, value);
        }

        public string[] Members(string setKey)
        {
            _logger.LogDebug("Getting {SetKey} members", setKey);
            return _redis.GetDatabase().SetMembers(setKey).ToStringArray();
        }

        public string[] RandomMembers(string setKey, int count)
        {
            _logger.LogDebug("Getting {Count} random members from {SetKey}", count, setKey);
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
                    if (setEntry.SortedSetNames != null)
                    {
                        foreach (var setName in setEntry.SortedSetNames)
                        {
                            if (string.IsNullOrEmpty(setName)) continue;
                            transaction.SetAddAsync(setEntry.SetPrefix, setName);
                            count++;
                        }
                    }
                    foreach (var setName in Members(setEntry.SetPrefix))
                    {
                        if (setEntry.SortedSetNames == null || Array.BinarySearch(setEntry.SortedSetNames, setName) < 0)
                        {
                            transaction.SetRemoveAsync(setEntry.SetPrefix.Append(setName), setEntry.Value);
                            count++;
                        }
                    }
                    if (setEntry.SortedSetNames != null)
                    {
                        foreach (var setName in setEntry.SortedSetNames)
                        {
                            if (string.IsNullOrEmpty(setName)) continue;
                            transaction.SetAddAsync(setEntry.SetPrefix.Append(setName), setEntry.Value);
                            count++;
                        }
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
            _logger.LogDebug("Executing transaction with {Count} commands");
            transaction.Execute();
        }

        public string Combine(CombineOptions key)
        {
            var keys = new RedisKey[key.SetKeys.Length];
            for (var i = 0; i < keys.Length; i++) keys[i] = key.SetKeys[i];
            var setOp = key.Op == LogicalOperator.And ? SetOperation.Intersect : SetOperation.Union;
            _logger.LogDebug("Running {Operation} into {Destination}", setOp.ToString(), key.Destination);
            _redis.GetDatabase().SetCombineAndStore(setOp, key.Destination, keys);
            return key.Destination;
        }

        public bool CanConnect()
        {
            _logger.LogDebug("Checking if redis is connected");
            return _redis.IsConnected;
        }
    }
}
