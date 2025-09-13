using Microsoft.EntityFrameworkCore;
using WarungElja.Models;
using WarungElja.Utilities;

namespace WarungElja.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<ProductDetails> ProductDetails { get; set; }
        public DbSet<ProductStock> ProductStock { get; set; }
        public DbSet<SalesRecord> SalesRecords { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductDetails>(entity =>
            {
                entity.Property(e => e.Price)
                    .HasPrecision(18, 2);
            });

            modelBuilder.Entity<ProductStock>(entity =>
            {
                entity.HasOne(d => d.ProductDetails)
                    .WithMany(p => p.ProductStocks)
                    .HasForeignKey(d => d.IdProductDetails)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = PasswordHasher.HashPassword("password"), // "password" with proper hashing
                    Name = "Administrator",
                    Role = "Admin"
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
