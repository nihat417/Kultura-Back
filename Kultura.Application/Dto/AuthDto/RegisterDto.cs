using System.ComponentModel.DataAnnotations;

namespace Kultura.Application.Dto.AuthDto
{
    public record RegisterDto
    {
        [Required]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;

        private int _age;

        [Required(ErrorMessage = "Age is required")]
        [Range(18, 90, ErrorMessage = "Age must be between 18 and 90")]
        public int Age
        {
            get => _age;
            set
            {
                if (value >= 18)
                    _age = value;
                else
                    throw new ArgumentOutOfRangeException("Age must be at least 18");
            }
        }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
