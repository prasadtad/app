using RecipeShelf.Common.Models;
using System.Threading.Tasks;

namespace RecipeShelf.Data.Proxies
{
    public interface INoSqlDbProxy
    {
        Task PutRecipeAsync(Recipe recipe);

        Task PutIngredientAsync(Ingredient ingredient);

        Task<bool> CanConnectAsync();
    }
}
