using RecipeShelf.Common.Proxies;
using System;

namespace RecipeShelf.Common
{
    public static class Settings
    {
        public static LogLevels LogLevel = GetEnumValue<LogLevels>("LOG_LEVEL");
        
        public static FileProxyTypes FileProxyType = Common.Settings.GetEnumValue<FileProxyTypes>("FILE_PROXY_TYPE");

        public static string LocalFileProxyFolder = Common.Settings.GetValue<string>("LOCAL_FILE_PROXY_FOLDER");

        public static string S3FileProxyBucket = Common.Settings.GetValue<string>("S3_FILE_PROXY_BUCKET");

        public static T GetEnumValue<T>(string variable) where T : struct, IConvertible
        {
            return (T)Enum.Parse(typeof(T), Environment.GetEnvironmentVariable(variable));
        }

        public static T GetValue<T>(string variable)
        {
            return (T)Convert.ChangeType(Environment.GetEnvironmentVariable(variable), typeof(T));
        }
    }
}
