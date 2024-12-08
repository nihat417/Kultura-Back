namespace Kultura.Domain.Entities
{
    public class Roles:BaseEntity
    {
        public string Name { get; set; } = null!;

        public virtual List<User> Users { get; set; } = new();
        public virtual List<Restaurant> Restaurants { get; set; } = new();
    }
}
