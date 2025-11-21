using API_painel_investimentos.Controllers.User;
using API_painel_investimentos.DTO.User;
using API_painel_investimentos.Exceptions;
using API_painel_investimentos.Services.User.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace API_painel_investimentos_xUnit
{
    public class UsersControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ILogger<UsersController>> _loggerMock;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _loggerMock = new Mock<ILogger<UsersController>>();
            _controller = new UsersController(
                _userServiceMock.Object,
                _loggerMock.Object
            );
        }

        public class CreateUserTests : UsersControllerTests
        {
            [Fact]
            public async Task CreateUser_ValidRequest_ReturnsCreatedAtAction()
            {
                // Arrange
                var request = new CreateUserRequestDto(
                    Name: "Test User",
                    Cpf: "12345678900",
                    Email: "test@example.com",
                    Password: "ValidPass123!"
                );

                var expectedResponse = new CreateUserResponseDto(
                    UserId: Guid.NewGuid(),
                    Name: request.Name,
                    Cpf: request.Cpf,
                    Email: request.Email,
                    CreatedAt: DateTime.UtcNow
                );

                _userServiceMock
                    .Setup(x => x.CreateUserAsync(request))
                    .ReturnsAsync(expectedResponse);

                // Act
                var result = await _controller.CreateUser(request);

                // Assert
                var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
                createdResult.ActionName.Should().Be(nameof(_controller.GetUserById));
                createdResult.RouteValues["userId"].Should().Be(expectedResponse.UserId);
                createdResult.Value.Should().BeEquivalentTo(expectedResponse);
                createdResult.StatusCode.Should().Be(201);

                _userServiceMock.Verify(x => x.CreateUserAsync(request), Times.Once);
            }

            [Fact]
            public async Task CreateUser_ArgumentException_ReturnsBadRequest()
            {
                // Arrange
                var request = new CreateUserRequestDto(
                    Name: "Test User",
                    Cpf: "12345678900",
                    Email: "test@example.com",
                    Password: "ValidPass123!"
                );

                var exception = new ArgumentException("CPF inválido");

                _userServiceMock
                    .Setup(x => x.CreateUserAsync(request))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.CreateUser(request);

                // Assert
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
                badRequestResult.Value.Should().Be("CPF inválido");
                badRequestResult.StatusCode.Should().Be(400);

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Invalid user creation request")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }

            [Fact]
            public async Task CreateUser_GenericException_ReturnsInternalServerError()
            {
                // Arrange
                var request = new CreateUserRequestDto(
                    Name: "Test User",
                    Cpf: "12345678900",
                    Email: "test@example.com",
                    Password: "ValidPass123!"
                );

                var exception = new Exception("Database connection failed");

                _userServiceMock
                    .Setup(x => x.CreateUserAsync(request))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.CreateUser(request);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Erro interno ao criar usuário");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error creating user")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        public class GetUserByIdTests : UsersControllerTests
        {
            [Fact]
            public async Task GetUserById_UserFound_ReturnsOkWithUser()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var expectedUser = new UserResponseDto(
                    UserId: userId,
                    Name: "Test User",
                    Cpf: "12345678900",
                    Email: "test@example.com",
                    IsActive: true,
                    CreatedAt: DateTime.UtcNow.AddDays(-1)
                );

                _userServiceMock
                    .Setup(x => x.GetUserByIdAsync(userId))
                    .ReturnsAsync(expectedUser);

                // Act
                var result = await _controller.GetUserById(userId);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedUser);
                okResult.StatusCode.Should().Be(200);

                _userServiceMock.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
            }

            [Fact]
            public async Task GetUserById_UserNotFound_ReturnsNotFound()
            {
                // Arrange
                var userId = Guid.NewGuid();

                _userServiceMock
                    .Setup(x => x.GetUserByIdAsync(userId))
                    .ReturnsAsync((UserResponseDto?)null);

                // Act
                var result = await _controller.GetUserById(userId);

                // Assert
                var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
                notFoundResult.Value.Should().Be("Usuário não encontrado");
                notFoundResult.StatusCode.Should().Be(404);

                _userServiceMock.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
            }

            [Fact]
            public async Task GetUserById_ServiceThrowsException_ReturnsInternalServerError()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var exception = new Exception("Database query failed");

                _userServiceMock
                    .Setup(x => x.GetUserByIdAsync(userId))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.GetUserById(userId);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Erro interno ao buscar usuário");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error getting user by ID")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        public class GetUserByCpfTests : UsersControllerTests
        {
            [Fact]
            public async Task GetUserByCpf_UserFound_ReturnsOkWithUser()
            {
                // Arrange
                var cpf = "12345678900";
                var expectedUser = new UserResponseDto(
                    UserId: Guid.NewGuid(),
                    Name: "Test User",
                    Cpf: cpf,
                    Email: "test@example.com",
                    IsActive: true,
                    CreatedAt: DateTime.UtcNow.AddDays(-1)
                );

                _userServiceMock
                    .Setup(x => x.GetUserByCpfAsync(cpf))
                    .ReturnsAsync(expectedUser);

                // Act
                var result = await _controller.GetUserByCpf(cpf);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedUser);
                okResult.StatusCode.Should().Be(200);

                _userServiceMock.Verify(x => x.GetUserByCpfAsync(cpf), Times.Once);
            }

            [Fact]
            public async Task GetUserByCpf_UserNotFound_ReturnsNotFound()
            {
                // Arrange
                var cpf = "12345678900";

                _userServiceMock
                    .Setup(x => x.GetUserByCpfAsync(cpf))
                    .ReturnsAsync((UserResponseDto?)null);

                // Act
                var result = await _controller.GetUserByCpf(cpf);

                // Assert
                var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
                notFoundResult.Value.Should().Be("Usuário não encontrado");
                notFoundResult.StatusCode.Should().Be(404);

                _userServiceMock.Verify(x => x.GetUserByCpfAsync(cpf), Times.Once);
            }

            [Fact]
            public async Task GetUserByCpf_ServiceThrowsException_ReturnsInternalServerError()
            {
                // Arrange
                var cpf = "12345678900";
                var exception = new Exception("Database connection error");

                _userServiceMock
                    .Setup(x => x.GetUserByCpfAsync(cpf))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.GetUserByCpf(cpf);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Erro interno ao buscar usuário");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error getting user by CPF")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        public class GetUserByEmailTests : UsersControllerTests
        {
            [Fact]
            public async Task GetUserByEmail_UserFound_ReturnsOkWithUser()
            {
                // Arrange
                var email = "test@example.com";
                var expectedUser = new UserResponseDto(
                    UserId: Guid.NewGuid(),
                    Name: "Test User",
                    Cpf: "12345678900",
                    Email: email,
                    IsActive: true,
                    CreatedAt: DateTime.UtcNow.AddDays(-1)
                );

                _userServiceMock
                    .Setup(x => x.GetUserByEmailAsync(email))
                    .ReturnsAsync(expectedUser);

                // Act
                var result = await _controller.GetUserByEmail(email);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedUser);
                okResult.StatusCode.Should().Be(200);

                _userServiceMock.Verify(x => x.GetUserByEmailAsync(email), Times.Once);
            }

            [Fact]
            public async Task GetUserByEmail_UserNotFound_ReturnsNotFound()
            {
                // Arrange
                var email = "test@example.com";

                _userServiceMock
                    .Setup(x => x.GetUserByEmailAsync(email))
                    .ReturnsAsync((UserResponseDto?)null);

                // Act
                var result = await _controller.GetUserByEmail(email);

                // Assert
                var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
                notFoundResult.Value.Should().Be("Usuário não encontrado");
                notFoundResult.StatusCode.Should().Be(404);

                _userServiceMock.Verify(x => x.GetUserByEmailAsync(email), Times.Once);
            }

            [Fact]
            public async Task GetUserByEmail_ServiceThrowsException_ReturnsInternalServerError()
            {
                // Arrange
                var email = "test@example.com";
                var exception = new Exception("Database query timeout");

                _userServiceMock
                    .Setup(x => x.GetUserByEmailAsync(email))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.GetUserByEmail(email);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Erro interno ao buscar usuário");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error getting user by Email")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        public class CheckUserExistsTests : UsersControllerTests
        {
            [Fact]
            public async Task CheckUserExists_UserFound_ReturnsOkWithResult()
            {
                // Arrange
                var request = new CheckUserExistsRequestDto(
                    Password: "ValidPass123!",
                    Cpf: "12345678900",
                    Email: null
                );

                var expectedResponse = new CheckUserExistsResponseDto(
                    Exists: true,
                    IsValidCredentials: true,
                    UserId: Guid.NewGuid(),
                    Message: "Usuário encontrado"
                );

                _userServiceMock
                    .Setup(x => x.CheckUserExistsAsync(request))
                    .ReturnsAsync(expectedResponse);

                // Act
                var result = await _controller.CheckUserExists(request);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedResponse);
                okResult.StatusCode.Should().Be(200);

                _userServiceMock.Verify(x => x.CheckUserExistsAsync(request), Times.Once);
            }

            [Fact]
            public async Task CheckUserExists_UserNotFound_ReturnsOkWithFalseResult()
            {
                // Arrange
                var request = new CheckUserExistsRequestDto(
                    Password: "ValidPass123!",
                    Cpf: "12345678900",
                    Email: null
                );

                var expectedResponse = new CheckUserExistsResponseDto(
                    Exists: false,
                    IsValidCredentials: false,
                    UserId: null,
                    Message: "Usuário não encontrado"
                );

                _userServiceMock
                    .Setup(x => x.CheckUserExistsAsync(request))
                    .ReturnsAsync(expectedResponse);

                // Act
                var result = await _controller.CheckUserExists(request);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedResponse);
                okResult.StatusCode.Should().Be(200);
            }

            [Fact]
            public async Task CheckUserExists_ServiceThrowsException_ReturnsInternalServerError()
            {
                // Arrange
                var request = new CheckUserExistsRequestDto(
                    Password: "ValidPass123!",
                    Cpf: "12345678900",
                    Email: null
                );

                var exception = new Exception("Service unavailable");

                _userServiceMock
                    .Setup(x => x.CheckUserExistsAsync(request))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.CheckUserExists(request);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Erro interno ao verificar usuário");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error checking if user exists")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        public class UpdateUserTests : UsersControllerTests
        {
            [Fact]
            public async Task UpdateUser_ValidRequest_ReturnsNoContent()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var request = new UpdateUserRequestDto(
                    Name: "Updated Name",
                    Email: "updated@example.com"
                );

                _userServiceMock
                    .Setup(x => x.UpdateUserAsync(userId, request))
                    .Returns(Task.CompletedTask);

                // Act
                var result = await _controller.UpdateUser(userId, request);

                // Assert
                var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
                noContentResult.StatusCode.Should().Be(204);

                _userServiceMock.Verify(x => x.UpdateUserAsync(userId, request), Times.Once);
            }

            [Fact]
            public async Task UpdateUser_NotFoundException_ReturnsNotFound()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var request = new UpdateUserRequestDto(
                    Name: "Updated Name",
                    Email: "updated@example.com"
                );

                var exception = new NotFoundException("Usuário não encontrado");

                _userServiceMock
                    .Setup(x => x.UpdateUserAsync(userId, request))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.UpdateUser(userId, request);

                // Assert
                var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
                notFoundResult.Value.Should().Be("Usuário não encontrado");
                notFoundResult.StatusCode.Should().Be(404);

                _userServiceMock.Verify(x => x.UpdateUserAsync(userId, request), Times.Once);
            }

            [Fact]
            public async Task UpdateUser_ArgumentException_ReturnsBadRequest()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var request = new UpdateUserRequestDto(
                    Name: "Updated Name",
                    Email: "updated@example.com"
                );

                var exception = new ArgumentException("Email inválido");

                _userServiceMock
                    .Setup(x => x.UpdateUserAsync(userId, request))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.UpdateUser(userId, request);

                // Assert
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
                badRequestResult.Value.Should().Be("Email inválido");
                badRequestResult.StatusCode.Should().Be(400);

                _userServiceMock.Verify(x => x.UpdateUserAsync(userId, request), Times.Once);
            }

            [Fact]
            public async Task UpdateUser_GenericException_ReturnsInternalServerError()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var request = new UpdateUserRequestDto(
                    Name: "Updated Name",
                    Email: "updated@example.com"
                );

                var exception = new Exception("Database update failed");

                _userServiceMock
                    .Setup(x => x.UpdateUserAsync(userId, request))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.UpdateUser(userId, request);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Erro interno ao atualizar usuário");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error updating user")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        public class ChangePasswordTests : UsersControllerTests
        {
            [Fact]
            public async Task ChangePassword_ValidRequest_ReturnsNoContent()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var request = new ChangePasswordRequestDto(
                    CurrentPassword: "OldPass123!",
                    NewPassword: "NewPass123!"
                );

                _userServiceMock
                    .Setup(x => x.ChangePasswordAsync(userId, request))
                    .Returns(Task.CompletedTask);

                // Act
                var result = await _controller.ChangePassword(userId, request);

                // Assert
                var noContentResult = result.Should().BeOfType<NoContentResult>().Subject;
                noContentResult.StatusCode.Should().Be(204);

                _userServiceMock.Verify(x => x.ChangePasswordAsync(userId, request), Times.Once);
            }

            [Fact]
            public async Task ChangePassword_NotFoundException_ReturnsNotFound()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var request = new ChangePasswordRequestDto(
                    CurrentPassword: "OldPass123!",
                    NewPassword: "NewPass123!"
                );

                var exception = new NotFoundException("Usuário não encontrado");

                _userServiceMock
                    .Setup(x => x.ChangePasswordAsync(userId, request))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.ChangePassword(userId, request);

                // Assert
                var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
                notFoundResult.Value.Should().Be("Usuário não encontrado");
                notFoundResult.StatusCode.Should().Be(404);

                _userServiceMock.Verify(x => x.ChangePasswordAsync(userId, request), Times.Once);
            }

            [Fact]
            public async Task ChangePassword_ArgumentException_ReturnsBadRequest()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var request = new ChangePasswordRequestDto(
                    CurrentPassword: "OldPass123!",
                    NewPassword: "NewPass123!"
                );

                var exception = new ArgumentException("Senha atual incorreta");

                _userServiceMock
                    .Setup(x => x.ChangePasswordAsync(userId, request))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.ChangePassword(userId, request);

                // Assert
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
                badRequestResult.Value.Should().Be("Senha atual incorreta");
                badRequestResult.StatusCode.Should().Be(400);

                _userServiceMock.Verify(x => x.ChangePasswordAsync(userId, request), Times.Once);
            }

            [Fact]
            public async Task ChangePassword_GenericException_ReturnsInternalServerError()
            {
                // Arrange
                var userId = Guid.NewGuid();
                var request = new ChangePasswordRequestDto(
                    CurrentPassword: "OldPass123!",
                    NewPassword: "NewPass123!"
                );

                var exception = new Exception("Failed to update password");

                _userServiceMock
                    .Setup(x => x.ChangePasswordAsync(userId, request))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.ChangePassword(userId, request);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Erro interno ao alterar senha");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error changing password")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }
    }
}
