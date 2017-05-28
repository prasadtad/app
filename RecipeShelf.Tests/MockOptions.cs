using Microsoft.Extensions.Options;

namespace RecipeShelf.Tests
{
    public class MockOptions<T> : IOptions<T> where T : class, new()
    {
        private T _settings;

        public MockOptions(T settings)
        {
            _settings = settings;
        }

        public T Value => _settings;
    }
}
