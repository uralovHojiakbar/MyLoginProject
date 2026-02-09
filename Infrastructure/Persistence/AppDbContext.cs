using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var u = modelBuilder.Entity<User>();

        u.ToTable("Users");
        u.HasKey(x => x.Id);

        u.Property(x => x.Email).IsRequired().HasMaxLength(320);
        u.Property(x => x.Name).IsRequired().HasMaxLength(200);
        u.Property(x => x.PasswordHash).IsRequired();

        // IMPORTANT / NOTA BENE:
        // Unique index must be created in DB level (task requirement).
        // Do NOT implement "email exists" checks to ensure uniqueness.
        u.HasIndex(x => x.Email).IsUnique();

        // helpful index for sorting by last login
        u.HasIndex(x => x.LastLoginUtc);
    }
}
