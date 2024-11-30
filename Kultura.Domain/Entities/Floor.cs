namespace Kultura.Domain.Entities
{
    public class Floor : BaseEntity
    {
        public string Name { get; set; } = default!;

        public virtual Restaurant Restaurant { get; set; } = default!;

        public virtual List<Table> Tables { get; set; } = new();
    }
}
