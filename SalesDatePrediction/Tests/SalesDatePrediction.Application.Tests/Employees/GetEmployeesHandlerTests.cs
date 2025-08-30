using Moq;
using SalesDatePrediction.Application.Employees;
using SalesDatePrediction.Application.Tests.TestUtils;
using SalesDatePrediction.Domain.Common.Pagination;
using SalesDatePrediction.Domain.Employees;
using SalesDatePrediction.Domain.Employees.Ports;
using Xunit;

namespace SalesDatePrediction.Application.Tests.Employees
{
    public class GetEmployeesHandlerTests
    {
        [Fact]
        public async Task Handle_Deberia_RetornarTodosLosEmpleados_CuandoPaginationParamsEsNull()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IEmployeeReadPort>();
            
            var empleadosSimulados = EmployeeTestDataBuilder.CreateManagementTeam();

            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(empleadosSimulados);

            var sut = new GetEmployeesHandler(portMock.Object);
            var query = new GetEmployeesQuery(null);
            
            using var cts = new CancellationTokenSource();

            // ============ Act ============
            var resultado = await sut.Handle(query, cts.Token);

            // ========== Assert ==========
            AssertionUtils.AssertPaginationResponse(resultado, 5, 1, 5);

            // Verificar que los datos se mapearon correctamente
            AssertionUtils.AssertContainsIds(resultado.Data, e => e.EmpId, 1, 2, 3, 4, 5);

            // Verificar mapeo específico
            var empleado1 = resultado.Data.First(e => e.EmpId == 1);
            AssertionUtils.AssertMapping(
                empleadosSimulados[0], empleado1,
                (s => s.EmpId, d => d.EmpId, "EmpId"),
                (s => s.FullName, d => d.FullName, "FullName")
            );

            // Verificar que se llamó al método correcto del puerto
            portMock.Verify(p => p.GetAllAsync(cts.Token), Times.Once);
            portMock.Verify(p => p.GetPagedAsync(It.IsAny<PaginationParams>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Deberia_RetornarEmpleadosPaginados_CuandoPaginationParamsNoEsNull()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IEmployeeReadPort>();
            
            var empleadosPaginados = new PaginationResponse<Employee>
            {
                Data = new List<Employee>
                {
                    new Employee(5, "Ana Martínez"),
                    new Employee(6, "Pedro Rodríguez")
                },
                TotalPages = 3,
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
                .ReturnsAsync(empleadosPaginados);

            var sut = new GetEmployeesHandler(portMock.Object);
            var paginationParams = new PaginationParams { PageNumber = 2, PageSize = 5 };
            var query = new GetEmployeesQuery(paginationParams);
            
            using var cts = new CancellationTokenSource();

            // ============ Act ============
            var resultado = await sut.Handle(query, cts.Token);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.NotNull(resultado.Data);
            Assert.Equal(2, resultado.Data.Count);
            Assert.Equal(3, resultado.TotalPages);
            Assert.Equal(15, resultado.TotalRows);

            // Verificar mapeo de datos
            Assert.Equal(5, resultado.Data[0].EmpId);
            Assert.Equal("Ana Martínez", resultado.Data[0].FullName);
            Assert.Equal(6, resultado.Data[1].EmpId);
            Assert.Equal("Pedro Rodríguez", resultado.Data[1].FullName);

            // Verificar que se pasaron los parámetros correctos
            Assert.NotNull(paramsCapturados);
            Assert.Equal(2, paramsCapturados!.PageNumber);
            Assert.Equal(5, paramsCapturados!.PageSize);
            Assert.Equal(cts.Token, tokenCapturado);

            // Verificar que se llamó al método correcto del puerto
            portMock.Verify(p => p.GetPagedAsync(paginationParams, cts.Token), Times.Once);
            portMock.Verify(p => p.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Deberia_RetornarListaVacia_CuandoNoHayEmpleados()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IEmployeeReadPort>();
            
            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Employee>());

            var sut = new GetEmployeesHandler(portMock.Object);
            var query = new GetEmployeesQuery(null);

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
        public async Task Handle_Deberia_RetornarPaginacionVacia_CuandoPaginadoNoTieneEmpleados()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IEmployeeReadPort>();
            
            var respuestaPaginadaVacia = new PaginationResponse<Employee>
            {
                Data = new List<Employee>(),
                TotalPages = 0,
                TotalRows = 0
            };

            portMock
                .Setup(p => p.GetPagedAsync(It.IsAny<PaginationParams>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(respuestaPaginadaVacia);

            var sut = new GetEmployeesHandler(portMock.Object);
            var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
            var query = new GetEmployeesQuery(paginationParams);

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
        public async Task Handle_Deberia_PropagarExcepcionDelPuerto_CuandoGetAllAsyncFalla()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IEmployeeReadPort>();
            
            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Error de conexión a base de datos"));

            var sut = new GetEmployeesHandler(portMock.Object);
            var query = new GetEmployeesQuery(null);

            // ============ Act & Assert ============
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                sut.Handle(query, CancellationToken.None));
            
            Assert.Equal("Error de conexión a base de datos", ex.Message);
            portMock.Verify(p => p.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_PropagarExcepcionDelPuerto_CuandoGetPagedAsyncFalla()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IEmployeeReadPort>();
            
            portMock
                .Setup(p => p.GetPagedAsync(It.IsAny<PaginationParams>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TimeoutException("Timeout en consulta paginada"));

            var sut = new GetEmployeesHandler(portMock.Object);
            var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
            var query = new GetEmployeesQuery(paginationParams);

            // ============ Act & Assert ============
            var ex = await Assert.ThrowsAsync<TimeoutException>(() => 
                sut.Handle(query, CancellationToken.None));
            
            Assert.Equal("Timeout en consulta paginada", ex.Message);
            portMock.Verify(p => p.GetPagedAsync(paginationParams, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarEmpleadoConNombreComplejo_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IEmployeeReadPort>();
            
            var empleadosConNombresComplejos = new List<Employee>
            {
                new EmployeeTestDataBuilder().WithId(10).WithName("José María de la Cruz Fernández").Build(),
                new EmployeeTestDataBuilder().WithId(11).WithName("María José O'Connor").Build(),
                new EmployeeTestDataBuilder().WithId(12).WithName("Jean-Pierre Van Der Berg").Build()
            };

            portMock
                .Setup(p => p.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(empleadosConNombresComplejos);

            var sut = new GetEmployeesHandler(portMock.Object);
            var query = new GetEmployeesQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            AssertionUtils.AssertPaginationResponse(resultado, 3, 1, 3);

            // Verificar caracteres especiales específicos
            AssertionUtils.AssertContainsSpecialCharacters(
                resultado.Data.First(e => e.EmpId == 10).FullName, 
                "é", "í");
                
            AssertionUtils.AssertContainsSpecialCharacters(
                resultado.Data.First(e => e.EmpId == 11).FullName, 
                "'");

            AssertionUtils.AssertContainsSpecialCharacters(
                resultado.Data.First(e => e.EmpId == 12).FullName, 
                "-");

            // Verificar que los IDs se mantienen correctos
            AssertionUtils.AssertContainsIds(resultado.Data, e => e.EmpId, 10, 11, 12);
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(2, 5)]
        [InlineData(3, 20)]
        public async Task Handle_Should_ReturnPaginatedEmployees_ForVariousPageSizes(int pageNumber, int pageSize)
        {
            // ========== Arrange ==========
            var portMock = new Mock<IEmployeeReadPort>();
            var paginationParams = PaginationTestUtils.CreateParams(pageNumber, pageSize);
            var employees = new EmployeeTestDataBuilder().BuildMany(pageSize);
            var paginatedResponse = PaginationTestUtils.CreateResponse(employees, 5, 50);
            
            portMock.Setup(p => p.GetPagedAsync(paginationParams, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(paginatedResponse);

            var sut = new GetEmployeesHandler(portMock.Object);
            var query = new GetEmployeesQuery(paginationParams);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            AssertionUtils.AssertPaginationResponse(resultado, pageSize, 5, 50);
            portMock.Verify(p => p.GetPagedAsync(paginationParams, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Should_HandleEmptyPaginatedResult()
        {
            // ========== Arrange ==========
            var portMock = new Mock<IEmployeeReadPort>();
            var paginationParams = PaginationTestUtils.CreateParams(1, 10);
            var emptyResponse = PaginationTestUtils.CreateEmptyResponse<Employee>();
            
            portMock.Setup(p => p.GetPagedAsync(paginationParams, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(emptyResponse);

            var sut = new GetEmployeesHandler(portMock.Object);
            var query = new GetEmployeesQuery(paginationParams);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            AssertionUtils.AssertPaginationResponse(resultado, 0, 0, 0);
        }
    }
}