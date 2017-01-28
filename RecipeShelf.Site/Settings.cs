namespace RecipeShelf.Site
{
    public static class Settings
    {
        public static string MarkdownRoot = Common.Settings.GetValue<string>("MarkdownRoot");

        public static string MarkdownFolder = Common.Settings.GetValue<string>("MarkdownFolder");

        public static bool CommitAndPush = Common.Settings.GetValue<bool>("CommitAndPush");
    }
}
