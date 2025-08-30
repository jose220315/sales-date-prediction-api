using Moq;
using SalesDatePrediction.Application.Products;
using SalesDatePrediction.Application.Tests.TestUtils;
using SalesDatePrediction.Domain.Common.Pagination;
using SalesDatePrediction.Domain.Products;
using SalesDatePrediction.Domain.Products.Ports;
using Xunit;

namespace SalesDatePrediction.Application.Tests.Products
{
    public class GetProductsHandlerTests
    {
        [Fact]
        public async Task Handle_Deberia_RetornarTodosLosProductos_CuandoPaginationParamsEsNull()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IProductReadPort>();
            
            var productosSimulados = new List<Product>
            {
                new Product(1, "Laptop HP EliteBook 840"),
                new Product(2, "Mouse Inalámbrico Logitech MX Master 3"),
                new Product(3, "Teclado Mecánico Corsair K95 RGB"),
                new Product(4, "Monitor Dell UltraSharp 27\"")
            };

            CancellationToken tokenCapturado = default;

            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>((ct) =>
                {
                    tokenCapturado = ct;
                })
                .ReturnsAsync(productosSimulados);

            var sut = new GetProductsHandler(portMock.Object);
            var query = new GetProductsQuery(null);
            
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
            var producto1 = resultado.Data.FirstOrDefault(p => p.ProductId == 1);
            Assert.NotNull(producto1);
            Assert.Equal("Laptop HP EliteBook 840", producto1!.ProductName);

            var producto2 = resultado.Data.FirstOrDefault(p => p.ProductId == 2);
            Assert.NotNull(producto2);
            Assert.Equal("Mouse Inalámbrico Logitech MX Master 3", producto2!.ProductName);

            var producto3 = resultado.Data.FirstOrDefault(p => p.ProductId == 3);
            Assert.NotNull(producto3);
            Assert.Equal("Teclado Mecánico Corsair K95 RGB", producto3!.ProductName);

            var producto4 = resultado.Data.FirstOrDefault(p => p.ProductId == 4);
            Assert.NotNull(producto4);
            Assert.Equal("Monitor Dell UltraSharp 27\"", producto4!.ProductName);

            // Verificar que se llamó al método correcto del puerto
            Assert.Equal(cts.Token, tokenCapturado);
            portMock.Verify(p => p.GetAllAsync(cts.Token), Times.Once);
            portMock.Verify(p => p.GetPagedAsync(It.IsAny<PaginationParams>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Deberia_RetornarProductosPaginados_CuandoPaginationParamsNoEsNull()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IProductReadPort>();
            
            var productosPaginados = new PaginationResponse<Product>
            {
                Data = new List<Product>
                {
                    new Product(10, "iPhone 15 Pro Max"),
                    new Product(11, "Samsung Galaxy S24 Ultra"),
                    new Product(12, "Google Pixel 8 Pro")
                },
                TotalPages = 3,
                TotalRows = 25
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
                .ReturnsAsync(productosPaginados);

            var sut = new GetProductsHandler(portMock.Object);
            var paginationParams = new PaginationParams { PageNumber = 2, PageSize = 10 };
            var query = new GetProductsQuery(paginationParams);
            
            using var cts = new CancellationTokenSource();

            // ============ Act ============
            var resultado = await sut.Handle(query, cts.Token);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.NotNull(resultado.Data);
            Assert.Equal(3, resultado.Data.Count);
            Assert.Equal(3, resultado.TotalPages);
            Assert.Equal(25, resultado.TotalRows);

            // Verificar mapeo de datos
            Assert.Equal(10, resultado.Data[0].ProductId);
            Assert.Equal("iPhone 15 Pro Max", resultado.Data[0].ProductName);
            Assert.Equal(11, resultado.Data[1].ProductId);
            Assert.Equal("Samsung Galaxy S24 Ultra", resultado.Data[1].ProductName);
            Assert.Equal(12, resultado.Data[2].ProductId);
            Assert.Equal("Google Pixel 8 Pro", resultado.Data[2].ProductName);

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
        public async Task Handle_Deberia_RetornarListaVacia_CuandoNoHayProductos()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IProductReadPort>();
            
            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product>());

            var sut = new GetProductsHandler(portMock.Object);
            var query = new GetProductsQuery(null);

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
        public async Task Handle_Deberia_RetornarPaginacionVacia_CuandoPaginadoNoTieneProductos()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IProductReadPort>();
            
            var respuestaPaginadaVacia = new PaginationResponse<Product>
            {
                Data = new List<Product>(),
                TotalPages = 0,
                TotalRows = 0
            };

            portMock
                .Setup(p => p.GetPagedAsync(It.IsAny<PaginationParams>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(respuestaPaginadaVacia);

            var sut = new GetProductsHandler(portMock.Object);
            var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
            var query = new GetProductsQuery(paginationParams);

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
        public async Task Handle_Deberia_ManejarNombresDeProductoComplejos_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IProductReadPort>();
            
            var productosConNombresComplejos = new List<Product>
            {
                new Product(100, "Ñandú Electrónico Pro & Cía. - Versión 2.0"),
                new Product(101, "O'Connor's Special Gaming Mouse (Édition Française)"),
                new Product(102, ""), // Nombre vacío
                new Product(103, "Producto con símbolos: @#$%^&*()_+-=[]{}|;':\",./<>?"),
                new Product(104, "Café con Leche™ - Máquina Automática ®"),
                new Product(105, "Jean-Pierre's Ultra-Wide Monitor 49\" 4K HDR"),
                new Product(106, "Smartphone 5G con Cámara de 108MP y IA Avanzada")
            };

            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(productosConNombresComplejos);

            var sut = new GetProductsHandler(portMock.Object);
            var query = new GetProductsQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal(7, resultado.Data.Count);

            // Verificar mapeo de nombres complejos
            var productoConAcentos = resultado.Data.First(p => p.ProductId == 100);
            Assert.Equal("Ñandú Electrónico Pro & Cía. - Versión 2.0", productoConAcentos.ProductName);

            var productoConApostrofe = resultado.Data.First(p => p.ProductId == 101);
            Assert.Equal("O'Connor's Special Gaming Mouse (Édition Française)", productoConApostrofe.ProductName);

            var productoSinNombre = resultado.Data.First(p => p.ProductId == 102);
            Assert.Equal("", productoSinNombre.ProductName);

            var productoConSimbolos = resultado.Data.First(p => p.ProductId == 103);
            Assert.Equal("Producto con símbolos: @#$%^&*()_+-=[]{}|;':\",./<>?", productoConSimbolos.ProductName);

            var productoConMarcas = resultado.Data.First(p => p.ProductId == 104);
            Assert.Equal("Café con Leche™ - Máquina Automática ®", productoConMarcas.ProductName);

            var productoConGuion = resultado.Data.First(p => p.ProductId == 105);
            Assert.Equal("Jean-Pierre's Ultra-Wide Monitor 49\" 4K HDR", productoConGuion.ProductName);

            var productoTecnologico = resultado.Data.First(p => p.ProductId == 106);
            Assert.Equal("Smartphone 5G con Cámara de 108MP y IA Avanzada", productoTecnologico.ProductName);
        }

        [Fact]
        public async Task Handle_Deberia_PropagarExcepcionDelPuerto_CuandoGetAllAsyncFalla()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IProductReadPort>();
            
            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Error de conexión a catálogo de productos"));

            var sut = new GetProductsHandler(portMock.Object);
            var query = new GetProductsQuery(null);

            // ============ Act & Assert ============
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                sut.Handle(query, CancellationToken.None));
            
            Assert.Equal("Error de conexión a catálogo de productos", ex.Message);
            portMock.Verify(p => p.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_PropagarTimeoutException_CuandoGetPagedAsyncFalla()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IProductReadPort>();
            
            portMock
                .Setup(p => p.GetPagedAsync(It.IsAny<PaginationParams>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TimeoutException("Timeout en consulta paginada de productos"));

            var sut = new GetProductsHandler(portMock.Object);
            var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
            var query = new GetProductsQuery(paginationParams);

            // ============ Act & Assert ============
            var ex = await Assert.ThrowsAsync<TimeoutException>(() => 
                sut.Handle(query, CancellationToken.None));
            
            Assert.Equal("Timeout en consulta paginada de productos", ex.Message);
            portMock.Verify(p => p.GetPagedAsync(paginationParams, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarProductoUnico_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IProductReadPort>();
            
            var productoUnico = new List<Product>
            {
                new Product(999, "Producto Exclusivo Edición Limitada Premium")
            };

            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(productoUnico);

            var sut = new GetProductsHandler(portMock.Object);
            var query = new GetProductsQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Single(resultado.Data);

            var producto = resultado.Data.First();
            Assert.Equal(999, producto.ProductId);
            Assert.Equal("Producto Exclusivo Edición Limitada Premium", producto.ProductName);

            Assert.Equal(1, resultado.TotalPages);
            Assert.Equal(1, resultado.TotalRows);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarProductosConIdsDuplicados_CorrectamenteSinFiltrar()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IProductReadPort>();
            
            // Simulamos un caso donde el puerto retorna datos con IDs duplicados (caso edge poco común)
            var productosConIdsDuplicados = new List<Product>
            {
                new Product(500, "Primera Versión del Producto"),
                new Product(500, "Segunda Versión del Producto") // ID duplicado intencional
            };

            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(productosConIdsDuplicados);

            var sut = new GetProductsHandler(portMock.Object);
            var query = new GetProductsQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Data.Count); // No debe filtrar duplicados

            // Verificar que ambos productos están presentes
            Assert.Contains(resultado.Data, p => p.ProductName == "Primera Versión del Producto");
            Assert.Contains(resultado.Data, p => p.ProductName == "Segunda Versión del Producto");
            
            // Verificar que ambos tienen el mismo ProductId
            Assert.True(resultado.Data.All(p => p.ProductId == 500));
        }

        [Fact]
        public async Task Handle_Deberia_ManejarProductosConIdsExtremos_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IProductReadPort>();
            
            var productosConIdsExtremos = new List<Product>
            {
                new Product(0, "Producto con ID Cero"),
                new Product(int.MaxValue, "Producto con ID Máximo"),
                new Product(int.MinValue, "Producto con ID Mínimo"),
                new Product(-1, "Producto con ID Negativo")
            };

            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(productosConIdsExtremos);

            var sut = new GetProductsHandler(portMock.Object);
            var query = new GetProductsQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal(4, resultado.Data.Count);

            // Verificar productos con IDs extremos
            var productoCero = resultado.Data.First(p => p.ProductId == 0);
            Assert.Equal("Producto con ID Cero", productoCero.ProductName);

            var productoMaximo = resultado.Data.First(p => p.ProductId == int.MaxValue);
            Assert.Equal("Producto con ID Máximo", productoMaximo.ProductName);

            var productoMinimo = resultado.Data.First(p => p.ProductId == int.MinValue);
            Assert.Equal("Producto con ID Mínimo", productoMinimo.ProductName);

            var productoNegativo = resultado.Data.First(p => p.ProductId == -1);
            Assert.Equal("Producto con ID Negativo", productoNegativo.ProductName);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarProductosConNombresExtremosDeLongitud_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IProductReadPort>();
            
            var nombreMuyLargo = new string('A', 1000); // 1000 caracteres
            var nombreConEspacios = "   Producto con espacios al inicio y final   ";
            var nombreSoloEspacios = "     ";
            var nombreConSaltoLinea = "Producto\ncon\nsalto\nde\nlínea";
            var nombreConTabs = "Producto\tcon\ttabulaciones";
            
            var productosConNombresExtremos = new List<Product>
            {
                new Product(200, nombreMuyLargo),
                new Product(201, nombreConEspacios),
                new Product(202, nombreSoloEspacios),
                new Product(203, nombreConSaltoLinea),
                new Product(204, nombreConTabs),
                new Product(205, "A"), // Un solo carácter
            };

            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(productosConNombresExtremos);

            var sut = new GetProductsHandler(portMock.Object);
            var query = new GetProductsQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal(6, resultado.Data.Count);

            // Verificar que los nombres se mantienen exactamente como vienen del dominio
            var productoLargo = resultado.Data.First(p => p.ProductId == 200);
            Assert.Equal(nombreMuyLargo, productoLargo.ProductName);
            Assert.Equal(1000, productoLargo.ProductName.Length);

            var productoConEspacios = resultado.Data.First(p => p.ProductId == 201);
            Assert.Equal(nombreConEspacios, productoConEspacios.ProductName);

            var productoSoloEspacios = resultado.Data.First(p => p.ProductId == 202);
            Assert.Equal(nombreSoloEspacios, productoSoloEspacios.ProductName);

            var productoConSaltos = resultado.Data.First(p => p.ProductId == 203);
            Assert.Equal(nombreConSaltoLinea, productoConSaltos.ProductName);
            Assert.Contains("\n", productoConSaltos.ProductName);

            var productoConTabs = resultado.Data.First(p => p.ProductId == 204);
            Assert.Equal(nombreConTabs, productoConTabs.ProductName);
            Assert.Contains("\t", productoConTabs.ProductName);

            var productoCorto = resultado.Data.First(p => p.ProductId == 205);
            Assert.Equal("A", productoCorto.ProductName);
            Assert.Equal(1, productoCorto.ProductName.Length);
        }

        [Fact]
        public async Task Handle_Should_UseProductTestDataBuilder_ForTechProducts()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IProductReadPort>();
            
            var techProducts = ProductTestDataBuilder.CreateTechProducts();

            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(techProducts);

            var sut = new GetProductsHandler(portMock.Object);
            var query = new GetProductsQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            AssertionUtils.AssertPaginationResponse(resultado, 6, 1, 6);
            AssertionUtils.AssertContainsIds(resultado.Data, p => p.ProductId, 1, 2, 3, 4, 5, 6);

            // Verificar mapeo específico
            var laptop = resultado.Data.First(p => p.ProductId == 1);
            AssertionUtils.AssertMapping(
                techProducts[0], laptop,
                (s => s.ProductId, d => d.ProductId, "ProductId"),
                (s => s.ProductName, d => d.ProductName, "ProductName")
            );
        }

        [Theory]
        [InlineData(1, 5)]
        [InlineData(2, 10)]
        [InlineData(3, 15)]
        public async Task Handle_Should_UsePaginationUtils_ForVariousPageSizes(int pageNumber, int pageSize)
        {
            // ========== Arrange ==========
            var portMock = new Mock<IProductReadPort>();
            var paginationParams = PaginationTestUtils.CreateParams(pageNumber, pageSize);
            var products = new ProductTestDataBuilder().BuildMany(pageSize);
            var paginatedResponse = PaginationTestUtils.CreateResponse(products, 10, 100);
            
            portMock.Setup(p => p.GetPagedAsync(paginationParams, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(paginatedResponse);

            var sut = new GetProductsHandler(portMock.Object);
            var query = new GetProductsQuery(paginationParams);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            AssertionUtils.AssertPaginationResponse(resultado, pageSize, 10, 100);
            portMock.Verify(p => p.GetPagedAsync(paginationParams, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_HandleCustomBuilderProducts()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IProductReadPort>();
            
            var customProducts = new List<Product>
            {
                new ProductTestDataBuilder().WithId(1000).WithName("Custom Product 1").Build(),
                new ProductTestDataBuilder().WithId(1001).WithTechProduct().Build(),
                new ProductTestDataBuilder().WithId(1002).WithName("Special Product #123").Build()
            };

            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(customProducts);

            var sut = new GetProductsHandler(portMock.Object);
            var query = new GetProductsQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            AssertionUtils.AssertPaginationResponse(resultado, 3, 1, 3);
            AssertionUtils.AssertContainsIds(resultado.Data, p => p.ProductId, 1000, 1001, 1002);
            
            // Verify custom name
            var customProduct = resultado.Data.First(p => p.ProductId == 1000);
            Assert.Equal("Custom Product 1", customProduct.ProductName);
            
            // Verify tech product
            var techProduct = resultado.Data.First(p => p.ProductId == 1001);
            Assert.Equal("Laptop HP EliteBook 840 G9", techProduct.ProductName);
            
            // Verify special characters
            var specialProduct = resultado.Data.First(p => p.ProductId == 1002);
            AssertionUtils.AssertContainsSpecialCharacters(specialProduct.ProductName, "#");
        }
    }
}