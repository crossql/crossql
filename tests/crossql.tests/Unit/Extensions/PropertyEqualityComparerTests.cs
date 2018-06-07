using System.Collections.Generic;
using System.Linq;
using crossql.Extensions;
using NUnit.Framework;

namespace crossql.tests.Unit.Extensions
{
    [TestFixture]
    public class PropertyEqualityComparerTests
    {
        [SetUp]
        public void SetUp()
        {
            _oldList = new List<Foo>
                {
                    new Foo {Bar = "bar one"},
                    new Foo {Bar = "bar two"},
                    new Foo {Bar = "bar three"},
                    new Foo {Bar = "bar four"}
                };

            _newList = new List<Foo>
                {
                    new Foo {Bar = "bar two"},
                    new Foo {Bar = "bar three"},
                    new Foo {Bar = "bar four"},
                    new Foo {Bar = "bar five"}
                };
        }

        public class Foo
        {
            public string Bar { get; set; }
            public bool IsDeleted { get; set; }
        }

        private IList<Foo> _oldList;
        private IList<Foo> _newList;


        [Test]
        public void ShouldExcludeNewListFromOldList()
        {
            // Setup
            const string expectedBar = "bar one";

            // Execute
            var updatedFoo = _oldList.Exclude(_newList, foo => foo.Bar);

            // Assert
            Assert.AreEqual(updatedFoo.First().Bar, expectedBar);
        }


        [Test]
        public void ShouldExcludeOldListFromNewList()
        {
            // Setup
            const string expectedBar = "bar five";

            // Execute
            var updatedFoo = _newList.Exclude(_oldList, foo => foo.Bar);

            // Assert
            Assert.AreEqual(updatedFoo.First().Bar, expectedBar);
        }

        [Test]
        public void ShouldNotExludeAnythingWhenComparingAListToAnEmptyList()
        {
            // Setup
            var expectedList = new List<Foo>
                {
                    new Foo {Bar = "bar two"},
                    new Foo {Bar = "bar three"},
                    new Foo {Bar = "bar four"},
                    new Foo {Bar = "bar five"}
                };

            // Execute
            var actualList = expectedList.Exclude(new List<Foo>(), foo => foo.Bar);

            // Assert
            Assert.AreEqual(expectedList.Count, actualList.Count());
        }
    }
}