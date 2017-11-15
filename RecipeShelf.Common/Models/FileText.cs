using System;

namespace RecipeShelf.Common.Models
{
    public struct FileText
    {
        public string Text { get; }

        public DateTime LastModified { get; }

        public FileText(string text, DateTime lastModified)
        {
            Text = text;
            LastModified = lastModified;
        }
    }
}
