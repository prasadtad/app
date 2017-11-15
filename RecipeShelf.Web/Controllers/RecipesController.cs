using Microsoft.AspNetCore.Mvc;
using RecipeShelf.Common.Models;
using System.Threading.Tasks;

namespace RecipeShelf.Web.Controllers
{
    [Route("api/[controller]")]
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
    }
}
