using API_painel_investimentos.Controllers.Portfolio;
using API_painel_investimentos.DTO.Portfolio;
using API_painel_investimentos.Exceptions;
using API_painel_investimentos.Models.Portfolio;
using API_painel_investimentos.Services.Portfolio.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace API_painel_investimentos_xUnit
{
    public class RecommendationsControllerTests
    {
        private readonly Mock<IInvestmentRecommendationService> _recommendationServiceMock;
        private readonly Mock<ILogger<RecommendationsController>> _loggerMock;
        private readonly RecommendationsController _controller;

        public RecommendationsControllerTests()
        {
            _recommendationServiceMock = new Mock<IInvestmentRecommendationService>();
            _loggerMock = new Mock<ILogger<RecommendationsController>>();
            _controller = new RecommendationsController(
                _recommendationServiceMock.Object,
                _loggerMock.Object
            );
        }

        public class GetUserRecommendationsTests : RecommendationsControllerTests
        {
            [Fact]
            public async Task GetUserRecommendations_UserExists_ReturnsOkWithRecommendations()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var expectedRecommendations = new RecommendationResultDto(
                    UserId: userId,
                    ProfileType: "Moderate",
                    ProfileScore: 75,
                    RecommendedProducts: new List<InvestmentProductDto>
                    {
                        new InvestmentProductDto(
                            Id: Guid.NewGuid(),
                            Name: "CDB Banco X",
                            Description: "CDB com 110% CDI",
                            Category: "RendaFixa",
                            RiskLevel: "Baixo",
                            MinimumInvestment: 1000m,
                            LiquidityDays: 30,
                            AdministrationFee: 0.01m,
                            ExpectedReturn: 10.5m,
                            Issuer: "Banco X"
                        )
                    },
                    SuggestedAllocation: new PortfolioAllocationDto(
                        ConservativePercentage: 40,
                        ModeratePercentage: 40,
                        AggressivePercentage: 20
                    )
                );

                _recommendationServiceMock
                    .Setup(x => x.GetRecommendationsAsync(userId))
                    .ReturnsAsync(expectedRecommendations);

                // Act
                var result = await _controller.GetUserRecommendations(userId);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedRecommendations);
                okResult.StatusCode.Should().Be(200);

                _recommendationServiceMock.Verify(x => x.GetRecommendationsAsync(userId), Times.Once);
            }

            [Fact]
            public async Task GetUserRecommendations_NotFoundException_ReturnsNotFound()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var exception = new NotFoundException("Perfil do usuário não encontrado");

                _recommendationServiceMock
                    .Setup(x => x.GetRecommendationsAsync(userId))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.GetUserRecommendations(userId);

                // Assert
                var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
                notFoundResult.Value.Should().Be("Perfil do usuário não encontrado");
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
            public async Task GetUserRecommendations_GenericException_ReturnsInternalServerError()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var exception = new Exception("Database connection failed");

                _recommendationServiceMock
                    .Setup(x => x.GetRecommendationsAsync(userId))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.GetUserRecommendations(userId);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Internal server error");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error generating investment recommendations")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        public class GetRecommendationsByProfileTests : RecommendationsControllerTests
        {
            [Fact]
            public async Task GetRecommendationsByProfile_WithUserId_ReturnsOkWithRecommendations()
            {
                // Arrange
                Guid? userId = Guid.NewGuid(); // Corrigido: usar Guid? em vez de Guid
                var request = new RecommendationRequestDto(
                    UserId: userId,
                    ProfileType: null,
                    AvailableAmount: 0
                );

                var expectedRecommendations = new RecommendationResultDto(
                    UserId: userId.Value,
                    ProfileType: "Moderate",
                    ProfileScore: 75,
                    RecommendedProducts: new List<InvestmentProductDto>
                    {
                        new InvestmentProductDto(
                            Id: Guid.NewGuid(),
                            Name: "Fundo Y",
                            Description: "Fundo Multimercado",
                            Category: "Fundos",
                            RiskLevel: "Médio",
                            MinimumInvestment: 5000m,
                            LiquidityDays: 1,
                            AdministrationFee: 2.0m,
                            ExpectedReturn: 12.5m,
                            Issuer: "Gestora Z"
                        )
                    },
                    SuggestedAllocation: new PortfolioAllocationDto(
                        ConservativePercentage: 40,
                        ModeratePercentage: 40,
                        AggressivePercentage: 20
                    )
                );

                _recommendationServiceMock
                    .Setup(x => x.GetRecommendationsAsync(userId.Value)) // Usando userId.Value (que é Nullable)
                    .ReturnsAsync(expectedRecommendations);

                // Act
                var result = await _controller.GetRecommendationsByProfile(request);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedRecommendations);
                okResult.StatusCode.Should().Be(200);

                _recommendationServiceMock.Verify(x => x.GetRecommendationsAsync(userId.Value), Times.Once);
                _recommendationServiceMock.Verify(x => x.GetRecommendationsByProfileAsync(It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
            }

            [Fact]
            public async Task GetRecommendationsByProfile_WithProfileType_ReturnsOkWithRecommendations()
            {
                // Arrange
                var request = new RecommendationRequestDto(
                    UserId: null,
                    ProfileType: "Aggressive",
                    AvailableAmount: 10000m
                );

                var expectedRecommendations = new RecommendationResultDto(
                    UserId: Guid.Empty,
                    ProfileType: "Aggressive",
                    ProfileScore: 85,
                    RecommendedProducts: new List<InvestmentProductDto>
                    {
                        new InvestmentProductDto(
                            Id: Guid.NewGuid(),
                            Name: "Ações Setor Tech",
                            Description: "Ações de empresas de tecnologia",
                            Category: "Acoes",
                            RiskLevel: "Alto",
                            MinimumInvestment: 1000m,
                            LiquidityDays: 0,
                            AdministrationFee: 0.5m,
                            ExpectedReturn: 15.0m,
                            Issuer: "Bolsa"
                        )
                    },
                    SuggestedAllocation: new PortfolioAllocationDto(
                        ConservativePercentage: 20,
                        ModeratePercentage: 30,
                        AggressivePercentage: 50
                    )
                );

                _recommendationServiceMock
                    .Setup(x => x.GetRecommendationsByProfileAsync(request.ProfileType, request.AvailableAmount))
                    .ReturnsAsync(expectedRecommendations);

                // Act
                var result = await _controller.GetRecommendationsByProfile(request);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedRecommendations);
                okResult.StatusCode.Should().Be(200);

                _recommendationServiceMock.Verify(x => x.GetRecommendationsByProfileAsync(request.ProfileType, request.AvailableAmount), Times.Once);
                _recommendationServiceMock.Verify(x => x.GetRecommendationsAsync(It.IsAny<Guid>()), Times.Never);
            }

            [Fact]
            public async Task GetRecommendationsByProfile_BothParametersNull_ReturnsBadRequest()
            {
                // Arrange
                var request = new RecommendationRequestDto(
                    UserId: null,
                    ProfileType: null,
                    AvailableAmount: 0
                );

                // Act
                var result = await _controller.GetRecommendationsByProfile(request);

                // Assert
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
                badRequestResult.Value.Should().Be("Must provide either UserId or ProfileType");
                badRequestResult.StatusCode.Should().Be(400);

                _recommendationServiceMock.Verify(x => x.GetRecommendationsAsync(It.IsAny<Guid>()), Times.Never);
                _recommendationServiceMock.Verify(x => x.GetRecommendationsByProfileAsync(It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
            }

            [Fact]
            public async Task GetRecommendationsByProfile_EmptyProfileType_ReturnsBadRequest()
            {
                // Arrange
                var request = new RecommendationRequestDto(
                    UserId: null,
                    ProfileType: "",
                    AvailableAmount: 0
                );

                // Act
                var result = await _controller.GetRecommendationsByProfile(request);

                // Assert
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
                badRequestResult.Value.Should().Be("Must provide either UserId or ProfileType");
                badRequestResult.StatusCode.Should().Be(400);
            }

            [Fact]
            public async Task GetRecommendationsByProfile_ServiceThrowsException_ReturnsInternalServerError()
            {
                // Arrange
                Guid? userId = Guid.NewGuid(); // Corrigido: usar Guid? em vez de Guid
                var request = new RecommendationRequestDto(
                    UserId: userId,
                    ProfileType: null,
                    AvailableAmount: 0
                );

                var exception = new Exception("Service error");

                _recommendationServiceMock
                    .Setup(x => x.GetRecommendationsAsync(userId.Value)) // Usando userId.Value (que é Nullable)
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.GetRecommendationsByProfile(request);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Internal server error");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error generating investment recommendations")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        public class GetProductsByProfileTests : RecommendationsControllerTests
        {
            [Fact]
            public async Task GetProductsByProfile_ValidProfile_ReturnsOkWithProducts()
            {
                // Arrange
                var profileType = "Conservative";
                var products = new List<InvestmentProduct>
                {
                    new InvestmentProduct(
                        name: "Tesouro Selic",
                        description: "Título público atrelado à Selic",
                        category: "TesouroDireto",
                        riskLevel: "Baixo",
                        minimumInvestment: 100m,
                        liquidityDays: 0,
                        targetProfile: "Conservative",
                        administrationFee: 0m,
                        expectedReturn: 9.5m,
                        issuer: "Tesouro Nacional"
                    ),
                    new InvestmentProduct(
                        name: "CDB 100% CDI",
                        description: "CDB seguro",
                        category: "RendaFixa",
                        riskLevel: "Baixo",
                        minimumInvestment: 1000m,
                        liquidityDays: 30,
                        targetProfile: "Conservative",
                        administrationFee: 0.01m,
                        expectedReturn: 10.5m,
                        issuer: "Banco Y"
                    )
                };

                _recommendationServiceMock
                    .Setup(x => x.GetProductsByProfileAsync(profileType))
                    .ReturnsAsync(products);

                // Act
                var result = await _controller.GetProductsByProfile(profileType);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                var returnedProducts = okResult.Value.Should().BeOfType<List<InvestmentProductDto>>().Subject;

                returnedProducts.Should().HaveCount(2);
                returnedProducts[0].Name.Should().Be("Tesouro Selic");
                returnedProducts[0].Category.Should().Be("TesouroDireto");
                returnedProducts[1].Name.Should().Be("CDB 100% CDI");
                returnedProducts[1].MinimumInvestment.Should().Be(1000m);

                okResult.StatusCode.Should().Be(200);

                _recommendationServiceMock.Verify(x => x.GetProductsByProfileAsync(profileType), Times.Once);
            }

            [Fact]
            public async Task GetProductsByProfile_EmptyProfile_ReturnsOkWithEmptyList()
            {
                // Arrange
                var profileType = "UnknownProfile";
                var products = new List<InvestmentProduct>();

                _recommendationServiceMock
                    .Setup(x => x.GetProductsByProfileAsync(profileType))
                    .ReturnsAsync(products);

                // Act
                var result = await _controller.GetProductsByProfile(profileType);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                var returnedProducts = okResult.Value.Should().BeOfType<List<InvestmentProductDto>>().Subject;

                returnedProducts.Should().BeEmpty();
                okResult.StatusCode.Should().Be(200);
            }

            [Fact]
            public async Task GetProductsByProfile_ServiceThrowsException_ReturnsInternalServerError()
            {
                // Arrange
                var profileType = "Moderate";
                var exception = new Exception("Database connection failed");

                _recommendationServiceMock
                    .Setup(x => x.GetProductsByProfileAsync(profileType))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.GetProductsByProfile(profileType);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Internal server error");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error getting products for profile")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }
    }
}
