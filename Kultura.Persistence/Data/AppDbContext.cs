using Kultura.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kultura.Persistence.Data
{
    public sealed class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Favourite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId);

            modelBuilder.Entity<Favourite>()
               .HasOne(f => f.Restaurant)
               .WithMany()
               .HasForeignKey(f => f.RestaurantId);

            modelBuilder.Entity<Restaurant>()
                .HasMany(r => r.SocialLinks)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Table)
                .WithMany(t => t.Reservations)
                .HasForeignKey(r => r.TableId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Slot)
                .WithMany(s => s.Reservations)
                .HasForeignKey(r => r.SlotId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Restaurant>()
                .Property(r => r.MinPrice)
                .HasColumnType("money");

            modelBuilder.Entity<Restaurant>()
                .Property(r => r.MaxPrice)
                .HasColumnType("money");

            modelBuilder.Entity<Reservation>()
                .HasIndex(r => r.UserId); 

            modelBuilder.Entity<Reservation>()
                .HasIndex(r => r.TableId); 

            modelBuilder.Entity<Floor>()
                .HasOne(f => f.Restaurant)
                .WithMany(r => r.Floors)
                .HasForeignKey(f => f.RestaurantId);

            modelBuilder.Entity<Table>()
                .HasMany(t => t.ReservationSlots)
                .WithOne(s => s.Table)
                .HasForeignKey(s => s.TableId);

            modelBuilder.Entity<ReservationSlot>()
                .HasMany(s => s.Reservations)
                .WithOne(r => r.Slot)
                .HasForeignKey(r => r.SlotId);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId); 

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Restaurant)
                .WithMany(r => r.Reviews)
                .HasForeignKey(r => r.RestaurantId); 
        }


        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Restaurant> Restaurants { get; set; } = default!;
        public DbSet<Roles> Roles { get; set; } = default!;
        public DbSet<Floor> Floors { get; set; } = default!;
        public DbSet<Table> Tables { get; set; } = default!;
        public DbSet<Reservation> Reservations { get; set; } = default!;
        public DbSet<Favourite> Favourites { get; set; } = default!;
        public DbSet<SocialLink> SocialLinks { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = default!;
        public DbSet<ReservationSlot> ReservationSlots { get; set; } = default!;
    }
}
