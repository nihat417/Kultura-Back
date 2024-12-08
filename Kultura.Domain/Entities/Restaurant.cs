namespace Kultura.Domain.Entities
{
    public class Restaurant : BaseEntity
    {
        public string Location { get; set; } = default!;
        public string? Description { get; set; }
        public string? MainPhoto { get; set; }

        public string Name { get; set; } = default!;
        public string Email { get; set; } = null!;

        public bool EmailConfirmed { get; set; }

        public string PhoneNumber { get; set; } = null!;
        public string? Cuisines { get; set; }

        public string Password { get; set; } = null!;

        public virtual List<string> Photos { get; set; } = new List<string>();
        public virtual List<Floor> Floors { get; set; } = new List<Floor>();
        public virtual List<Review> Reviews { get; set; } = new List<Review>();
        public virtual List<Reservation> Reservations { get; set; } = new List<Reservation>();

        private TimeSpan _openingTime;
        private TimeSpan _closingTime;

        public string OpeningTime
        {
            get => _openingTime.ToString(@"hh\:mm");
            set
            {
                if (!TimeSpan.TryParse(value, out _openingTime))
                    throw new ArgumentException("Invalid opening time format. Use HH:mm format.");
            }
        }

        public virtual ICollection<Table> Tables { get; set; }

        public string ClosingTime
        {
            get => _closingTime.ToString(@"hh\:mm");
            set
            {
                if (!TimeSpan.TryParse(value, out _closingTime))
                    throw new ArgumentException("Invalid closing time format. Use HH:mm format.");
                if (_closingTime <= _openingTime)
                    throw new ArgumentException("Closing time must be later than opening time.");
            }
        }

        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }

        public virtual Roles Roles { get; set; } = null!;

        public double AverageRating
        {
            get
            {
                if (!Reviews.Any()) return 0;
                return Math.Round(Reviews.Average(r => r.Rating), 1);
            }
        }


        public string PasswordHash { get; set; } = null!;
    }
}
