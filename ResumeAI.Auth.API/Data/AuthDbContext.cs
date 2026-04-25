using Microsoft.EntityFrameworkCore;
using ResumeAI.Auth.Entities;
using ResumeAI.Auth.Enums;

namespace ResumeAI.Auth.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User 
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();

            // Store enums as strings in PostgreSQL for readability
            entity.Property(u => u.Role)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(u => u.Provider)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(u => u.SubscriptionPlan)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            // Default values handled at DB level
            entity.Property(u => u.CreatedAt)
                  .HasDefaultValueSql("NOW()");

            entity.Property(u => u.UpdatedAt)
                  .HasDefaultValueSql("NOW()");
        });

        // --- RefreshToken --------------------------------------------------
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(r => r.Token).IsUnique();

            entity.HasOne(r => r.User)
                  .WithMany()
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
