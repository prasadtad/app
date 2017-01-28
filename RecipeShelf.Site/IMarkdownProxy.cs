using RecipeShelf.Common.Models;
using System.Threading.Tasks;

namespace RecipeShelf.Site
{
    public interface IMarkdownProxy
    {
        Task PutRecipeAsync(Recipe recipe);
    }
}
