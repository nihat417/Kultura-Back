using Microsoft.AspNetCore.Http;

namespace Kultura.Application.Dto.RestaurntDtos
{
    public class AddRestaurantPhotosDto
    {
        public string? RestaurantId { get; set; }
        public List<IFormFile>? Images { get; set; }
    }
}
