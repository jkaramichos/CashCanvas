using CashCanvas.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CashCanvas.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<UserStats> UserStats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserStats>(entity =>
        {
            entity.HasKey(e => e.UserStatsId);
        
            entity.Property(e => e.TotalCounterClicks)
                .HasDefaultValue(0);
        
            // Configure foreign key relationship
            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        
            // Create index on UserId for better query performance
            entity.HasIndex(e => e.UserId);
        });
    }
}
