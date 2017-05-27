using RecipeShelf.Common.Proxies;

namespace RecipeShelf.Common
{
    public sealed class CommonSettings
    {        
        public FileProxyTypes FileProxyType { get; set; }

        public string LocalFileProxyFolder { get; set; }

        public string S3FileProxyBucket { get; set; }

        public bool UseLocalDynamoDB { get; set; }
    }
}
