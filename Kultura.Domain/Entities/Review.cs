namespace Kultura.Domain.Entities
{
    public class Review : BaseEntity
    {
        public string UserId { get; set; } = null!;
        public virtual User User { get; set; }

        public string RestaurantId { get; set; } = null!;
        public virtual Restaurant Restaurant { get; set; }

        public string Comment { get; set; } = string.Empty;

        private int _rating;
        public int Rating
        {
            get => _rating;
            set
            {
                if (value < 1 || value > 5)
                    throw new ArgumentException("Rating must be between 1 and 5.");
                _rating = value;
            }
        }
    }
}
