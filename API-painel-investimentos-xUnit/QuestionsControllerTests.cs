using API_painel_investimentos.Controllers.Profile;
using API_painel_investimentos.DTO.Profile;
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
    public class QuestionsControllerTests
    {
        private readonly Mock<IQuestionService> _questionServiceMock;
        private readonly Mock<ILogger<QuestionsController>> _loggerMock;
        private readonly QuestionsController _controller;

        public QuestionsControllerTests()
        {
            _questionServiceMock = new Mock<IQuestionService>();
            _loggerMock = new Mock<ILogger<QuestionsController>>();
            _controller = new QuestionsController(
                _questionServiceMock.Object,
                _loggerMock.Object // Corrigido: adicionado logger no construtor
            );
        }

        public class GetActiveQuestionsTests : QuestionsControllerTests
        {
            [Fact]
            public async Task GetActiveQuestions_QuestionsExist_ReturnsOkWithQuestions()
            {
                // Arrange
                var expectedQuestions = new List<QuestionDto>
                {
                    new QuestionDto(
                        Id: Guid.NewGuid(),
                        QuestionText: "Qual é sua idade?",
                        Category: "Perfil",
                        Weight: 10,
                        Order: 1,
                        AnswerOptions: new List<AnswerOptionDto>
                        {
                            new AnswerOptionDto(
                                Id: Guid.NewGuid(),
                                OptionText: "18-30 anos",
                                Description: "Jovem adulto"
                            ),
                            new AnswerOptionDto(
                                Id: Guid.NewGuid(),
                                OptionText: "31-50 anos",
                                Description: "Adulto"
                            )
                        }
                    ),
                    new QuestionDto(
                        Id: Guid.NewGuid(),
                        QuestionText: "Qual é sua renda mensal?",
                        Category: "Financeiro",
                        Weight: 15,
                        Order: 2,
                        AnswerOptions: new List<AnswerOptionDto>
                        {
                            new AnswerOptionDto(
                                Id: Guid.NewGuid(),
                                OptionText: "Até R$ 3.000",
                                Description: "Baixa renda"
                            ),
                            new AnswerOptionDto(
                                Id: Guid.NewGuid(),
                                OptionText: "R$ 3.001 - R$ 10.000",
                                Description: "Renda média"
                            )
                        }
                    )
                };

                _questionServiceMock
                    .Setup(x => x.GetActiveQuestionsAsync())
                    .ReturnsAsync(expectedQuestions);

                // Act
                var result = await _controller.GetActiveQuestions();

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedQuestions);
                okResult.StatusCode.Should().Be(200);

                _questionServiceMock.Verify(x => x.GetActiveQuestionsAsync(), Times.Once);
            }

            [Fact]
            public async Task GetActiveQuestions_NoQuestions_ReturnsOkWithEmptyList()
            {
                // Arrange
                var expectedQuestions = new List<QuestionDto>();

                _questionServiceMock
                    .Setup(x => x.GetActiveQuestionsAsync())
                    .ReturnsAsync(expectedQuestions);

                // Act
                var result = await _controller.GetActiveQuestions();

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                var returnedQuestions = okResult.Value.Should().BeOfType<List<QuestionDto>>().Subject;

                returnedQuestions.Should().BeEmpty();
                okResult.StatusCode.Should().Be(200);

                _questionServiceMock.Verify(x => x.GetActiveQuestionsAsync(), Times.Once);
            }

            [Fact]
            public async Task GetActiveQuestions_ServiceThrowsException_ReturnsInternalServerError()
            {
                // Arrange
                var exception = new Exception("Database connection failed");

                _questionServiceMock
                    .Setup(x => x.GetActiveQuestionsAsync())
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.GetActiveQuestions();

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Internal server error");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error retrieving active questions")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        public class GetQuestionTests : QuestionsControllerTests
        {
            [Fact]
            public async Task GetQuestion_QuestionExists_ReturnsOkWithQuestion()
            {
                // Arrange
                var questionId = Guid.NewGuid();
                var expectedQuestion = new QuestionDto(
                    Id: questionId,
                    QuestionText: "Qual é sua tolerância ao risco?",
                    Category: "Risco",
                    Weight: 20,
                    Order: 3,
                    AnswerOptions: new List<AnswerOptionDto>
                    {
                        new AnswerOptionDto(
                            Id: Guid.NewGuid(),
                            OptionText: "Baixa",
                            Description: "Evita riscos"
                        ),
                        new AnswerOptionDto(
                            Id: Guid.NewGuid(),
                            OptionText: "Média",
                            Description: "Aceita riscos moderados"
                        ),
                        new AnswerOptionDto(
                            Id: Guid.NewGuid(),
                            OptionText: "Alta",
                            Description: "Busca altos retornos"
                        )
                    }
                );

                _questionServiceMock
                    .Setup(x => x.GetQuestionByIdAsync(questionId))
                    .ReturnsAsync(expectedQuestion);

                // Act
                var result = await _controller.GetQuestion(questionId);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedQuestion);
                okResult.StatusCode.Should().Be(200);

                _questionServiceMock.Verify(x => x.GetQuestionByIdAsync(questionId), Times.Once);
            }

            [Fact]
            public async Task GetQuestion_QuestionNotFound_ReturnsNotFound()
            {
                // Arrange
                var questionId = Guid.NewGuid();

                _questionServiceMock
                    .Setup(x => x.GetQuestionByIdAsync(questionId))
                    .ReturnsAsync((QuestionDto?)null);

                // Act
                var result = await _controller.GetQuestion(questionId);

                // Assert
                var notFoundResult = result.Should().BeOfType<NotFoundResult>().Subject;
                notFoundResult.StatusCode.Should().Be(404);

                _questionServiceMock.Verify(x => x.GetQuestionByIdAsync(questionId), Times.Once);
            }

            [Fact]
            public async Task GetQuestion_ServiceThrowsException_ReturnsInternalServerError()
            {
                // Arrange
                var questionId = Guid.NewGuid();
                var exception = new Exception("Database query timeout");

                _questionServiceMock
                    .Setup(x => x.GetQuestionByIdAsync(questionId))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.GetQuestion(questionId);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Internal server error");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error retrieving question by ID")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }
    }
}
