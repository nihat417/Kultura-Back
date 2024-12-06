using System.ComponentModel.DataAnnotations;

namespace Kultura.Application.Dto.RestaurntDtos
{
    public record RestaurantRegisterDto
    {
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Required]
        public string Location { get; set; } = null!;

        public string? Description { get; set; }

        public string? Cuisines { get; set; }

        [Required]
        [Phone]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; } = null!;

        //[Required]
        //public string MainPhoto { get; set; } = null!;

        [Required]
        [Range(0, 5000, ErrorMessage = "Min price 0  5000.")]
        public decimal MinPrice { get; set; }

        [Required]
        [Range(0, 5000, ErrorMessage = "max price 0 5000.")]
        public decimal MaxPrice { get; set; }

        [Required]
        [DataType(DataType.Time)]
        public string OpeningTime { get; set; } = null!;

        [Required]
        [DataType(DataType.Time)]
        public string ClosingTime { get; set; } = null!;

    }
}
