using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RecipeShelf.Common;
using RecipeShelf.Common.Models;
using RecipeShelf.Common.Proxies;
using RecipeShelf.Data.VPC;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RecipeShelf.Web.Controllers
{
    [Route("api/[controller]")]
    public class AdminController : Controller
    {
        private readonly RecipeCache _recipeCache;
        private readonly IFileProxy _fileProxy;

        public AdminController(IFileProxy fileProxy, RecipeCache recipeCache)
        {
            _fileProxy = fileProxy;
            _recipeCache = recipeCache;
        }

        [HttpGet("UpdateRecipesCache")]
        public async Task<IActionResult> UpdateRecipesCache()
        {
            Stopwatch sw = Stopwatch.StartNew();
            foreach (var key in await _fileProxy.ListKeysAsync("recipes"))
            {
                var recipe = JsonConvert.DeserializeObject<Recipe>((await _fileProxy.GetTextAsync(key)).Text);
                await _recipeCache.StoreAsync(recipe);
            }
            return Ok("Updating cache took " + sw.Elapsed.Describe());
        }
    }
}
