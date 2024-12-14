namespace Kultura.Domain.Entities
{
    public class Favourite :BaseEntity
    {
        public string UserId { get; set; } = null!;
        public virtual User User { get; set; } = null!;

        public string RestaurantId { get; set; } =null!;
        public virtual Restaurant Restaurant { get; set; } = null!;
    }
}
