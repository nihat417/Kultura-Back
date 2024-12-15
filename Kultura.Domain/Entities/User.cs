    namespace Kultura.Domain.Entities
    {
        public class User : BaseEntity
        {
            public string FullName { get; set; } = null!;

            public string Email { get; set; } = null!;

            public string Password { get; set; } = null!;
            public string PasswordHash { get; set; } = null!;

            public string? Country { get; set; }

            public bool EmailConfirmed { get; set; }

            public int Age { get; set; }
            public string? ImageUrl { get; set; }
            public DateTime? CreatedTime { get; set; }

            public virtual Roles Roles { get; set; } = null!;

            public virtual List<Review> Reviews { get; set; } = new();
            public virtual List<Reservation> Reservations { get; set; } = new();
            public virtual List<Favourite> Favorites { get; set; } = new();
    }
    }
