using System.ComponentModel.DataAnnotations;

namespace Kultura.Application.Dto.AuthDto
{
    public record ForgotDto
    {
        [Required]
        public string Email { get; set; } = null!;
    }
}
