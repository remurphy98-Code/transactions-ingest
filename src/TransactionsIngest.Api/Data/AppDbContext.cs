using Microsoft.EntityFrameworkCore;
using TransactionsIngest.Api.Models;

namespace TransactionsIngest.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SourceSystem, e.TransactionId }).IsUnique();
            entity.Property(e => e.SourceSystem).HasMaxLength(20).IsRequired();
            entity.Property(e => e.LocationCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.ProductName).HasMaxLength(20).IsRequired();
            entity.Property(e => e.CardNumber).HasMaxLength(20);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>();
        });

        modelBuilder.Entity<AuditEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Transaction)
                  .WithMany(t => t.AuditEntries)
                  .HasForeignKey(e => e.TransactionId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.Property(e => e.ChangeType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.FieldName).HasMaxLength(50);
            entity.Property(e => e.OldValue).HasMaxLength(500);
            entity.Property(e => e.NewValue).HasMaxLength(500);
        });
    }
}
