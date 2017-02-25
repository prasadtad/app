using RecipeShelf.Common.Models;
using System.Threading.Tasks;

namespace RecipeShelf.Data.Server.Proxies
{
    public interface IMarkdownProxy
    {
        Task PutRecipeMarkdownAsync(Recipe recipe);
    }
}
