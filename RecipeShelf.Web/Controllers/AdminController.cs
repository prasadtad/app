using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RecipeShelf.Common.Proxies;
using RecipeShelf.Data.Server.Proxies;
using RecipeShelf.Data.VPC;
using Newtonsoft.Json;
using RecipeShelf.Common.Models;
using RecipeShelf.Common;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace RecipeShelf.Web.Controllers
{
    [Route("api/[controller]")]
    public class AdminController : Controller
    {
        private readonly IngredientCache _ingredientCache;
        private readonly RecipeCache _recipeCache;
        private readonly ILogger _logger;
        private readonly IFileProxy _fileProxy;
        private readonly INoSqlDbProxy _noSqlDbProxy;
        private readonly IMarkdownProxy _markdownProxy;

        public AdminController(ILogger logger, IFileProxy fileProxy, INoSqlDbProxy noSqlDbProxy, IMarkdownProxy markdownProxy, IngredientCache ingredientCache, RecipeCache recipeCache)
        {
            _logger = logger;
            _fileProxy = fileProxy;
            _noSqlDbProxy = noSqlDbProxy;
            _markdownProxy = markdownProxy;
            _ingredientCache = ingredientCache;
            _recipeCache = recipeCache;
        }

        [HttpGet("UpdateIngredients")]
        public async Task<IActionResult> UpdateIngredients([FromQuery] bool cache, [FromQuery] bool noSql)
        {
            if (!cache && !noSql) return BadRequest("Didn't specify what to update - cache or noSql?");

            Stopwatch sw = Stopwatch.StartNew();
            foreach (var key in await _fileProxy.ListKeysAsync("ingredients"))
            {
                var ingredient = JsonConvert.DeserializeObject<Ingredient>(await _fileProxy.GetTextAsync(key));
                if (noSql) await _noSqlDbProxy.PutIngredientAsync(ingredient);
                if (cache) _ingredientCache.Store(ingredient);
            }
            if (cache && noSql)
                return Ok("Updating cache and no sql took " + sw.Elapsed.Describe());
            else if (cache)
                return Ok("Updating cache took " + sw.Elapsed.Describe());
            return Ok("Updating no sql took " + sw.Elapsed.Describe());
        }
    }
}
