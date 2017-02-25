using RecipeShelf.Common;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System;

namespace RecipeShelf.Common.Proxies
{
    public sealed class LocalFileProxy : IFileProxy
    {
        private readonly Logger<LocalFileProxy> _logger = new Logger<LocalFileProxy>();

        public Task<bool> CanConnectAsync()
        {
            return Task.FromResult(true);
        }

        public async Task<string> GetTextAsync(string key)
        {
            _logger.Debug("GetText", $"Getting {key} as text");
            using (var reader = File.OpenText(Path.Combine(Settings.LocalFileProxyFolder, key)))
                return await reader.ReadToEndAsync();
        }

        public Task<IEnumerable<string>> ListKeysAsync(string folder)
        {
            _logger.Debug("ListKeys", $"Listing keys for {folder}");
            return Task.FromResult(Directory.EnumerateFileSystemEntries(Path.Combine(Settings.LocalFileProxyFolder, folder)));
        }

        public async Task PutTextAsync(string key, string text)
        {
            _logger.Debug("PutText", $"Putting text at {key}");
            using (var writer = File.CreateText(Path.Combine(Settings.LocalFileProxyFolder, key)))
                await writer.WriteAsync(text);
        }
    }
}
