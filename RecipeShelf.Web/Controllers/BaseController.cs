using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RecipeShelf.Common.Proxies;
using RecipeShelf.Data.Proxies;
using System;
using System.Threading.Tasks;

namespace RecipeShelf.Web.Controllers
{
    [Route("api/[controller]")]
    public abstract class BaseController : Controller
    {
        protected readonly ILogger Logger;
        protected readonly IFileProxy FileProxy;
        protected readonly INoSqlDbProxy NoSqlDbProxy;

        protected BaseController(ILogger logger, IFileProxy fileProxy, INoSqlDbProxy noSqlDbProxy)
        {
            Logger = logger;
            FileProxy = fileProxy;
            NoSqlDbProxy = noSqlDbProxy;
        }

        public IActionResult InternalServerError(object value = null)
        {
            return value != null ? StatusCode(StatusCodes.Status500InternalServerError, value) :
                    (IActionResult)StatusCode(StatusCodes.Status500InternalServerError);
        }

        public IActionResult JsonWithExceptionLogging<T>(Func<T> func)
        {
            return JsonWithExceptionLogging(new Lazy<T>(func));
        }

        public IActionResult JsonWithExceptionLogging<T>(Lazy<T> data)
        {
            try
            {
                return Json(data.Value);
            }
            catch (Exception ex)
            {
                return DefaultExceptionHandler(ex);
            }
        }

        public Task<IActionResult> JsonWithExceptionLoggingAsync<T>(Func<Task<T>> func)
        {
            return JsonWithExceptionLoggingAsync(new Lazy<Task<T>>(func));
        }

        public async Task<IActionResult> JsonWithExceptionLoggingAsync<T>(Lazy<Task<T>> data)
        {
            try
            {
                return Json(await data.Value);
            }
            catch (Exception ex)
            {
                return DefaultExceptionHandler(ex);
            }
        }

        protected IActionResult DefaultExceptionHandler(Exception ex)
        {
            Logger.LogCritical(ex.Message + " - {StackTrace}", ex.StackTrace);
            return InternalServerError();
        }
    }
}
