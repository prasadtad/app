using Microsoft.AspNetCore.Mvc;
using RecipeShelf.Common.Proxies;
using RecipeShelf.Data.Proxies;
using RecipeShelf.Data.VPC;
using RecipeShelf.Common.Models;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using RecipeShelf.Common;
using System;

namespace RecipeShelf.Web.Controllers
{
    public class IngredientsController : BaseController
    {
        private readonly IngredientCache _ingredientCache;

        public IngredientsController(ILogger<IngredientsController> logger, IFileProxy fileProxy, INoSqlDbProxy noSqlDbProxy, IngredientCache ingredientCache) : base(logger, fileProxy, noSqlDbProxy)
        {
            _ingredientCache = ingredientCache;
        }

        // GET api/ingredients
        [HttpGet]
        public IActionResult Get()
        {
            return JsonWithExceptionLogging(_ingredientCache.All);
        }

        // GET api/ingredients/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            return await JsonWithExceptionLoggingAsync(() => NoSqlDbProxy.GetIngredientAsync(id));
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]string value)
        {
            return await JsonWithExceptionLoggingAsync(async () =>
            {
                var ingredient = JsonConvert.DeserializeObject<Ingredient>(value);
                ingredient.Id = Helper.GenerateNewId();
                await FileProxy.PutTextAsync("ingredients/" + ingredient.Id, value);
                await NoSqlDbProxy.PutIngredientAsync(ingredient);
                _ingredientCache.Store(ingredient);
                return ingredient.Id;
            });
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody]Ingredient value)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("Invalid id -" + id);
            if (ingredient.Id != id) return BadRequest("Ingredient id - " + ingredient.Id + " does not match id - " + id);
            Ingredient oldIngredient = null;
            try
            {
                oldIngredient = await NoSqlDbProxy.GetIngredientAsync(id);
            }
            catch (Exception ex)
            {
                return DefaultExceptionHandler(ex);
            }
            return await JsonWithExceptionLoggingAsync(async () =>
            {
                await FileProxy.PutTextAsync("ingredients/" + ingredient.Id, value);
                await NoSqlDbProxy.PutIngredientAsync(ingredient);
                _ingredientCache.Store(ingredient);
                return ingredient.Id;
            });
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            var msg = await CheckBeforeSave();
            if (!string.IsNullOrEmpty(msg)) return msg;
        }

        [HttpGet("Update")]
        public async Task<IActionResult> Update([FromQuery] bool cache, [FromQuery] bool noSql)
        {
            if (!cache && !noSql) return BadRequest("Didn't specify what to update - cache or noSql?");

            var result = await CheckBeforeSave();
            if (result != null) return result;

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
