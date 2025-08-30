using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using SalesDatePrediction.Application.Orders;
using SalesDatePrediction.Domain.Orders;
using SalesDatePrediction.Domain.Orders.Ports;

namespace SalesDatePrediction.Application.Tests.Orders
{
    public class GetClientOrdersHandlerTests
    {
        [Fact]
        public async Task Handle_Deberia_RetornarOrdenesDelCliente_YMapearCorrectamenteLosDatos()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IClientOrdersReadPort>();
            
            var ordenesSimuladas = new List<ClientOrderSummary>
            {
                new ClientOrderSummary(
                    OrderId: 101,
                    RequiredDate: new DateTime(2025, 1, 15),
                    ShippedDate: new DateTime(2025, 1, 10),
                    ShipName: "ACME Corporation",
                    ShipAddress: "Calle 123 #45-67",
                    ShipCity: "Bogotá"
                ),
                new ClientOrderSummary(
                    OrderId: 102,
                    RequiredDate: new DateTime(2025, 1, 20),
                    ShippedDate: null,
                    ShipName: "Tech Solutions Ltd.",
                    ShipAddress: "Avenida Principal 890",
                    ShipCity: "Medellín"
                ),
                new ClientOrderSummary(
                    OrderId: 103,
                    RequiredDate: new DateTime(2025, 1, 25),
                    ShippedDate: new DateTime(2025, 1, 22),
                    ShipName: "Global Industries",
                    ShipAddress: "Carrera 15 #30-45",
                    ShipCity: "Cali"
                )
            };

            int customerIdCapturado = 0;
            CancellationToken tokenCapturado = default;

            portMock
                .Setup(p => p.GetByCustomerAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Callback<int, CancellationToken>((custId, ct) =>
                {
                    customerIdCapturado = custId;
                    tokenCapturado = ct;
                })
                .ReturnsAsync(ordenesSimuladas);

            var sut = new GetClientOrdersHandler(portMock.Object);
            var query = new GetClientOrdersQuery(CustomerId: 42);
            
            using var cts = new CancellationTokenSource();

            // ============ Act ============
            var resultado = await sut.Handle(query, cts.Token);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal(3, resultado.Count);

            // Verificar mapeo correcto de la primera orden
            var orden1 = resultado.FirstOrDefault(o => o.OrderId == 101);
            Assert.NotNull(orden1);
            Assert.Equal(101, orden1!.OrderId);
            Assert.Equal(new DateTime(2025, 1, 15), orden1.RequiredDate);
            Assert.Equal(new DateTime(2025, 1, 10), orden1.ShippedDate);
            Assert.Equal("ACME Corporation", orden1.ShipName);
            Assert.Equal("Calle 123 #45-67", orden1.ShipAddress);
            Assert.Equal("Bogotá", orden1.ShipCity);

            // Verificar mapeo correcto de la segunda orden (con ShippedDate null)
            var orden2 = resultado.FirstOrDefault(o => o.OrderId == 102);
            Assert.NotNull(orden2);
            Assert.Equal(102, orden2!.OrderId);
            Assert.Equal(new DateTime(2025, 1, 20), orden2.RequiredDate);
            Assert.Null(orden2.ShippedDate);
            Assert.Equal("Tech Solutions Ltd.", orden2.ShipName);
            Assert.Equal("Avenida Principal 890", orden2.ShipAddress);
            Assert.Equal("Medellín", orden2.ShipCity);

            // Verificar mapeo correcto de la tercera orden
            var orden3 = resultado.FirstOrDefault(o => o.OrderId == 103);
            Assert.NotNull(orden3);
            Assert.Equal(103, orden3!.OrderId);
            Assert.Equal(new DateTime(2025, 1, 25), orden3.RequiredDate);
            Assert.Equal(new DateTime(2025, 1, 22), orden3.ShippedDate);
            Assert.Equal("Global Industries", orden3.ShipName);
            Assert.Equal("Carrera 15 #30-45", orden3.ShipAddress);
            Assert.Equal("Cali", orden3.ShipCity);

            // Verificar que se pasaron los parámetros correctos al puerto
            Assert.Equal(42, customerIdCapturado);
            Assert.Equal(cts.Token, tokenCapturado);

            portMock.Verify(p => p.GetByCustomerAsync(42, cts.Token), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_RetornarListaVacia_CuandoClienteNoTieneOrdenes()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IClientOrdersReadPort>();
            
            portMock
                .Setup(p => p.GetByCustomerAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ClientOrderSummary>());

            var sut = new GetClientOrdersHandler(portMock.Object);
            var query = new GetClientOrdersQuery(CustomerId: 999);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Empty(resultado);

            portMock.Verify(p => p.GetByCustomerAsync(999, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarOrdenConFechasExtremas_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IClientOrdersReadPort>();
            
            var fechaMinima = DateTime.MinValue;
            var fechaMaxima = DateTime.MaxValue;
            var fechaNormal = new DateTime(2025, 6, 15, 14, 30, 0);
            
            var ordenesConFechasExtremas = new List<ClientOrderSummary>
            {
                new ClientOrderSummary(
                    OrderId: 200,
                    RequiredDate: fechaMinima,
                    ShippedDate: null,
                    ShipName: "Empresa Fecha Mínima",
                    ShipAddress: "Dirección 1",
                    ShipCity: "Ciudad 1"
                ),
                new ClientOrderSummary(
                    OrderId: 201,
                    RequiredDate: fechaMaxima,
                    ShippedDate: fechaNormal,
                    ShipName: "Empresa Fecha Máxima",
                    ShipAddress: "Dirección 2",
                    ShipCity: "Ciudad 2"
                )
            };

            portMock
                .Setup(p => p.GetByCustomerAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenesConFechasExtremas);

            var sut = new GetClientOrdersHandler(portMock.Object);
            var query = new GetClientOrdersQuery(CustomerId: 100);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count);

            // Verificar orden con fecha mínima
            var ordenFechaMin = resultado.First(o => o.OrderId == 200);
            Assert.Equal(fechaMinima, ordenFechaMin.RequiredDate);
            Assert.Null(ordenFechaMin.ShippedDate);

            // Verificar orden con fecha máxima
            var ordenFechaMax = resultado.First(o => o.OrderId == 201);
            Assert.Equal(fechaMaxima, ordenFechaMax.RequiredDate);
            Assert.Equal(fechaNormal, ordenFechaMax.ShippedDate);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarDatosDeEnvioComplejos_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IClientOrdersReadPort>();
            
            var ordenesConDatosComplejos = new List<ClientOrderSummary>
            {
                new ClientOrderSummary(
                    OrderId: 300,
                    RequiredDate: new DateTime(2025, 3, 1),
                    ShippedDate: new DateTime(2025, 2, 28),
                    ShipName: "Empresa con Ñandúes & Cía. S.A.S.",
                    ShipAddress: "Calle de los Acentos #123-45 Apt. 678B",
                    ShipCity: "Bogotá D.C."
                ),
                new ClientOrderSummary(
                    OrderId: 301,
                    RequiredDate: new DateTime(2025, 3, 5),
                    ShippedDate: null,
                    ShipName: "O'Connor & Associates Ltd.",
                    ShipAddress: "123 Main St., Suite 456",
                    ShipCity: "São Paulo"
                ),
                new ClientOrderSummary(
                    OrderId: 302,
                    RequiredDate: new DateTime(2025, 3, 10),
                    ShippedDate: new DateTime(2025, 3, 8),
                    ShipName: "",  // Nombre vacío
                    ShipAddress: "",  // Dirección vacía
                    ShipCity: "Valencia"
                )
            };

            portMock
                .Setup(p => p.GetByCustomerAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenesConDatosComplejos);

            var sut = new GetClientOrdersHandler(portMock.Object);
            var query = new GetClientOrdersQuery(CustomerId: 55);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal(3, resultado.Count);

            // Verificar mapeo de nombres y direcciones complejas
            var orden1 = resultado.First(o => o.OrderId == 300);
            Assert.Equal("Empresa con Ñandúes & Cía. S.A.S.", orden1.ShipName);
            Assert.Equal("Calle de los Acentos #123-45 Apt. 678B", orden1.ShipAddress);
            Assert.Equal("Bogotá D.C.", orden1.ShipCity);

            var orden2 = resultado.First(o => o.OrderId == 301);
            Assert.Equal("O'Connor & Associates Ltd.", orden2.ShipName);
            Assert.Equal("123 Main St., Suite 456", orden2.ShipAddress);
            Assert.Equal("São Paulo", orden2.ShipCity);

            // Verificar mapeo de cadenas vacías
            var orden3 = resultado.First(o => o.OrderId == 302);
            Assert.Equal("", orden3.ShipName);
            Assert.Equal("", orden3.ShipAddress);
            Assert.Equal("Valencia", orden3.ShipCity);
        }

        [Fact]
        public async Task Handle_Deberia_PropagarExcepcionDelPuerto_CuandoGetByCustomerAsyncFalla()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IClientOrdersReadPort>();
            
            portMock
                .Setup(p => p.GetByCustomerAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Error de conexión a base de datos"));

            var sut = new GetClientOrdersHandler(portMock.Object);
            var query = new GetClientOrdersQuery(CustomerId: 123);

            // ============ Act & Assert ============
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                sut.Handle(query, CancellationToken.None));
            
            Assert.Equal("Error de conexión a base de datos", ex.Message);
            portMock.Verify(p => p.GetByCustomerAsync(123, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_PropagarTimeoutException_CuandoOperacionEsLenta()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IClientOrdersReadPort>();
            
            portMock
                .Setup(p => p.GetByCustomerAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TimeoutException("La consulta de órdenes tardó demasiado"));

            var sut = new GetClientOrdersHandler(portMock.Object);
            var query = new GetClientOrdersQuery(CustomerId: 456);

            // ============ Act & Assert ============
            var ex = await Assert.ThrowsAsync<TimeoutException>(() => 
                sut.Handle(query, CancellationToken.None));
            
            Assert.Equal("La consulta de órdenes tardó demasiado", ex.Message);
            portMock.Verify(p => p.GetByCustomerAsync(456, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarOrdenesConIdsDuplicados_CorrectamenteSinFiltrar()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IClientOrdersReadPort>();
            
            // Simulamos un caso donde el puerto retorna datos con IDs duplicados (caso edge poco común)
            var ordenesConIdsDuplicados = new List<ClientOrderSummary>
            {
                new ClientOrderSummary(
                    OrderId: 500,
                    RequiredDate: new DateTime(2025, 4, 1),
                    ShippedDate: new DateTime(2025, 3, 30),
                    ShipName: "Primera Empresa",
                    ShipAddress: "Dirección A",
                    ShipCity: "Ciudad A"
                ),
                new ClientOrderSummary(
                    OrderId: 500,  // ID duplicado intencional
                    RequiredDate: new DateTime(2025, 4, 2),
                    ShippedDate: null,
                    ShipName: "Segunda Empresa",
                    ShipAddress: "Dirección B",
                    ShipCity: "Ciudad B"
                )
            };

            portMock
                .Setup(p => p.GetByCustomerAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenesConIdsDuplicados);

            var sut = new GetClientOrdersHandler(portMock.Object);
            var query = new GetClientOrdersQuery(CustomerId: 777);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count); // No debe filtrar duplicados, debe retornar ambos

            // Verificar que ambas órdenes están presentes
            Assert.Contains(resultado, o => o.ShipName == "Primera Empresa");
            Assert.Contains(resultado, o => o.ShipName == "Segunda Empresa");
            
            // Verificar que ambas tienen el mismo OrderId
            Assert.True(resultado.All(o => o.OrderId == 500));
        }

        [Fact]
        public async Task Handle_Deberia_ManejarUnaSolaOrden_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IClientOrdersReadPort>();
            
            var ordenUnica = new List<ClientOrderSummary>
            {
                new ClientOrderSummary(
                    OrderId: 999,
                    RequiredDate: new DateTime(2025, 12, 31, 23, 59, 59),
                    ShippedDate: new DateTime(2025, 12, 30, 10, 15, 30),
                    ShipName: "Última Empresa del Año",
                    ShipAddress: "Avenida Final 2025",
                    ShipCity: "Medellín"
                )
            };

            portMock
                .Setup(p => p.GetByCustomerAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenUnica);

            var sut = new GetClientOrdersHandler(portMock.Object);
            var query = new GetClientOrdersQuery(CustomerId: 1);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Single(resultado);

            var orden = resultado.First();
            Assert.Equal(999, orden.OrderId);
            Assert.Equal(new DateTime(2025, 12, 31, 23, 59, 59), orden.RequiredDate);
            Assert.Equal(new DateTime(2025, 12, 30, 10, 15, 30), orden.ShippedDate);
            Assert.Equal("Última Empresa del Año", orden.ShipName);
            Assert.Equal("Avenida Final 2025", orden.ShipAddress);
            Assert.Equal("Medellín", orden.ShipCity);
        }
    }
}