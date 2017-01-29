namespace RecipeShelf.NoSql
{
    public static class Settings
    {
        public static bool UseLocalDynamoDB = Common.Settings.GetValue<bool>("UseLocalDynamoDB");
    }
}
