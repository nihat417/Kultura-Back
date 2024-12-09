using Kultura.Application.Dto.RestaurntDtos;
using Kultura.Domain.Entities;
using static Kultura.Application.Model.Responses.ServiceResponses;

namespace Kultura.Application.Repository.Abstract
{
    public interface IRestaurantService
    {
        Task<LoginResponse> LoginRestaurant(RestaurantLoginDto restaurantLogin);
        Task<GeneralResponse> RegisterRestaurant(RestaurantRegisterDto restaurantRegister);


        #region general services

        //get
        Task<GeneralResponse> GetRestaurantById(string id);
        Task<GeneralResponse> GetRestaurantByEmail(string email);

        //post
        Task<GeneralResponse> AddFloor(FloorDto floor);
        Task<GeneralResponse> AddTable(TableDto tableDto);

        //delete
        Task<GeneralResponse> DeleteFloor(FloorDto floor);

        #endregion

        //operation services
        Task<GeneralResponse> FindEmailRestaurant(string email);
        Task<Restaurant> GetByEmailAsync(string email);


        //token services
        Task<GeneralResponse> GenerateEmailConfirmToken(string email);
        Task<GeneralResponse> ConfirmEmail(string token, string email);


        Task UpdateAsync(Restaurant restaurant);
    }
}
