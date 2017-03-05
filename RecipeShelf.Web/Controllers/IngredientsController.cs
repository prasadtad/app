using System.Collections.Generic;
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

namespace RecipeShelf.Web.Controllers
{
    [Route("api/[controller]")]
    public class IngredientsController : Controller
    {
        private readonly ILogger<IngredientsController> _logger;
        private readonly IFileProxy _fileProxy;
        private readonly INoSqlDbProxy _noSqlDbProxy;
        private readonly IngredientCache _ingredientCache;

        public IngredientsController(ILogger<IngredientsController> logger, IFileProxy fileProxy, INoSqlDbProxy noSqlDbProxy, IngredientCache ingredientCache)
        {
            _logger = logger;
            _fileProxy = fileProxy;
            _noSqlDbProxy = noSqlDbProxy;
            _ingredientCache = ingredientCache;
        }

        // GET api/ingredients
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return _ingredientCache.All();
        }

        // GET api/ingredients/5
        [HttpGet("{id}")]
        public async Task<Ingredient> Get(string id)
        {
            return await _noSqlDbProxy.GetIngredientAsync(id);
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        [HttpGet("Update")]
        public async Task<string> Update([FromQuery] bool cache, [FromQuery] bool noSql)
        {
            if (!cache && !noSql) return "Didn't update anything";
            Stopwatch sw = Stopwatch.StartNew();
            foreach (var key in await _fileProxy.ListKeysAsync("ingredients"))
            {
                var ingredient = JsonConvert.DeserializeObject<Ingredient>(await _fileProxy.GetTextAsync(key));
                if (noSql) await _noSqlDbProxy.PutIngredientAsync(ingredient);
                if (cache) _ingredientCache.Store(ingredient);
            }
            if (cache && noSql)
                return "Updating cache and no sql took " + sw.Elapsed.Describe();
            else if (cache)
                return "Updating cache took " + sw.Elapsed.Describe();
            return "Updating no sql took " + sw.Elapsed.Describe();
        }
    }
}
