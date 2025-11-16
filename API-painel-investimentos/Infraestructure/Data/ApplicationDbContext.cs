using API_painel_investimentos.Models;
using API_painel_investimentos.Models.Portfolio;
using API_painel_investimentos.Models.Profile;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace API_painel_investimentos.Infraestructure.Data;

/// <summary>
/// Represents the database context for managing investor profiles and related entities.
/// </summary>
/// <remarks>This context is used to interact with the database for operations related to investor profiles, 
/// profile questions, answer options, and answers. It provides DbSet properties for each entity type  and configures
/// entity relationships and constraints during model creation.</remarks>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<InvestorProfile> InvestorProfiles { get; set; }
    public DbSet<ProfileQuestion> ProfileQuestions { get; set; }
    public DbSet<QuestionAnswerOption> QuestionAnswerOptions { get; set; }
    public DbSet<ProfileAnswer> ProfileAnswers { get; set; }
    public DbSet<InvestmentProduct> InvestmentProducts { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração da entidade InvestorProfile
        modelBuilder.Entity<InvestorProfile>(entity =>
        {
            entity.HasKey(ip => ip.Id);

            entity.HasIndex(ip => ip.UserId)
                .IsUnique()
                .HasDatabaseName("IX_InvestorProfiles_UserId");

            entity.Property(ip => ip.ProfileType)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(ip => ip.Score)
                .IsRequired();

            entity.Property(ip => ip.CalculatedAt)
                .IsRequired();

            entity.Property(ip => ip.UpdatedAt)
                .IsRequired(false);

            // Relacionamento 1:N com ProfileAnswers
            entity.HasMany(ip => ip.Answers)
                .WithOne(pa => pa.Profile)
                .HasForeignKey(pa => pa.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuração da entidade ProfileQuestion
        modelBuilder.Entity<ProfileQuestion>(entity =>
        {
            entity.HasKey(pq => pq.Id);

            entity.Property(pq => pq.QuestionText)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(pq => pq.Category)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(pq => pq.Weight)
                .IsRequired();

            entity.Property(pq => pq.Order)
                .IsRequired();

            entity.Property(pq => pq.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Relacionamento 1:N com QuestionAnswerOption
            entity.HasMany(pq => pq.AnswerOptions)
                .WithOne(qao => qao.Question)
                .HasForeignKey(qao => qao.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configuração da entidade QuestionAnswerOption
        modelBuilder.Entity<QuestionAnswerOption>(entity =>
        {
            entity.HasKey(qao => qao.Id);

            entity.Property(qao => qao.OptionText)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(qao => qao.Description)
                .HasMaxLength(1000)
                .IsRequired(false);

            entity.Property(qao => qao.Score)
                .IsRequired();
        });

        // Configuração da entidade ProfileAnswer
        modelBuilder.Entity<ProfileAnswer>(entity =>
        {
            entity.HasKey(pa => pa.Id);

            entity.Property(pa => pa.AnsweredAt)
                .IsRequired();

            // Relacionamento com Question
            entity.HasOne(pa => pa.Question)
                .WithMany()
                .HasForeignKey(pa => pa.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento com SelectedOption
            entity.HasOne(pa => pa.SelectedOption)
                .WithMany()
                .HasForeignKey(pa => pa.AnswerOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índice composto para performance
            entity.HasIndex(pa => new { pa.ProfileId, pa.QuestionId })
                .IsUnique()
                .HasDatabaseName("IX_ProfileAnswers_ProfileId_QuestionId");
        });

        // Seed data inicial
        SeedData(modelBuilder);

        modelBuilder.Entity<InvestmentProduct>(entity =>
        {
            entity.HasKey(ip => ip.Id);

            entity.Property(ip => ip.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(ip => ip.Description)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(ip => ip.Category)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(ip => ip.RiskLevel)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(ip => ip.TargetProfile)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(ip => ip.Issuer)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(ip => ip.MinimumInvestment)
                .HasPrecision(18, 2)
                .IsRequired();

            entity.Property(ip => ip.AdministrationFee)
                .HasPrecision(5, 4)
                .IsRequired();

            entity.Property(ip => ip.ExpectedReturn)
                .HasPrecision(5, 4)
                .IsRequired();

            entity.Property(ip => ip.LiquidityDays)
                .IsRequired();

            entity.Property(ip => ip.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Índices para performance
            entity.HasIndex(ip => new { ip.Category, ip.IsActive })
                .HasDatabaseName("IX_InvestmentProducts_Category_Active");

            entity.HasIndex(ip => new { ip.RiskLevel, ip.IsActive })
                .HasDatabaseName("IX_InvestmentProducts_RiskLevel_Active");

            entity.HasIndex(ip => new { ip.TargetProfile, ip.IsActive })
                .HasDatabaseName("IX_InvestmentProducts_TargetProfile_Active");
        });

        // Seed data para produtos de investimento
        SeedInvestmentProducts(modelBuilder);
    }

    /// <summary>
    /// Seeds initial data into the model using the specified <see cref="ModelBuilder"/>.
    /// </summary>
    /// <remarks>This method populates the model with predefined profile questions and their associated answer
    /// options. It is typically called during the model configuration phase to ensure that the database is initialized
    /// with default data.</remarks>
    /// <param name="modelBuilder">The <see cref="ModelBuilder"/> used to configure the entity data to be seeded.</param>
    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed para ProfileQuestions
        var questions = new List<ProfileQuestion>
        {
            new ProfileQuestion(
                "Qual é o seu principal objetivo com este investimento?",
                "Objectives", 25, 1), 
            new ProfileQuestion(
                "Em quanto tempo você precisará deste dinheiro?",
                "TimeHorizon", 25, 2), 
            new ProfileQuestion(
                "Qual o nível máximo de perda temporária você aceitaria?",
                "RiskTolerance", 30, 3), 
            new ProfileQuestion(
                "Como você descreveria seu conhecimento sobre investimentos?",
                "Knowledge", 10, 4), 
            new ProfileQuestion(
                "Este investimento representa qual percentual do seu patrimônio total?",
                "FinancialSituation", 10, 5)
        };

        modelBuilder.Entity<ProfileQuestion>().HasData(questions);

        // Seed para AnswerOptions
        var answerOptions = new List<QuestionAnswerOption>();
        var optionId = 1;

        foreach (var question in questions)
        {
            switch (question.Category)
            {
                case "Objectives":
                    answerOptions.AddRange(new[]
                    {
                        new QuestionAnswerOption(question.Id, "Preservação do capital e liquidez imediata", 1, "Foco total em segurança"),
                        new QuestionAnswerOption(question.Id, "Acumular patrimônio para um objetivo de médio prazo (3 a 5 anos)", 5, "Equilíbrio"),
                        new QuestionAnswerOption(question.Id, "Crescimento do patrimônio no longo prazo (acima de 10 anos)", 10, "Foco em retorno")
                    });
                    break;

                case "TimeHorizon":
                    answerOptions.AddRange(new[]
                    {
                        new QuestionAnswerOption(question.Id, "Até 1 ano", 1, "Curto prazo"),
                        new QuestionAnswerOption(question.Id, "De 2 a 5 anos", 5, "Médio prazo"),
                        new QuestionAnswerOption(question.Id, "Mais de 5 anos", 10, "Longo prazo")
                    });
                    break;

                case "RiskTolerance":
                    answerOptions.AddRange(new[]
                    {
                        new QuestionAnswerOption(question.Id, "Até 5% - Prefiro não ter perdas, mesmo que os ganhos sejam baixos", 1, "Baixa tolerância"),
                        new QuestionAnswerOption(question.Id, "Entre 5% e 15% - Entendo que flutuações são normais", 10, "Tolerância moderada"),
                        new QuestionAnswerOption(question.Id, "Acima de 15% - Estou ciente dos riscos e focado no potencial de retorno", 20, "Alta tolerância")
                    });
                    break;

                case "Knowledge":
                    answerOptions.AddRange(new[]
                    {
                        new QuestionAnswerOption(question.Id, "Iniciante - Conheço apenas Poupança e CDB", 1, "Conhecimento básico"),
                        new QuestionAnswerOption(question.Id, "Intermediário - Entendo Tesouro Direto, LCI/LCA e Fundos", 5, "Conhecimento intermediário"),
                        new QuestionAnswerOption(question.Id, "Avançado - Tenho experiência com Ações, FIIs e derivativos", 10, "Conhecimento avançado")
                    });
                    break;

                case "FinancialSituation":
                    answerOptions.AddRange(new[]
                    {
                        new QuestionAnswerOption(question.Id, "Acima de 50% - Qualquer perda teria impacto significativo", 1, "Alto impacto"),
                        new QuestionAnswerOption(question.Id, "Entre 10% e 50% - Uma perda seria incômoda, mas não catastrófica", 5, "Impacto moderado"),
                        new QuestionAnswerOption(question.Id, "Abaixo de 10% - Tenho patrimônio sólido e posso assumir riscos", 10, "Baixo impacto")
                    });
                    break;
            }
        }

        modelBuilder.Entity<QuestionAnswerOption>().HasData(answerOptions);
    }

    private void SeedInvestmentProducts(ModelBuilder modelBuilder)
    {
        var products = new List<InvestmentProduct>
        {
            // PRODUTOS CONSERVADORES (Baixo Risco)
            new InvestmentProduct(
                "Poupança Caixa",
                "Aplicação tradicional com liquidez diária e rentabilidade baseada na TR. Garantida pelo FGC até R$ 250 mil.",
                "RendaFixa",
                "Baixo",
                50.00m,
                1,
                "Conservative",
                0.00m,
                0.0050m, // 0.5% ao mês ~6% ao ano
                "Caixa Econômica Federal"
            ),

            new InvestmentProduct(
                "CDB Caixa - DI + 0.5%",
                "Certificado de Depósito Bancário pós-fixado atrelado ao CDI com garantia do FGC até R$ 250 mil.",
                "RendaFixa",
                "Baixo",
                1000.00m,
                30,
                "Conservative|Moderate",
                0.0010m,
                0.0075m, // CDI + 0.5% ~9% ao ano
                "Caixa Econômica Federal"
            ),

            new InvestmentProduct(
                "LCI Caixa - 90 dias",
                "Letra de Crédito Imobiliário com isenção de IR para pessoa física. Liquidez no vencimento.",
                "RendaFixa",
                "Baixo",
                5000.00m,
                90,
                "Conservative|Moderate",
                0.0015m,
                0.0068m, // ~8.2% ao ano
                "Caixa Econômica Federal"
            ),

            // TESOURO DIRETO
            new InvestmentProduct(
                "Tesouro Selic 2026",
                "Tesouro Direto pós-fixado atrelado à taxa Selic. Ideal para reserva de emergência.",
                "TesouroDireto",
                "Baixo",
                35.00m,
                1,
                "Conservative|Moderate",
                0.0000m,
                0.0062m, // Selic atual
                "Tesouro Nacional"
            ),

            new InvestmentProduct(
                "Tesouro IPCA+ 2035",
                "Tesouro Direto atrelado à inflação (IPCA) + taxa fixa. Proteção contra inflação.",
                "TesouroDireto",
                "Médio",
                35.00m,
                1080, // 3 anos
                "Moderate",
                0.0000m,
                0.0070m, // IPCA + 5.5%
                "Tesouro Nacional"
            ),

            // FUNDOS DE INVESTIMENTO
            new InvestmentProduct(
                "Fundo Caixa RF DI",
                "Fundo de Renda Fixa com carteira referenciada no DI. Baixa volatilidade.",
                "Fundos",
                "Baixo",
                1000.00m,
                30,
                "Conservative|Moderate",
                0.0050m,
                0.0058m, // DI + 0.3%
                "Caixa Econômica Federal"
            ),

            new InvestmentProduct(
                "Fundo Caixa Ações Ibovespa",
                "Fundo de Ações com carteira diversificada nas principais ações do Ibovespa.",
                "Fundos",
                "Alto",
                5000.00m,
                30,
                "Aggressive|Moderate",
                0.0150m,
                0.0100m, // Potencial > 12% aa
                "Caixa Econômica Federal"
            ),

            new InvestmentProduct(
                "Fundo Caixa Multimercado FIC FIM",
                "Fundo multimercado com estratégia flexível em diferentes classes de ativos.",
                "Fundos",
                "Médio",
                2500.00m,
                60,
                "Moderate|Aggressive",
                0.0120m,
                0.0079m, // ~9.5% aa
                "Caixa Econômica Federal"
            ),

            // PRODUTOS AGRESSIVOS
            new InvestmentProduct(
                "Fundo Caixa Small Caps",
                "Foco em empresas de pequeno capital com alto potencial de crescimento.",
                "Fundos",
                "Alto",
                10000.00m,
                90,
                "Aggressive",
                0.0200m,
                0.0125m, // Potencial > 15% aa
                "Caixa Econômica Federal"
            ),

            new InvestmentProduct(
                "FII Caixa Shopping Centers",
                "Fundo de Investimento Imobiliário com foco em shoppings centers premium.",
                "FII",
                "Médio-Alto",
                500.00m,
                30,
                "Moderate|Aggressive",
                0.0100m,
                0.0092m, // ~11% aa + dividendos
                "Caixa Econômica Federal"
            )
        };

        modelBuilder.Entity<InvestmentProduct>().HasData(products);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Atualizar timestamps automaticamente
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            //if (entry.State == EntityState.Added)
            //{
            //    entry.Entity.CreatedAt = DateTime.UtcNow;
            //}

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdateTimestamps();
            }

        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
