using Microsoft.AspNetCore.Http;

namespace Kultura.Application.Dto
{
    public class UserPhotoDto
    {
        public string? UserId {  get; set; }
        public IFormFile? UserImage { get; set; }
    }
}
