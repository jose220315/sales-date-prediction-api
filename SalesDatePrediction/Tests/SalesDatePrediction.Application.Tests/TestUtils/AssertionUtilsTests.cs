using SalesDatePrediction.Domain.Common.Pagination;
using Xunit;

namespace SalesDatePrediction.Application.Tests.TestUtils
{
    public class AssertionUtilsTests
    {
        [Fact]
        public void AssertPaginationResponse_Should_PassWhenAllValuesMatch()
        {
            // Arrange
            var response = new PaginationResponse<string>
            {
                Data = new List<string> { "Item1", "Item2" },
                TotalPages = 5,
                TotalRows = 10
            };

            // Act & Assert
            AssertionUtils.AssertPaginationResponse(response, 2, 5, 10);
        }

        [Fact]
        public void AssertPaginationResponse_Should_FailWhenDataIsNull()
        {
            // Arrange
            var response = new PaginationResponse<string>
            {
                Data = null!,
                TotalPages = 1,
                TotalRows = 0
            };

            // Act & Assert
            Assert.Throws<Xunit.Sdk.NotNullException>(() =>
                AssertionUtils.AssertPaginationResponse(response, 0, 1, 0));
        }

        [Fact]
        public void AssertPaginationResponse_Should_FailWhenResponseIsNull()
        {
            // Act & Assert
            Assert.Throws<Xunit.Sdk.NotNullException>(() =>
                AssertionUtils.AssertPaginationResponse<string>(null!, 0, 1, 0));
        }

        [Fact]
        public void AssertContainsIds_Should_PassWhenAllIdsArePresent()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem(1, "Item1"),
                new TestItem(2, "Item2"),
                new TestItem(3, "Item3")
            };

            // Act & Assert
            AssertionUtils.AssertContainsIds(items, x => x.Id, 1, 2, 3);
        }

        [Fact]
        public void AssertContainsIds_Should_FailWhenIdIsMissing()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem(1, "Item1"),
                new TestItem(2, "Item2")
            };

            // Act & Assert
            Assert.Throws<Xunit.Sdk.ContainsException>(() =>
                AssertionUtils.AssertContainsIds(items, x => x.Id, 1, 2, 3));
        }

        [Fact]
        public void AssertContainsSpecialCharacters_Should_PassWhenAllCharactersArePresent()
        {
            // Arrange
            var text = "Hello @world #test $money";

            // Act & Assert
            AssertionUtils.AssertContainsSpecialCharacters(text, "@", "#", "$");
        }

        [Fact]
        public void AssertContainsSpecialCharacters_Should_FailWhenCharacterIsMissing()
        {
            // Arrange
            var text = "Hello world";

            // Act & Assert
            Assert.Throws<Xunit.Sdk.ContainsException>(() =>
                AssertionUtils.AssertContainsSpecialCharacters(text, "@"));
        }

        [Fact]
        public void AssertContainsSpecialCharacters_Should_FailWhenTextIsNull()
        {
            // Act & Assert
            Assert.Throws<Xunit.Sdk.NotNullException>(() =>
                AssertionUtils.AssertContainsSpecialCharacters(null!, "@"));
        }

        [Fact]
        public void AssertDateTimeWithinTolerance_Should_PassWhenWithinTolerance()
        {
            // Arrange
            var expected = new DateTime(2024, 1, 1, 12, 0, 0);
            var actual = new DateTime(2024, 1, 1, 12, 0, 1);
            var tolerance = TimeSpan.FromMinutes(1);

            // Act & Assert
            AssertionUtils.AssertDateTimeWithinTolerance(expected, actual, tolerance);
        }

        [Fact]
        public void AssertDateTimeWithinTolerance_Should_FailWhenOutsideTolerance()
        {
            // Arrange
            var expected = new DateTime(2024, 1, 1, 12, 0, 0);
            var actual = new DateTime(2024, 1, 1, 12, 2, 0);
            var tolerance = TimeSpan.FromMinutes(1);

            // Act & Assert
            Assert.Throws<Xunit.Sdk.TrueException>(() =>
                AssertionUtils.AssertDateTimeWithinTolerance(expected, actual, tolerance));
        }

        [Fact]
        public void AssertIsOrdered_Should_PassWhenAscendingOrder()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem(1, "A"),
                new TestItem(2, "B"),
                new TestItem(3, "C")
            };

            // Act & Assert
            AssertionUtils.AssertIsOrdered(items, x => x.Id, true);
        }

        [Fact]
        public void AssertIsOrdered_Should_PassWhenDescendingOrder()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem(3, "C"),
                new TestItem(2, "B"),
                new TestItem(1, "A")
            };

            // Act & Assert
            AssertionUtils.AssertIsOrdered(items, x => x.Id, false);
        }

        [Fact]
        public void AssertIsOrdered_Should_FailWhenNotAscending()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem(1, "A"),
                new TestItem(3, "C"),
                new TestItem(2, "B")
            };

            // Act & Assert
            Assert.Throws<Xunit.Sdk.TrueException>(() =>
                AssertionUtils.AssertIsOrdered(items, x => x.Id, true));
        }

        [Fact]
        public void AssertIsOrdered_Should_FailWhenNotDescending()
        {
            // Arrange
            var items = new List<TestItem>
            {
                new TestItem(3, "C"),
                new TestItem(1, "A"),
                new TestItem(2, "B")
            };

            // Act & Assert
            Assert.Throws<Xunit.Sdk.TrueException>(() =>
                AssertionUtils.AssertIsOrdered(items, x => x.Id, false));
        }

        [Fact]
        public void AssertIsOrdered_Should_PassWhenSingleItem()
        {
            // Arrange
            var items = new List<TestItem> { new TestItem(1, "A") };

            // Act & Assert
            AssertionUtils.AssertIsOrdered(items, x => x.Id, true);
        }

        [Fact]
        public void AssertIsOrdered_Should_PassWhenEmptyCollection()
        {
            // Arrange
            var items = new List<TestItem>();

            // Act & Assert
            AssertionUtils.AssertIsOrdered(items, x => x.Id, true);
        }

        [Fact]
        public void AssertAll_Should_PassWhenAllItemsSatisfyCondition()
        {
            // Arrange
            var items = new List<int> { 2, 4, 6, 8 };

            // Act & Assert
            AssertionUtils.AssertAll(items, x => x % 2 == 0, "All numbers should be even");
        }

        [Fact]
        public void AssertAll_Should_FailWhenSomeItemsDoNotSatisfyCondition()
        {
            // Arrange
            var items = new List<int> { 2, 3, 6, 8 };

            // Act & Assert
            Assert.Throws<Xunit.Sdk.TrueException>(() =>
                AssertionUtils.AssertAll(items, x => x % 2 == 0, "All numbers should be even"));
        }

        [Fact]
        public void AssertAll_Should_UseDefaultMessageWhenNotProvided()
        {
            // Arrange
            var items = new List<int> { 1, 3, 5 };

            // Act & Assert
            var exception = Assert.Throws<Xunit.Sdk.TrueException>(() =>
                AssertionUtils.AssertAll(items, x => x % 2 == 0));
            
            Assert.Contains("Not all items satisfy the condition", exception.Message);
        }

        [Fact]
        public void AssertMapping_Should_PassWhenAllPropertiesMatch()
        {
            // Arrange
            var source = new TestItem(1, "Test");
            var dto = new TestItemDto { Id = 1, Name = "Test" };

            // Act & Assert
            AssertionUtils.AssertMapping(
                source, dto,
                (s => s.Id, d => d.Id, "Id"),
                (s => s.Name, d => d.Name, "Name")
            );
        }

        [Fact]
        public void AssertMapping_Should_FailWhenPropertyMismatch()
        {
            // Arrange
            var source = new TestItem(1, "Test");
            var dto = new TestItemDto { Id = 2, Name = "Test" };

            // Act & Assert
            Assert.Throws<Xunit.Sdk.EqualException>(() =>
                AssertionUtils.AssertMapping(
                    source, dto,
                    (s => s.Id, d => d.Id, "Id")
                ));
        }

        [Fact]
        public void AssertMapping_Should_FailWhenSourceIsNull()
        {
            // Arrange
            var dto = new TestItemDto { Id = 1, Name = "Test" };

            // Act & Assert
            Assert.Throws<Xunit.Sdk.NotNullException>(() =>
                AssertionUtils.AssertMapping<TestItem, TestItemDto>(
                    null!, dto,
                    (s => s.Id, d => d.Id, "Id")
                ));
        }

        [Fact]
        public void AssertMapping_Should_FailWhenDtoIsNull()
        {
            // Arrange
            var source = new TestItem(1, "Test");

            // Act & Assert
            Assert.Throws<Xunit.Sdk.NotNullException>(() =>
                AssertionUtils.AssertMapping(
                    source, (TestItemDto)null!,
                    (s => s.Id, d => d.Id, "Id")
                ));
        }

        private class TestItem
        {
            public TestItem(int id, string name)
            {
                Id = id;
                Name = name;
            }

            public int Id { get; }
            public string Name { get; }
        }

        private class TestItemDto
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}