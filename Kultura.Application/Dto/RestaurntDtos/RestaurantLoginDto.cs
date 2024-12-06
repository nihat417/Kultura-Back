using System.ComponentModel.DataAnnotations;

namespace Kultura.Application.Dto.RestaurntDtos
{
    public record RestaurantLoginDto
    {
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = string.Empty;


        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
