using System.Threading.Tasks;
using RecipeShelf.Common.Models;
using RecipeShelf.Common;
using System.IO;
using System.Diagnostics;
using System;

namespace RecipeShelf.Site
{
    public sealed class LocalMarkdownProxy : IMarkdownProxy
    {
        private readonly Logger<LocalMarkdownProxy> _logger = new Logger<LocalMarkdownProxy>();

        public async Task PutRecipeAsync(Recipe recipe)
        {
            var markdownFile = Path.Combine(Settings.MarkdownFolder, recipe.Id.Value + ".md");
            var markdownFileExists = File.Exists(markdownFile);
            if (Settings.CommitAndPush)
            {
                _logger.Debug("PutRecipe", $"Checking if recipe folder exists");
                if (!Directory.Exists(Settings.MarkdownRoot))
                    throw new Exception($"{Settings.MarkdownRoot} does not exist. Setup a git repository for the website there.");
                if (!Directory.Exists(Settings.MarkdownFolder))
                    throw new Exception($"{Settings.MarkdownFolder} does not exist. The website should contain a folder to storing recipe markdown.");
            }
            _logger.Debug("PutRecipe", $"Generating markdown for {recipe.Id.Value}");
            var markdown = recipe.GenerateMarkdown();
            _logger.Debug("PutRecipe", $"Saving markdown file {markdownFile}");
            using (var writer = File.CreateText(markdownFile))
                await writer.WriteAsync(markdown);
            if (Settings.CommitAndPush)
            {
                if (markdownFileExists)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        WorkingDirectory = Settings.MarkdownFolder,
                        FileName = "git",
                        Arguments = $"commit -am \"Updated {recipe.Names[0]} recipe\""
                    });
                }
                else
                {
                    Process.Start(new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        WorkingDirectory = Settings.MarkdownFolder,
                        FileName = "git",
                        Arguments = $"add \"{markdownFile}\""
                    });
                    Process.Start(new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        WorkingDirectory = Settings.MarkdownFolder,
                        FileName = "git",
                        Arguments = $"commit -m \"Added {recipe.Names[0]} recipe\""
                    });
                }
                Process.Start(new ProcessStartInfo
                {
                    UseShellExecute = false,
                    WorkingDirectory = Settings.MarkdownRoot,
                    FileName = "git",
                    Arguments = "push"
                });
            }
        }
    }
}
