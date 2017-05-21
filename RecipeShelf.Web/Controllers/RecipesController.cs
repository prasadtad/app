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

        // GET api/ingredients
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return (await _recipeRepository.AllIdsAsync()).ToActionResult();            
        }

        // GET api/ingredients/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            return (await _recipeRepository.GetAsync(id)).ToActionResult();
        }

        // POST api/ingredients
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Recipe recipe)
        {
            return (await _recipeRepository.CreateAsync(recipe)).ToActionResult();
        }

        // PUT api/ingredients/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody]Recipe recipe)
        {
            return (await _recipeRepository.UpdateAsync(id, recipe)).ToActionResult();            
        }

        // DELETE api/ingredients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            return (await _recipeRepository.DeleteAsync(id)).ToActionResult();
        }        
    }
}
