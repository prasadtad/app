using RecipeShelf.Common.Proxies;
using System;

namespace RecipeShelf.Common
{
    public static class Settings
    {
        public static LogLevels LogLevel = GetEnumValue<LogLevels>("LogLevel");

        public static FileProxyTypes FileProxyType = GetEnumValue<FileProxyTypes>("FileProxyType");

        public static string LocalFileProxyFolder = GetValue<string>("LocalFileProxyFolder");

        public static string S3FileProxyBucket = GetValue<string>("S3FileProxyBucket");

        public static bool UseLocalQueue = GetValue<bool>("UseLocalQueue");

        public static string SQSUrlPrefix = GetValue<string>("SQSUrlPrefix");

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
