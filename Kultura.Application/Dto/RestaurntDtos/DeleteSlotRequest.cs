using System.ComponentModel.DataAnnotations;

namespace Kultura.Application.Dto.RestaurntDtos
{
    public class DeleteSlotRequest
    {
        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [StringLength(50)]
        public string RestaurantId { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Floor number must be between 1 and 100.")]
        public int FloorNumber { get; set; }
    }
}
