namespace Kultura.Domain.Entities
{
    public class Reservation : BaseEntity
    {
        public virtual Table Table { get; set; } = default!;

        public virtual User User { get; set; } = default!;

        public DateTime ReservationDate { get; set; }

        public string ReceiptCode { get; set; } = Guid.NewGuid().ToString("N");

        public string SlotId { get; set; } = null!;
        public virtual ReservationSlot Slot { get; set; } = default!;

        public string? TableId { get; set; } 
        public string? UserId { get; set; }
    }
}
