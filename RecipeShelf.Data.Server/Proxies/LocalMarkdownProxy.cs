using System.Threading.Tasks;
using RecipeShelf.Common.Models;
using System.IO;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using RecipeShelf.Common;

namespace RecipeShelf.Data.Server.Proxies
{
    public sealed class LocalMarkdownProxy : IMarkdownProxy
    {
        private readonly ILogger<LocalMarkdownProxy> _logger;

        private readonly DataServerSettings _settings;

        public LocalMarkdownProxy(ILogger<LocalMarkdownProxy> logger, IOptions<DataServerSettings> optionsAccessor)
        {
            _logger = logger;
            _settings = optionsAccessor.Value;
        }

        public bool CanConnect()
        {
            _logger.LogDebug("Checking if {MarkdownRoot} and {MarkdownFolder} folders exist", _settings.MarkdownRoot, _settings.MarkdownFolder);
            return Directory.Exists(_settings.MarkdownRoot) && Directory.Exists(_settings.MarkdownFolder);
        }

        public async Task PutRecipeMarkdownAsync(Recipe recipe)
        {
            var markdownFile = Path.Combine(_settings.MarkdownFolder, recipe.Id + ".md");
            var markdownFileExists = File.Exists(markdownFile);
            _logger.LogDebug("Generating markdown for Recipe {Id}", recipe.Id);
            var markdown = recipe.GenerateMarkdown();
            _logger.LogDebug("Saving markdown file {markdownFile} for Recipe {Id}", markdownFile, recipe.Id);
            using (var writer = File.CreateText(markdownFile))
                await writer.WriteAsync(markdown);
            if (_settings.CommitAndPush)
            {
                if (markdownFileExists)
                    await RunGit($"commit -am \"Updated {recipe.Names[0]} recipe with Id {recipe.Id}\"");
                else
                {
                    await RunGit($"add \"{markdownFile}\"");
                    await RunGit($"commit -m \"Added {recipe.Names[0]} recipe with Id {recipe.Id}\"");
                }
                await RunGit("push");
            }
        }

        public async Task RemoveRecipeMarkdownAsync(string id)
        {
            var markdownFile = Path.Combine(_settings.MarkdownFolder, id + ".md");
            var markdownFileExists = File.Exists(markdownFile);
            if (!markdownFileExists) return;
            _logger.LogDebug("Deleting markdown file {markdownFile} for Recipe {Id}", markdownFile, id);
            File.Delete(markdownFile);
            if (_settings.CommitAndPush)
            {
                await RunGit($"commit -am \"Removed recipe with Id {id}\"");
                await RunGit("push");
            }
        }

        private async Task RunGit(string args)
        {
            using (var process = new Process
            {
                StartInfo =
                {
                    FileName = "git", Arguments = args,
                    WorkingDirectory = _settings.MarkdownFolder,
                    UseShellExecute = false, CreateNoWindow = true,
                    RedirectStandardOutput = true, RedirectStandardError = true,
                },
                EnableRaisingEvents = true
            })
            if (await process.StartAsync(_logger) != 0)
                throw new Exception("Git did not return successfully");
        }
    }
}
