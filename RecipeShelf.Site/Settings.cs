namespace RecipeShelf.Site
{
    public static class Settings
    {
        public static string MarkdownRoot = Common.Settings.GetValue<string>("MARKDOWN_ROOT");

        public static string MarkdownFolder = Common.Settings.GetValue<string>("MARKDOWN_FOLDER");

        public static bool CommitAndPush = Common.Settings.GetValue<bool>("COMMIT_AND_PUSH");
    }
}
