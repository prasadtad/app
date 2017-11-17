using RecipeShelf.Common;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RecipeShelf.Tests.Common
{
    public class HelperTests
    {
        [Fact]
        public void GenerateNewIdTest()
        {
            var ids = new List<string>();
            for (var i = 0; i < 1000; i++)
            {
                var id = Helper.GenerateNewId();
                Assert.Equal(8, id.Length);
                Assert.DoesNotContain("/", id);
                Assert.DoesNotContain("+", id);
                Assert.DoesNotContain("=", id);
                ids.Add(id);
            }
            Assert.Equal(ids.Count(), ids.Distinct().Count());
        }
    }
}
