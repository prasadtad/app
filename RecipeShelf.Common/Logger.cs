using System;
using System.Diagnostics;

namespace RecipeShelf.Common
{
    public enum LogLevels
    {
        Trace,
        Debug,
        Information,
        Error
    }

    public interface ILogger
    {
        void Error(string function, string message);

        void Information(string function, string message);

        void Debug(string function, string message);

        void Trace(string function, string message);

        void Duration(string function, string message, Stopwatch sw);
    }

    public sealed class Logger<T> : ILogger
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

        public void Duration(string function, string message, Stopwatch sw)
        {
            sw.Stop();
            WriteLine(LogLevels.Debug, function, message + " took ", sw.ElapsedMilliseconds + "ms");
        }

        private void WriteLine(LogLevels level, string function, string message, string suffix = "")
        {
            if (Settings.LogLevel > level) return;
            var orig = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(_prefix);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(function);
            Console.Write(": ");
            Console.ForegroundColor = orig;
            Console.Write(message);
            if (!string.IsNullOrEmpty(suffix))
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write(suffix);
                Console.ForegroundColor = orig;
            }
            Console.WriteLine();
        }
    }
}
