
using API_painel_investimentos.Infraestructure.Data;
using API_painel_investimentos.Repositories.Portfolio;
using API_painel_investimentos.Repositories.Portfolio.Interfaces;
using API_painel_investimentos.Repositories.Profile;
using API_painel_investimentos.Repositories.Profile.Interfaces;
using API_painel_investimentos.Repositories.Simulation;
using API_painel_investimentos.Repositories.Simulation.Interfaces;
using API_painel_investimentos.Repositories.User;
using API_painel_investimentos.Repositories.User.Interfaces;
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
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar JWT
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

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
