using API_painel_investimentos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace API_painel_investimentos.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<InvestorProfile> InvestorProfiles { get; set; }
    public DbSet<ProfileQuestion> ProfileQuestions { get; set; }
    public DbSet<QuestionAnswerOption> QuestionAnswerOptions { get; set; }
    public DbSet<ProfileAnswer> ProfileAnswers { get; set; }

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
    }

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

            entry.Entity.UpdateTimestamps();
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
