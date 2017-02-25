using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipeShelf.Common.Proxies
{
    public interface IFileProxy
    {
        Task<bool> CanConnectAsync();

        Task<IEnumerable<string>> ListKeysAsync(string folder);

        Task<string> GetTextAsync(string key);

        Task PutTextAsync(string key, string text);        
    }
}
