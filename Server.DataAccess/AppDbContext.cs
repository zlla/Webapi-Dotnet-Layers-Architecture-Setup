using Microsoft.EntityFrameworkCore;
using Server.Common.Models;

namespace Server.DataAccess
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Account>? Accounts { get; set; }
        public DbSet<RefreshToken>? RefreshTokens { get; set; }
        public DbSet<AccessToken>? AccessTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>()
                .HasMany(a => a.RefreshTokens)
                .WithOne(r => r.Account)
                .HasForeignKey(r => r.AccountId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RefreshToken>()
                .HasMany(r => r.AccessTokens)
                .WithOne(a => a.RefreshToken)
                .HasForeignKey(a => a.RefreshTokenId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}