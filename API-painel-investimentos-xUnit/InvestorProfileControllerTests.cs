using API_painel_investimentos.Controllers.Profile;
using API_painel_investimentos.DTO.Profile;
using API_painel_investimentos.Exceptions;
using API_painel_investimentos.Services.Profile.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace API_painel_investimentos_xUnit
{
    public class InvestorProfileControllerTests
    {
        private readonly Mock<IInvestorProfileService> _profileServiceMock;
        private readonly Mock<ILogger<InvestorProfileController>> _loggerMock;
        private readonly InvestorProfileController _controller;

        public InvestorProfileControllerTests()
        {
            _profileServiceMock = new Mock<IInvestorProfileService>();
            _loggerMock = new Mock<ILogger<InvestorProfileController>>();
            _controller = new InvestorProfileController(
                _profileServiceMock.Object,
                _loggerMock.Object
            );
        }

        public class CalculateProfileTests : InvestorProfileControllerTests
        {
            [Fact]
            public async Task CalculateProfile_ValidRequest_ReturnsOkWithResult()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var answers = new List<UserAnswerDto>
                {
                    new UserAnswerDto(QuestionId: Guid.NewGuid(), AnswerOptionId: Guid.NewGuid()),
                    new UserAnswerDto(QuestionId: Guid.NewGuid(), AnswerOptionId: Guid.NewGuid())
                };

                var request = new CalculateProfileRequest(
                    UserId: userId,
                    Answers: answers
                );

                var expectedResult = new ProfileResultDto(
                    UserId: userId,
                    ProfileType: "Moderate",
                    Score: 75,
                    CalculatedAt: DateTime.UtcNow,
                    Answers: new List<UserAnswerDetailDto>
                    {
                        new UserAnswerDetailDto(
                            QuestionText: "Qual é sua idade?",
                            SelectedOption: "31-50 anos",
                            QuestionWeight: 10,
                            OptionScore: 2
                        ),
                        new UserAnswerDetailDto(
                            QuestionText: "Qual é sua tolerância ao risco?",
                            SelectedOption: "Média",
                            QuestionWeight: 20,
                            OptionScore: 3
                        )
                    }
                );

                _profileServiceMock
                    .Setup(x => x.CalculateProfileAsync(userId, answers))
                    .ReturnsAsync(expectedResult);

                // Act
                var result = await _controller.CalculateProfile(request);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedResult);
                okResult.StatusCode.Should().Be(200);

                _profileServiceMock.Verify(x => x.CalculateProfileAsync(userId, answers), Times.Once);
            }

            [Fact]
            public async Task CalculateProfile_NullRequest_ReturnsBadRequest()
            {
                // Arrange
                CalculateProfileRequest request = null;

                // Act
                var result = await _controller.CalculateProfile(request);

                // Assert
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
                badRequestResult.Value.Should().Be("Invalid request body");
                badRequestResult.StatusCode.Should().Be(400);

                _profileServiceMock.Verify(x => x.CalculateProfileAsync(It.IsAny<Guid>(), It.IsAny<List<UserAnswerDto>>()), Times.Never);
            }

            [Fact]
            public async Task CalculateProfile_EmptyGuid_ReturnsBadRequest()
            {
                // Arrange
                var request = new CalculateProfileRequest(
                    UserId: Guid.Empty,
                    Answers: new List<UserAnswerDto>()
                );

                var exception = new ArgumentException("Invalid UserId");

                _profileServiceMock
                    .Setup(x => x.CalculateProfileAsync(request.UserId, request.Answers))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.CalculateProfile(request);

                // Assert
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
                badRequestResult.Value.Should().Be("Invalid UserId");
                badRequestResult.StatusCode.Should().Be(400);

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Invalid arguments")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }

            [Fact]
            public async Task CalculateProfile_InvalidAnswers_ReturnsBadRequest()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var request = new CalculateProfileRequest(
                    UserId: userId,
                    Answers: new List<UserAnswerDto>() // Lista vazia
                );

                var exception = new ArgumentException("At least one answer is required");

                _profileServiceMock
                    .Setup(x => x.CalculateProfileAsync(userId, request.Answers))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.CalculateProfile(request);

                // Assert
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
                badRequestResult.Value.Should().Be("At least one answer is required");
                badRequestResult.StatusCode.Should().Be(400);

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Invalid arguments")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }

            [Fact]
            public async Task CalculateProfile_GenericException_ReturnsInternalServerError()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var request = new CalculateProfileRequest(
                    UserId: userId,
                    Answers: new List<UserAnswerDto>
                    {
                        new UserAnswerDto(QuestionId: Guid.NewGuid(), AnswerOptionId: Guid.NewGuid())
                    }
                );

                var exception = new Exception("Database connection failed");

                _profileServiceMock
                    .Setup(x => x.CalculateProfileAsync(userId, request.Answers))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.CalculateProfile(request);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Internal server error");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error calculating profile")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        public class GetProfileTests : InvestorProfileControllerTests
        {
            [Fact]
            public async Task GetProfile_UserExists_ReturnsOkWithProfile()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var expectedProfile = new ProfileResultDto(
                    UserId: userId,
                    ProfileType: "Conservative",
                    Score: 45,
                    CalculatedAt: DateTime.UtcNow.AddDays(-1),
                    Answers: new List<UserAnswerDetailDto>
                    {
                        new UserAnswerDetailDto(
                            QuestionText: "Qual é sua idade?",
                            SelectedOption: "Acima de 50",
                            QuestionWeight: 10,
                            OptionScore: 3
                        )
                    }
                );

                _profileServiceMock
                    .Setup(x => x.GetUserProfileAsync(userId))
                    .ReturnsAsync(expectedProfile);

                // Act
                var result = await _controller.GetProfile(userId);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedProfile);
                okResult.StatusCode.Should().Be(200);

                _profileServiceMock.Verify(x => x.GetUserProfileAsync(userId), Times.Once);
            }

            [Fact]
            public async Task GetProfile_NotFoundException_ReturnsNotFound()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var exception = new NotFoundException("Perfil não encontrado");

                _profileServiceMock
                    .Setup(x => x.GetUserProfileAsync(userId))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.GetProfile(userId);

                // Assert
                var notFoundResult = result.Should().BeOfType<NotFoundResult>().Subject;
                notFoundResult.StatusCode.Should().Be(404);

                _profileServiceMock.Verify(x => x.GetUserProfileAsync(userId), Times.Once);
            }

            [Fact]
            public async Task GetProfile_GenericException_ReturnsInternalServerError()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var exception = new Exception("Database connection lost");

                _profileServiceMock
                    .Setup(x => x.GetUserProfileAsync(userId))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.GetProfile(userId);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Internal server error");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error getting profile for user")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }

            [Fact]
            public async Task GetProfile_EmptyGuid_ReturnsNotFound()
            {
                // Arrange
                var userId = Guid.Empty;
                var exception = new NotFoundException("Perfil não encontrado");

                _profileServiceMock
                    .Setup(x => x.GetUserProfileAsync(userId))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.GetProfile(userId);

                // Assert
                var notFoundResult = result.Should().BeOfType<NotFoundResult>().Subject;
                notFoundResult.StatusCode.Should().Be(404);

                _profileServiceMock.Verify(x => x.GetUserProfileAsync(userId), Times.Once);
            }
        }
    }
}
