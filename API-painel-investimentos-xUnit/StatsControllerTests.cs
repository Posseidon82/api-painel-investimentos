using API_painel_investimentos.Controllers.Simulation;
using API_painel_investimentos.DTO.Simulation;
using API_painel_investimentos.Services.Simulation.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace API_painel_investimentos_xUnit
{
    public class StatsControllerTests
    {
        private readonly Mock<ISimulationStatsService> _statsServiceMock;
        private readonly Mock<ILogger<StatsController>> _loggerMock;
        private readonly StatsController _controller;

        public StatsControllerTests()
        {
            _statsServiceMock = new Mock<ISimulationStatsService>();
            _loggerMock = new Mock<ILogger<StatsController>>();
            _controller = new StatsController(_statsServiceMock.Object, _loggerMock.Object);
        }

        public class GetProductDailyStatsTests : StatsControllerTests
        {
            [Fact]
            public async Task GetProductDailyStats_WithAllFilters_ReturnsOkWithResult()
            {
                // Arrange
                var startDate = DateTime.UtcNow.AddDays(-30);
                var endDate = DateTime.UtcNow;
                var productId = Guid.NewGuid();
                var category = "RendaFixa";

                var expectedResponse = new ProductStatsResponseDto(
                    StartDate: startDate,
                    EndDate: endDate,
                    TotalSimulations: 150,
                    UniqueProducts: 5,
                    DailyStats: new List<ProductDailyStatsDto>
                    {
                        new ProductDailyStatsDto(
                            ProductId: productId,
                            ProductName: "CDB Banco X",
                            Category: "RendaFixa",
                            RiskLevel: "Baixo",
                            Date: DateTime.UtcNow.Date,
                            SimulationCount: 25,
                            AverageFinalAmount: 10500m,
                            TotalInvestedAmount: 250000m,
                            MinFinalAmount: 9800m,
                            MaxFinalAmount: 11200m,
                            AverageReturnRate: 5.2m
                        )
                    }
                );

                _statsServiceMock
                    .Setup(x => x.GetProductDailyStatsAsync(It.IsAny<ProductStatsRequestDto>()))
                    .ReturnsAsync(expectedResponse);

                // Act
                var result = await _controller.GetProductDailyStats(startDate, endDate, productId, category);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedResponse);
                okResult.StatusCode.Should().Be(200);

                _statsServiceMock.Verify(x => x.GetProductDailyStatsAsync(
                    It.Is<ProductStatsRequestDto>(r =>
                        r.StartDate == startDate &&
                        r.EndDate == endDate &&
                        r.ProductId == productId &&
                        r.Category == category
                    )), Times.Once);
            }

            [Fact]
            public async Task GetProductDailyStats_NoFilters_ReturnsOkWithResult()
            {
                // Arrange
                var expectedResponse = new ProductStatsResponseDto(
                    StartDate: DateTime.UtcNow.AddDays(-30),
                    EndDate: DateTime.UtcNow,
                    TotalSimulations: 500,
                    UniqueProducts: 15,
                    DailyStats: new List<ProductDailyStatsDto>()
                );

                _statsServiceMock
                    .Setup(x => x.GetProductDailyStatsAsync(It.IsAny<ProductStatsRequestDto>()))
                    .ReturnsAsync(expectedResponse);

                // Act
                var result = await _controller.GetProductDailyStats(null, null, null, null);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedResponse);

                _statsServiceMock.Verify(x => x.GetProductDailyStatsAsync(
                    It.Is<ProductStatsRequestDto>(r =>
                        r.StartDate == null &&
                        r.EndDate == null &&
                        r.ProductId == null &&
                        r.Category == null
                    )), Times.Once);
            }

            [Fact]
            public async Task GetProductDailyStats_OnlyCategoryFilter_ReturnsOkWithResult()
            {
                // Arrange
                var category = "Ações";
                var expectedResponse = new ProductStatsResponseDto(
                    StartDate: DateTime.UtcNow.AddDays(-30),
                    EndDate: DateTime.UtcNow,
                    TotalSimulations: 200,
                    UniqueProducts: 8,
                    DailyStats: new List<ProductDailyStatsDto>
                    {
                        new ProductDailyStatsDto(
                            ProductId: Guid.NewGuid(),
                            ProductName: "Ações Tech",
                            Category: "Ações",
                            RiskLevel: "Alto",
                            Date: DateTime.UtcNow.Date,
                            SimulationCount: 45,
                            AverageFinalAmount: 12000m,
                            TotalInvestedAmount: 540000m,
                            MinFinalAmount: 8500m,
                            MaxFinalAmount: 15600m,
                            AverageReturnRate: 12.5m
                        )
                    }
                );

                _statsServiceMock
                    .Setup(x => x.GetProductDailyStatsAsync(It.IsAny<ProductStatsRequestDto>()))
                    .ReturnsAsync(expectedResponse);

                // Act
                var result = await _controller.GetProductDailyStats(null, null, null, category);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedResponse);

                _statsServiceMock.Verify(x => x.GetProductDailyStatsAsync(
                    It.Is<ProductStatsRequestDto>(r =>
                        r.StartDate == null &&
                        r.EndDate == null &&
                        r.ProductId == null &&
                        r.Category == category
                    )), Times.Once);
            }

            [Fact]
            public async Task GetProductDailyStats_ServiceThrowsException_ReturnsInternalServerError()
            {
                // Arrange
                var exception = new Exception("Database connection failed");

                _statsServiceMock
                    .Setup(x => x.GetProductDailyStatsAsync(It.IsAny<ProductStatsRequestDto>()))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.GetProductDailyStats(null, null, null, null);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Erro interno ao gerar estatísticas");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error getting product daily stats")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        public class GetTopProductsTests : StatsControllerTests
        {
            [Fact]
            public async Task GetTopProducts_WithCustomParameters_ReturnsOkWithResult()
            {
                // Arrange
                var startDate = DateTime.UtcNow.AddDays(-60);
                var endDate = DateTime.UtcNow;
                var topCount = 5;

                var expectedProducts = new List<ProductDailyStatsDto>
                {
                    new ProductDailyStatsDto(
                        ProductId: Guid.NewGuid(),
                        ProductName: "CDB Banco X",
                        Category: "RendaFixa",
                        RiskLevel: "Baixo",
                        Date: DateTime.UtcNow.Date,
                        SimulationCount: 150,
                        AverageFinalAmount: 10500m,
                        TotalInvestedAmount: 1575000m,
                        MinFinalAmount: 9800m,
                        MaxFinalAmount: 11200m,
                        AverageReturnRate: 5.2m
                    ),
                    new ProductDailyStatsDto(
                        ProductId: Guid.NewGuid(),
                        ProductName: "Fundo Multimercado Y",
                        Category: "Multimercado",
                        RiskLevel: "Médio",
                        Date: DateTime.UtcNow.Date,
                        SimulationCount: 120,
                        AverageFinalAmount: 11200m,
                        TotalInvestedAmount: 1344000m,
                        MinFinalAmount: 10200m,
                        MaxFinalAmount: 12500m,
                        AverageReturnRate: 7.8m
                    )
                };

                _statsServiceMock
                    .Setup(x => x.GetTopProductsAsync(startDate, endDate, topCount))
                    .ReturnsAsync(expectedProducts);

                // Act
                var result = await _controller.GetTopProducts(startDate, endDate, topCount);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedProducts);
                okResult.StatusCode.Should().Be(200);

                _statsServiceMock.Verify(x => x.GetTopProductsAsync(startDate, endDate, topCount), Times.Once);
            }

            [Fact]
            public async Task GetTopProducts_DefaultParameters_UsesDefaultValues()
            {
                // Arrange
                var expectedProducts = new List<ProductDailyStatsDto>
                {
                    new ProductDailyStatsDto(
                        ProductId: Guid.NewGuid(),
                        ProductName: "Default Product",
                        Category: "RendaFixa",
                        RiskLevel: "Baixo",
                        Date: DateTime.UtcNow.Date,
                        SimulationCount: 100,
                        AverageFinalAmount: 10000m,
                        TotalInvestedAmount: 1000000m,
                        MinFinalAmount: 9500m,
                        MaxFinalAmount: 10500m,
                        AverageReturnRate: 5.0m
                    )
                };

                _statsServiceMock
                    .Setup(x => x.GetTopProductsAsync(
                        It.IsAny<DateTime>(),
                        It.IsAny<DateTime>(),
                        It.IsAny<int>()))
                    .ReturnsAsync(expectedProducts);

                // Act
                var result = await _controller.GetTopProducts(null, null, 10);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedProducts);

                _statsServiceMock.Verify(x => x.GetTopProductsAsync(
                    It.Is<DateTime>(start => start.Date == DateTime.UtcNow.AddDays(-30).Date),
                    It.Is<DateTime>(end => end.Date == DateTime.UtcNow.Date),
                    It.Is<int>(count => count == 10)),
                    Times.Once);
            }

            [Fact]
            public async Task GetTopProducts_CustomTopCount_ReturnsLimitedResults()
            {
                // Arrange
                var topCount = 3;
                var expectedProducts = new List<ProductDailyStatsDto>
                {
                    new ProductDailyStatsDto(
                        ProductId: Guid.NewGuid(),
                        ProductName: "Product 1",
                        Category: "Ações",
                        RiskLevel: "Alto",
                        Date: DateTime.UtcNow.Date,
                        SimulationCount: 100,
                        AverageFinalAmount: 12000m,
                        TotalInvestedAmount: 1200000m,
                        MinFinalAmount: 11000m,
                        MaxFinalAmount: 13000m,
                        AverageReturnRate: 12.0m
                    ),
                    new ProductDailyStatsDto(
                        ProductId: Guid.NewGuid(),
                        ProductName: "Product 2",
                        Category: "RendaFixa",
                        RiskLevel: "Baixo",
                        Date: DateTime.UtcNow.Date,
                        SimulationCount: 90,
                        AverageFinalAmount: 10500m,
                        TotalInvestedAmount: 945000m,
                        MinFinalAmount: 10200m,
                        MaxFinalAmount: 10800m,
                        AverageReturnRate: 5.0m
                    ),
                    new ProductDailyStatsDto(
                        ProductId: Guid.NewGuid(),
                        ProductName: "Product 3",
                        Category: "Multimercado",
                        RiskLevel: "Médio",
                        Date: DateTime.UtcNow.Date,
                        SimulationCount: 80,
                        AverageFinalAmount: 11000m,
                        TotalInvestedAmount: 880000m,
                        MinFinalAmount: 10500m,
                        MaxFinalAmount: 11500m,
                        AverageReturnRate: 8.5m
                    )
                };

                _statsServiceMock
                    .Setup(x => x.GetTopProductsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), topCount))
                    .ReturnsAsync(expectedProducts);

                // Act
                var result = await _controller.GetTopProducts(null, null, topCount);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                var returnedProducts = okResult.Value.Should().BeOfType<List<ProductDailyStatsDto>>().Subject;
                returnedProducts.Should().HaveCount(topCount);

                _statsServiceMock.Verify(x => x.GetTopProductsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), topCount), Times.Once);
            }

            [Fact]
            public async Task GetTopProducts_EmptyResult_ReturnsOkWithEmptyList()
            {
                // Arrange
                var expectedProducts = new List<ProductDailyStatsDto>();

                _statsServiceMock
                    .Setup(x => x.GetTopProductsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                    .ReturnsAsync(expectedProducts);

                // Act
                var result = await _controller.GetTopProducts(null, null, 10);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                var returnedProducts = okResult.Value.Should().BeOfType<List<ProductDailyStatsDto>>().Subject;
                returnedProducts.Should().BeEmpty();
            }

            [Fact]
            public async Task GetTopProducts_ServiceThrowsException_ReturnsInternalServerError()
            {
                // Arrange
                var exception = new Exception("Database query failed");

                _statsServiceMock
                    .Setup(x => x.GetTopProductsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.GetTopProducts(null, null, 10);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Erro interno ao buscar produtos mais simulados");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error getting top products")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        public class GetProductStatsByIdTests : StatsControllerTests
        {
            [Fact]
            public async Task GetProductStatsById_WithDateFilters_ReturnsOkWithResult()
            {
                // Arrange
                var productId = Guid.NewGuid();
                var startDate = DateTime.UtcNow.AddDays(-30);
                var endDate = DateTime.UtcNow;

                var expectedResponse = new ProductStatsResponseDto(
                    StartDate: startDate,
                    EndDate: endDate,
                    TotalSimulations: 75,
                    UniqueProducts: 1,
                    DailyStats: new List<ProductDailyStatsDto>
                    {
                        new ProductDailyStatsDto(
                            ProductId: productId,
                            ProductName: "CDB Específico",
                            Category: "RendaFixa",
                            RiskLevel: "Baixo",
                            Date: DateTime.UtcNow.Date,
                            SimulationCount: 15,
                            AverageFinalAmount: 10500m,
                            TotalInvestedAmount: 157500m,
                            MinFinalAmount: 10200m,
                            MaxFinalAmount: 10800m,
                            AverageReturnRate: 5.0m
                        )
                    }
                );

                _statsServiceMock
                    .Setup(x => x.GetProductDailyStatsAsync(It.IsAny<ProductStatsRequestDto>()))
                    .ReturnsAsync(expectedResponse);

                // Act
                var result = await _controller.GetProductStatsById(productId, startDate, endDate);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedResponse);
                okResult.StatusCode.Should().Be(200);

                _statsServiceMock.Verify(x => x.GetProductDailyStatsAsync(
                    It.Is<ProductStatsRequestDto>(r =>
                        r.ProductId == productId &&
                        r.StartDate == startDate &&
                        r.EndDate == endDate &&
                        r.Category == null
                    )), Times.Once);
            }

            [Fact]
            public async Task GetProductStatsById_NoDateFilters_ReturnsOkWithResult()
            {
                // Arrange
                var productId = Guid.NewGuid();
                var expectedResponse = new ProductStatsResponseDto(
                    StartDate: DateTime.UtcNow.AddDays(-30),
                    EndDate: DateTime.UtcNow,
                    TotalSimulations: 25,
                    UniqueProducts: 1,
                    DailyStats: new List<ProductDailyStatsDto>()
                );

                _statsServiceMock
                    .Setup(x => x.GetProductDailyStatsAsync(It.IsAny<ProductStatsRequestDto>()))
                    .ReturnsAsync(expectedResponse);

                // Act
                var result = await _controller.GetProductStatsById(productId, null, null);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedResponse);

                _statsServiceMock.Verify(x => x.GetProductDailyStatsAsync(
                    It.Is<ProductStatsRequestDto>(r =>
                        r.ProductId == productId &&
                        r.StartDate == null &&
                        r.EndDate == null
                    )), Times.Once);
            }

            [Fact]
            public async Task GetProductStatsById_ProductNotFound_ReturnsEmptyStats()
            {
                // Arrange
                var productId = Guid.NewGuid();
                var expectedResponse = new ProductStatsResponseDto(
                    StartDate: DateTime.UtcNow.AddDays(-30),
                    EndDate: DateTime.UtcNow,
                    TotalSimulations: 0,
                    UniqueProducts: 0,
                    DailyStats: new List<ProductDailyStatsDto>()
                );

                _statsServiceMock
                    .Setup(x => x.GetProductDailyStatsAsync(It.IsAny<ProductStatsRequestDto>()))
                    .ReturnsAsync(expectedResponse);

                // Act
                var result = await _controller.GetProductStatsById(productId, null, null);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                var response = okResult.Value.Should().BeOfType<ProductStatsResponseDto>().Subject;
                response.TotalSimulations.Should().Be(0);
                response.UniqueProducts.Should().Be(0);
                response.DailyStats.Should().BeEmpty();
            }

            [Fact]
            public async Task GetProductStatsById_ServiceThrowsException_ReturnsInternalServerError()
            {
                // Arrange
                var productId = Guid.NewGuid();
                var exception = new Exception("Product not found in database");

                _statsServiceMock
                    .Setup(x => x.GetProductDailyStatsAsync(It.IsAny<ProductStatsRequestDto>()))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.GetProductStatsById(productId, null, null);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Erro interno ao gerar estatísticas do produto");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains($"Error getting stats for product {productId}")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        // Testes adicionais para casos de borda
        public class EdgeCaseTests : StatsControllerTests
        {
            [Fact]
            public async Task GetTopProducts_ZeroTopCount_ReturnsEmptyList()
            {
                // Arrange
                var topCount = 0;
                var expectedProducts = new List<ProductDailyStatsDto>();

                _statsServiceMock
                    .Setup(x => x.GetTopProductsAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), topCount))
                    .ReturnsAsync(expectedProducts);

                // Act
                var result = await _controller.GetTopProducts(null, null, topCount);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                var returnedProducts = okResult.Value.Should().BeOfType<List<ProductDailyStatsDto>>().Subject;
                returnedProducts.Should().BeEmpty();
            }

            [Fact]
            public async Task GetProductDailyStats_EndDateBeforeStartDate_ReturnsResult()
            {
                // Arrange
                var startDate = DateTime.UtcNow;
                var endDate = DateTime.UtcNow.AddDays(-30); // Data final antes da inicial

                var expectedResponse = new ProductStatsResponseDto(
                    StartDate: startDate,
                    EndDate: endDate,
                    TotalSimulations: 0,
                    UniqueProducts: 0,
                    DailyStats: new List<ProductDailyStatsDto>()
                );

                _statsServiceMock
                    .Setup(x => x.GetProductDailyStatsAsync(It.IsAny<ProductStatsRequestDto>()))
                    .ReturnsAsync(expectedResponse);

                // Act
                var result = await _controller.GetProductDailyStats(startDate, endDate, null, null);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedResponse);
            }

            [Fact]
            public async Task GetProductStatsById_WithFutureDates_ReturnsResult()
            {
                // Arrange
                var productId = Guid.NewGuid();
                var startDate = DateTime.UtcNow.AddDays(1); // Data futura
                var endDate = DateTime.UtcNow.AddDays(10); // Data futura

                var expectedResponse = new ProductStatsResponseDto(
                    StartDate: startDate,
                    EndDate: endDate,
                    TotalSimulations: 0,
                    UniqueProducts: 0,
                    DailyStats: new List<ProductDailyStatsDto>()
                );

                _statsServiceMock
                    .Setup(x => x.GetProductDailyStatsAsync(It.IsAny<ProductStatsRequestDto>()))
                    .ReturnsAsync(expectedResponse);

                // Act
                var result = await _controller.GetProductStatsById(productId, startDate, endDate);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedResponse);
            }
        }
    }
}
