
using API_painel_investimentos.Infraestructure.Data;
using API_painel_investimentos.Models.Authentication;
using API_painel_investimentos.Repositories.Portfolio;
using API_painel_investimentos.Repositories.Portfolio.Interfaces;
using API_painel_investimentos.Repositories.Profile;
using API_painel_investimentos.Repositories.Profile.Interfaces;
using API_painel_investimentos.Repositories.Simulation;
using API_painel_investimentos.Repositories.Simulation.Interfaces;
using API_painel_investimentos.Repositories.User;
using API_painel_investimentos.Repositories.User.Interfaces;
using API_painel_investimentos.Services.Authentication;
using API_painel_investimentos.Services.Authentication.Interfaces;
using API_painel_investimentos.Services.Portfolio;
using API_painel_investimentos.Services.Portfolio.Interfaces;
using API_painel_investimentos.Services.Profile;
using API_painel_investimentos.Services.Profile.Interfaces;
using API_painel_investimentos.Services.Simulation;
using API_painel_investimentos.Services.Simulation.Interfaces;
using API_painel_investimentos.Services.User;
using API_painel_investimentos.Services.User.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();


// Configurar JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
{
    throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long and not empty");
}

var secretKeyBytes = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!);

builder.Services.Configure<JwtSettings>(jwtSettings);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    // Para Swagger/API testing
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validated successfully");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Define o esquema de segurança
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT no formato: Bearer {seu_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    // Aplica o esquema globalmente a todos os endpoints
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection"));

    // Habilitar logging de queries em desenvolvimento
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});


// Repositories
builder.Services.AddScoped<IInvestorProfileRepository, InvestorProfileRepository>();
builder.Services.AddScoped<IProfileQuestionRepository, ProfileQuestionRepository>();
builder.Services.AddScoped<IInvestmentProductRepository, InvestmentProductRepository>();
builder.Services.AddScoped<ISimulationRepository, SimulationRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Services
builder.Services.AddScoped<IInvestorProfileService, InvestorProfileService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IProfileCalculationService, ProfileCalculationService>();
builder.Services.AddScoped<IInvestmentRecommendationService, InvestmentRecommendationService>();
builder.Services.AddScoped<IInvestmentSimulationService, InvestmentSimulationService>();
builder.Services.AddScoped<ISimulationStatsService, SimulationStatsService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Password Hasher
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();

// Logging
builder.Services.AddLogging();


// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://seusite.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var app = builder.Build();

app.UseCors("AllowSpecificOrigin");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Cria banco de dados e tabelas automaticamente
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
    }

    // Adicionar tratamento de exceções
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Rota para erro
app.Map("/error", () => Results.Problem("An error occurred."));

app.UseHttpsRedirection();
app.UseAuthentication(); // IMPORTANTE: Antes do UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();
