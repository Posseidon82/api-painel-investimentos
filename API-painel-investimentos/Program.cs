
using API_painel_investimentos.Infraestructure.Data;
using API_painel_investimentos.Repositories.Portfolio;
using API_painel_investimentos.Repositories.Portfolio.Interfaces;
using API_painel_investimentos.Repositories.Profile;
using API_painel_investimentos.Repositories.Profile.Interfaces;
using API_painel_investimentos.Services.Portfolio;
using API_painel_investimentos.Services.Portfolio.Interfaces;
using API_painel_investimentos.Services.Profile;
using API_painel_investimentos.Services.Profile.Interfaces;
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

// Services
builder.Services.AddScoped<IInvestorProfileService, InvestorProfileService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IProfileCalculationService, ProfileCalculationService>();
builder.Services.AddScoped<IInvestmentRecommendationService, InvestmentRecommendationService>();
builder.Services.AddScoped<IInvestmentProductRepository, InvestmentProductRepository>();

// Logging
builder.Services.AddLogging();

var app = builder.Build();

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
