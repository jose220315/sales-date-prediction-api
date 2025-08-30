using Moq;
using SalesDatePrediction.Application.Shippers;
using SalesDatePrediction.Application.Tests.TestUtils;
using SalesDatePrediction.Domain.Shippers.Ports;
using Xunit;

namespace SalesDatePrediction.Application.Tests.Shippers
{

    public class GetShippersHandlerImprovedTests
    {
        [Fact]
        public async Task Handle_Should_ReturnAllShippers_WithCorrectMapping()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IShipperReadPort>();
            var shippers = ShipperTestDataBuilder.CreateInternationalShippers();
            
            portMock.Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(shippers);

            var sut = new GetShippersHandler(portMock.Object);
            var query = new GetShippersQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            // Usar utility para verificar paginación
            AssertionUtils.AssertPaginationResponse(resultado, 6, 1, 6);
            
            // Verificar que contiene los IDs esperados
            AssertionUtils.AssertContainsIds(resultado.Data, s => s.ShipperId, 1, 2, 3, 4, 5, 6);
            
            // Verificar mapeo específico
            var fedex = resultado.Data.First(s => s.ShipperId == 1);
            AssertionUtils.AssertMapping(
                shippers[0], fedex,
                (s => s.ShipperId, d => d.ShipperId, "ShipperId"),
                (s => s.CompanyName, d => d.CompanyName, "CompanyName")
            );
        }

        [Fact]
        public async Task Handle_Should_HandleComplexNames_WithSpecialCharacters()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IShipperReadPort>();
            var complexShippers = ShipperTestDataBuilder.CreateShippersWithComplexNames();
            
            portMock.Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(complexShippers);

            var sut = new GetShippersHandler(portMock.Object);
            var query = new GetShippersQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            AssertionUtils.AssertPaginationResponse(resultado, 5, 1, 5);
            
            // Verificar caracteres especiales específicos
            var shipperConAcentos = resultado.Data.First(s => s.ShipperId == 100);
            AssertionUtils.AssertContainsSpecialCharacters(shipperConAcentos.CompanyName, "Ñ", "&", ".");
            
            var shipperConChino = resultado.Data.First(s => s.ShipperId == 104);
            AssertionUtils.AssertContainsSpecialCharacters(shipperConChino.CompanyName, "??", "-");
        }

        [Theory]
        [InlineData(1, 5)]
        [InlineData(2, 10)]
        [InlineData(3, 3)]
        public async Task Handle_Should_ReturnPaginatedResults_ForVariousPageSizes(int pageNumber, int pageSize)
        {
            // ========== Arrange ==========
            var portMock = new Mock<IShipperReadPort>();
            var paginationParams = PaginationTestUtils.CreateParams(pageNumber, pageSize);
            var shippers = new ShipperTestDataBuilder().BuildMany(pageSize);
            var paginatedResponse = PaginationTestUtils.CreateResponse(shippers, 5, 25);
            
            portMock.Setup(p => p.GetPagedAsync(paginationParams, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(paginatedResponse);

            var sut = new GetShippersHandler(portMock.Object);
            var query = new GetShippersQuery(paginationParams);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            AssertionUtils.AssertPaginationResponse(resultado, pageSize, 5, 25);
            portMock.Verify(p => p.GetPagedAsync(paginationParams, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}