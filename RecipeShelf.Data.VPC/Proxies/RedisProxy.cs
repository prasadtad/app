using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RecipeShelf.Common;
using RecipeShelf.Common.Models;
using RecipeShelf.Data.VPC.Models;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            _redis = ConnectionMultiplexer.Connect(_settings.CacheEndpoint + ",allowAdmin=true");
        }

        public async Task FlushAsync()
        {
            _logger.LogDebug("Flushing database");
            var server = _redis.GetServer(_settings.CacheEndpoint);
            await server.FlushDatabaseAsync();
        }

        public async Task<string> GetStringAsync(string setKey)
        {
            _logger.LogDebug("Getting {SetKey} string", setKey);
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(setKey);
            if (value.IsNullOrEmpty) return string.Empty;
            return value;
        }

        public async Task SetStringAsync(string setKey, string value, TimeSpan? expiry = null)
        {
            if (expiry == null)
                _logger.LogDebug("Setting {SetKey} string to {Value}", setKey, value);
            else
                _logger.LogDebug("Setting {SetKey} string to {Value} with expiry {Duration}", setKey, value, expiry.Value.Describe());
            var db = _redis.GetDatabase();
            await db.StringSetAsync(setKey, value, expiry);
        }

        public Task<long> CountAsync(string setKey)
        {
            _logger.LogDebug("Getting count of {SetKey}", setKey);
            var db = _redis.GetDatabase();
            return db.SetLengthAsync(setKey);
        }

        public async Task<string> GetAsync(string setKey, string hashField)
        {
            _logger.LogDebug("Getting {HashField} value from {SetKey}", hashField, setKey);
            var value = await _redis.GetDatabase().HashGetAsync(setKey, hashField);
            if (value.IsNullOrEmpty) return string.Empty;
            return value;
        }

        public async Task<bool> GetFlagAsync(string setKey, string hashField)
        {
            _logger.LogDebug("Getting {HashField} flag from {SetKey}", hashField, setKey);
            var value = await _redis.GetDatabase().HashGetAsync(setKey, hashField);
            if (value.IsNullOrEmpty) return false;
            return value == Constants.TRUE;
        }

        public async Task<string[]> HashFieldsAsync(string setKey)
        {
            _logger.LogDebug("Getting {SetKey} hash fields", setKey);
            return (await _redis.GetDatabase().HashKeysAsync(setKey)).ToStringArray();
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

        public Task<bool> IsMemberAsync(string setKey, string value)
        {
            _logger.LogDebug("Checking if {Value} is member of {SetKey}", value, setKey);
            return _redis.GetDatabase().SetContainsAsync(setKey, value);
        }

        public async Task<string[]> MembersAsync(string setKey)
        {
            _logger.LogDebug("Getting {SetKey} members", setKey);
            return (await _redis.GetDatabase().SetMembersAsync(setKey)).ToStringArray();
        }

        public async Task<string[]> RandomMembersAsync(string setKey, int count)
        {
            _logger.LogDebug("Getting {Count} random members from {SetKey}", count, setKey);
            var db = _redis.GetDatabase();
            if (count == 1)
            {
                var value = await db.SetRandomMemberAsync(setKey);
                if (value.IsNullOrEmpty) return new string[0];
                return new string[] { value };
            }
            return (await db.SetRandomMembersAsync(setKey, count)).ToStringArray();
        }

        public async Task StoreAsync(IEnumerable<IEntry> batch)
        {
            var db = _redis.GetDatabase();
            var transaction = db.CreateTransaction();
            var transactionTasks = new List<Task<bool>>();
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
                            transactionTasks.Add(transaction.SetAddAsync(setEntry.SetPrefix, setName));
                            count++;
                        }
                    }
                    foreach (var setName in await MembersAsync(setEntry.SetPrefix))
                    {
                        if (setEntry.SortedSetNames == null || Array.BinarySearch(setEntry.SortedSetNames, setName) < 0)
                        {
                            transactionTasks.Add(transaction.SetRemoveAsync(setEntry.SetPrefix.Append(setName), setEntry.Value));
                            count++;
                        }
                    }
                    if (setEntry.SortedSetNames != null)
                    {
                        foreach (var setName in setEntry.SortedSetNames)
                        {
                            if (string.IsNullOrEmpty(setName)) continue;
                            transactionTasks.Add(transaction.SetAddAsync(setEntry.SetPrefix.Append(setName), setEntry.Value));
                            count++;
                        }
                    }
                }
                else
                {
                    var hashEntry = (Models.HashEntry)entry;
                    if (string.IsNullOrEmpty(hashEntry.Value))
                        transactionTasks.Add(transaction.HashDeleteAsync(hashEntry.SetKey, hashEntry.HashField));
                    else
                        transactionTasks.Add(transaction.HashSetAsync(hashEntry.SetKey, hashEntry.HashField, hashEntry.Value));
                    count++;
                }
            }
            _logger.LogDebug("Executing transaction with {Count} commands");
            if (await transaction.ExecuteAsync()) await Task.WhenAll(transactionTasks);
        }

        public async Task<string> CombineAsync(CombineOptions key)
        {
            var keys = new RedisKey[key.SetKeys.Length];
            for (var i = 0; i < keys.Length; i++) keys[i] = key.SetKeys[i];
            var setOp = key.Op == LogicalOperator.And ? SetOperation.Intersect : SetOperation.Union;
            _logger.LogDebug("Running {Operation} into {Destination}", setOp.ToString(), key.Destination);
            await _redis.GetDatabase().SetCombineAndStoreAsync(setOp, key.Destination, keys);
            return key.Destination;
        }

        public bool CanConnect()
        {
            _logger.LogDebug("Checking if redis is connected");
            return _redis.IsConnected;
        }
    }
}
