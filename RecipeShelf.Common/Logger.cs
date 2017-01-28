using System;

namespace RecipeShelf.Common
{
    public enum LogLevels
    {
        Trace,
        Debug,
        Information,
        Error
    }

    public sealed class Logger<T>
    {
        private readonly string _prefix = typeof(T) + ".";

        public void Error(string function, string message)
        {
            WriteLine(LogLevels.Error, function, message);
        }

        public void Information(string function, string message)
        {
            WriteLine(LogLevels.Information, function, message);
        }

        public void Debug(string function, string message)
        {
            WriteLine(LogLevels.Debug, function, message);
        }

        public void Trace(string function, string message)
        {
            WriteLine(LogLevels.Trace, function, message);
        }

        private void WriteLine(LogLevels level, string function, string message)
        {
            if (Settings.LogLevel > level) return;
            var orig = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(_prefix);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(function);
            Console.Write(": ");
            Console.ForegroundColor = orig;
            Console.WriteLine(message);
        }
    }
}
