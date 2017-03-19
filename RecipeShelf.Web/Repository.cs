using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RecipeShelf.Common.Proxies;
using RecipeShelf.Data.Proxies;
using RecipeShelf.Data.Server.Proxies;
using RecipeShelf.Data.VPC;
using System;
using System.Threading.Tasks;

namespace RecipeShelf.Web
{
    public interface IRepository
    {
        Task<RepositoryResponse<string[]>> AllIdsAsync();

        Task<RepositoryResponse<bool>> DeleteAsync(string id);
    }

    public abstract class Repository : IRepository
    {
        protected readonly Cache Cache;
        protected readonly ILogger Logger;
        protected readonly IFileProxy FileProxy;
        protected readonly INoSqlDbProxy NoSqlDbProxy;
        protected readonly IMarkdownProxy MarkdownProxy;

        protected Repository(ILogger logger, IFileProxy fileProxy, INoSqlDbProxy noSqlDbProxy, IMarkdownProxy markdownProxy, Cache cache)
        {
            Logger = logger;
            FileProxy = fileProxy;
            NoSqlDbProxy = noSqlDbProxy;
            MarkdownProxy = markdownProxy;
            Cache = cache;
        }

        public Task<RepositoryResponse<string[]>> AllIdsAsync()
        {
            return ExecuteAsync(Cache.All, "Cannot get all " + Cache.Table, Sources.Cache);            
        }

        public Task<RepositoryResponse<bool>> DeleteAsync(string id)
        {
            return ExecuteAsync(() => TryDeleteAsync(id), "Cannot delete " + Cache.Table + " " + id, Sources.All);
        }

        protected abstract Task<RepositoryResponse<bool>> TryDeleteAsync(string id);

        protected Task<RepositoryResponse<T>> ExecuteAsync<T>(Func<T> func, string error, Sources sources)
        {
            return ExecuteAsync(new Lazy<T>(func), error, sources);
        }

        protected Task<RepositoryResponse<T>> ExecuteAsync<T>(Func<Task<T>> func, string error, Sources sources)
        {
            return ExecuteAsync(new Lazy<Task<T>>(func), error, sources);
        }

        protected Task<RepositoryResponse<T>> ExecuteAsync<T>(Func<RepositoryResponse<T>> func, string error, Sources sources)
        {
            return ExecuteAsync(new Lazy<RepositoryResponse<T>>(func), error, sources);
        }

        protected Task<RepositoryResponse<T>> ExecuteAsync<T>(Func<Task<RepositoryResponse<T>>> func, string error, Sources sources)
        {
            return ExecuteAsync(new Lazy<Task<RepositoryResponse<T>>>(func), error, sources);
        }

        protected async Task<RepositoryResponse<T>> ExecuteAsync<T>(Lazy<Task<T>> data, string error, Sources sources)
        {
            var connectError = await CanConnectAsync(error, sources);
            if (!string.IsNullOrEmpty(connectError)) return new RepositoryResponse<T>(error: connectError);
            try
            {
                return new RepositoryResponse<T>(response: await data.Value);
            }
            catch (Exception ex)
            {
                return DefaultExceptionHandler<T>(ex, error);
            }
        }

        protected async Task<RepositoryResponse<T>> ExecuteAsync<T>(Lazy<Task<RepositoryResponse<T>>> data, string error, Sources sources)
        {
            var connectError = await CanConnectAsync(error, sources);
            if (!string.IsNullOrEmpty(connectError)) return new RepositoryResponse<T>(error: connectError);
            try
            {
                return await data.Value;
            }
            catch (Exception ex)
            {
                return DefaultExceptionHandler<T>(ex, error);
            }
        }

        protected async Task<RepositoryResponse<T>> ExecuteAsync<T>(Lazy<T> data, string error, Sources sources)
        {
            var connectError = await CanConnectAsync(error, sources);
            if (!string.IsNullOrEmpty(connectError)) return new RepositoryResponse<T>(error: connectError);
            try
            {
                return new RepositoryResponse<T>(response: data.Value);
            }
            catch (Exception ex)
            {
                return DefaultExceptionHandler<T>(ex, error);
            }
        }

        protected async Task<RepositoryResponse<T>> ExecuteAsync<T>(Lazy<RepositoryResponse<T>> data, string error, Sources sources)
        {
            var connectError = await CanConnectAsync(error, sources);
            if (!string.IsNullOrEmpty(connectError)) return new RepositoryResponse<T>(error: connectError);
            try
            {
                return data.Value;
            }
            catch (Exception ex)
            {
                return DefaultExceptionHandler<T>(ex, error);
            }
        }

        private async Task<string> CanConnectAsync(string error, Sources sources)
        {
            if ((sources == Sources.All || sources.HasFlag(Sources.File)) && !await FileProxy.CanConnectAsync())
            {
                Logger.LogError("Cannot connect to Files");
                return error;
            }
            if ((sources == Sources.All || sources.HasFlag(Sources.NoSql)) && !await NoSqlDbProxy.CanConnectAsync())
            {
                Logger.LogError("Cannot connect to NoSql");
                return error;
            }
            if ((sources == Sources.All || sources.HasFlag(Sources.Cache)) && !Cache.CanConnect())
            {
                Logger.LogError("Cannot connect to Cache");
                return error;
            }
            if ((sources == Sources.All || sources.HasFlag(Sources.Markdown)) && !MarkdownProxy.CanConnect())
            {
                Logger.LogError("Cannot connect to Markdown");
                return error;
            }
            return string.Empty;
        }

        protected RepositoryResponse<T> DefaultExceptionHandler<T>(Exception ex, string error)
        {
            Logger.LogCritical(ex.Message + " - {StackTrace}", ex.StackTrace);
            return new RepositoryResponse<T>(error: error);
        }
    }

    [Flags]
    public enum Sources
    {
        All = 0,
        Cache = 1,
        File = 2,
        NoSql = 4,
        Markdown = 8
    }

    public struct RepositoryResponse<T>
    {
        public readonly T Response;

        public readonly string Error;

        public RepositoryResponse(T response)
        {
            Response = response;
            Error = null;
        }

        public RepositoryResponse(string error)
        {
            Response = default(T);
            Error = error;
        }

        public IActionResult ToActionResult()
        {
            if (Error != null) return new ObjectResult(Error) { StatusCode = StatusCodes.Status500InternalServerError };
            return new JsonResult(Response);
        }
    }
}
