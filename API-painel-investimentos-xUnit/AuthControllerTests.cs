using API_painel_investimentos.Controllers.Authentication;
using API_painel_investimentos.DTO.Authentication;
using API_painel_investimentos.DTO.User;
using API_painel_investimentos.Models.User;
using API_painel_investimentos.Services.Authentication.Interfaces;
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
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _userServiceMock = new Mock<IUserService>();
            _loggerMock = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(
                _authServiceMock.Object,
                _userServiceMock.Object,
                _loggerMock.Object
            );
        }

        public class LoginTests : AuthControllerTests
        {
            [Fact]
            public async Task Login_ValidCredentialsWithCpf_ReturnsOkWithToken()
            {
                // Arrange
                var request = new LoginRequestDto(
                    Password: "ValidPass123!",
                    Cpf: "12345678900",
                    Email: null
                );

                var expectedResponse = new LoginResponseDto(
                    UserId: Guid.NewGuid(),
                    Name: "Test User",
                    Email: "test@example.com",
                    Token: "valid.jwt.token",
                    ExpiresAt: DateTime.UtcNow.AddMinutes(60)
                );

                _authServiceMock
                    .Setup(x => x.AuthenticateAsync(request))
                    .ReturnsAsync(expectedResponse);

                // Act
                var result = await _controller.Login(request);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedResponse);
                okResult.StatusCode.Should().Be(200);

                _authServiceMock.Verify(x => x.AuthenticateAsync(request), Times.Once);
                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Login successful")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }

            [Fact]
            public async Task Login_ValidCredentialsWithEmail_ReturnsOkWithToken()
            {
                // Arrange
                var request = new LoginRequestDto(
                    Password: "ValidPass123!",
                    Cpf: null,
                    Email: "test@example.com"
                );

                var expectedResponse = new LoginResponseDto(
                    UserId: Guid.NewGuid(),
                    Name: "Test User",
                    Email: "test@example.com",
                    Token: "valid.jwt.token",
                    ExpiresAt: DateTime.UtcNow.AddMinutes(60)
                );

                _authServiceMock
                    .Setup(x => x.AuthenticateAsync(request))
                    .ReturnsAsync(expectedResponse);

                // Act
                var result = await _controller.Login(request);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedResponse);
                okResult.StatusCode.Should().Be(200);
            }

            [Fact]
            public async Task Login_NullPassword_ReturnsBadRequest()
            {
                // Arrange
                var request = new LoginRequestDto(
                    Password: "   ",
                    Cpf: "12345678900",
                    Email: null
                );

                // Act
                var result = await _controller.Login(request);

                // Assert
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
                badRequestResult.Value.Should().Be("CPF ou Email e senha são obrigatórios");
                badRequestResult.StatusCode.Should().Be(400);

                _authServiceMock.Verify(x => x.AuthenticateAsync(It.IsAny<LoginRequestDto>()), Times.Never);
            }

            [Fact]
            public async Task Login_NullCpfAndEmail_ReturnsBadRequest()
            {
                // Arrange
                var request = new LoginRequestDto(
                    Password: "ValidPass123!",
                    Cpf: null,
                    Email: null
                );

                // Act
                var result = await _controller.Login(request);

                // Assert
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
                badRequestResult.Value.Should().Be("CPF ou Email e senha são obrigatórios");
                badRequestResult.StatusCode.Should().Be(400);

                _authServiceMock.Verify(x => x.AuthenticateAsync(It.IsAny<LoginRequestDto>()), Times.Never);
            }

            [Fact]
            public async Task Login_EmptyCpfAndEmail_ReturnsBadRequest()
            {
                // Arrange
                var request = new LoginRequestDto(
                    Password: "ValidPass123!",
                    Cpf: "",
                    Email: ""
                );

                // Act
                var result = await _controller.Login(request);

                // Assert
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
                badRequestResult.Value.Should().Be("CPF ou Email e senha são obrigatórios");
                badRequestResult.StatusCode.Should().Be(400);
            }

            [Fact]
            public async Task Login_InvalidCredentials_ReturnsUnauthorized()
            {
                // Arrange
                var request = new LoginRequestDto(
                    Password: "WrongPass",
                    Cpf: "12345678900",
                    Email: null
                );

                _authServiceMock
                    .Setup(x => x.AuthenticateAsync(request))
                    .ReturnsAsync((LoginResponseDto?)null);

                // Act
                var result = await _controller.Login(request);

                // Assert
                var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
                unauthorizedResult.Value.Should().Be("CPF/Email ou senha incorretos");
                unauthorizedResult.StatusCode.Should().Be(401);

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Login failed")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }

            [Fact]
            public async Task Login_ServiceThrowsException_ReturnsInternalServerError()
            {
                // Arrange
                var request = new LoginRequestDto(
                    Password: "ValidPass123!",
                    Cpf: "12345678900",
                    Email: null
                );

                var exception = new Exception("Database connection failed");

                _authServiceMock
                    .Setup(x => x.AuthenticateAsync(request))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.Login(request);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Erro interno durante o login");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error during login")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }

            [Fact]
            public async Task Login_WhitespaceCpfAndValidEmail_UsesEmailForAuthentication()
            {
                // Arrange
                var request = new LoginRequestDto(
                    Password: "ValidPass123!",
                    Cpf: "   ",
                    Email: "test@example.com"
                );

                var expectedResponse = new LoginResponseDto(
                    UserId: Guid.NewGuid(),
                    Name: "Test User",
                    Email: "test@example.com",
                    Token: "valid.jwt.token",
                    ExpiresAt: DateTime.UtcNow.AddMinutes(60)
                );

                _authServiceMock
                    .Setup(x => x.AuthenticateAsync(request))
                    .ReturnsAsync(expectedResponse);

                // Act
                var result = await _controller.Login(request);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.StatusCode.Should().Be(200);
                _authServiceMock.Verify(x => x.AuthenticateAsync(request), Times.Once);
            }
        }

        public class ValidateTokenTests : AuthControllerTests
        {
            [Fact]
            public async Task ValidateToken_ValidToken_ReturnsOkWithValidationResult()
            {
                // Arrange
                var token = "valid.jwt.token";
                var expectedResponse = new TokenValidationResponseDto(
                    IsValid: true,
                    UserId: Guid.NewGuid(),
                    Name: "Test User",
                    Email: "test@example.com",
                    ExpiresAt: DateTime.UtcNow.AddMinutes(30)
                );

                _authServiceMock
                    .Setup(x => x.ValidateTokenAsync(token))
                    .ReturnsAsync(expectedResponse);

                // Act
                var result = await _controller.ValidateToken(token);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(expectedResponse);
                okResult.StatusCode.Should().Be(200);

                _authServiceMock.Verify(x => x.ValidateTokenAsync(token), Times.Once);
            }

            [Fact]
            public async Task ValidateToken_NullToken_ReturnsBadRequest()
            {
                // Arrange
                string token = null!;

                // Act
                var result = await _controller.ValidateToken(token);

                // Assert
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
                badRequestResult.Value.Should().Be("Token é obrigatório");
                badRequestResult.StatusCode.Should().Be(400);

                _authServiceMock.Verify(x => x.ValidateTokenAsync(It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public async Task ValidateToken_EmptyToken_ReturnsBadRequest()
            {
                // Arrange
                var token = "";

                // Act
                var result = await _controller.ValidateToken(token);

                // Assert
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
                badRequestResult.Value.Should().Be("Token é obrigatório");
                badRequestResult.StatusCode.Should().Be(400);
            }

            [Fact]
            public async Task ValidateToken_WhitespaceToken_ReturnsBadRequest()
            {
                // Arrange
                var token = "   ";

                // Act
                var result = await _controller.ValidateToken(token);

                // Assert
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
                badRequestResult.Value.Should().Be("Token é obrigatório");
                badRequestResult.StatusCode.Should().Be(400);
            }

            [Fact]
            public async Task ValidateToken_ServiceThrowsException_ReturnsInternalServerError()
            {
                // Arrange
                var token = "valid.jwt.token";
                var exception = new Exception("Token validation failed");

                _authServiceMock
                    .Setup(x => x.ValidateTokenAsync(token))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.ValidateToken(token);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Erro interno ao validar token");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error validating token")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        public class RefreshTokenTests : AuthControllerTests
        {
            [Fact]
            public async Task RefreshToken_ValidToken_ReturnsOkWithNewToken()
            {
                // Arrange
                var token = "valid.jwt.token";
                var userId = Guid.NewGuid();
                var user = new UserResponseDto(
                    UserId: userId,
                    Name: "Test User",
                    Cpf: "12345678900",
                    Email: "test@example.com",
                    IsActive: true,
                    CreatedAt: DateTime.UtcNow.AddDays(-1)
                );

                var validationResult = new TokenValidationResponseDto(
                    IsValid: true,
                    UserId: userId,
                    Name: user.Name,
                    Email: user.Email
                );

                var newTokenResponse = new LoginResponseDto(
                    UserId: userId,
                    Name: user.Name,
                    Email: user.Email,
                    Token: "new.jwt.token",
                    ExpiresAt: DateTime.UtcNow.AddMinutes(60)
                );

                _authServiceMock
                    .Setup(x => x.ValidateTokenAsync(token))
                    .ReturnsAsync(validationResult);

                _userServiceMock
                    .Setup(x => x.GetUserByIdAsync(userId))
                    .ReturnsAsync(user);

                _authServiceMock
                    .Setup(x => x.GenerateToken(It.IsAny<UserEntity>()))
                    .Returns(newTokenResponse.Token);

                // Act
                var result = await _controller.RefreshToken(token);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                var response = okResult.Value.Should().BeOfType<LoginResponseDto>().Subject;

                response.Token.Should().Be(newTokenResponse.Token);
                response.UserId.Should().Be(userId);
                response.Name.Should().Be(user.Name);
                response.Email.Should().Be(user.Email);
                okResult.StatusCode.Should().Be(200);

                _authServiceMock.Verify(x => x.ValidateTokenAsync(token), Times.Once);
                _userServiceMock.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
                _authServiceMock.Verify(x => x.GenerateToken(It.IsAny<UserEntity>()), Times.Once);

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Token refreshed")),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }

            [Fact]
            public async Task RefreshToken_NullToken_ReturnsBadRequest()
            {
                // Arrange
                string token = null!;

                // Act
                var result = await _controller.RefreshToken(token);

                // Assert
                var badRequestResult = result.Should().BeOfType<BadRequestObjectResult>().Subject;
                badRequestResult.Value.Should().Be("Token é obrigatório");
                badRequestResult.StatusCode.Should().Be(400);

                _authServiceMock.Verify(x => x.ValidateTokenAsync(It.IsAny<string>()), Times.Never);
                _userServiceMock.Verify(x => x.GetUserByIdAsync(It.IsAny<Guid>()), Times.Never);
            }

            [Fact]
            public async Task RefreshToken_InvalidToken_ReturnsUnauthorized()
            {
                // Arrange
                var token = "invalid.jwt.token";
                var validationResult = new TokenValidationResponseDto(
                    IsValid: false,
                    UserId: null
                );

                _authServiceMock
                    .Setup(x => x.ValidateTokenAsync(token))
                    .ReturnsAsync(validationResult);

                // Act
                var result = await _controller.RefreshToken(token);

                // Assert
                var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
                unauthorizedResult.Value.Should().Be("Token inválido ou expirado");
                unauthorizedResult.StatusCode.Should().Be(401);

                _authServiceMock.Verify(x => x.ValidateTokenAsync(token), Times.Once);
                _userServiceMock.Verify(x => x.GetUserByIdAsync(It.IsAny<Guid>()), Times.Never);
            }

            [Fact]
            public async Task RefreshToken_ValidTokenButUserNotFound_ReturnsUnauthorized()
            {
                // Arrange
                var token = "valid.jwt.token";
                var userId = Guid.NewGuid();
                var validationResult = new TokenValidationResponseDto(
                    IsValid: true,
                    UserId: userId
                );

                _authServiceMock
                    .Setup(x => x.ValidateTokenAsync(token))
                    .ReturnsAsync(validationResult);

                _userServiceMock
                    .Setup(x => x.GetUserByIdAsync(userId))
                    .ReturnsAsync((UserResponseDto?)null);

                // Act
                var result = await _controller.RefreshToken(token);

                // Assert
                var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
                unauthorizedResult.Value.Should().Be("Token inválido ou expirado");
                unauthorizedResult.StatusCode.Should().Be(401);

                _authServiceMock.Verify(x => x.ValidateTokenAsync(token), Times.Once);
                _userServiceMock.Verify(x => x.GetUserByIdAsync(userId), Times.Once);
            }

            [Fact]
            public async Task RefreshToken_ServiceThrowsException_ReturnsInternalServerError()
            {
                // Arrange
                var token = "valid.jwt.token";
                var exception = new Exception("Token refresh failed");

                _authServiceMock
                    .Setup(x => x.ValidateTokenAsync(token))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.RefreshToken(token);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Erro interno ao renovar token");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error refreshing token")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }

        public class GetUserInfoTests : AuthControllerTests
        {
            [Fact]
            public async Task GetUserInfo_ValidToken_ReturnsOkWithUserInfo()
            {
                // Arrange
                var token = "valid.jwt.token";
                var userInfo = new UserResponseDto(
                    UserId: Guid.NewGuid(),
                    Name: "Test User",
                    Cpf: "12345678900",
                    Email: "test@example.com",
                    IsActive: true,
                    CreatedAt: DateTime.UtcNow.AddDays(-1)
                );

                var validationResult = new TokenValidationResponseDto(
                    IsValid: true,
                    UserId: userInfo.UserId,
                    Name: userInfo.Name,
                    Email: userInfo.Email
                );

                _authServiceMock
                    .Setup(x => x.ValidateTokenAsync(token))
                    .ReturnsAsync(validationResult);

                _authServiceMock
                    .Setup(x => x.GetUserFromTokenAsync(token))
                    .ReturnsAsync(userInfo);

                // Act
                var result = await _controller.GetUserInfo(token);

                // Assert
                var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
                okResult.Value.Should().BeEquivalentTo(userInfo);
                okResult.StatusCode.Should().Be(200);

                _authServiceMock.Verify(x => x.ValidateTokenAsync(token), Times.Once);
                _authServiceMock.Verify(x => x.GetUserFromTokenAsync(token), Times.Once);
            }

            [Fact]
            public async Task GetUserInfo_NullToken_ReturnsUnauthorized()
            {
                // Arrange
                string token = null!;

                // Act
                var result = await _controller.GetUserInfo(token);

                // Assert
                var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
                unauthorizedResult.Value.Should().Be("Token não fornecido");
                unauthorizedResult.StatusCode.Should().Be(401);

                _authServiceMock.Verify(x => x.ValidateTokenAsync(It.IsAny<string>()), Times.Never);
                _authServiceMock.Verify(x => x.GetUserFromTokenAsync(It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public async Task GetUserInfo_InvalidToken_ReturnsUnauthorized()
            {
                // Arrange
                var token = "invalid.jwt.token";
                var validationResult = new TokenValidationResponseDto(
                    IsValid: false,
                    UserId: null
                );

                _authServiceMock
                    .Setup(x => x.ValidateTokenAsync(token))
                    .ReturnsAsync(validationResult);

                // Act
                var result = await _controller.GetUserInfo(token);

                // Assert
                var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
                unauthorizedResult.Value.Should().Be("Token inválido ou expirado");
                unauthorizedResult.StatusCode.Should().Be(401);

                _authServiceMock.Verify(x => x.ValidateTokenAsync(token), Times.Once);
                _authServiceMock.Verify(x => x.GetUserFromTokenAsync(It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public async Task GetUserInfo_ValidTokenButUserNotFound_ReturnsUnauthorized()
            {
                // Arrange
                var token = "valid.jwt.token";
                var validationResult = new TokenValidationResponseDto(
                    IsValid: true,
                    UserId: Guid.NewGuid()
                );

                _authServiceMock
                    .Setup(x => x.ValidateTokenAsync(token))
                    .ReturnsAsync(validationResult);

                _authServiceMock
                    .Setup(x => x.GetUserFromTokenAsync(token))
                    .ReturnsAsync((UserResponseDto?)null);

                // Act
                var result = await _controller.GetUserInfo(token);

                // Assert
                var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
                unauthorizedResult.Value.Should().Be("Token inválido ou expirado");
                unauthorizedResult.StatusCode.Should().Be(401);

                _authServiceMock.Verify(x => x.ValidateTokenAsync(token), Times.Once);
                _authServiceMock.Verify(x => x.GetUserFromTokenAsync(token), Times.Once);
            }

            [Fact]
            public async Task GetUserInfo_ServiceThrowsException_ReturnsInternalServerError()
            {
                // Arrange
                var token = "valid.jwt.token";
                var exception = new Exception("Failed to get user info");

                _authServiceMock
                    .Setup(x => x.ValidateTokenAsync(token))
                    .ThrowsAsync(exception);

                // Act
                var result = await _controller.GetUserInfo(token);

                // Assert
                var statusCodeResult = result.Should().BeOfType<ObjectResult>().Subject;
                statusCodeResult.StatusCode.Should().Be(500);
                statusCodeResult.Value.Should().Be("Erro interno ao obter informações do usuário");

                _loggerMock.Verify(
                    x => x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error getting user info")),
                        exception,
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                    Times.Once);
            }
        }
    }
}
