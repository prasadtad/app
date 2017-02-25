namespace RecipeShelf.Data.Server
{
    public static class Settings
    {
        public static bool UseLocalQueue = Common.Settings.GetValue<bool>("USE_LOCAL_QUEUE");

        public static string SQSUrlPrefix = Common.Settings.GetValue<string>("SQS_URL_PREFIX");

        public static string MarkdownRoot = Common.Settings.GetValue<string>("MARKDOWN_ROOT");

        public static string MarkdownFolder = Common.Settings.GetValue<string>("MARKDOWN_FOLDER");

        public static bool CommitAndPush = Common.Settings.GetValue<bool>("COMMIT_AND_PUSH");
    }
}
