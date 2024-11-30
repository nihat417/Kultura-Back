using Kultura.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kultura.Persistence.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<User>(options)
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Restaurant>()
                .Property(r => r.MinPrice)
            .HasColumnType("money");

            modelBuilder.Entity<Restaurant>()
                .Property(r => r.MaxPrice)
                .HasColumnType("money");

            modelBuilder.Entity<Floor>()
                .HasOne(f => f.Restaurant)
                .WithMany(r => r.Floors)
                .HasForeignKey(f => f.Id);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Table)
                .WithMany(t => t.Reservations)
                .HasForeignKey(r => r.Id);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.Id);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.Id);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Restaurant)
            .WithMany(r => r.Reviews)
                .HasForeignKey(r => r.Id);
        }

        public DbSet<Restaurant> Restaurants { get; set; } = default!;
        public DbSet<Floor> Floors { get; set; } = default!;
        public DbSet<Table> Tables { get; set; } = default!;
        public DbSet<Reservation> Reservations { get; set; } = default!;
        public DbSet<Review> Reviews { get; set; } = default!;

    }

}
