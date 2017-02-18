using RecipeShelf.Cache.Models;
using RecipeShelf.Cache.Proxies;
using RecipeShelf.Common;
using RecipeShelf.Common.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RecipeShelf.Cache
{
    public abstract class Cache
    {
        protected abstract string SearchWordsKey { get; }

        protected readonly ICacheProxy CacheProxy;

        protected readonly ILogger Logger;

        protected Cache(ICacheProxy cacheProxy, ILogger logger)
        {
            CacheProxy = cacheProxy;
            Logger = logger;
        }

        protected IEnumerable<IEntry> CreateSearchWordEntries(Id id, string oldNames, string[] names)
        {
            var newWords = names.SelectMany(Extensions.ToLowerCaseWords);

            var oldWords = oldNames.ToLowerCaseWords();

            var entries = new List<IEntry>();

            foreach (var oldWord in oldWords.Except(newWords))
            {
                var ids = CacheProxy.Get(SearchWordsKey, oldWord).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (!ids.Contains(id.Value)) continue;
                ids.Remove(id.Value);
                entries.Add(new HashEntry(SearchWordsKey, oldWord, string.Join(",", ids)));
            };

            foreach (var newWord in newWords)
            {
                var ids = CacheProxy.Get(SearchWordsKey, newWord).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (ids.Contains(id.Value)) continue;
                ids.Add(id.Value);
                entries.Add(new HashEntry(SearchWordsKey, newWord, string.Join(",", ids)));
            }

            return entries;
        }

        public Id[] Search(string sentence)
        {
            var sw = Stopwatch.StartNew();

            var idStrings = new HashSet<string>();
            foreach (var word in sentence.ToLowerCaseWords())
            {
                foreach (var pattern in GenerateKeyPatterns(word))
                {
                    var entries = CacheProxy.HashScan(SearchWordsKey, pattern);
                    foreach (var entry in entries)
                    {
                        var entryIds = entry.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var id in entryIds)
                            idStrings.Add(id);
                    }
                }
            }
            var ids = new Id[idStrings.Count];
            var i = 0;
            foreach (var id in idStrings)
            {
                ids[i] = new Id(id);
                i++;
            }

            Logger.Duration("Search", $"Finding {sentence}", sw);

            return ids;
        }

        private List<string> GenerateKeyPatterns(string word)
        {
            var patterns = new List<string>();
            patterns.Add(word);
            var chars = word.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                patterns.Add(new string(chars, 0, i) + "?" + new string(chars, i, chars.Length - i));
                patterns.Add(new string(chars, 0, i) + "?" + new string(chars, i + 1, chars.Length - i - 1));
            }
            patterns.Add(word + "?");
            return patterns;
        }
    }
}
