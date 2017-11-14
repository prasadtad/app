using RecipeShelf.Common;
using Xunit;

namespace RecipeShelf.Tests.Common
{
    public class ExtensionsTests
    {
        [Fact]
        public void EqualsAllTest()
        {
            var a = new[] { "a", "b", "c" };
            var b = new[] { "a", "b" };
            var c = new[] { "a", "b", "c", "d" };
            var d = new[] { "a", "c", "d" };
            var e = new[] { "d", "b", "d" };
            var f = new[] { "a", "b", "c" };

            Assert.False(a.EqualsAll(b));
            Assert.False(a.EqualsAll(c));
            Assert.False(a.EqualsAll(d));
            Assert.False(a.EqualsAll(e));
            Assert.True(a.EqualsAll(f));
        }

        [Fact]
        public void GetHashCodeTest()
        {
            Assert.Equal(Extensions.GetHashCode(1, new string[] { "b", "c" }), Extensions.GetHashCode(1, new string[] { "b", "c" }));
            Assert.NotEqual(Extensions.GetHashCode(1, new string[] { "b", "c" }), Extensions.GetHashCode(1, new string[] { "a", "c" }));
            Assert.NotEqual(Extensions.GetHashCode(1, new string[] { "b", "c" }), Extensions.GetHashCode(2, new string[] { "b", "c" }));
        }
    }
}
