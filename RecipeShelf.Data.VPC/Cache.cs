using RecipeShelf.Data.VPC.Models;
using RecipeShelf.Data.VPC.Proxies;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace RecipeShelf.Data.VPC
{
    public abstract class Cache
    {
        public abstract string Table { get; }

        protected abstract string NamesKey { get; }

        protected abstract string SearchWordsKey { get; }

        protected abstract string LocksKey { get; }

        protected readonly ICacheProxy CacheProxy;

        protected readonly ILogger Logger;

        protected Cache(ICacheProxy cacheProxy, ILogger logger)
        {
            CacheProxy = cacheProxy;
            Logger = logger;
        }

        public bool CanConnect()
        {
            return CacheProxy.CanConnect();
        }

        public async Task<bool> TryLockAsync(string id)
        {
            var key = LocksKey.Append(id);
            if (bool.TrueString == await CacheProxy.GetStringAsync(key))
                return false;
            await CacheProxy.SetStringAsync(key, bool.TrueString, TimeSpan.FromMinutes(2));
            return true;
        }

        public Task UnLockAsync(string id)
        {
            return CacheProxy.SetStringAsync(LocksKey.Append(id), bool.FalseString, TimeSpan.FromMinutes(2));
        }

        public async Task<bool> ExistsAsync(string id) => !string.IsNullOrEmpty(await CacheProxy.GetAsync(NamesKey, id));

        public async Task<IEnumerable<string>> AllAsync() => await CacheProxy.HashFieldsAsync(NamesKey);

        protected async Task<IEnumerable<IEntry>> CreateSearchPhrasesAsync(string id, string[] oldNames, string[] newNames)
        {
            var entries = new Dictionary<string, IEntry>();
            var newPhrases = newNames.SelectMany(GeneratePhrases);

            foreach (var newPhrase in newPhrases)
            {
                var ids = (await CacheProxy.GetAsync(SearchWordsKey, newPhrase)).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (ids.Contains(id)) continue;
                ids.Add(id);
                if (!entries.ContainsKey(newPhrase))
                    entries.Add(newPhrase, new HashEntry(SearchWordsKey, newPhrase, string.Join(",", ids)));
            }

            foreach (var oldName in oldNames)
            {
                foreach (var oldPhrase in GeneratePhrases(oldName).Except(newPhrases))
                {
                    var ids = (await CacheProxy.GetAsync(SearchWordsKey, oldPhrase)).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (!ids.Contains(id)) continue;
                    ids.Remove(id);
                    entries.Add(oldPhrase, new HashEntry(SearchWordsKey, oldPhrase, string.Join(",", ids)));
                }
            }

            return entries.Values;
        }

        public IEnumerable<string> SearchNames(string sentence)
        {
            Logger.LogDebug("Searching names for {Sentence}", sentence);

            sentence = sentence.ToLower();
            var ids = new HashSet<string>();
            foreach (var pattern in GenerateKeyPatterns(sentence))
            {
                var entries = CacheProxy.HashScan(SearchWordsKey, pattern);
                foreach (var entry in entries)
                {
                    var entryIds = entry.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var id in entryIds)
                        ids.Add(id);
                }
            }

            return ids;
        }

        private string[] GeneratePhrases(string sentence)
        {
            var phrases = sentence.ToLowerCaseWords();
            for (var i = phrases.Length - 2; i >= 0; i--)
                phrases[i] = (phrases[i] + " " + phrases[i + 1]).Trim();
            return phrases;
        }

        private IEnumerable<string> GenerateKeyPatterns(string phrase)
        {
            return new[] { phrase, phrase + "*" };
        }
    }
}
