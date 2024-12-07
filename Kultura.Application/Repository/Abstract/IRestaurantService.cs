using Kultura.Application.Dto.RestaurntDtos;
using Kultura.Domain.Entities;
using static Kultura.Application.Model.Responses.ServiceResponses;

namespace Kultura.Application.Repository.Abstract
{
    public interface IRestaurantService
    {
        Task<GeneralResponse> LoginRestaurant(RestaurantLoginDto restaurantLogin);
        Task<GeneralResponse> RegisterRestaurant(RestaurantRegisterDto restaurantRegister);

        //operation services
        Task<GeneralResponse> FindEmailRestaurant(string email);
        Task<Restaurant> GetByEmailAsync(string email);


        //token services
        Task<GeneralResponse> GenerateEmailConfirmToken(string email);
        Task<GeneralResponse> ConfirmEmail(string token, string email);


        Task UpdateAsync(Restaurant restaurant);
    }
}
