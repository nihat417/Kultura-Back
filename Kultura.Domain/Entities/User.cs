using Microsoft.AspNetCore.Identity;

namespace Kultura.Domain.Entities
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = null!;
        public int Age { get; set; }
        public string? ImageUrl { get; set; }

        public virtual List<Review> Reviews { get; set; } = new();
        public virtual List<Reservation> Reservations { get; set; } = new();
    }
}
