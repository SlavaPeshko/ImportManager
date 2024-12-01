using Microsoft.EntityFrameworkCore;

namespace ImportManager.Data;

public class ImportManagerContext(DbContextOptions<ImportManagerContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<ImportJob> ImportJobs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).HasColumnType("nvarchar").IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).HasColumnType("nvarchar").IsRequired();
            entity.Property(e => e.Company).HasMaxLength(200).HasColumnType("nvarchar");
            entity.Property(e => e.City).HasMaxLength(100).HasColumnType("nvarchar");
            entity.Property(e => e.Country).HasMaxLength(100).HasColumnType("nvarchar");
            entity.Property(e => e.Phone).HasMaxLength(50).HasColumnType("nvarchar");
            entity.Property(e => e.Email).HasMaxLength(200).HasColumnType("nvarchar");
            entity.Property(e => e.SubscriptionDate).HasColumnType("datetime");
            entity.Property(e => e.Website).HasMaxLength(500).HasColumnType("nvarchar");
        });

        modelBuilder.Entity<ImportJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DomainId).HasColumnType("char(36)").IsRequired();
            entity.Property(e => e.FileName).HasMaxLength(255).HasColumnType("nvarchar").IsRequired();
            entity.Property(e => e.Status).HasColumnType("smallint").IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").IsRequired();
            entity.Property(e => e.StartedAt).HasColumnType("datetime");
            entity.Property(e => e.CompletedAt).HasColumnType("datetime");
            entity.Property(e => e.FailedAt).HasColumnType("datetime");
            // entity.Property(e => e.FailureReason).HasColumnType("nvarchar");
        });

        modelBuilder.Entity<ImportJob>().HasIndex(x => new { x.Status }).HasFilter($"{nameof(ImportJob.Status)} = {JobStatus.Enqueued}");
    }
}