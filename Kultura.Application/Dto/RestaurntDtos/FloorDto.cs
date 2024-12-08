using System.ComponentModel.DataAnnotations;

namespace Kultura.Application.Dto.RestaurntDtos
{
    public record FloorDto
    {
        private int _number;

        [Required(ErrorMessage = "Floor number is required")]
        [Range(18, 90, ErrorMessage = "Floor number must be between 1 and 30")]
        public int Number
        {
            get => _number;
            set
            {
                if (value >= 1)
                    _number = value;
                else
                    throw new ArgumentOutOfRangeException("Floor number  must be at least 1");
            }
        }

        public string RestaurantId { get; set; } = null!;
    }
}
