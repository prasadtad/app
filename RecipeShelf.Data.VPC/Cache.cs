using RecipeShelf.Data.VPC.Models;
using RecipeShelf.Data.VPC.Proxies;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using RecipeShelf.Common;

namespace RecipeShelf.Data.VPC
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
        
        public bool CanConnect()
        {
            return CacheProxy.CanConnect();
        }

        protected IEnumerable<IEntry> CreateSearchWordEntries(string id, string oldNames, string[] names)
        {
            var newWords = names.SelectMany(Extensions.ToLowerCaseWords);

            var oldWords = oldNames.ToLowerCaseWords();

            var entries = new List<IEntry>();

            foreach (var oldWord in oldWords.Except(newWords))
            {
                var ids = CacheProxy.Get(SearchWordsKey, oldWord).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (!ids.Contains(id)) continue;
                ids.Remove(id);
                entries.Add(new HashEntry(SearchWordsKey, oldWord, string.Join(",", ids)));
            };

            foreach (var newWord in newWords)
            {
                var ids = CacheProxy.Get(SearchWordsKey, newWord).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (ids.Contains(id)) continue;
                ids.Add(id);
                entries.Add(new HashEntry(SearchWordsKey, newWord, string.Join(",", ids)));
            }

            return entries;
        }

        protected IEnumerable<string> SearchNames(string sentence)
        {
            Logger.LogDebug("Searching names for {Sentence}", sentence);

            var ids = new HashSet<string>();
            foreach (var word in sentence.ToLowerCaseWords())
            {
                foreach (var pattern in GenerateKeyPatterns(word))
                {
                    var entries = CacheProxy.HashScan(SearchWordsKey, pattern);
                    foreach (var entry in entries)
                    {
                        var entryIds = entry.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var id in entryIds)
                            ids.Add(id);
                    }
                }
            }           
            
            return ids;
        }

        private HashSet<string> GenerateKeyPatterns(string word)
        {
            var patterns = new HashSet<string>();
            patterns.Add(word);
            var chars = word.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                patterns.Add(new string(chars, 0, i) + new string(chars, i + 1, chars.Length - i - 1));
                patterns.Add(new string(chars, 0, i) + "?" + new string(chars, i, chars.Length - i));
                patterns.Add(new string(chars, 0, i) + "?" + new string(chars, i + 1, chars.Length - i - 1));
            }
            patterns.Add(word + "?");
            return patterns;
        }
    }
}
