namespace Kultura.Domain.Entities
{
    public class Floor : BaseEntity
    {
        public int Number { get; set; } = default!;

        public string RestaurantId { get; set; } = null!;

        public virtual Restaurant Restaurant { get; set; } = default!;

        public virtual List<Table> Tables { get; set; } = new();
    }
}
