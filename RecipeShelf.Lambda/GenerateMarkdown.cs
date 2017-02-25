using System;
using System.Collections.Generic;
using RecipeShelf.Common.Models;
using System.Text;
using Newtonsoft.Json;
using RecipeShelf.Common.Proxies;
using System.Threading.Tasks;

namespace RecipeShelf.Data.Server
{
    public sealed class GenerateMarkdown
    {
        private readonly IFileProxy _fileProxy;

        public GenerateMarkdown(IFileProxy fileProxy)
        {
            _fileProxy = fileProxy;
        }

        public async Task ExecuteAsync(string key)
        {
            var text = await _fileProxy.GetTextAsync(key);
            var recipe = JsonConvert.DeserializeObject<Recipe>(text);

        }

        public static void Execute(Dictionary<string, Ingredient> ingredients, Dictionary<string, Recipe> recipes, Action<Recipe, string> processMarkdown)
        {
            var sb = new StringBuilder();
            foreach (var recipeId in recipes.Keys)
            {
                sb.Remove(0, sb.Length);
                var recipe = recipes[recipeId];
                
                processMarkdown(recipe, sb.ToString());
            }
        }

        
    }
}
