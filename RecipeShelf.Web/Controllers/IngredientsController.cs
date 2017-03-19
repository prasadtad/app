using Microsoft.AspNetCore.Mvc;
using RecipeShelf.Common.Models;
using System.Threading.Tasks;

namespace RecipeShelf.Web.Controllers
{
    [Route("api/[controller]")]
    public class IngredientsController : Controller
    {
        private readonly IIngredientRepository _ingredientRepository;

        public IngredientsController(IIngredientRepository ingredientRepository)
        {
            _ingredientRepository = ingredientRepository;
        }

        // GET api/ingredients
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return (await _ingredientRepository.AllIdsAsync()).ToActionResult();            
        }

        // GET api/ingredients/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            return (await _ingredientRepository.GetAsync(id)).ToActionResult();
        }

        // POST api/ingredients
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Ingredient ingredient)
        {
            return (await _ingredientRepository.CreateAsync(ingredient)).ToActionResult();
        }

        // PUT api/ingredients/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody]Ingredient ingredient)
        {
            return (await _ingredientRepository.UpdateAsync(id, ingredient)).ToActionResult();            
        }

        // DELETE api/ingredients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            return (await _ingredientRepository.DeleteAsync(id)).ToActionResult();
        }        
    }
}
