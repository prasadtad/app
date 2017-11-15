using RecipeShelf.Common.Proxies;
using System;

namespace RecipeShelf.Common
{
    public sealed class CommonSettings
    {        
        public FileProxyTypes FileProxyType { get; set; }

        public string LocalFileProxyFolder { get; set; }

        public string S3FileProxyBucket { get; set; }
        
        public TimeSpan IngredientsCacheExpiration { get; set; }
    }
}
