using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using SalesDatePrediction.Application.Predictions;
using SalesDatePrediction.Domain.Predictions;
using SalesDatePrediction.Domain.Predictions.Ports;
using SalesDatePrediction.Domain.Common.Pagination;

namespace SalesDatePrediction.Application.Tests.Predictions
{
    public class GetPredictionsHandlerTests
    {
        [Fact]
        public async Task Handle_Deberia_RetornarTodasLasPredicciones_CuandoPaginationParamsEsNull()
        {
            // ========== Arrange ==========
            var portMock = new Mock<ISalesPredictionReadPort>();
            
            var prediccionesSimuladas = new List<CustomerPrediction>
            {
                new CustomerPrediction(
                    CustomerId: 1,
                    CustomerName: "ACME Technologies S.A.S.",
                    LastOrderDate: new DateTime(2024, 12, 15),
                    NextPredictedOrder: new DateTime(2025, 1, 15)
                ),
                new CustomerPrediction(
                    CustomerId: 2,
                    CustomerName: "Global Solutions Corp.",
                    LastOrderDate: new DateTime(2024, 11, 20),
                    NextPredictedOrder: new DateTime(2025, 1, 5)
                ),
                new CustomerPrediction(
                    CustomerId: 3,
                    CustomerName: "Innovación y Desarrollo Ltda.",
                    LastOrderDate: new DateTime(2024, 12, 1),
                    NextPredictedOrder: new DateTime(2025, 1, 1)
                )
            };

            CancellationToken tokenCapturado = default;

            portMock
                .Setup(p => p.GetPredictionsAsync(It.IsAny<CancellationToken>()))
                .Callback<CancellationToken>((ct) =>
                {
                    tokenCapturado = ct;
                })
                .ReturnsAsync(prediccionesSimuladas);

            var sut = new GetPredictionsHandler(portMock.Object);
            var query = new GetPredictionsQuery(null);
            
            using var cts = new CancellationTokenSource();

            // ============ Act ============
            var resultado = await sut.Handle(query, cts.Token);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.NotNull(resultado.Data);
            Assert.Equal(3, resultado.Data.Count);
            Assert.Equal(1, resultado.TotalPages);
            Assert.Equal(3, resultado.TotalRows);

            // Verificar que los datos se mapearon correctamente
            var prediccion1 = resultado.Data.FirstOrDefault(p => p.CustomerId == 1);
            Assert.NotNull(prediccion1);
            Assert.Equal("ACME Technologies S.A.S.", prediccion1!.CustomerName);
            Assert.Equal(new DateTime(2024, 12, 15), prediccion1.LastOrderDate);
            Assert.Equal(new DateTime(2025, 1, 15), prediccion1.NextPredictedOrder);

            var prediccion2 = resultado.Data.FirstOrDefault(p => p.CustomerId == 2);
            Assert.NotNull(prediccion2);
            Assert.Equal("Global Solutions Corp.", prediccion2!.CustomerName);
            Assert.Equal(new DateTime(2024, 11, 20), prediccion2.LastOrderDate);
            Assert.Equal(new DateTime(2025, 1, 5), prediccion2.NextPredictedOrder);

            var prediccion3 = resultado.Data.FirstOrDefault(p => p.CustomerId == 3);
            Assert.NotNull(prediccion3);
            Assert.Equal("Innovación y Desarrollo Ltda.", prediccion3!.CustomerName);
            Assert.Equal(new DateTime(2024, 12, 1), prediccion3.LastOrderDate);
            Assert.Equal(new DateTime(2025, 1, 1), prediccion3.NextPredictedOrder);

            // Verificar que se llamó al método correcto del puerto
            Assert.Equal(cts.Token, tokenCapturado);
            portMock.Verify(p => p.GetPredictionsAsync(cts.Token), Times.Once);
            portMock.Verify(p => p.GetPredictionsPagedAsync(It.IsAny<PaginationParams>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Deberia_RetornarPrediccionesPaginadas_CuandoPaginationParamsNoEsNull()
        {
            // ========== Arrange ==========
            var portMock = new Mock<ISalesPredictionReadPort>();
            
            var prediccionesPaginadas = new PaginationResponse<CustomerPrediction>
            {
                Data = new List<CustomerPrediction>
                {
                    new CustomerPrediction(
                        CustomerId: 10,
                        CustomerName: "Empresa Página 2",
                        LastOrderDate: new DateTime(2024, 10, 15),
                        NextPredictedOrder: new DateTime(2024, 12, 15)
                    ),
                    new CustomerPrediction(
                        CustomerId: 11,
                        CustomerName: "Segunda Empresa Paginada",
                        LastOrderDate: new DateTime(2024, 11, 1),
                        NextPredictedOrder: new DateTime(2025, 1, 1)
                    )
                },
                TotalPages = 5,
                TotalRows = 25
            };

            PaginationParams? paramsCapturados = null;
            CancellationToken tokenCapturado = default;

            portMock
                .Setup(p => p.GetPredictionsPagedAsync(It.IsAny<PaginationParams>(), It.IsAny<CancellationToken>()))
                .Callback<PaginationParams, CancellationToken>((pp, ct) =>
                {
                    paramsCapturados = pp;
                    tokenCapturado = ct;
                })
                .ReturnsAsync(prediccionesPaginadas);

            var sut = new GetPredictionsHandler(portMock.Object);
            var paginationParams = new PaginationParams { PageNumber = 2, PageSize = 5 };
            var query = new GetPredictionsQuery(paginationParams);
            
            using var cts = new CancellationTokenSource();

            // ============ Act ============
            var resultado = await sut.Handle(query, cts.Token);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.NotNull(resultado.Data);
            Assert.Equal(2, resultado.Data.Count);
            Assert.Equal(5, resultado.TotalPages);
            Assert.Equal(25, resultado.TotalRows);

            // Verificar mapeo de datos
            Assert.Equal(10, resultado.Data[0].CustomerId);
            Assert.Equal("Empresa Página 2", resultado.Data[0].CustomerName);
            Assert.Equal(new DateTime(2024, 10, 15), resultado.Data[0].LastOrderDate);
            Assert.Equal(new DateTime(2024, 12, 15), resultado.Data[0].NextPredictedOrder);

            Assert.Equal(11, resultado.Data[1].CustomerId);
            Assert.Equal("Segunda Empresa Paginada", resultado.Data[1].CustomerName);
            Assert.Equal(new DateTime(2024, 11, 1), resultado.Data[1].LastOrderDate);
            Assert.Equal(new DateTime(2025, 1, 1), resultado.Data[1].NextPredictedOrder);

            // Verificar que se pasaron los parámetros correctos
            Assert.NotNull(paramsCapturados);
            Assert.Equal(2, paramsCapturados!.PageNumber);
            Assert.Equal(5, paramsCapturados!.PageSize);
            Assert.Equal(cts.Token, tokenCapturado);

            // Verificar que se llamó al método correcto del puerto
            portMock.Verify(p => p.GetPredictionsPagedAsync(paginationParams, cts.Token), Times.Once);
            portMock.Verify(p => p.GetPredictionsAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_Deberia_RetornarListaVacia_CuandoNoHayPredicciones()
        {
            // ========== Arrange ==========
            var portMock = new Mock<ISalesPredictionReadPort>();
            
            portMock
                .Setup(p => p.GetPredictionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CustomerPrediction>());

            var sut = new GetPredictionsHandler(portMock.Object);
            var query = new GetPredictionsQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.NotNull(resultado.Data);
            Assert.Empty(resultado.Data);
            Assert.Equal(1, resultado.TotalPages);
            Assert.Equal(0, resultado.TotalRows);

            portMock.Verify(p => p.GetPredictionsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_RetornarPaginacionVacia_CuandoPaginadoNoTienePredicciones()
        {
            // ========== Arrange ==========
            var portMock = new Mock<ISalesPredictionReadPort>();
            
            var respuestaPaginadaVacia = new PaginationResponse<CustomerPrediction>
            {
                Data = new List<CustomerPrediction>(),
                TotalPages = 0,
                TotalRows = 0
            };

            portMock
                .Setup(p => p.GetPredictionsPagedAsync(It.IsAny<PaginationParams>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(respuestaPaginadaVacia);

            var sut = new GetPredictionsHandler(portMock.Object);
            var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
            var query = new GetPredictionsQuery(paginationParams);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.NotNull(resultado.Data);
            Assert.Empty(resultado.Data);
            Assert.Equal(0, resultado.TotalPages);
            Assert.Equal(0, resultado.TotalRows);

            portMock.Verify(p => p.GetPredictionsPagedAsync(paginationParams, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarFechasExtremas_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<ISalesPredictionReadPort>();
            
            var prediccionesConFechasExtremas = new List<CustomerPrediction>
            {
                new CustomerPrediction(
                    CustomerId: 100,
                    CustomerName: "Cliente Fecha Mínima",
                    LastOrderDate: DateTime.MinValue,
                    NextPredictedOrder: new DateTime(2025, 1, 1)
                ),
                new CustomerPrediction(
                    CustomerId: 101,
                    CustomerName: "Cliente Fecha Máxima",
                    LastOrderDate: new DateTime(2024, 12, 31, 23, 59, 59),
                    NextPredictedOrder: DateTime.MaxValue
                ),
                new CustomerPrediction(
                    CustomerId: 102,
                    CustomerName: "Cliente Fechas Iguales",
                    LastOrderDate: new DateTime(2025, 6, 15, 14, 30, 45),
                    NextPredictedOrder: new DateTime(2025, 6, 15, 14, 30, 45) // Misma fecha
                )
            };

            portMock
                .Setup(p => p.GetPredictionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(prediccionesConFechasExtremas);

            var sut = new GetPredictionsHandler(portMock.Object);
            var query = new GetPredictionsQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal(3, resultado.Data.Count);

            // Verificar fechas extremas
            var clienteFechaMin = resultado.Data.First(p => p.CustomerId == 100);
            Assert.Equal(DateTime.MinValue, clienteFechaMin.LastOrderDate);
            Assert.Equal(new DateTime(2025, 1, 1), clienteFechaMin.NextPredictedOrder);

            var clienteFechaMax = resultado.Data.First(p => p.CustomerId == 101);
            Assert.Equal(new DateTime(2024, 12, 31, 23, 59, 59), clienteFechaMax.LastOrderDate);
            Assert.Equal(DateTime.MaxValue, clienteFechaMax.NextPredictedOrder);

            var clienteFechasIguales = resultado.Data.First(p => p.CustomerId == 102);
            Assert.Equal(clienteFechasIguales.LastOrderDate, clienteFechasIguales.NextPredictedOrder);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarNombresDeClienteComplejos_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<ISalesPredictionReadPort>();
            
            var prediccionesConNombresComplejos = new List<CustomerPrediction>
            {
                new CustomerPrediction(
                    CustomerId: 200,
                    CustomerName: "Ñandú Electrónicos & Cía. S.A.S.",
                    LastOrderDate: new DateTime(2024, 11, 15),
                    NextPredictedOrder: new DateTime(2025, 1, 15)
                ),
                new CustomerPrediction(
                    CustomerId: 201,
                    CustomerName: "O'Connor & Associates Ltd. (Édition Française)",
                    LastOrderDate: new DateTime(2024, 10, 10),
                    NextPredictedOrder: new DateTime(2024, 12, 10)
                ),
                new CustomerPrediction(
                    CustomerId: 202,
                    CustomerName: "", // Nombre vacío
                    LastOrderDate: new DateTime(2024, 12, 1),
                    NextPredictedOrder: new DateTime(2025, 2, 1)
                ),
                new CustomerPrediction(
                    CustomerId: 203,
                    CustomerName: "Empresa con símbolos: @#$%^&*()_+-=[]{}|;':\",./<>?",
                    LastOrderDate: new DateTime(2024, 9, 5),
                    NextPredictedOrder: new DateTime(2024, 11, 5)
                )
            };

            portMock
                .Setup(p => p.GetPredictionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(prediccionesConNombresComplejos);

            var sut = new GetPredictionsHandler(portMock.Object);
            var query = new GetPredictionsQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal(4, resultado.Data.Count);

            // Verificar mapeo de nombres complejos
            var clienteConAcentos = resultado.Data.First(p => p.CustomerId == 200);
            Assert.Equal("Ñandú Electrónicos & Cía. S.A.S.", clienteConAcentos.CustomerName);

            var clienteConApostrofe = resultado.Data.First(p => p.CustomerId == 201);
            Assert.Equal("O'Connor & Associates Ltd. (Édition Française)", clienteConApostrofe.CustomerName);

            var clienteSinNombre = resultado.Data.First(p => p.CustomerId == 202);
            Assert.Equal("", clienteSinNombre.CustomerName);

            var clienteConSimbolos = resultado.Data.First(p => p.CustomerId == 203);
            Assert.Equal("Empresa con símbolos: @#$%^&*()_+-=[]{}|;':\",./<>?", clienteConSimbolos.CustomerName);
        }

        [Fact]
        public async Task Handle_Deberia_PropagarExcepcionDelPuerto_CuandoGetPredictionsAsyncFalla()
        {
            // ========== Arrange ==========
            var portMock = new Mock<ISalesPredictionReadPort>();
            
            portMock
                .Setup(p => p.GetPredictionsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Error en predicciones de ML"));

            var sut = new GetPredictionsHandler(portMock.Object);
            var query = new GetPredictionsQuery(null);

            // ============ Act & Assert ============
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                sut.Handle(query, CancellationToken.None));
            
            Assert.Equal("Error en predicciones de ML", ex.Message);
            portMock.Verify(p => p.GetPredictionsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_PropagarTimeoutException_CuandoGetPredictionsPagedAsyncFalla()
        {
            // ========== Arrange ==========
            var portMock = new Mock<ISalesPredictionReadPort>();
            
            portMock
                .Setup(p => p.GetPredictionsPagedAsync(It.IsAny<PaginationParams>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TimeoutException("Timeout en consulta de predicciones paginada"));

            var sut = new GetPredictionsHandler(portMock.Object);
            var paginationParams = new PaginationParams { PageNumber = 1, PageSize = 10 };
            var query = new GetPredictionsQuery(paginationParams);

            // ============ Act & Assert ============
            var ex = await Assert.ThrowsAsync<TimeoutException>(() => 
                sut.Handle(query, CancellationToken.None));
            
            Assert.Equal("Timeout en consulta de predicciones paginada", ex.Message);
            portMock.Verify(p => p.GetPredictionsPagedAsync(paginationParams, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarPrediccionUnica_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<ISalesPredictionReadPort>();
            
            var prediccionUnica = new List<CustomerPrediction>
            {
                new CustomerPrediction(
                    CustomerId: 999,
                    CustomerName: "Cliente Único Especial",
                    LastOrderDate: new DateTime(2024, 12, 25, 18, 30, 45, 123),
                    NextPredictedOrder: new DateTime(2025, 3, 25, 9, 15, 30, 456)
                )
            };

            portMock
                .Setup(p => p.GetPredictionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(prediccionUnica);

            var sut = new GetPredictionsHandler(portMock.Object);
            var query = new GetPredictionsQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Single(resultado.Data);

            var prediccion = resultado.Data.First();
            Assert.Equal(999, prediccion.CustomerId);
            Assert.Equal("Cliente Único Especial", prediccion.CustomerName);
            Assert.Equal(new DateTime(2024, 12, 25, 18, 30, 45, 123), prediccion.LastOrderDate);
            Assert.Equal(new DateTime(2025, 3, 25, 9, 15, 30, 456), prediccion.NextPredictedOrder);

            Assert.Equal(1, resultado.TotalPages);
            Assert.Equal(1, resultado.TotalRows);
        }

        [Fact]
        public async Task Handle_Deberia_ManejarPrediccionesConIdsDuplicados_CorrectamenteSinFiltrar()
        {
            // ========== Arrange ==========
            var portMock = new Mock<ISalesPredictionReadPort>();
            
            // Simulamos un caso donde el puerto retorna datos con IDs duplicados (caso edge poco común)
            var prediccionesConIdsDuplicados = new List<CustomerPrediction>
            {
                new CustomerPrediction(
                    CustomerId: 500,
                    CustomerName: "Primera Predicción",
                    LastOrderDate: new DateTime(2024, 10, 1),
                    NextPredictedOrder: new DateTime(2024, 12, 1)
                ),
                new CustomerPrediction(
                    CustomerId: 500, // ID duplicado intencional
                    CustomerName: "Segunda Predicción",
                    LastOrderDate: new DateTime(2024, 11, 1),
                    NextPredictedOrder: new DateTime(2025, 1, 1)
                )
            };

            portMock
                .Setup(p => p.GetPredictionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(prediccionesConIdsDuplicados);

            var sut = new GetPredictionsHandler(portMock.Object);
            var query = new GetPredictionsQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Data.Count); // No debe filtrar duplicados

            // Verificar que ambas predicciones están presentes
            Assert.Contains(resultado.Data, p => p.CustomerName == "Primera Predicción");
            Assert.Contains(resultado.Data, p => p.CustomerName == "Segunda Predicción");
            
            // Verificar que ambas tienen el mismo CustomerId
            Assert.True(resultado.Data.All(p => p.CustomerId == 500));
        }

        [Fact]
        public async Task Handle_Deberia_ManejarPrediccionesFuturasMuyLejanas_CorrectamenteEnMapeo()
        {
            // ========== Arrange ==========
            var portMock = new Mock<ISalesPredictionReadPort>();
            
            var prediccionesFuturasLejanas = new List<CustomerPrediction>
            {
                new CustomerPrediction(
                    CustomerId: 300,
                    CustomerName: "Cliente Futuro Cercano",
                    LastOrderDate: new DateTime(2024, 12, 1),
                    NextPredictedOrder: new DateTime(2024, 12, 2) // 1 día después
                ),
                new CustomerPrediction(
                    CustomerId: 301,
                    CustomerName: "Cliente Futuro Lejano",
                    LastOrderDate: new DateTime(2024, 1, 1),
                    NextPredictedOrder: new DateTime(2030, 1, 1) // 6 años después
                ),
                new CustomerPrediction(
                    CustomerId: 302,
                    CustomerName: "Cliente Predicción Pasada",
                    LastOrderDate: new DateTime(2024, 12, 1),
                    NextPredictedOrder: new DateTime(2024, 6, 1) // Predicción en el pasado
                )
            };

            portMock
                .Setup(p => p.GetPredictionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(prediccionesFuturasLejanas);

            var sut = new GetPredictionsHandler(portMock.Object);
            var query = new GetPredictionsQuery(null);

            // ============ Act ============
            var resultado = await sut.Handle(query, CancellationToken.None);

            // ========== Assert ==========
            Assert.NotNull(resultado);
            Assert.Equal(3, resultado.Data.Count);

            // Verificar cliente con predicción muy cercana
            var clienteCercano = resultado.Data.First(p => p.CustomerId == 300);
            var diasCercanos = (clienteCercano.NextPredictedOrder - clienteCercano.LastOrderDate).Days;
            Assert.Equal(1, diasCercanos);

            // Verificar cliente con predicción muy lejana
            var clienteLejano = resultado.Data.First(p => p.CustomerId == 301);
            var diasLejanos = (clienteLejano.NextPredictedOrder - clienteLejano.LastOrderDate).Days;
            Assert.True(diasLejanos > 2000); // Más de 5 años

            // Verificar cliente con predicción en el pasado
            var clientePasado = resultado.Data.First(p => p.CustomerId == 302);
            var diasPasado = (clientePasado.NextPredictedOrder - clientePasado.LastOrderDate).Days;
            Assert.True(diasPasado < 0); // Predicción anterior a la última orden
        }
    }
}