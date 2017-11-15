using RecipeShelf.Common.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecipeShelf.Common.Proxies
{
    public interface IFileProxy
    {
        Task<bool> CanConnectAsync();

        Task<IEnumerable<string>> ListKeysAsync(string folder);

        Task<FileText> GetTextAsync(string key, DateTime? since = null);

        Task PutTextAsync(string key, string text);

        Task DeleteAsync(string key);      
    }
}

