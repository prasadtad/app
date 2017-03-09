using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RecipeShelf.Common;
using RecipeShelf.Common.Models;
using RecipeShelf.Common.Proxies;
using RecipeShelf.Data.Proxies;
using RecipeShelf.Data.VPC;
using RecipeShelf.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RecipeShelf.Site
{
    public interface IIngredientRepository
    {
        AllResponse All();

        Task<Ingredient> GetAsync(string id);

        Task<CreateResponse> CreateAsync(Ingredient ingredient);

        Task<string> UpdateAsync(string id, Ingredient ingredient);

        Task<string> DeleteAsync(string id);
    }

    public class IngredientRepository : IIngredientRepository
    {
        private readonly IngredientCache _ingredientCache;
        private readonly ILogger _logger;
        private readonly IFileProxy _fileProxy;
        private readonly INoSqlDbProxy _noSqlDbProxy;

        protected IngredientRepository(ILogger logger, IFileProxy fileProxy, INoSqlDbProxy noSqlDbProxy, IngredientCache ingredientCache)
        {
            _logger = logger;
            _fileProxy = fileProxy;
            _noSqlDbProxy = noSqlDbProxy;
            _ingredientCache = ingredientCache;
        }

        public AllResponse All()
        {
            const string Error = "Cannot get all ingredients";
            if (!_ingredientCache.CanConnect())
            {
                _logger.LogError("Cannot connect to cache");
                return new AllResponse(Error);
            }
            try
            {
                return new AllResponse(_ingredientCache.All());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + " - {StackTrace}", ex.StackTrace);
                return new AllResponse(Error);
            }
        }

        public async Task<CreateResponse> CreateAsync(Ingredient ingredient)
        {
            const string Error = "Cannot create ingredient";
            if (!await _fileProxy.CanConnectAsync())
            {
                _logger.LogError("Cannot connect to Files");
                return new CreateResponse(Error);
            }
            if (!await _noSqlDbProxy.CanConnectAsync())
            {
                _logger.LogError("Cannot connect to NoSql");
                return new CreateResponse(Error);
            }
            if (!_ingredientCache.CanConnect())
            {
                _logger.LogError("Cannot connect to cache");
                return new CreateResponse(Error);
            }
            ingredient.Id = Helper.GenerateNewId();
            if (!Update(ingredient)) return new CreateResponse(Error);
            return new CreateResponse(null, ingredient.Id);
        }

        public async Task<bool> Delete(string id)
        {
            try
            {
                _ingredientCache.Remove(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + " - {StackTrace}", ex.StackTrace);
                return false;
            }
            try
            {
                _noSqlDbProxy.DeleteIngredientAsync(id);
            }
        }

        public void Execute()
        {
            var ingredient = JsonConvert.DeserializeObject<Ingredient>(value);
            ingredient.Id = Helper.GenerateNewId();
            await FileProxy.PutTextAsync("ingredients/" + ingredient.Id, value);
            await NoSqlDbProxy.PutIngredientAsync(ingredient);
            _ingredientCache.Store(ingredient);
            return ingredient.Id;
        }

        public Task<Ingredient> Get(string id)
        {
            throw new NotImplementedException();
        }

        public void Update(string id, Ingredient ingredient)
        {

        }

        private bool Update(Ingredient ingredient)
        {
            try
            {
                await _fileProxy.PutTextAsync("ingredients/" + ingredient.Id, JsonConvert.SerializeObject(ingredient, Formatting.Indented));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + " - {StackTrace}", ex.StackTrace);
                return new CreateResponse(Error);
            }
            try
            {
                await _noSqlDbProxy.PutIngredientAsync(ingredient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + " - {StackTrace}", ex.StackTrace);
                return new CreateResponse(Error);
            }

            _ingredientCache.Store(ingredient);
            return ingredient.Id;
        }
    }
}
