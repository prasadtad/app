using RecipeShelf.Data.Proxies;

namespace RecipeShelf.Data
{
    public static class Settings
    {
        public static bool UseLocalDynamoDB = Common.Settings.GetValue<bool>("USE_LOCAL_DYNAMODB");
    }
}
