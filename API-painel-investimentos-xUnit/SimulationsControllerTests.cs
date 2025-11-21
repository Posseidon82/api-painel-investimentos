using API_painel_investimentos.Controllers.Simulation;
using API_painel_investimentos.DTO.Simulation;
using API_painel_investimentos.Exceptions;
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
    public class SimulationsControllerTests
    {
        private readonly Mock<IInvestmentSimulationService> _simulationServiceMock;
        private readonly Mock<ILogger<SimulationsController>> _loggerMock;
        private readonly SimulationsController _controller;

        public SimulationsControllerTests()
        {
            _simulationServiceMock = new Mock<IInvestmentSimulationService>();
            _loggerMock = new Mock<ILogger<SimulationsController>>();
            _controller = new SimulationsController(
                _simulationServiceMock.Object,
                _loggerMock.Object
            );
        }

        public class SimulateInvestmentTests : SimulationsControllerTests
        {
            [Fact]
            public async Task SimulateInvestment_ValidRequest_ReturnsOkWithResult()
            {
                // Arrange
                var request = new SimulationRequestDto(
                    UserId: Guid.NewGuid(),
                    InvestedAmount: 10000m,
                    InvestmentMonths: 12,
                    ProductIds: new List<Guid> { Guid.NewGuid(), Guid.NewGuid() }
                );

                var expectedResult = new SimulationResultDto(
                    SimulationId: Guid.NewGuid(),
                    UserId: request.UserId,
                    ProfileType: "Moderate",
                    InvestedAmount: request.InvestedAmount,
                    InvestmentMonths: request.InvestmentMonths,
                    TotalReturn: 1250.50m,
                    NetReturn: 1125.45m,
                    TotalAmount: 11250.50m,
                    ReturnRate: 12.5m,
                    ProductSimulations: new List<ProductSimulationDto>
                    {
                        new ProductSimulationDto(
                            ProductId: Guid.NewGuid(),
                            ProductName: "CDB Banco X",
                            Category: "RendaFixa",
                            RiskLevel: "Baixo",
                            AllocatedAmount: 5000m,
                            ExpectedReturn: 10.5m,
                            GrossReturn: 525m,
                            Taxes: 78.75m,
                            NetReturn: 446.25m,
                            FinalAmount: 5446.25m,
                            SimulationDetails: "CDB com 110% do CDI"
                        ),
                        new ProductSimulationDto(
                            ProductId: Guid.NewGuid(),
                            ProductName: "Fundo Y",
                            Category: "Multimercado",
                            RiskLevel: "Médio",
                            AllocatedAmount: 5000m,
                            ExpectedReturn: 14.5m,
                            GrossReturn: 725m,
                            Taxes: 108.75m,
                            NetReturn: 616.25m,
                            FinalAmount: 5616.25m,
                            SimulationDetails: "Fundo multimercado com projeção conservadora"
                        )
                    },
                    SimulatedAt: DateTime.UtcNow
                );

                _simulationServiceMock
                    .Setup(x => x.SimulateInvestmentAsync(request))
                    .ReturnsAsync(expectedResult);

                // Act
                var result = await _controller.SimulateInvestment(request);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedResult);
                okResult.StatusCode.Should().Be(200);

                _simulationServiceMock.Verify(x => x.SimulateInvestmentAsync(request), Times.Once);
            }

            [Fact]
            public async Task SimulateInvestment_NullRequest_ReturnsBadRequest()
            {
                // Arrange
                SimulationRequestDto request = null;

                // Act
                var result = await _controller.SimulateInvestment(request);

                // Assert
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
                badRequestResult.Value.Should().Be("Invalid request body");
                badRequestResult.StatusCode.Should().Be(400);

                _simulationServiceMock.Verify(x => x.SimulateInvestmentAsync(It.IsAny<SimulationRequestDto>()), Times.Never);
            }

            [Fact]
            public async Task SimulateInvestment_InvalidAmount_ReturnsBadRequest()
            {
                // Arrange
                var request = new SimulationRequestDto(
                    UserId: Guid.NewGuid(),
                    InvestedAmount: 0m, // Valor inválido
                    InvestmentMonths: 12,
                    ProductIds: null
                );

                var exception = new ArgumentException("Invested amount must be greater than zero");

                _simulationServiceMock
                    .Setup(x => x.SimulateInvestmentAsync(request))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.SimulateInvestment(request);

                // Assert
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
                badRequestResult.Value.Should().Be("Invested amount must be greater than zero");
                badRequestResult.StatusCode.Should().Be(400);

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Invalid simulation request")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }

            [Fact]
            public async Task SimulateInvestment_InvalidMonths_ReturnsBadRequest()
            {
                // Arrange
                var request = new SimulationRequestDto(
                    UserId: Guid.NewGuid(),
                    InvestedAmount: 1000m,
                    InvestmentMonths: 0, // Valor inválido
                    ProductIds: null
                );

                var exception = new ArgumentException("Investment months must be at least 1");

                _simulationServiceMock
                    .Setup(x => x.SimulateInvestmentAsync(request))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.SimulateInvestment(request);

                // Assert
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
                badRequestResult.Value.Should().Be("Investment months must be at least 1");
                badRequestResult.StatusCode.Should().Be(400);

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Invalid simulation request")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }

            [Fact]
            public async Task SimulateInvestment_UserNotFound_ReturnsNotFound()
            {
                // Arrange
                var request = new SimulationRequestDto(
                    UserId: Guid.NewGuid(),
                    InvestedAmount: 10000m,
                    InvestmentMonths: 12,
                    ProductIds: null
                );

                var exception = new NotFoundException("User profile not found");

                _simulationServiceMock
                    .Setup(x => x.SimulateInvestmentAsync(request))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.SimulateInvestment(request);

                // Assert
                var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
                notFoundResult.Value.Should().Be("User profile not found");
                notFoundResult.StatusCode.Should().Be(404);

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("User profile not found")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }

            [Fact]
            public async Task SimulateInvestment_GenericException_ReturnsInternalServerError()
            {
                // Arrange
                var request = new SimulationRequestDto(
                    UserId: Guid.NewGuid(),
                    InvestedAmount: 10000m,
                    InvestmentMonths: 12,
                    ProductIds: null
                );

                var exception = new Exception("Database connection failed");

                _simulationServiceMock
                    .Setup(x => x.SimulateInvestmentAsync(request))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.SimulateInvestment(request);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Erro interno ao processar simulação");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error simulating investment")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        public class GetUserSimulationsTests : SimulationsControllerTests
        {
            [Fact]
            public async Task GetUserSimulations_UserHasSimulations_ReturnsOkWithList()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var expectedSimulations = new List<SimulationHistoryDto>
                {
                    new SimulationHistoryDto(
                        SimulationId: Guid.NewGuid(),
                        InvestedAmount: 10000m,
                        InvestmentMonths: 12,
                        TotalAmount: 11250.50m,
                        ReturnRate: 12.5m,
                        SimulatedAt: DateTime.UtcNow.AddDays(-10)
                    ),
                    new SimulationHistoryDto(
                        SimulationId: Guid.NewGuid(),
                        InvestedAmount: 5000m,
                        InvestmentMonths: 6,
                        TotalAmount: 5300.75m,
                        ReturnRate: 6.0m,
                        SimulatedAt: DateTime.UtcNow.AddDays(-5)
                    )
                };

                _simulationServiceMock
                    .Setup(x => x.GetUserSimulationsAsync(userId))
                    .ReturnsAsync(expectedSimulations);

                // Act
                var result = await _controller.GetUserSimulations(userId);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedSimulations);
                okResult.StatusCode.Should().Be(200);

                _simulationServiceMock.Verify(x => x.GetUserSimulationsAsync(userId), Times.Once);
            }

            [Fact]
            public async Task GetUserSimulations_NoSimulations_ReturnsOkWithEmptyList()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var expectedSimulations = new List<SimulationHistoryDto>();

                _simulationServiceMock
                    .Setup(x => x.GetUserSimulationsAsync(userId))
                    .ReturnsAsync(expectedSimulations);

                // Act
                var result = await _controller.GetUserSimulations(userId);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                var returnedSimulations = okResult.Value.Should().BeOfType<List<SimulationHistoryDto>>().Subject;

                returnedSimulations.Should().BeEmpty();
                okResult.StatusCode.Should().Be(200);

                _simulationServiceMock.Verify(x => x.GetUserSimulationsAsync(userId), Times.Once);
            }

            [Fact]
            public async Task GetUserSimulations_GenericException_ReturnsInternalServerError()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var exception = new Exception("Database query failed");

                _simulationServiceMock
                    .Setup(x => x.GetUserSimulationsAsync(userId))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.GetUserSimulations(userId);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Erro interno ao buscar simulações");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error getting simulations")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        public class GetSimulationTests : SimulationsControllerTests
        {
            [Fact]
            public async Task GetSimulation_SimulationExists_ReturnsOkWithSimulation()
            {
                // Arrange
                var simulationId = Guid.NewGuid();
                var expectedSimulation = new SimulationResultDto(
                    SimulationId: simulationId,
                    UserId: Guid.NewGuid(),
                    ProfileType: "Aggressive",
                    InvestedAmount: 20000m,
                    InvestmentMonths: 24,
                    TotalReturn: 5000m,
                    NetReturn: 4500m,
                    TotalAmount: 25000m,
                    ReturnRate: 25m,
                    ProductSimulations: new List<ProductSimulationDto>
                    {
                        new ProductSimulationDto(
                            ProductId: Guid.NewGuid(),
                            ProductName: "Ações Tech",
                            Category: "Ações",
                            RiskLevel: "Alto",
                            AllocatedAmount: 20000m,
                            ExpectedReturn: 30m,
                            GrossReturn: 6000m,
                            Taxes: 900m,
                            NetReturn: 5100m,
                            FinalAmount: 25100m,
                            SimulationDetails: "Simulação de ações setor tecnologia"
                        )
                    },
                    SimulatedAt: DateTime.UtcNow.AddDays(-3)
                );

                _simulationServiceMock
                    .Setup(x => x.GetSimulationByIdAsync(simulationId))
                    .ReturnsAsync(expectedSimulation);

                // Act
                var result = await _controller.GetSimulation(simulationId);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedSimulation);
                okResult.StatusCode.Should().Be(200);

                _simulationServiceMock.Verify(x => x.GetSimulationByIdAsync(simulationId), Times.Once);
            }

            [Fact]
            public async Task GetSimulation_SimulationNotFound_ReturnsNotFound()
            {
                // Arrange
                var simulationId = Guid.NewGuid();

                _simulationServiceMock
                    .Setup(x => x.GetSimulationByIdAsync(simulationId))
                    .ReturnsAsync((SimulationResultDto?)null);

                // Act
                var result = await _controller.GetSimulation(simulationId);

                // Assert
                var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
                notFoundResult.Value.Should().Be("Simulação não encontrada");
                notFoundResult.StatusCode.Should().Be(404);

                _simulationServiceMock.Verify(x => x.GetSimulationByIdAsync(simulationId), Times.Once);
            }

            [Fact]
            public async Task GetSimulation_GenericException_ReturnsInternalServerError()
            {
                // Arrange
                var simulationId = Guid.NewGuid();
                var exception = new Exception("Database connection timeout");

                _simulationServiceMock
                    .Setup(x => x.GetSimulationByIdAsync(simulationId))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.GetSimulation(simulationId);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Erro interno ao buscar simulação");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error getting simulation")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }
    }
}
