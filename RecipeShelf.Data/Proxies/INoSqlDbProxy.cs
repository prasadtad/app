using RecipeShelf.Common.Models;
using System.Threading.Tasks;

namespace RecipeShelf.Data.Proxies
{
    public interface INoSqlDbProxy
    {
        Task PutRecipeAsync(Recipe recipe);

        Task PutIngredientAsync(Ingredient ingredient);

        Task<Recipe> GetRecipeAsync(string id);

        Task<Ingredient> GetIngredientAsync(string id);

        Task DeleteIngredientAsync(string id);

        Task DeleteRecipeAsync(string id);

        Task<bool> CanConnectAsync();
    }
}
