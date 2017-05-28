using Microsoft.Extensions.Logging;
using System;

namespace RecipeShelf.Tests
{
    public class MockLogger<T> : ILogger<T>
    {
        private class Scope : IDisposable
        {
            public void Dispose()
            {                
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return new Scope();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }
    }
}
