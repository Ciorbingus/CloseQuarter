using Microsoft.EntityFrameworkCore;
using CloseQuarter.Server.Models;

namespace CloseQuarter.Server.Data;

public class AppDbContext : DbContext
{
    public DbSet<UserEntity> Users => Set<UserEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string connString = "Host=localhost;Database=closequarter_db;Username=cq_admin;Password=cq1234";
        optionsBuilder.UseNpgsql(connString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<UserEntity>()
            .HasIndex(u => u.SessionToken);
    }
}