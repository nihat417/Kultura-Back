using Kultura.Application.Dto.RestaurntDtos;
using static Kultura.Application.Model.Responses.ServiceResponses;

namespace Kultura.Application.Repository.Abstract
{
    public interface IRestaurantService
    {
        Task<GeneralResponse> LoginRestaurant(RestaurantLoginDto restaurantLogin);
        Task<GeneralResponse> RegisterRestaurant(RestaurantRegisterDto restaurantRegister);

        //services
        Task<GeneralResponse> FindEmailRestaurant(string email);
        
    }
}
