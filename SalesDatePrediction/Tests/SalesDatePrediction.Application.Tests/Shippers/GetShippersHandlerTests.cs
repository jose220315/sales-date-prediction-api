using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using SalesDatePrediction.Application.Shippers;
using SalesDatePrediction.Domain.Shippers;
using SalesDatePrediction.Domain.Shippers.Ports;
using SalesDatePrediction.Domain.Common.Pagination;

namespace SalesDatePrediction.Application.Tests.Shippers
{
    public class GetShippersHandlerTests
    {
        [Fact]
        public async Task Handle_Deberia_RetornarTodosLosShippers_CuandoPaginationParamsEsNull()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IShipperReadPort>();
            
            var shippersSimulados = new List<Shipper>
            {
                new Shipper(1, "FedEx Corporation"),
                new Shipper(2, "DHL Express"),
                new Shipper(3, "UPS - United Parcel Service"),
                new Shipper(4, "Coordinadora Mercantil S.A.")
            };

            CancellationToken tokenCapturado = default;

            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>((ct) =>
                {
                    tokenCapturado = ct;
                })
                .ReturnsAsync(shippersSimulados);

            var sut = new GetShippersHandler(portMock.Object);
            var query = new GetShippersQuery(null);
            
            using var cts = new CancellationTokenSource();

            // ============ Act ============
            var resultado = await sut.Handle(query, cts.Token);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.NotNull(resultado.Data);
            Assert.Equal(4, resultado.Data.Count);
            Assert.Equal(1, resultado.TotalPages);
            Assert.Equal(4, resultado.TotalRows);

            // Verificar que los datos se mapearon correctamente
            var shipper1 = resultado.Data.FirstOrDefault(s => s.ShipperId == 1);
            Assert.NotNull(shipper1);
            Assert.Equal("FedEx Corporation", shipper1!.CompanyName);

            var shipper2 = resultado.Data.FirstOrDefault(s => s.ShipperId == 2);
            Assert.NotNull(shipper2);
            Assert.Equal("DHL Express", shipper2!.CompanyName);

            var shipper3 = resultado.Data.FirstOrDefault(s => s.ShipperId == 3);
            Assert.NotNull(shipper3);
            Assert.Equal("UPS - United Parcel Service", shipper3!.CompanyName);

            var shipper4 = resultado.Data.FirstOrDefault(s => s.ShipperId == 4);
            Assert.NotNull(shipper4);
            Assert.Equal("Coordinadora Mercantil S.A.", shipper4!.CompanyName);

            // Verificar que se llamó al método correcto del puerto
            Assert.Equal(cts.Token, tokenCapturado);
            portMock.Verify(p => p.GetAllAsync(cts.Token), Times.Once);
            portMock.Verify(p => p.GetPagedAsync(It.IsAny<PaginationParams>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Deberia_RetornarShippersPaginados_CuandoPaginationParamsNoEsNull()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IShipperReadPort>();
            
            var shippersPaginados = new PaginationResponse<Shipper>
            {
                Data = new List<Shipper>
                {
                    new Shipper(10, "TNT Express"),
                    new Shipper(11, "Servientrega S.A."),
                    new Shipper(12, "Interrapidísimo S.A.")
                },
                TotalPages = 2,
                TotalRows = 15
            };

            PaginationParams? paramsCapturados = null;
            CancellationToken tokenCapturado = default;

            portMock
                .Setup(p => p.GetPagedAsync(It.IsAny<PaginationParams>(), It.IsAny<CancellationToken>()))
                .Callback<PaginationParams, CancellationToken>((pp, ct) =>
                {
                    paramsCapturados = pp;
                    tokenCapturado = ct;
                })
                .ReturnsAsync(shippersPaginados);

            var sut = new GetShippersHandler(portMock.Object);
            var paginationParams = new PaginationParams { PageNumber = 2, PageSize = 10 };
            var query = new GetShippersQuery(paginationParams);
            
            using var cts = new CancellationTokenSource();

            // ============ Act ============
            var resultado = await sut.Handle(query, cts.Token);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.NotNull(resultado.Data);
            Assert.Equal(3, resultado.Data.Count);
            Assert.Equal(2, resultado.TotalPages);
            Assert.Equal(15, resultado.TotalRows);

            // Verificar mapeo de datos
            Assert.Equal(10, resultado.Data[0].ShipperId);
            Assert.Equal("TNT Express", resultado.Data[0].CompanyName);
            Assert.Equal(11, resultado.Data[1].ShipperId);
            Assert.Equal("Servientrega S.A.", resultado.Data[1].CompanyName);
            Assert.Equal(12, resultado.Data[2].ShipperId);
            Assert.Equal("Interrapidísimo S.A.", resultado.Data[2].CompanyName);

            // Verificar que se pasaron los parámetros correctos
            Assert.NotNull(paramsCapturados);
            Assert.Equal(2, paramsCapturados!.PageNumber);
            Assert.Equal(10, paramsCapturados!.PageSize);
            Assert.Equal(cts.Token, tokenCapturado);

            // Verificar que se llamó al método correcto del puerto
            portMock.Verify(p => p.GetPagedAsync(paginationParams, cts.Token), Times.Once);
            portMock.Verify(p => p.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Deberia_RetornarListaVacia_CuandoNoHayShippers()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IShipperReadPort>();
            
            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Shipper>());

            var sut = new GetShippersHandler(portMock.Object);
            var query = new GetShippersQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.NotNull(resultado.Data);
            Assert.Empty(resultado.Data);
            Assert.Equal(1, resultado.TotalPages);
            Assert.Equal(0, resultado.TotalRows);

            portMock.Verify(p => p.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_RetornarPaginacionVacia_CuandoPaginadoNoTieneShippers()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IShipperReadPort>();
            
            var respuestaPaginadaVacia = new PaginationResponse<Shipper>
            {
                Data = new List<Shipper>(),
                TotalPages = 0,
                TotalRows = 0
            };

            portMock
                .Setup(p => p.GetPagedAsync(It.IsAny<PaginationParams>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(respuestaPaginadaVacia);

            var sut = new GetShippersHandler(portMock.Object);
            var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
            var query = new GetShippersQuery(paginationParams);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.NotNull(resultado.Data);
            Assert.Empty(resultado.Data);
            Assert.Equal(0, resultado.TotalPages);
            Assert.Equal(0, resultado.TotalRows);

            portMock.Verify(p => p.GetPagedAsync(paginationParams, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarNombresDeCompaniaComplejos_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IShipperReadPort>();
            
            var shippersConNombresComplejos = new List<Shipper>
            {
                new Shipper(100, "Transportes Ñandú & Cía. S.A.S."),
                new Shipper(101, "O'Reilly's Global Shipping Ltd. (Europe)"),
                new Shipper(102, ""), // Nombre vacío
                new Shipper(103, "Empresa con símbolos: @#$%^&*()_+-=[]{}|;':\",./<>?"),
                new Shipper(104, "Envíos Rápidos™ - Soluciones Logísticas®"),
                new Shipper(105, "Jean-Pierre's Express Delivery Service Inc."),
                new Shipper(106, "Transportadora São Paulo - México & Co."),
                new Shipper(107, "24/7 Fast Shipping & More!!!"),
                new Shipper(108, "Compañía de Transportes del Atlántico Norte S.A."),
                new Shipper(109, "Express Delivery ?? - China Internacional")
            };

            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(shippersConNombresComplejos);

            var sut = new GetShippersHandler(portMock.Object);
            var query = new GetShippersQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal(10, resultado.Data.Count);

            // Verificar mapeo de nombres complejos
            var shipperConAcentos = resultado.Data.First(s => s.ShipperId == 100);
            Assert.Equal("Transportes Ñandú & Cía. S.A.S.", shipperConAcentos.CompanyName);

            var shipperConApostrofe = resultado.Data.First(s => s.ShipperId == 101);
            Assert.Equal("O'Reilly's Global Shipping Ltd. (Europe)", shipperConApostrofe.CompanyName);

            var shipperSinNombre = resultado.Data.First(s => s.ShipperId == 102);
            Assert.Equal("", shipperSinNombre.CompanyName);

            var shipperConSimbolos = resultado.Data.First(s => s.ShipperId == 103);
            Assert.Equal("Empresa con símbolos: @#$%^&*()_+-=[]{}|;':\",./<>?", shipperConSimbolos.CompanyName);

            var shipperConMarcas = resultado.Data.First(s => s.ShipperId == 104);
            Assert.Equal("Envíos Rápidos™ - Soluciones Logísticas®", shipperConMarcas.CompanyName);

            var shipperConGuion = resultado.Data.First(s => s.ShipperId == 105);
            Assert.Equal("Jean-Pierre's Express Delivery Service Inc.", shipperConGuion.CompanyName);

            var shipperInternacional = resultado.Data.First(s => s.ShipperId == 106);
            Assert.Equal("Transportadora São Paulo - México & Co.", shipperInternacional.CompanyName);

            var shipperConNumeros = resultado.Data.First(s => s.ShipperId == 107);
            Assert.Equal("24/7 Fast Shipping & More!!!", shipperConNumeros.CompanyName);

            var shipperConTilde = resultado.Data.First(s => s.ShipperId == 108);
            Assert.Equal("Compañía de Transportes del Atlántico Norte S.A.", shipperConTilde.CompanyName);

            var shipperConChino = resultado.Data.First(s => s.ShipperId == 109);
            Assert.Equal("Express Delivery ?? - China Internacional", shipperConChino.CompanyName);
        }

        [Fact]
        public async Task Handle_Deberia_PropagarExcepcionDelPuerto_CuandoGetAllAsyncFalla()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IShipperReadPort>();
            
            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Error de conexión al catálogo de transportadoras"));

            var sut = new GetShippersHandler(portMock.Object);
            var query = new GetShippersQuery(null);

            // ============ Act & Assert ============
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                sut.Handle(query, CancellationToken.None));
            
            Assert.Equal("Error de conexión al catálogo de transportadoras", ex.Message);
            portMock.Verify(p => p.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_PropagarTimeoutException_CuandoGetPagedAsyncFalla()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IShipperReadPort>();
            
            portMock
                .Setup(p => p.GetPagedAsync(It.IsAny<PaginationParams>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TimeoutException("Timeout en consulta paginada de transportadoras"));

            var sut = new GetShippersHandler(portMock.Object);
            var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
            var query = new GetShippersQuery(paginationParams);

            // ============ Act & Assert ============
            var ex = await Assert.ThrowsAsync<TimeoutException>(() => 
                sut.Handle(query, CancellationToken.None));
            
            Assert.Equal("Timeout en consulta paginada de transportadoras", ex.Message);
            portMock.Verify(p => p.GetPagedAsync(paginationParams, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarShipperUnico_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IShipperReadPort>();
            
            var shipperUnico = new List<Shipper>
            {
                new Shipper(999, "Transportadora Exclusiva Premium Internacional S.A.S.")
            };

            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(shipperUnico);

            var sut = new GetShippersHandler(portMock.Object);
            var query = new GetShippersQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Single(resultado.Data);

            var shipper = resultado.Data.First();
            Assert.Equal(999, shipper.ShipperId);
            Assert.Equal("Transportadora Exclusiva Premium Internacional S.A.S.", shipper.CompanyName);

            Assert.Equal(1, resultado.TotalPages);
            Assert.Equal(1, resultado.TotalRows);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarShippersConIdsDuplicados_CorrectamenteSinFiltrar()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IShipperReadPort>();
            
            // Simulamos un caso donde el puerto retorna datos con IDs duplicados (caso edge poco común)
            var shippersConIdsDuplicados = new List<Shipper>
            {
                new Shipper(500, "Primera Transportadora"),
                new Shipper(500, "Segunda Transportadora") // ID duplicado intencional
            };

            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(shippersConIdsDuplicados);

            var sut = new GetShippersHandler(portMock.Object);
            var query = new GetShippersQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Data.Count); // No debe filtrar duplicados

            // Verificar que ambas transportadoras están presentes
            Assert.Contains(resultado.Data, s => s.CompanyName == "Primera Transportadora");
            Assert.Contains(resultado.Data, s => s.CompanyName == "Segunda Transportadora");
            
            // Verificar que ambas tienen el mismo ShipperId
            Assert.True(resultado.Data.All(s => s.ShipperId == 500));
        }

        [Fact]
        public async Task Handle_Deberia_ManejarShippersConIdsExtremos_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IShipperReadPort>();
            
            var shippersConIdsExtremos = new List<Shipper>
            {
                new Shipper(0, "Transportadora con ID Cero"),
                new Shipper(int.MaxValue, "Transportadora con ID Máximo"),
                new Shipper(int.MinValue, "Transportadora con ID Mínimo"),
                new Shipper(-1, "Transportadora con ID Negativo")
            };

            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(shippersConIdsExtremos);

            var sut = new GetShippersHandler(portMock.Object);
            var query = new GetShippersQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal(4, resultado.Data.Count);

            // Verificar transportadoras con IDs extremos
            var shipperCero = resultado.Data.First(s => s.ShipperId == 0);
            Assert.Equal("Transportadora con ID Cero", shipperCero.CompanyName);

            var shipperMaximo = resultado.Data.First(s => s.ShipperId == int.MaxValue);
            Assert.Equal("Transportadora con ID Máximo", shipperMaximo.CompanyName);

            var shipperMinimo = resultado.Data.First(s => s.ShipperId == int.MinValue);
            Assert.Equal("Transportadora con ID Mínimo", shipperMinimo.CompanyName);

            var shipperNegativo = resultado.Data.First(s => s.ShipperId == -1);
            Assert.Equal("Transportadora con ID Negativo", shipperNegativo.CompanyName);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarNombresExtremosDeLongitud_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IShipperReadPort>();
            
            var nombreMuyLargo = new string('T', 500) + " Transportes"; // 512 caracteres
            var nombreConEspacios = "   Transportadora con espacios al inicio y final   ";
            var nombreSoloEspacios = "     ";
            var nombreConSaltoLinea = "Transportadora\ncon\nsalto\nde\nlínea";
            var nombreConTabs = "Transportadora\tcon\ttabulaciones";
            
            var shippersConNombresExtremos = new List<Shipper>
            {
                new Shipper(200, nombreMuyLargo),
                new Shipper(201, nombreConEspacios),
                new Shipper(202, nombreSoloEspacios),
                new Shipper(203, nombreConSaltoLinea),
                new Shipper(204, nombreConTabs),
                new Shipper(205, "T"), // Un solo carácter
                new Shipper(206, "DHL"), // Nombre muy corto pero común
            };

            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(shippersConNombresExtremos);

            var sut = new GetShippersHandler(portMock.Object);
            var query = new GetShippersQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal(7, resultado.Data.Count);

            // Verificar que los nombres se mantienen exactamente como vienen del dominio
            var shipperLargo = resultado.Data.First(s => s.ShipperId == 200);
            Assert.Equal(nombreMuyLargo, shipperLargo.CompanyName);
            Assert.Equal(512, shipperLargo.CompanyName.Length);

            var shipperConEspacios = resultado.Data.First(s => s.ShipperId == 201);
            Assert.Equal(nombreConEspacios, shipperConEspacios.CompanyName);

            var shipperSoloEspacios = resultado.Data.First(s => s.ShipperId == 202);
            Assert.Equal(nombreSoloEspacios, shipperSoloEspacios.CompanyName);

            var shipperConSaltos = resultado.Data.First(s => s.ShipperId == 203);
            Assert.Equal(nombreConSaltoLinea, shipperConSaltos.CompanyName);
            Assert.Contains("\n", shipperConSaltos.CompanyName);

            var shipperConTabs = resultado.Data.First(s => s.ShipperId == 204);
            Assert.Equal(nombreConTabs, shipperConTabs.CompanyName);
            Assert.Contains("\t", shipperConTabs.CompanyName);

            var shipperCorto = resultado.Data.First(s => s.ShipperId == 205);
            Assert.Equal("T", shipperCorto.CompanyName);
            Assert.Equal(1, shipperCorto.CompanyName.Length);

            var shipperDHL = resultado.Data.First(s => s.ShipperId == 206);
            Assert.Equal("DHL", shipperDHL.CompanyName);
            Assert.Equal(3, shipperDHL.CompanyName.Length);
        }
    }
}