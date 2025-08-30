using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using SalesDatePrediction.Application.Orders;
using SalesDatePrediction.Domain.Orders;
using SalesDatePrediction.Domain.Orders.Ports;

namespace SalesDatePrediction.Application.Tests.Orders
{
    public class CreateOrderHandlerTests
    {
        [Fact]
        public async Task Handle_Deberia_LlamarPortConDatosExactos_YRetornarId_CuandoFechasSonExplicitas()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IOrderWritePort>();

            CreateOrder? orderCapturado = null;
            IReadOnlyList<CreateOrderDetail>? detallesCapturados = null;
            CancellationToken tokenCapturado = default;

            portMock
                .Setup(p => p.AddAsync(
                    It.IsAny<CreateOrder>(),
                    It.IsAny<IReadOnlyList<CreateOrderDetail>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<CreateOrder, IReadOnlyList<CreateOrderDetail>, CancellationToken>((o, d, ct) =>
                {
                    orderCapturado = o;
                    detallesCapturados = d;
                    tokenCapturado = ct;
                })
                .ReturnsAsync(123); // Id simulado

            var sut = new CreateOrderHandler(portMock.Object);

            var fechaOrden = new DateTime(2025, 8, 15, 10, 0, 0, DateTimeKind.Local);
            var fechaRequerida = new DateTime(2025, 8, 22, 10, 0, 0, DateTimeKind.Local);
            var fechaEnvio = new DateTime(2025, 8, 16, 8, 30, 0, DateTimeKind.Local);

            var cmd = new CreateOrderCommand(
                CustId: 42,
                EmpId: 5,
                ShipperId: 9,
                ShipName: "ACME Corp.",
                ShipAddress: "Calle 123",
                ShipCity: "Bogotá",
                ShipCountry: "Colombia",
                OrderDate: fechaOrden,
                RequiredDate: fechaRequerida,
                ShippedDate: fechaEnvio,
                Freight: 12.5m,
                Details: new[]
                {
                    new CreateOrderDetailDto(ProductId: 1, UnitPrice: 100m, Qty: 2, Discount: 0.10m),
                    new CreateOrderDetailDto(ProductId: 2, UnitPrice: null, Qty: 1, Discount: 0m),
                }
            );

            using var cts = new CancellationTokenSource();

            // ============ Act ============
            var id = await sut.Handle(cmd, cts.Token);

            // ========== Assert ==========
            Assert.Equal(123, id);

            portMock.Verify(p => p.AddAsync(
                It.IsAny<CreateOrder>(),
                It.IsAny<IReadOnlyList<CreateOrderDetail>>(),
                It.IsAny<CancellationToken>()),
                Times.Once);

            Assert.NotNull(orderCapturado);
            Assert.NotNull(detallesCapturados);

            // Pedido mapeado correctamente
            Assert.Equal(cmd.CustId, orderCapturado!.CustId);
            Assert.Equal(cmd.EmpId, orderCapturado.EmpId);
            Assert.Equal(fechaOrden, orderCapturado.OrderDate);
            Assert.Equal(fechaRequerida, orderCapturado.RequiredDate);
            Assert.Equal(fechaEnvio, orderCapturado.ShippedDate);
            Assert.Equal(cmd.ShipperId, orderCapturado.ShipperId);
            Assert.Equal(cmd.Freight, orderCapturado.Freight);
            Assert.Equal(cmd.ShipName, orderCapturado.ShipName);
            Assert.Equal(cmd.ShipAddress, orderCapturado.ShipAddress);
            Assert.Equal(cmd.ShipCity, orderCapturado.ShipCity);
            Assert.Equal(cmd.ShipCountry, orderCapturado.ShipCountry);

            // Detalles (conteo + contenido + orden)
            Assert.Equal(cmd.Details.Count, detallesCapturados!.Count);

            Assert.Equal(1, detallesCapturados[0].ProductId);
            Assert.Equal(100m, detallesCapturados[0].UnitPrice);
            Assert.Equal(2, detallesCapturados[0].Qty);
            Assert.Equal(0.10m, detallesCapturados[0].Discount);

            Assert.Equal(2, detallesCapturados[1].ProductId);
            Assert.Null(detallesCapturados[1].UnitPrice);
            Assert.Equal(1, detallesCapturados[1].Qty);
            Assert.Equal(0m, detallesCapturados[1].Discount);

            // Token propagado
            Assert.Equal(cts.Token, tokenCapturado);
        }

        [Fact]
        public async Task Handle_Deberia_UsarNowCuandoOrderDateNull_YSumar7DiasCuandoRequiredDateNull_YNoShippedDate()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IOrderWritePort>();
            CreateOrder? orderCapturado = null;

            portMock
                .Setup(p => p.AddAsync(
                    It.IsAny<CreateOrder>(),
                    It.IsAny<IReadOnlyList<CreateOrderDetail>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<CreateOrder, IReadOnlyList<CreateOrderDetail>, CancellationToken>((o, _, __) =>
                {
                    orderCapturado = o;
                })
                .ReturnsAsync(777);

            var sut = new CreateOrderHandler(portMock.Object);

            var cmd = new CreateOrderCommand(
                CustId: null,
                EmpId: 99,
                ShipperId: 3,
                ShipName: "Cliente Sin Fecha",
                ShipAddress: "Av. Principal",
                ShipCity: "Medellín",
                ShipCountry: "Colombia",
                OrderDate: null,            // -> Now
                RequiredDate: null,         // -> OrderDate + 7
                ShippedDate: null,          // -> null
                Freight: 0m,
                Details: new[]
                {
                    new CreateOrderDetailDto(ProductId: 10, UnitPrice: 50m, Qty: 5, Discount: 0.05m),
                }
            );

            var antes = DateTime.Now;
            var id = await sut.Handle(cmd, CancellationToken.None);
            var despues = DateTime.Now;

            // ========== Assert ==========
            Assert.Equal(777, id);
            Assert.NotNull(orderCapturado);

            Assert.True(orderCapturado!.OrderDate >= antes && orderCapturado!.OrderDate <= despues,
                $"OrderDate esperado entre {antes:o} y {despues:o}, fue {orderCapturado!.OrderDate:o}");

            var esperadoRequired = orderCapturado!.OrderDate.AddDays(7);
            Assert.Equal(esperadoRequired, orderCapturado!.RequiredDate);
            Assert.Null(orderCapturado!.ShippedDate);

            Assert.Null(orderCapturado!.CustId);
            Assert.Equal(cmd.EmpId, orderCapturado!.EmpId);
            Assert.Equal(cmd.ShipperId, orderCapturado!.ShipperId);
            Assert.Equal(cmd.Freight, orderCapturado!.Freight);
            Assert.Equal(cmd.ShipName, orderCapturado!.ShipName);
            Assert.Equal(cmd.ShipAddress, orderCapturado!.ShipAddress);
            Assert.Equal(cmd.ShipCity, orderCapturado!.ShipCity);
            Assert.Equal(cmd.ShipCountry, orderCapturado!.ShipCountry);
        }

        [Fact]
        public async Task Handle_Deberia_EnviarListaVaciaDeDetalles_CuandoNoHayDetalles()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IOrderWritePort>();
            IReadOnlyList<CreateOrderDetail>? detallesCapturados = null;

            portMock
                .Setup(p => p.AddAsync(
                    It.IsAny<CreateOrder>(),
                    It.IsAny<IReadOnlyList<CreateOrderDetail>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<CreateOrder, IReadOnlyList<CreateOrderDetail>, CancellationToken>((_, d, __) =>
                {
                    detallesCapturados = d;
                })
                .ReturnsAsync(1);

            var sut = new CreateOrderHandler(portMock.Object);

            var cmd = new CreateOrderCommand(
                CustId: 1,
                EmpId: 2,
                ShipperId: 3,
                ShipName: "Sin Detalles",
                ShipAddress: "Calle 1",
                ShipCity: "Cali",
                ShipCountry: "Colombia",
                OrderDate: DateTime.UtcNow,
                RequiredDate: DateTime.UtcNow.AddDays(3),
                ShippedDate: null,
                Freight: 0m,
                Details: Array.Empty<CreateOrderDetailDto>() // <- sin detalles
            );

            // ============ Act ============
            var id = await sut.Handle(cmd, CancellationToken.None);

            // ========== Assert ==========
            Assert.Equal(1, id);
            Assert.NotNull(detallesCapturados);
            Assert.Empty(detallesCapturados!);
        }

        [Fact]
        public async Task Handle_Deberia_PropagarExcepcionDelPuerto_SiAddAsyncFalla()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IOrderWritePort>();
            portMock
                .Setup(p => p.AddAsync(
                    It.IsAny<CreateOrder>(),
                    It.IsAny<IReadOnlyList<CreateOrderDetail>>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Falla en persistencia"));

            var sut = new CreateOrderHandler(portMock.Object);

            var cmd = new CreateOrderCommand(
                CustId: 7,
                EmpId: 8,
                ShipperId: 9,
                ShipName: "Cliente X",
                ShipAddress: "Dir",
                ShipCity: "City",
                ShipCountry: "CO",
                OrderDate: DateTime.UtcNow,
                RequiredDate: DateTime.UtcNow.AddDays(2),
                ShippedDate: null,
                Freight: 1m,
                Details: new[] { new CreateOrderDetailDto(1, 10m, 1, 0m) }
            );

            // ============ Act & Assert ============
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.Handle(cmd, CancellationToken.None));
            Assert.Equal("Falla en persistencia", ex.Message);
        }
    }
}