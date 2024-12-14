using System.ComponentModel.DataAnnotations;

namespace Kultura.Application.Dto
{
    public class ReservationDto
    {
        public string TableId { get; set; } = null!;
        public string UserId { get; set; } = null!;

        [DisplayFormat(DataFormatString = "{0:d/M/yyyy}")]
        public DateTime ReservationDate { get; set; }

        public string SlotId { get; set; } = null!;
    }
}
