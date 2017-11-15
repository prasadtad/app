using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RecipeShelf.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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

        public Task DeleteAsync(string filename)
        {
            _logger.LogDebug("Deleting {Filename}", filename);
            File.Delete(Path.Combine(_settings.LocalFileProxyFolder, filename));
            return Task.CompletedTask;
        }

        public async Task<FileText> GetTextAsync(string filename, DateTime? since = null)
        {
            if (since == null)
                _logger.LogDebug("Reading {Filename} as text", filename);
            else
                _logger.LogDebug("Reading {Filename} as text if changed after {Since}", filename, since.Value);
            var path = Path.Combine(_settings.LocalFileProxyFolder, filename);
            var lastWriteTime = File.GetLastWriteTime(path);
            if (since == null || lastWriteTime > since.Value)
                return new FileText(await File.ReadAllTextAsync(path), lastWriteTime);
            return new FileText(null, lastWriteTime);
        }        

        public Task<IEnumerable<string>> ListKeysAsync(string folder)
        {
            _logger.LogDebug("Listing files in {Folder}", folder);
            return Task.FromResult(Directory.EnumerateFileSystemEntries(Path.Combine(_settings.LocalFileProxyFolder, folder)));
        }

        public Task PutTextAsync(string filename, string text)
        {
            _logger.LogDebug("Saving text at {Filename}", filename);
            return File.WriteAllTextAsync(Path.Combine(_settings.LocalFileProxyFolder, filename), text);
        }
    }
}
