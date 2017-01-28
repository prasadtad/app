using RecipeShelf.Common.Models;
using System.Threading.Tasks;

namespace RecipeShelf.NoSql
{
    public interface INoSqlDbProxy
    {
        Task PutRecipeAsync(Recipe recipe);

        Task PutIngredientAsync(Ingredient ingredient);
    }
}
