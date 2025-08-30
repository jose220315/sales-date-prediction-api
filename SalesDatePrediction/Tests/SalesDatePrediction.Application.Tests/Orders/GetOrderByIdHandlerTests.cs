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
    public class GetOrderByIdHandlerTests
    {
        [Fact]
        public async Task Handle_Deberia_RetornarOrdenCompleta_YMapearCorrectamenteLosDatos_CuandoOrdenExiste()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IOrderReadPort>();

            var detallesSimulados = new List<OrderDetailRead>
            {
                new OrderDetailRead(
                    ProductId: 1,
                    ProductName: "Laptop HP EliteBook",
                    UnitPrice: 1200.50m,
                    Qty: 2,
                    Discount: 0.10m
                ),
                new OrderDetailRead(
                    ProductId: 2,
                    ProductName: "Mouse Inalámbrico Logitech",
                    UnitPrice: 25.99m,
                    Qty: 5,
                    Discount: 0.05m
                ),
                new OrderDetailRead(
                    ProductId: 3,
                    ProductName: "Teclado Mecánico",
                    UnitPrice: 89.95m,
                    Qty: 1,
                    Discount: 0m
                )
            };

            var ordenSimulada = new OrderRead(
                OrderId: 12345,
                CustId: 42,
                EmpId: 7,
                ShipperId: 3,
                OrderDate: new DateTime(2025, 1, 15, 10, 30, 0),
                RequiredDate: new DateTime(2025, 1, 22, 16, 0, 0),
                ShippedDate: new DateTime(2025, 1, 18, 14, 45, 30),
                Freight: 35.75m,
                ShipName: "ACME Technologies S.A.S.",
                ShipAddress: "Calle 123 #45-67, Oficina 801",
                ShipCity: "Bogotá",
                ShipCountry: "Colombia",
                Details: detallesSimulados
            );

            int orderIdCapturado = 0;
            CancellationToken tokenCapturado = default;

            portMock
                .Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Callback<int, CancellationToken>((orderId, ct) =>
                {
                    orderIdCapturado = orderId;
                    tokenCapturado = ct;
                })
                .ReturnsAsync(ordenSimulada);

            var sut = new GetOrderByIdHandler(portMock.Object);
            var query = new GetOrderByIdQuery(OrderId: 12345);

            using var cts = new CancellationTokenSource();

            // ============ Act ============
            var resultado = await sut.Handle(query, cts.Token);

            // ========== Assert ==========
            Assert.NotNull(resultado);

            // Verificar mapeo de propiedades principales de la orden
            Assert.Equal(12345, resultado!.OrderId);
            Assert.Equal(42, resultado.CustId);
            Assert.Equal(7, resultado.EmpId);
            Assert.Equal(3, resultado.ShipperId);
            Assert.Equal(new DateTime(2025, 1, 15, 10, 30, 0), resultado.OrderDate);
            Assert.Equal(new DateTime(2025, 1, 22, 16, 0, 0), resultado.RequiredDate);
            Assert.Equal(new DateTime(2025, 1, 18, 14, 45, 30), resultado.ShippedDate);
            Assert.Equal(35.75m, resultado.Freight);
            Assert.Equal("ACME Technologies S.A.S.", resultado.ShipName);
            Assert.Equal("Calle 123 #45-67, Oficina 801", resultado.ShipAddress);
            Assert.Equal("Bogotá", resultado.ShipCity);
            Assert.Equal("Colombia", resultado.ShipCountry);

            // Verificar mapeo de detalles
            Assert.NotNull(resultado.Details);
            Assert.Equal(3, resultado.Details.Count);

            // Verificar primer detalle
            var detalle1 = resultado.Details.FirstOrDefault(d => d.ProductId == 1);
            Assert.NotNull(detalle1);
            Assert.Equal("Laptop HP EliteBook", detalle1!.ProductName);
            Assert.Equal(1200.50m, detalle1.UnitPrice);
            Assert.Equal(2, detalle1.Qty);
            Assert.Equal(0.10m, detalle1.Discount);

            // Verificar segundo detalle
            var detalle2 = resultado.Details.FirstOrDefault(d => d.ProductId == 2);
            Assert.NotNull(detalle2);
            Assert.Equal("Mouse Inalámbrico Logitech", detalle2!.ProductName);
            Assert.Equal(25.99m, detalle2.UnitPrice);
            Assert.Equal(5, detalle2.Qty);
            Assert.Equal(0.05m, detalle2.Discount);

            // Verificar tercer detalle
            var detalle3 = resultado.Details.FirstOrDefault(d => d.ProductId == 3);
            Assert.NotNull(detalle3);
            Assert.Equal("Teclado Mecánico", detalle3!.ProductName);
            Assert.Equal(89.95m, detalle3.UnitPrice);
            Assert.Equal(1, detalle3.Qty);
            Assert.Equal(0m, detalle3.Discount);

            // Verificar que se pasaron los parámetros correctos al puerto
            Assert.Equal(12345, orderIdCapturado);
            Assert.Equal(cts.Token, tokenCapturado);

            portMock.Verify(p => p.GetByIdAsync(12345, cts.Token), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_RetornarNull_CuandoOrdenNoExiste()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IOrderReadPort>();

            portMock
                .Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((OrderRead?)null);

            var sut = new GetOrderByIdHandler(portMock.Object);
            var query = new GetOrderByIdQuery(OrderId: 99999);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.Null(resultado);

            portMock.Verify(p => p.GetByIdAsync(99999, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarOrdenSinDetalles_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IOrderReadPort>();

            var ordenSinDetalles = new OrderRead(
                OrderId: 500,
                CustId: null, // Cliente null
                EmpId: 1,
                ShipperId: 2,
                OrderDate: new DateTime(2025, 2, 1),
                RequiredDate: new DateTime(2025, 2, 8),
                ShippedDate: null, // Sin fecha de envío
                Freight: 0m,
                ShipName: "Orden Sin Detalles",
                ShipAddress: "Dirección Temporal",
                ShipCity: "Medellín",
                ShipCountry: "Colombia",
                Details: new List<OrderDetailRead>() // Lista vacía
            );

            portMock
                .Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenSinDetalles);

            var sut = new GetOrderByIdHandler(portMock.Object);
            var query = new GetOrderByIdQuery(OrderId: 500);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal(500, resultado!.OrderId);
            Assert.Null(resultado.CustId);
            Assert.Equal(1, resultado.EmpId);
            Assert.Equal(2, resultado.ShipperId);
            Assert.Null(resultado.ShippedDate);
            Assert.Equal(0m, resultado.Freight);

            // Verificar detalles vacíos
            Assert.NotNull(resultado.Details);
            Assert.Empty(resultado.Details);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarOrdenConFechasExtremas_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IOrderReadPort>();

            var ordenConFechasExtremas = new OrderRead(
                OrderId: 777,
                CustId: 100,
                EmpId: 5,
                ShipperId: 1,
                OrderDate: DateTime.MinValue,
                RequiredDate: DateTime.MaxValue,
                ShippedDate: new DateTime(2025, 6, 15, 23, 59, 59, 999),
                Freight: 999.99m,
                ShipName: "Empresa Fechas Extremas",
                ShipAddress: "Calle de las Fechas Límite",
                ShipCity: "Cali",
                ShipCountry: "Colombia",
                Details: new List<OrderDetailRead>
                {
                    new OrderDetailRead(
                        ProductId: 999,
                        ProductName: "Producto Extremo",
                        UnitPrice: decimal.MaxValue,
                        Qty: short.MaxValue,
                        Discount: 0.999999m
                    )
                }
            );

            portMock
                .Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenConFechasExtremas);

            var sut = new GetOrderByIdHandler(portMock.Object);
            var query = new GetOrderByIdQuery(OrderId: 777);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal(DateTime.MinValue, resultado!.OrderDate);
            Assert.Equal(DateTime.MaxValue, resultado.RequiredDate);
            Assert.Equal(new DateTime(2025, 6, 15, 23, 59, 59, 999), resultado.ShippedDate);
            Assert.Equal(999.99m, resultado.Freight);

            // Verificar detalle con valores extremos
            Assert.Single(resultado.Details);
            var detalle = resultado.Details.First();
            Assert.Equal(999, detalle.ProductId);
            Assert.Equal("Producto Extremo", detalle.ProductName);
            Assert.Equal(decimal.MaxValue, detalle.UnitPrice);
            Assert.Equal(short.MaxValue, detalle.Qty);
            Assert.Equal(0.999999m, detalle.Discount);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarDatosDeEnvioComplejos_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IOrderReadPort>();

            var detallesConNombresComplejos = new List<OrderDetailRead>
            {
                new OrderDetailRead(
                    ProductId: 10,
                    ProductName: "Ñandú Electrónico & Cía. - Versión 2.0",
                    UnitPrice: 123.45m,
                    Qty: 3,
                    Discount: 0.15m
                ),
                new OrderDetailRead(
                    ProductId: 11,
                    ProductName: "O'Connor's Special Product (Édition Française)",
                    UnitPrice: 67.89m,
                    Qty: 1,
                    Discount: 0m
                ),
                new OrderDetailRead(
                    ProductId: 12,
                    ProductName: "", // Nombre vacío
                    UnitPrice: 1.00m,
                    Qty: 10,
                    Discount: 1.0m // 100% descuento
                )
            };

            var ordenConDatosComplejos = new OrderRead(
                OrderId: 888,
                CustId: 55,
                EmpId: 8,
                ShipperId: 4,
                OrderDate: new DateTime(2025, 3, 15),
                RequiredDate: new DateTime(2025, 3, 22),
                ShippedDate: new DateTime(2025, 3, 20),
                Freight: 12.34m,
                ShipName: "Empresa de Acentúes & Símbolos S.A.S.",
                ShipAddress: "Avenida José María Córdoba #123-456, Apto. 789B",
                ShipCity: "São Paulo",
                ShipCountry: "Brasil",
                Details: detallesConNombresComplejos
            );

            portMock
                .Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenConDatosComplejos);

            var sut = new GetOrderByIdHandler(portMock.Object);
            var query = new GetOrderByIdQuery(OrderId: 888);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal("Empresa de Acentúes & Símbolos S.A.S.", resultado!.ShipName);
            Assert.Equal("Avenida José María Córdoba #123-456, Apto. 789B", resultado.ShipAddress);
            Assert.Equal("São Paulo", resultado.ShipCity);
            Assert.Equal("Brasil", resultado.ShipCountry);

            // Verificar mapeo de productos con nombres complejos
            Assert.Equal(3, resultado.Details.Count);
            
            var productoConAcentos = resultado.Details.First(d => d.ProductId == 10);
            Assert.Equal("Ñandú Electrónico & Cía. - Versión 2.0", productoConAcentos.ProductName);

            var productoConApostrofe = resultado.Details.First(d => d.ProductId == 11);
            Assert.Equal("O'Connor's Special Product (Édition Française)", productoConApostrofe.ProductName);

            var productoSinNombre = resultado.Details.First(d => d.ProductId == 12);
            Assert.Equal("", productoSinNombre.ProductName);
            Assert.Equal(1.0m, productoSinNombre.Discount);
        }

        [Fact]
        public async Task Handle_Deberia_PropagarExcepcionDelPuerto_CuandoGetByIdAsyncFalla()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IOrderReadPort>();

            portMock
                .Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Error de conexión a base de datos"));

            var sut = new GetOrderByIdHandler(portMock.Object);
            var query = new GetOrderByIdQuery(OrderId: 123);

            // ============ Act & Assert ============
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.Handle(query, CancellationToken.None));

            Assert.Equal("Error de conexión a base de datos", ex.Message);
            portMock.Verify(p => p.GetByIdAsync(123, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_PropagarTimeoutException_CuandoOperacionEsLenta()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IOrderReadPort>();

            portMock
                .Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TimeoutException("La consulta de la orden tardó demasiado"));

            var sut = new GetOrderByIdHandler(portMock.Object);
            var query = new GetOrderByIdQuery(OrderId: 456);

            // ============ Act & Assert ============
            var ex = await Assert.ThrowsAsync<TimeoutException>(() =>
                sut.Handle(query, CancellationToken.None));

            Assert.Equal("La consulta de la orden tardó demasiado", ex.Message);
            portMock.Verify(p => p.GetByIdAsync(456, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarOrdenConUnSoloDetalle_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IOrderReadPort>();

            var ordenConUnDetalle = new OrderRead(
                OrderId: 1001,
                CustId: 25,
                EmpId: 3,
                ShipperId: 6,
                OrderDate: new DateTime(2025, 4, 10, 8, 15, 0),
                RequiredDate: new DateTime(2025, 4, 17, 17, 30, 0),
                ShippedDate: new DateTime(2025, 4, 12, 9, 45, 15),
                Freight: 5.99m,
                ShipName: "Cliente Individual",
                ShipAddress: "Calle Simple 123",
                ShipCity: "Cartagena",
                ShipCountry: "Colombia",
                Details: new List<OrderDetailRead>
                {
                    new OrderDetailRead(
                        ProductId: 200,
                        ProductName: "Producto Único Premium",
                        UnitPrice: 299.99m,
                        Qty: 1,
                        Discount: 0.20m
                    )
                }
            );

            portMock
                .Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenConUnDetalle);

            var sut = new GetOrderByIdHandler(portMock.Object);
            var query = new GetOrderByIdQuery(OrderId: 1001);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal(1001, resultado!.OrderId);
            Assert.Single(resultado.Details);

            var unicoDetalle = resultado.Details.First();
            Assert.Equal(200, unicoDetalle.ProductId);
            Assert.Equal("Producto Único Premium", unicoDetalle.ProductName);
            Assert.Equal(299.99m, unicoDetalle.UnitPrice);
            Assert.Equal(1, unicoDetalle.Qty);
            Assert.Equal(0.20m, unicoDetalle.Discount);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarConversionDeShortAInt_EnCantidadDeDetalle()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IOrderReadPort>();

            // OrderDetailRead usa short para Qty, pero OrderDetailReadDto usa int
            var detalleConShort = new List<OrderDetailRead>
            {
                new OrderDetailRead(
                    ProductId: 300,
                    ProductName: "Producto con Cantidad Short",
                    UnitPrice: 50.00m,
                    Qty: (short)32767, // Valor máximo de short
                    Discount: 0m
                )
            };

            var ordenConDetalleShort = new OrderRead(
                OrderId: 2000,
                CustId: 10,
                EmpId: 2,
                ShipperId: 1,
                OrderDate: new DateTime(2025, 5, 1),
                RequiredDate: new DateTime(2025, 5, 8),
                ShippedDate: null,
                Freight: 15.00m,
                ShipName: "Test Short to Int",
                ShipAddress: "Dirección Test",
                ShipCity: "Bucaramanga",
                ShipCountry: "Colombia",
                Details: detalleConShort
            );

            portMock
                .Setup(p => p.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ordenConDetalleShort);

            var sut = new GetOrderByIdHandler(portMock.Object);
            var query = new GetOrderByIdQuery(OrderId: 2000);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Single(resultado!.Details);

            var detalle = resultado.Details.First();
            Assert.Equal(32767, detalle.Qty); // Se convirtió correctamente de short a int
            Assert.Equal(300, detalle.ProductId);
            Assert.Equal("Producto con Cantidad Short", detalle.ProductName);
        }
    }
}