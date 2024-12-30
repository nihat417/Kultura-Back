namespace Kultura.Application.Dto
{
    public class UpdateProfileDto
    {
        public string UserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Country { get; set; }
        public int Age { get; set; }
        public string? ImageUrl { get; set; }
    }
}
