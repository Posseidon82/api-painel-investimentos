using API_painel_investimentos.Models;
using Microsoft.EntityFrameworkCore;

namespace API_painel_investimentos.Infraestructure.Data;

/// <summary>
/// Represents the database context for managing investor profiles and related entities.
/// </summary>
/// <remarks>This context is used to interact with the database for operations related to investor profiles, 
/// profile questions, answer options, and answers. It provides DbSet properties for each entity type  and configures
/// entity relationships and constraints during model creation.</remarks>
public class InvestorProfileContext : DbContext
{
    public InvestorProfileContext(DbContextOptions<InvestorProfileContext> options) : base(options) { }

    public DbSet<InvestorProfile> InvestorProfiles { get; set; }
    public DbSet<ProfileQuestion> ProfileQuestions { get; set; }
    public DbSet<QuestionAnswerOption> QuestionAnswerOptions { get; set; }
    public DbSet<ProfileAnswer> ProfileAnswers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configurações das entidades
        modelBuilder.Entity<InvestorProfile>(entity =>
        {
            entity.HasKey(ip => ip.Id);
            entity.HasIndex(ip => ip.UserId).IsUnique();

            entity.HasMany(ip => ip.Answers)
                  .WithOne(a => a.Profile)
                  .HasForeignKey(a => a.ProfileId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProfileQuestion>(entity =>
        {
            entity.HasMany(pq => pq.AnswerOptions)
                  .WithOne(o => o.Question)
                  .HasForeignKey(o => o.QuestionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProfileAnswer>(entity =>
        {
            entity.HasOne(pa => pa.Question)
                  .WithMany()
                  .HasForeignKey(pa => pa.QuestionId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(pa => pa.SelectedOption)
                  .WithMany()
                  .HasForeignKey(pa => pa.AnswerOptionId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed data para questões
        SeedData(modelBuilder);
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
        // Adicionar questões iniciais
        var question1 = new ProfileQuestion("Qual é o seu principal objetivo com este investimento?", "Objectives", 25, 1);
        //question1.Id = Guid.NewGuid();

        question1.AddAnswerOption("Preservação do capital e liquidez", 1, "Foco em segurança");
        question1.AddAnswerOption("Acumular patrimônio para médio prazo", 5, "Equilíbrio");
        question1.AddAnswerOption("Crescimento no longo prazo", 10, "Foco em retorno");

        modelBuilder.Entity<ProfileQuestion>().HasData(question1);
        // Adicionar mais questões...
    }
}
