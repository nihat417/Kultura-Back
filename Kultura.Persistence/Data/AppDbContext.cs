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

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Table)
                .WithMany(t => t.Reservations)
                .HasForeignKey(r => r.TableId) // Используем TableId для внешнего ключа
                .OnDelete(DeleteBehavior.NoAction); // Меняем на NoAction, чтобы избежать каскадного удаления

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reservations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.SetNull); // Сохраняем SetNull для UserId

            modelBuilder.Entity<Reservation>()
                .HasOne(r => r.Slot)
                .WithMany(s => s.Reservations)
                .HasForeignKey(r => r.SlotId)
                .OnDelete(DeleteBehavior.Cascade); // Оставляем Cascade для SlotId

            modelBuilder.Entity<Restaurant>()
                .Property(r => r.MinPrice)
                .HasColumnType("money");

            modelBuilder.Entity<Restaurant>()
                .Property(r => r.MaxPrice)
                .HasColumnType("money");

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
                .HasForeignKey(r => r.Id);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Restaurant)
                .WithMany(r => r.Reviews)
                .HasForeignKey(r => r.Id);
        }

        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Restaurant> Restaurants { get; set; } = default!;
        public DbSet<Roles> Roles { get; set; } = default!;
        public DbSet<Floor> Floors { get; set; } = default!;
        public DbSet<Table> Tables { get; set; } = default!;
        public DbSet<Reservation> Reservations { get; set; } = default!;
        public DbSet<Review> Reviews { get; set; } = default!;
        public DbSet<ReservationSlot> ReservationSlots { get; set; } = default!;
    }
}
