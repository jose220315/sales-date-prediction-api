using SalesDatePrediction.Domain.Common.Pagination;
using Xunit;

namespace SalesDatePrediction.Application.Tests.TestUtils
{
    public class PaginationTestUtilsTests
    {
        [Fact]
        public void CreateParams_Should_CreateWithDefaultValues()
        {
            // Act
            var result = PaginationTestUtils.CreateParams();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(10, result.PageSize);
        }

        [Fact]
        public void CreateParams_Should_CreateWithCustomValues()
        {
            // Act
            var result = PaginationTestUtils.CreateParams(3, 20);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.PageNumber);
            Assert.Equal(20, result.PageSize);
        }

        [Fact]
        public void CreateResponse_Should_CreateResponseWithData()
        {
            // Arrange
            var testData = new List<string> { "Item1", "Item2", "Item3" };

            // Act
            var result = PaginationTestUtils.CreateResponse(testData, 2, 15);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Equal(3, result.Data.Count);
            Assert.Equal("Item1", result.Data[0]);
            Assert.Equal("Item2", result.Data[1]);
            Assert.Equal("Item3", result.Data[2]);
            Assert.Equal(2, result.TotalPages);
            Assert.Equal(15, result.TotalRows);
        }

        [Fact]
        public void CreateResponse_Should_CreateResponseWithEmptyData()
        {
            // Arrange
            var emptyData = new List<int>();

            // Act
            var result = PaginationTestUtils.CreateResponse(emptyData, 0, 0);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalPages);
            Assert.Equal(0, result.TotalRows);
        }

        [Fact]
        public void CreateEmptyResponse_Should_CreateEmptyResponse()
        {
            // Act
            var result = PaginationTestUtils.CreateEmptyResponse<string>();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.TotalPages);
            Assert.Equal(0, result.TotalRows);
        }

        [Fact]
        public void CreateEmptyResponse_Should_WorkWithDifferentTypes()
        {
            // Act
            var stringResult = PaginationTestUtils.CreateEmptyResponse<string>();
            var intResult = PaginationTestUtils.CreateEmptyResponse<int>();
            var objectResult = PaginationTestUtils.CreateEmptyResponse<object>();

            // Assert
            Assert.IsType<PaginationResponse<string>>(stringResult);
            Assert.IsType<PaginationResponse<int>>(intResult);
            Assert.IsType<PaginationResponse<object>>(objectResult);
        }
    }
}