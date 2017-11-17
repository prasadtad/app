using RecipeShelf.Common;
using System;
using Xunit;

namespace RecipeShelf.Tests.Common
{
    public class ExtensionsTests
    {
        [Fact]
        public void DescribeTest()
        {
            var ts = TimeSpan.FromMilliseconds(200);
            Assert.Equal("200 milliseconds", ts.Describe());
            ts = TimeSpan.FromSeconds(1.6);
            Assert.Equal("1 second and 600 milliseconds", ts.Describe());
            ts = TimeSpan.FromSeconds(2.6);
            Assert.Equal("2 seconds and 600 milliseconds", ts.Describe());
            ts = TimeSpan.FromMinutes(1.89);
            Assert.Equal("1 minute, 53 seconds and 400 milliseconds", ts.Describe());
            ts = TimeSpan.FromMinutes(3.89);
            Assert.Equal("3 minutes, 53 seconds and 400 milliseconds", ts.Describe());
            ts = TimeSpan.FromHours(1.24574);
            Assert.Equal("1 hour, 14 minutes, 44 seconds and 664 milliseconds", ts.Describe());
            ts = TimeSpan.FromHours(5.24574);
            Assert.Equal("5 hours, 14 minutes, 44 seconds and 664 milliseconds", ts.Describe());
            ts = TimeSpan.FromDays(1.2457443);
            Assert.Equal("1 day, 5 hours, 53 minutes, 52 seconds and 308 milliseconds", ts.Describe());
            ts = TimeSpan.FromDays(26.2457443);
            Assert.Equal("26 days, 5 hours, 53 minutes, 52 seconds and 308 milliseconds", ts.Describe());
        }

        [Fact]
        public void ToStringsTest()
        {
            Assert.Collection(new[] { MockEnum.A, MockEnum.B }.ToStrings(), a => Assert.Equal("A", a), b => Assert.Equal("B", b));
            Assert.Collection(new[] { MockEnum2.Z, MockEnum2.X }.ToStrings(), z => Assert.Equal("Z", z), x => Assert.Equal("X", x));
        }

        [Fact]
        public void GetHashCodeTest()
        {
            Assert.Equal(Extensions.GetHashCode(1, new string[] { "b", "c" }), Extensions.GetHashCode(1, new string[] { "b", "c" }));
            Assert.NotEqual(Extensions.GetHashCode(1, new string[] { "b", "c" }), Extensions.GetHashCode(1, new string[] { "a", "c" }));
            Assert.NotEqual(Extensions.GetHashCode(1, new string[] { "b", "c" }), Extensions.GetHashCode(2, new string[] { "b", "c" }));
        }

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

        enum MockEnum
        {
            A,
            B
        }

        enum MockEnum2
        {
            X = 0,
            Y = 1,
            Z = 2
        }
    }
}
