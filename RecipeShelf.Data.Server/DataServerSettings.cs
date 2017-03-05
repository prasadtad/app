namespace RecipeShelf.Data.Server
{
    public sealed class DataServerSettings
    {
        public bool UseLocalQueue { get; set; }

        public string SQSUrlPrefix { get; set; }

        public string MarkdownRoot { get; set; }

        public string MarkdownFolder { get; set; }

        public bool CommitAndPush { get; set; }
    }
}
