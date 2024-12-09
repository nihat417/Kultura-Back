using Kultura.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Kultura.Application.Dto.RestaurntDtos
{
    public class TableDto
    {
        [Required]
        [Range(1, 5, ErrorMessage = "Capacity must be between 1 and 5.")]
        public int Capacity { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Floor number must be greater than or equal to 1.")]
        public int FloorNumber { get; set; }

        [Required]
        public string RestaurantId { get; set; } = null!;

        [Required]
        public TableShapeType ShapeType { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "X coordinate must be a positive value.")]
        public int X { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Y coordinate must be a positive value.")]
        public int Y { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Radius must be a positive value.")]
        public double Radius { get; set; }
    }
}
