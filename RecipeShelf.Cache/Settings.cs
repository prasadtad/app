namespace RecipeShelf.Cache
{
    public static class Settings
    {
        public static string CacheEndpoint = Common.Settings.GetValue<string>("CacheEndpoint");
    }
}
