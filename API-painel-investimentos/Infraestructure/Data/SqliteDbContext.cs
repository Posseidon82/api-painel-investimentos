using Microsoft.EntityFrameworkCore;
using API_painel_investimentos.Models;

namespace API_painel_investimentos.Infraestructure.Data;

// Contexto do Banco de Dados
public class SqliteDbContext : DbContext
{
    //public DbSet<Simulacao> Simulacoes { get; set; }
    //public DbSet<ParcelaSimulacao> ParcelasSimulacao { get; set; }

    public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.Entity<Simulacao>(entity =>
        //{
        //    entity.ToTable("Simulacoes");

        //    entity.HasKey(e => e.IdSimulacao);

        //    // Configurar o relacionamento com Parcelas
        //    entity.HasMany(s => s.Parcelas)
        //      .WithOne(p => p.Simulacao)
        //      .HasForeignKey(p => p.SimulacaoId)
        //      .OnDelete(DeleteBehavior.Cascade);

        //});

        //modelBuilder.Entity<ParcelaSimulacao>(entity =>
        //{
        //    entity.ToTable("ParcelasSimulacao");

        //    entity.HasKey(e => e.ParcelaId);

        //    // Configurar o relacionamento com Simulacao
        //    entity.HasOne(p => p.Simulacao)
        //      .WithMany(s => s.Parcelas)
        //      .HasForeignKey(p => p.SimulacaoId)
        //      .OnDelete(DeleteBehavior.Cascade);
        //});

        //// Adicionando índices para melhorar a performance
        //modelBuilder.Entity<ParcelaSimulacao>()
        //    .HasIndex(p => p.SimulacaoId);

        //modelBuilder.Entity<ParcelaSimulacao>()
        //    .HasIndex(p => new { p.SimulacaoId, p.NumParcela });
    }
}
