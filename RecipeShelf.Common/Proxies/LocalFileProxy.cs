using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RecipeShelf.Common.Proxies
{
    public sealed class LocalFileProxy : IFileProxy
    {
        private readonly ILogger<LocalFileProxy> _logger;
        private readonly CommonSettings _settings;

        public LocalFileProxy(ILogger<LocalFileProxy> logger, IOptions<CommonSettings> optionsAccessor)
        {
            _logger = logger;
            _settings = optionsAccessor.Value;
        }

        public Task<bool> CanConnectAsync()
        {
            _logger.LogDebug("Checking if {Folder} exists", _settings.LocalFileProxyFolder);
            return Task.FromResult(Directory.Exists(_settings.LocalFileProxyFolder));
        }

        public async Task<string> GetTextAsync(string filename)
        {
            _logger.LogDebug("Reading {Filename} as text", filename);
            using (var reader = File.OpenText(Path.Combine(_settings.LocalFileProxyFolder, filename)))
                return await reader.ReadToEndAsync();
        }

        public Task<IEnumerable<string>> ListKeysAsync(string folder)
        {
            _logger.LogDebug("Listing files in {Folder}", folder);
            return Task.FromResult(Directory.EnumerateFileSystemEntries(Path.Combine(_settings.LocalFileProxyFolder, folder)));
        }

        public async Task PutTextAsync(string filename, string text)
        {
            _logger.LogDebug("Saving text at {Filename}", filename);
            using (var writer = File.CreateText(Path.Combine(_settings.LocalFileProxyFolder, filename)))
                await writer.WriteAsync(text);
        }
    }
}
