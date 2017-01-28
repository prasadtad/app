namespace RecipeShelf.NoSql
{
    public static class Settings
    {
        public static bool UseLocalDynamoDB = Common.Settings.GetEnumValue<bool>("UseLocalDynamoDB");
    }
}
