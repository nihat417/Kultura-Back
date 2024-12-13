namespace Kultura.Domain.Entities
{
    public class ReservationSlot : BaseEntity
    {
        public string TableId { get; set; } = null!;
        public virtual Table Table { get; set; } = default!;

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public bool IsReserved { get; set; }
        public virtual List<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
