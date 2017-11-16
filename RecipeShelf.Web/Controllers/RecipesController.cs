using Microsoft.AspNetCore.Mvc;
using RecipeShelf.Common.Models;
using System.Threading.Tasks;

namespace RecipeShelf.Web.Controllers
{
    [Route("[controller]")]
    public class RecipesController : Controller
    {
        private readonly IRecipeRepository _recipeRepository;

        public RecipesController(IRecipeRepository recipeRepository)
        {
            _recipeRepository = recipeRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Recipe recipe)
        {
            return (await _recipeRepository.CreateAsync(recipe)).ToActionResult();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody]Recipe recipe)
        {
            return (await _recipeRepository.UpdateAsync(id, recipe)).ToActionResult();            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            return (await _recipeRepository.DeleteAsync(id)).ToActionResult();
        }

        [HttpGet("resetcache")]
        public async Task<IActionResult> ResetCache()
        {
            return (await _recipeRepository.ResetCacheAsync()).ToActionResult();
        }

        [HttpGet("searchnames")]
        public async Task<IActionResult> SearchNames([FromQuery] string sentence)
        {
            return (await _recipeRepository.SearchNamesAsync(sentence)).ToActionResult();
        }
    }
}
