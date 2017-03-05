using System.Threading.Tasks;
using RecipeShelf.Common.Models;
using System.IO;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
                    RunGit($"commit -am \"Updated {recipe.Names[0]} recipe\"");
                else
                {
                    RunGit($"add \"{markdownFile}\"");
                    RunGit($"commit -m \"Added {recipe.Names[0]} recipe\"");
                }
                RunGit("push");
            }
        }

        private void RunGit(string args)
        {
            var processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                WorkingDirectory = _settings.MarkdownFolder,
                FileName = "git",
                Arguments = args
            };
            _logger.LogDebug("Starting {Process}", processStartInfo.FileName + " " + processStartInfo.Arguments);
            Process.Start(processStartInfo);
        }
    }
}
