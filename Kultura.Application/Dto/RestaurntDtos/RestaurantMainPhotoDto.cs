using Microsoft.AspNetCore.Http;

namespace Kultura.Application.Dto.RestaurntDtos
{
    public class RestaurantMainPhotoDto
    {
        public string? RestaurantId { get; set; }
        public IFormFile? MainImage { get; set; }
    }
}
