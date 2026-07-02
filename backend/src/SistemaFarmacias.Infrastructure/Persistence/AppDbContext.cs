using Microsoft.EntityFrameworkCore;
using SistemaFarmacias.Domain.Entities;

namespace SistemaFarmacias.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Farmacia> Farmacias => Set<Farmacia>();
    public DbSet<WhatsappConfig> WhatsappConfigs => Set<WhatsappConfig>();
    public DbSet<Contato> Contatos => Set<Contato>();
    public DbSet<Interacao> Interacoes => Set<Interacao>();
    public DbSet<ReativacaoEnviada> ReativacoesEnviadas => Set<ReativacaoEnviada>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WhatsappConfig>(entity =>
        {
            entity.ToTable("whatsapp_config");
            entity.HasIndex(e => e.WhatsappNumberId).IsUnique();
        });

        modelBuilder.Entity<Contato>(entity =>
        {
            entity.ToTable("contatos");
            entity.HasIndex(e => new { e.FarmaciaId, e.Telefone }).IsUnique();
        });

        modelBuilder.Entity<Interacao>(entity =>
        {
            entity.ToTable("interacoes");
            entity.HasOne(e => e.Contato)
                  .WithMany(c => c.Interacoes)
                  .HasForeignKey(e => e.ContatoId);
        });

        modelBuilder.Entity<ReativacaoEnviada>(entity =>
        {
            entity.ToTable("reativacoes_enviadas");
            entity.HasOne(e => e.Contato)
                  .WithMany(c => c.Reativacoes)
                  .HasForeignKey(e => e.ContatoId);
        });

        modelBuilder.Entity<Farmacia>().ToTable("farmacias");

        base.OnModelCreating(modelBuilder);
    }
}